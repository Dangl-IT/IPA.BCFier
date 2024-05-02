using Dangl.Data.Shared.AspNetCore.SpaUtilities;
using ElectronNET.API;
using IPA.Bcfier.App.Configuration;
using IPA.Bcfier.App.Data;
using IPA.Bcfier.App.Services;
using IPA.Bcfier.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IPA.Bcfier.App
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddElectron();
            services.AddLocalizedSpaStaticFiles(".IPA.App.Locale", new[] { "en" }, "dist");
            services.AddSingleton<ElectronWindowProvider>();
            services.AddTransient<SettingsService>();
            services.AddHttpContextAccessor();
            services.AddSingleton(new RevitParameters());
            services.AddHttpClient<TeamsMessagesService>();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddBcfierSwagger();
            services.AddTransient<DatabaseLocationService>();

            AddDatabaseServices(services);
        }

        private static void AddDatabaseServices(IServiceCollection services)
        {
            if (Environment.GetEnvironmentVariable("BCFIER_USE_SQLITE_DESIGN_TIME_CONTEXT") == "true")
            {
                services.AddDbContext<BcfierDbContext>(optionsAction: (_, contextOptionsBuilder) =>
                {
                    contextOptionsBuilder
                        .UseSqlite(DatabaseLocationService.GetSqliteInMemoryConnectionString(), opt => opt.MigrationsAssembly(typeof(Program).Assembly.FullName));
                });
            }
            else
            {
                services.AddScoped<BcfierDbContext>(s =>
                {
                    var databaseLocationService = s.GetRequiredService<DatabaseLocationService>();
                    var databaseLocation = databaseLocationService
                        .GetDatabaseConnectionStringAsync()
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();

                    var contextOptionsBuilder = new DbContextOptionsBuilder<BcfierDbContext>();
                    contextOptionsBuilder
                        .UseSqlite(databaseLocation, sqlOptions => sqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName));

                    return new BcfierDbContext(contextOptionsBuilder.Options);
                });
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
            app.UseRouting();

            app.UseBcfierSwaggerUi();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            if (env.IsDevelopment())
            {
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "../ipa-bcfier-ui";
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                });
            }
            else
            {
                app.UseLocalizedSpaStaticFiles("index.html", "dist", cacheFilesInRootPath: false);
            }
        }
    }
}
