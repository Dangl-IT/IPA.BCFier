using ElectronNET.API;
using ElectronNET.API.Entities;
using IPA.Bcfier.App.Configuration;
using IPA.Bcfier.App.Data;
using IPA.Bcfier.App.Services;
using IPA.Bcfier.Ipc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace IPA.Bcfier.App
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();

                await host.StartAsync();

                var browserWindowOptions = new BrowserWindowOptions
                {
                    Title = "IPA.BCFier",
                    Icon = Path.Combine(Directory.GetCurrentDirectory(), "bcfier.png"),
                    Height = 800,
                    Width = 1200,
                    AutoHideMenuBar = true,
                    AlwaysOnTop = true
                };
                var window = await Electron.WindowManager.CreateWindowAsync(browserWindowOptions);

                var hasRevitIntegration = false;
                var hasNavisworksIntegration = false;
                Guid? appCorrelationId = null;
                using (var scope = host.Services.CreateScope())
                {
                    scope.ServiceProvider.GetRequiredService<ElectronWindowProvider>().SetBrowserWindow(window);
                    hasRevitIntegration = await Electron.App.CommandLine.HasSwitchAsync("revit-integration");
                    scope.ServiceProvider.GetRequiredService<RevitParameters>().IsConnectedToRevit = hasRevitIntegration;
                    hasNavisworksIntegration = await Electron.App.CommandLine.HasSwitchAsync("navisworks-integration");
                    scope.ServiceProvider.GetRequiredService<NavisworksParameters>().IsConnectedToNavisworks = hasNavisworksIntegration;
                    var appCorrelationIdString = await Electron.App.CommandLine.GetSwitchValueAsync("app-correlation-id");
                    if (!string.IsNullOrWhiteSpace(appCorrelationIdString)
                        && (args?.Any() ?? false)
                        && Array.IndexOf(args, "--app-correlation-id") > -1)
                    {
                        var index = Array.IndexOf(args, "--app-correlation-id");
                        if (args.Length > index + 1)
                        {
                            appCorrelationIdString = args[index + 1];
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(appCorrelationIdString) && Guid.TryParse(appCorrelationIdString, out var correlationId))
                    {
                        appCorrelationId = correlationId;
                        scope.ServiceProvider.GetRequiredService<AppParameters>().ApplicationId = correlationId;
                    }

                    var revitProjectPath = await Electron.App.CommandLine.GetSwitchValueAsync("revit-project-path");
                    if (!string.IsNullOrWhiteSpace(revitProjectPath))
                    {
                        scope.ServiceProvider.GetRequiredService<RevitParameters>().RevitProjectPath = revitProjectPath;
                    }

                    try
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<BcfierDbContext>();
                        await dbContext.Database.MigrateAsync();
                    }
                    catch
                    {
                        // Ignoring here, it's likely a problem since we're still using the InMemory
                        // db if no real database is configured
                    }
                }

                await Electron.IpcMain.On("closeApp", async (e) =>
                {
                    if (appCorrelationId != null)
                    {
                        if (hasRevitIntegration)
                        {
                            try
                            {
                                using var ipcHandler = new IpcHandler(thisAppName: "BcfierApp", otherAppName: "Revit", appCorrelationId.Value);
                                await ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                                {
                                    Command = IpcMessageCommand.AppClosed,
                                    Data = appCorrelationId.ToString()
                                }), timeout: 500);
                            }
                            catch (Exception ex)
                            {
                                // We're not really handling failures here, just write them to the
                                // console and then continue with closing the window
                                Console.WriteLine(ex);
                            }
                        }
                        if (hasNavisworksIntegration)
                        {
                            try
                            {
                                using var ipcHandler = new IpcHandler(thisAppName: "BcfierAppNavisworks", otherAppName: "Navisworks", appCorrelationId.Value);
                                await ipcHandler.SendMessageAsync(JsonConvert.SerializeObject(new IpcMessage
                                {
                                    Command = IpcMessageCommand.AppClosed,
                                    Data = appCorrelationId.ToString()
                                }), timeout: 500);
                            }
                            catch (Exception ex)
                            {
                                // We're not really handling failures here, just write them to the
                                // console and then continue with closing the window
                                Console.WriteLine(ex);
                            }
                        }
                    }
                    window.Close();
                });

                await host.WaitForShutdownAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging(c => c.AddDebug().AddConsole())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseElectron(args);
                    webBuilder.UseStartup<Startup>();
                });
    }
}
