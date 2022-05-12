using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace DevIO.Api.Configuration
{
    public static class ApiConfig
    {
        public static IServiceCollection AddWebApiConfig(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("Development",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());

                options.AddPolicy("Production",
                    builder => builder.AllowAnyOrigin()
                        .WithMethods("GET")
                        .WithOrigins("http://desenvolvedor.io")
                        //.WithHeaders(HeaderNames.ContentType,"x-custom-header")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            return services;
        }

        public static IApplicationBuilder UseWebApiConfig(this WebApplication app)
        {
            app.UseHttpsRedirection();

            //app.UseCors("Development");

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            return app;
        }
    }
}