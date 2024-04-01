using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Coverlet;
using System.IO;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.GitHub.GitHubTasks;
using Nuke.Common.Tools.DocFX;
using static Nuke.Common.Tools.DocFX.DocFXTasks;
using Nuke.GitHub;
using Nuke.Common.Tools.AzureKeyVault;
using static Nuke.GitHub.ChangeLogExtensions;
using static Nuke.WebDocu.WebDocuTasks;
using Nuke.WebDocu;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitVersion(Framework = "net8.0", NoFetch = true)] GitVersion GitVersion;

    [GitRepository] readonly GitRepository GitRepository;

    AbsolutePath OutputDirectory => RootDirectory / "output";

    [Solution] readonly Solution Solution;

    [Parameter] readonly string GitHubAuthenticationToken;

    AbsolutePath ChangeLogFile => RootDirectory / "CHANGELOG.md";
    AbsolutePath DocFxFile => RootDirectory / "docfx.json";

    [Parameter] private readonly string DocuBaseUrl = "https://docs.dangl-it.com";
    [Parameter] private readonly string DocuApiKey;

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
namespace IPA.BCFier
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

    private Target UploadDocumentation => _ => _
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

    Target PublishGitHubRelease => _ => _
        .Requires(() => GitHubAuthenticationToken)
        .OnlyWhenDynamic(() => GitVersion.BranchName.Equals("master") || GitVersion.BranchName.Equals("origin/master"))
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
}
