using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Coverlet;
using System.IO;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.GitHub.GitHubTasks;
using Nuke.Common.Tools.DocFX;
using static Nuke.Common.Tools.DocFX.DocFXTasks;
using Nuke.GitHub;
using static Nuke.GitHub.ChangeLogExtensions;
using static Nuke.WebDocu.WebDocuTasks;
using Nuke.WebDocu;
using Nuke.Common.Tools.NSwag;
using System.Text.RegularExpressions;
using NJsonSchema.Generation;
using NJsonSchema;
using NSwag;
using NSwag.CodeGeneration.TypeScript;
using NJsonSchema.CodeGeneration.TypeScript;
using System.Collections.Generic;
using static Nuke.Common.Tools.Npm.NpmTasks;
using Nuke.Common.Tools.Npm;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO.Compression;
using Nuke.Common.Utilities;
using System.Configuration;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitVersion(Framework = "net8.0", NoFetch = true)] GitVersion GitVersion;

    [GitRepository] readonly GitRepository GitRepository;

    AbsolutePath OutputDirectory => RootDirectory / "output";

    [Solution(SuppressBuildProjectCheck = true)] readonly Solution Solution;

    [Parameter] readonly string GitHubAuthenticationToken;

    AbsolutePath ChangeLogFile => RootDirectory / "CHANGELOG.md";
    AbsolutePath DocFxFile => RootDirectory / "docfx.json";
    AbsolutePath SourceDirectory => RootDirectory / "src";

    [Parameter] private readonly string DocuBaseUrl = "https://docs.dangl-it.com";
    [Parameter] private readonly string DocuApiKey;

    [Parameter] readonly string CodeSigningCertificateKeyVaultBaseUrl;
    [Parameter] readonly string KeyVaultClientId;
    [Parameter] readonly string KeyVaultClientSecret;
    [Parameter] readonly string CodeSigningKeyVaultTenantId;
    [Parameter] readonly string CodeSigningCertificateName;
    [Parameter] readonly string MigrationName;

    [Parameter] AbsolutePath ExecutablesToSignFolder;
    [NuGetPackage("Tools.InnoSetup", "tools/ISCC.exe")] readonly Tool InnoSetup;

    [NuGetPackage("AzureSignTool", "tools/net8.0/any/AzureSignTool.dll")]
    readonly Tool AzureSign;

    Target Clean => _ => _
        .Executes(() =>
        {
            (RootDirectory / "src").GlobDirectories("**/bin", "**/obj").ForEach(d => d.DeleteDirectory());
            (RootDirectory / "tests").GlobDirectories("**/bin", "**/obj").ForEach(d => d.DeleteDirectory());
            OutputDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            WriteFileVersionProvider();

            DotNetBuild(s => s
               .SetProjectFile(Solution)
               .SetConfiguration(Configuration)
               .SetAssemblyVersion(GitVersion.AssemblySemVer)
               .SetFileVersion(GitVersion.AssemblySemVer)
               .SetInformationalVersion(GitVersion.InformationalVersion)
               .EnableNoRestore());
        });

    private void WriteFileVersionProvider()
    {
        var fileVersionPath = RootDirectory / "src" / "IPA.BCFier" / "FileVersionProvider.cs";
        var date = System.DateTime.UtcNow;
        var dateCode = $"new DateTime({date.Year}, {date.Month}, {date.Day}, {date.Hour}, {date.Minute}, {date.Second}, DateTimeKind.Utc)";
        var fileVersionCode = $@"using System;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace IPA.Bcfier
{{
    // This file is automatically generated from the build script
    [System.CodeDom.Compiler.GeneratedCode(""GitVersionBuild"", """")]
    public static class FileVersionProvider
    {{
        public static string AssemblyVersion => ""{GitVersion.Major}.{GitVersion.Minor}.{GitVersion.Patch}.0"";
        public static string FileVersion => ""{GitVersion.MajorMinorPatch}"";
        public static string NuGetVersion => ""{GitVersion.NuGetVersion}"";
        public static DateTime BuildDateUtc => {dateCode};
    }}
}}
";

        fileVersionPath.WriteAllText(fileVersionCode);
    }

    Target Tests => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var testProjects = new[]
            {
                RootDirectory / "tests" / "IPA.BCFier.Tests"
            };

            DotNetTest(c => c
                .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                .EnableNoBuild()
                .SetTestAdapterPath(".")
                .CombineWith(cc => testProjects
                    .Select(testProject =>
                    {
                        var projectDirectory = Path.GetDirectoryName(testProject);
                        var projectName = Path.GetFileNameWithoutExtension(testProject);
                        return cc
                         .SetProjectFile(testProject)
                         .SetLoggers($"xunit;LogFilePath={OutputDirectory / projectName}_testresults.xml");
                    })),
                        degreeOfParallelism: Environment.ProcessorCount,
                        completeOnFailure: true);
        });

    private Target BuildDocFxMetadata => _ => _
         .DependsOn(Restore)
         .Executes(() =>
         {
             DocFXMetadata(x => x.SetProjects(DocFxFile));
         });

    private Target BuildDocumentation => _ => _
         .DependsOn(Clean)
         .DependsOn(BuildDocFxMetadata)
         .Executes(() =>
         {
             // Using README.md as index.md
             if (File.Exists(RootDirectory / "index.md"))
             {
                 File.Delete(RootDirectory / "index.md");
             }

             File.Copy(RootDirectory / "README.md", RootDirectory / "index.md");

             DocFXBuild(x => x.SetConfigFile(DocFxFile));

             File.Delete(RootDirectory / "index.md");
             Directory.Delete(RootDirectory / "api", true);
             Directory.Delete(RootDirectory / "obj", true);
         });

    Target UploadDocumentation => _ => _
         .DependsOn(BuildDocumentation)
         .Requires(() => DocuApiKey)
         .Requires(() => DocuBaseUrl)
         .Executes(() =>
         {
             var changeLog = GetCompleteChangeLog(ChangeLogFile);

             WebDocu(s => s
                 .SetDocuBaseUrl(DocuBaseUrl)
                 .SetDocuApiKey(DocuApiKey)
                 .SetMarkdownChangelog(changeLog)
                 .SetSourceDirectory(OutputDirectory / "docs")
                 .SetVersion(GitVersion.NuGetVersion)
             );
         });

    Target GenerateFrontendVersion => _ => _
        .Executes(() =>
        {
            var buildDate = DateTime.UtcNow;
            var filePath = SourceDirectory / "ipa-bcfier-ui" / "src" / "app" / "version.ts";

            var currentDateUtc = $"new Date(Date.UTC({buildDate.Year}, {buildDate.Month - 1}, {buildDate.Day}, {buildDate.Hour}, {buildDate.Minute}, {buildDate.Second}))";

            var content = $@"// This file is automatically generated as part of the build process

export const version = {{
    version: ""{GitVersion.NuGetVersionV2}"",
    commitInfo: ""{GitVersion.FullBuildMetaData}"",
    commitDate: ""{GitVersion.CommitDate}"",
    commitHash: ""{GitVersion.Sha}"",
    informationalVersion: ""{GitVersion.InformationalVersion}"",
    buildDateUtc: {currentDateUtc}
}}";
            filePath.WriteAllText(content);
        });

    Target FrontEndRestore => _ => _
        .DependsOn(GenerateFrontendVersion)
        .After(Clean)
        .Executes(() =>
        {
            (SourceDirectory / "ipa-bcfier-ui" / "node_modules").CreateOrCleanDirectory();
            (SourceDirectory / "ipa-bcfier-ui" / "node_modules").DeleteDirectory();
            Npm("ci", SourceDirectory / "ipa-bcfier-ui");
        });

    Target BuildFrontend => _ => _
        .DependsOn(Clean)
        .DependsOn(FrontEndRestore)
        .DependsOn(GenerateFrontendVersion)
        .Executes(() =>
        {
            NpmRun(c =>
            {
                c = c
                    .SetProcessWorkingDirectory(SourceDirectory / "ipa-bcfier-ui")
                    .SetCommand("build");

                return c;
            });
        });

    Target BuildRevitPlugin => _ => _
        .DependsOn(BuildFrontend)
        .DependsOn(Compile)
        .DependsOn(BuildElectronApp)
        .Executes(() =>
        {

            CopyDirectoryRecursively(SourceDirectory / "ipa-bcfier-ui" / "dist" / "ipa-bcfier-ui" / "browser",
                SourceDirectory / "IPA.Bcfier.Revit" / "Resources" / "Browser",
                DirectoryExistsPolicy.Merge,
                FileExistsPolicy.Overwrite);
            var revitPluginOutputDirectory = OutputDirectory / "RevitPlugin";
            var navisworksPluginOutputDirectory = OutputDirectory / "NavisworksPlugin";

            var configurations = new[]
            {
                "Release-2024",
                "Release-2023",
                "Release-2022",
                "Release-2021"
            };

            // Compiling Revit plugin
            foreach (var configuration in configurations)
            {
                var outputDirectory = revitPluginOutputDirectory / configuration;
                DotNetBuild(c => c.SetProjectFile(SourceDirectory / "IPA.Bcfier.Revit" / "IPA.Bcfier.Revit.csproj")
                                .SetConfiguration(configuration)
                                .SetOutputDirectory(outputDirectory)
                                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                                .SetFileVersion(GitVersion.AssemblySemVer)
                                .SetInformationalVersion(GitVersion.InformationalVersion));
                SignExecutablesInFolder(outputDirectory, includeDll: true);
            }

            // Compiling Navisworks plugin
            XmlTasks.XmlPoke(SourceDirectory / "IPA.Bcfier.Navisworks" / "PackageContents.xml", "//ApplicationPackage/@AppVersion", GitVersion.NuGetVersion);
            foreach (var configuration in configurations)
            {
                var outputDirectory = navisworksPluginOutputDirectory / configuration;
                DotNetBuild(c => c.SetProjectFile(SourceDirectory / "IPA.Bcfier.Navisworks" / "IPA.Bcfier.Navisworks.csproj")
                                .SetConfiguration(configuration)
                                .SetOutputDirectory(outputDirectory)
                                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                                .SetFileVersion(GitVersion.AssemblySemVer)
                                .SetInformationalVersion(GitVersion.InformationalVersion));
                SignExecutablesInFolder(outputDirectory, includeDll: true);
                XmlTasks.XmlPoke(outputDirectory / "PackageContents.xml", "//ApplicationPackage[@AppVersion]", GitVersion.NuGetVersion);
            }

            File.Copy(RootDirectory / "Installer.iss", OutputDirectory / "Installer.iss", overwrite: true);

            var installerDirectory = OutputDirectory / "Installer";
            installerDirectory.CreateOrCleanDirectory();

            using var zipStream = File.OpenRead(OutputDirectory / "electron" / "IPA.Bcfier_Unzipped_Windows_X64.zip");
            ZipFile.ExtractToDirectory(zipStream, installerDirectory / "bcfier-app");

            CopyDirectoryRecursively(SourceDirectory / "IPA.Bcfier.Revit" / "InstallerAssets", installerDirectory / "InstallerAssets", DirectoryExistsPolicy.Merge, FileExistsPolicy.Overwrite);
            foreach (var configuration in configurations)
            {
                (installerDirectory / configuration).CreateOrCleanDirectory();
                File.Copy(revitPluginOutputDirectory / configuration / "Dangl.BCF.dll", installerDirectory / configuration / "Dangl.BCF.dll");
                File.Copy(revitPluginOutputDirectory / configuration / "IPA.Bcfier.dll", installerDirectory / configuration / "IPA.Bcfier.dll");
                File.Copy(revitPluginOutputDirectory / configuration / "IPA.Bcfier.Revit.dll", installerDirectory / configuration / "IPA.Bcfier.Revit.dll");
                File.Copy(revitPluginOutputDirectory / configuration / "DecimalEx.dll", installerDirectory / configuration / "DecimalEx.dll");
                File.Copy(revitPluginOutputDirectory / configuration / "IPA.Bcfier.Revit.addin", installerDirectory / configuration / "IPA.Bcfier.Revit.addin");
            }
            foreach (var configuration in configurations)
            {
                (installerDirectory / configuration).CreateOrCleanDirectory();
                File.Copy(navisworksPluginOutputDirectory / configuration / "Dangl.BCF.dll", installerDirectory / configuration / "Dangl.BCF.dll");
                File.Copy(navisworksPluginOutputDirectory / configuration / "IPA.Bcfier.dll", installerDirectory / configuration / "IPA.Bcfier.dll");
                File.Copy(navisworksPluginOutputDirectory / configuration / "IPA.Bcfier.Navisworks.dll", installerDirectory / configuration / "IPA.Bcfier.Navisworks.dll");
                File.Copy(navisworksPluginOutputDirectory / configuration / "DecimalEx.dll", installerDirectory / configuration / "DecimalEx.dll");
                File.Copy(navisworksPluginOutputDirectory / configuration / "Newtonsoft.Json.dll", installerDirectory / configuration / "Newtonsoft.Json.dll");
            }

            InnoSetup($"/dAppVersion=\"{GitVersion.AssemblySemVer}\" {OutputDirectory / "Installer.iss"}");

            SignExecutablesInFolder(installerDirectory / "output", false);
        });

    Target UploadRevitPlugin => _ => _
        .DependsOn(BuildRevitPlugin)
        .Executes(() =>
        {
            var changeLog = GetCompleteChangeLog(ChangeLogFile);
            var assets = (OutputDirectory / "output").GlobFiles("*.exe")
                .Select(f => f.ToString())
                .ToArray();
            Assert.NotEmpty(assets);

            AssetFileUpload(s => s
                           .SetDocuBaseUrl(DocuBaseUrl)
                           .SetDocuApiKey(DocuApiKey)
                           .SetVersion(GitVersion.NuGetVersion)
                           .SetAssetFilePaths(assets)
                           .SetSkipForVersionConflicts(true));
        });

    Target BuildElectronApp => _ => _
        .DependsOn(BuildFrontend)
        .DependsOn(Compile)
        .Executes(() =>
        {
            CopyDirectoryRecursively(SourceDirectory / "ipa-bcfier-ui" / "dist" / "ipa-bcfier-ui" / "browser", SourceDirectory / "IPA.Bcfier.App" / "wwwroot" / "dist" / "en", DirectoryExistsPolicy.Merge, FileExistsPolicy.Overwrite);

            // To ensure the tool is always up to date
            DotNet("tool update ElectronNET.CLI -g");

            BuildElectronAppInternal(
                        new[] { "Windows_X64", "/target win" }
                    );

            SignExecutablesInFolder(OutputDirectory / "electron", false);
        });

    Target SignExecutables => _ => _
        .Requires(() => ExecutablesToSignFolder)
        .Executes(() =>
        {
            SignExecutablesInFolder(ExecutablesToSignFolder, false);
        });

    private void SignExecutablesInFolder(AbsolutePath folderPath, bool includeDll)
    {
        Assert.True(!string.IsNullOrWhiteSpace(CodeSigningCertificateKeyVaultBaseUrl), "!string.IsNullOrWhitespace(CodeSigningCertificateKeyVaultBaseUrl)");
        Assert.True(!string.IsNullOrWhiteSpace(KeyVaultClientId), "!string.IsNullOrWhitespace(KeyVaultClientId)");
        Assert.True(!string.IsNullOrWhiteSpace(KeyVaultClientSecret), "!string.IsNullOrWhitespace(KeyVaultClientSecret)");
        Assert.True(!string.IsNullOrWhiteSpace(CodeSigningKeyVaultTenantId), "!string.IsNullOrWhitespace(CodeSigningKeyVaultTenantId)");
        Assert.True(!string.IsNullOrWhiteSpace(CodeSigningCertificateName), "!string.IsNullOrWhitespace(CodeSigningCertificateName)");

        var globPattern = includeDll ? new[] { "*.dll", "*.exe" } : new[] { "*.exe" };
        var inputFiles = folderPath.GlobFiles(globPattern);
        var filesListPath = OutputDirectory / $"{Guid.NewGuid()}.txt";
        filesListPath.WriteAllText(inputFiles.Select(f => f.ToString()).Join(Environment.NewLine) + Environment.NewLine);

        try
        {
            var azureSignArguments = new Arguments()
                .Add("sign")
                .Add("--azure-key-vault-url {0}", CodeSigningCertificateKeyVaultBaseUrl)
                .Add("--azure-key-vault-client-id {0}", KeyVaultClientId)
                .Add("--azure-key-vault-client-secret {0}", KeyVaultClientSecret)
                .Add("--azure-key-vault-tenant-id {0}", CodeSigningKeyVaultTenantId)
                .Add("--azure-key-vault-certificate {0}", CodeSigningCertificateName)
                .Add("--input-file-list {0}", filesListPath)
                .Add("--timestamp-rfc3161 {0}", "http://timestamp.digicert.com")
                .ToString();

            AzureSign(azureSignArguments);
        }
        finally
        {
            (filesListPath).DeleteFile();
        }
    }

    private void BuildElectronAppInternal(params string[][] electronOptions)
    {
        foreach (var electronOption in electronOptions)
        {
            (SourceDirectory / "IPA.Bcfier.App" / "bin").CreateOrCleanDirectory();

            var releaseIdentifier = electronOption[0];
            var electronArguments = electronOption[1];

            // Electron Build
            SetVersionInElectronManifest();

            var electronNet = ToolResolver.GetPathTool("electronize");
            electronNet(arguments: $"build /dotnet-configuration Release {electronArguments:nq}",
                workingDirectory: SourceDirectory / "IPA.Bcfier.App"
                );

            var exeFile = (SourceDirectory / "IPA.Bcfier.App" / "bin" / "Desktop").GlobFiles("IPA.Bcfier*.exe").Single();
            MoveFile(exeFile, OutputDirectory / "electron" / $"IPA.Bcfier.Setup_{releaseIdentifier}.exe");

            var unpackedDir = (SourceDirectory / "IPA.Bcfier.App" / "bin" / "Desktop").GlobDirectories("*unpacked").Single();
            (SourceDirectory / "IPA.Bcfier.App" / "bin" / "Desktop").GlobFiles("**/*.pdb").ForEach(f => f.DeleteFile());

            ZipFile.CreateFromDirectory(unpackedDir, OutputDirectory / "electron" / $"IPA.Bcfier_Unzipped_{releaseIdentifier}.zip");
        }
    }

    private void SetVersionInElectronManifest()
    {
        var manifestPath = SourceDirectory / "IPA.Bcfier.App" / "electron.manifest.json";
        var manifestJson = JObject.Parse(manifestPath.ReadAllText());
        manifestJson["build"]["buildVersion"] = GitVersion.SemVer;
        manifestPath.WriteAllText(manifestJson.ToString(Formatting.Indented));
    }

    Target UploadElectronApp => _ => _
        .DependsOn(BuildElectronApp)
        .DependsOn(UploadDocumentation)
        .Requires(() => DocuApiKey)
        .Requires(() => DocuBaseUrl)
        .Executes(() =>
        {
            var changeLog = GetCompleteChangeLog(ChangeLogFile);
            var assets = (OutputDirectory / "electron").GlobFiles("*.zip", "*.exe")
                .Select(f => f.ToString())
                .ToArray();
            Assert.NotEmpty(assets);

            AssetFileUpload(s => s
                           .SetDocuBaseUrl(DocuBaseUrl)
                           .SetDocuApiKey(DocuApiKey)
                           .SetVersion(GitVersion.NuGetVersion)
                           .SetAssetFilePaths(assets)
                           .SetSkipForVersionConflicts(true));
        });

    Target PublishGitHubRelease => _ => _
        .Requires(() => GitHubAuthenticationToken)
        .OnlyWhenDynamic(() => IsOnBranch("master"))
        .Executes(async () =>
        {
            var releaseTag = $"v{GitVersion.MajorMinorPatch}";

            var changeLogSectionEntries = ExtractChangelogSectionNotes(ChangeLogFile);
            var latestChangeLog = changeLogSectionEntries
                .Aggregate((c, n) => c + Environment.NewLine + n);
            var completeChangeLog = $"## {releaseTag}" + Environment.NewLine + latestChangeLog;

            var repositoryInfo = GetGitHubRepositoryInfo(GitRepository);
            var nuGetPackages = OutputDirectory.GlobFiles("*.nupkg").Select(p => p.ToString()).ToArray();
            Assert.NotEmpty(nuGetPackages);

            await PublishRelease(x => x
                .SetCommitSha(GitVersion.Sha)
                .SetReleaseNotes(completeChangeLog)
                .SetRepositoryName(repositoryInfo.repositoryName)
                .SetRepositoryOwner(repositoryInfo.gitHubOwner)
                .SetTag(releaseTag)
                .SetToken(GitHubAuthenticationToken));
        });

    Target CreateMigration => _ => _
        .Requires(() => MigrationName)
        .Executes(() =>
        {
            // We'll get all the current environment variables and place them in a string dictionary
            var environmentVariables = new Dictionary<string, string>();
            foreach (var environmentVariable in Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>())
            {
                environmentVariables.Add(environmentVariable.Key.ToString(), environmentVariable.Value.ToString());
            }

            // Then we'll also set a special one to instruct the app to use the design time SQLite
            // context options
            environmentVariables.Add("BCFIER_USE_SQLITE_DESIGN_TIME_CONTEXT", "true");
            DotNet($"ef migrations add {MigrationName}", workingDirectory: SourceDirectory / "IPA.Bcfier.App", environmentVariables: environmentVariables);
        });

    Target BuildFrontendSwaggerClient => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var nSwagConfigPath = SourceDirectory / "ipa-bcfier-ui" / "src" / "nswag.json";
            var nSwagToolPath = NuGetToolPathResolver.GetPackageExecutable("NSwag.MSBuild", "tools/Net80/dotnet-nswag.dll");
            DotNetRun(x => x
                .SetProcessToolPath(nSwagToolPath)
                .SetProcessWorkingDirectory(SourceDirectory / "ipa-bcfier-ui" / "src")
                .AddProcessEnvironmentVariable("BCFIER_USE_SQLITE_DESIGN_TIME_CONTEXT", "true")
                .SetProcessArgumentConfigurator(y => y
                    .Add($"/Input:\"{nSwagConfigPath}\"")));
        });

    private bool IsOnBranch(string branchName)
    {
        return GitVersion.BranchName.Equals(branchName) || GitVersion.BranchName.Equals($"origin/{branchName}");
    }
}
