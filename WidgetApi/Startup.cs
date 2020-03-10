using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WidgetApi.EFCore;
using WidgetApi.Swagger;

namespace WidgetApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
        {
            services
                .AddControllers()
                .AddFluentValidation();

            services.AddApiVersioning(o =>
            {
                // returns the headers "api-supported-versions" and "api-deprecated-versions"
                o.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });


            var cn = Configuration.GetConnectionString("DefaultConnection");

            if (env.IsDevelopment())
            {
                services
                    .AddDbContext<WidgetContext>(o => o
                    .UseSqlite(cn));

                var context = services
                    .BuildServiceProvider()
                    .GetService<WidgetContext>();

                WidgetData.Seed(context);
            }
            else
            {
                services
                    .AddDbContext<WidgetContext>(o => o
                    .UseSqlServer(cn));
            }

            // register validators
            services.AddTransient<IValidator<V1.DTO.WidgetDTO>, V1.Validators.WidgetDTOValidatorCollection>();
            services.AddTransient<IValidator<V2.DTO.WidgetDTO>, V2.Validators.WidgetDTOValidatorCollection>();

            services.AddSwaggerService();
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwaggerService(provider);
        }
    }
}

