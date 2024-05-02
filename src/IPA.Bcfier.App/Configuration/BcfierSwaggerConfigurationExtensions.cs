using NSwag;

namespace IPA.Bcfier.App.Configuration
{
    public static class BcfierSwaggerConfigurationExtensions
    {
        public static IServiceCollection AddBcfierSwagger(this IServiceCollection services)
        {
            services.AddOpenApiDocument(c =>
            {
                c.Description = "\"Access to the IPA.Bcfier API\" API Specification";
                c.Version = FileVersionProvider.NuGetVersion;
                c.Title = $"\"Access to the IPA.Bcfier API\" API {FileVersionProvider.NuGetVersion}";
            });

            return services;
        }

        public static IApplicationBuilder UseBcfierSwaggerUi(this IApplicationBuilder app)
        {
            app.UseOpenApi(c =>
            {
                c.Path = "/swagger/swagger.json";
                c.PostProcess = (doc, _) =>
                {
                    // This makes sure that Azure warmup requests that are sent via Http instead of
                    // Https don't set the document schema to http only
                    doc.Schemes = new List<OpenApiSchema> { OpenApiSchema.Https };
                };
            });
            app.UseSwaggerUi(settings =>
            {
                settings.DocumentPath = "/swagger/swagger.json";
                settings.Path = "/swagger";
            });

            return app;
        }
    }
}
