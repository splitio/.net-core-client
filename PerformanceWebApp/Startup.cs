using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using App.Metrics.Configuration;
using App.Metrics.Data;
using App.Metrics.DependencyInjection;
using App.Metrics.Formatters.Json;
using App.Metrics.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using App.Metrics.Extensions.Reporting.InfluxDB;
using App.Metrics.Extensions.Reporting.InfluxDB.Client;
using App.Metrics.Reporting.Interfaces;

namespace PerformanceWebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc(options =>
                options.AddMetricsResourceFilter());

            var database = "appmetricsdemo";
            var uri = new Uri("http://127.0.0.1:8083");

            services.AddMetrics(options =>
            {
                options.WithGlobalTags((globalTags, info) =>
                {
                    globalTags.Add("app", info.EntryAssemblyName);
                    globalTags.Add("env", "stage");
                });
            })
               .AddHealthChecks()
               .AddJsonSerialization()
               .AddReporting(
                  factory =>
                  {
                      factory.AddInfluxDb(
                new InfluxDBReporterSettings
                        {
                            InfluxDbSettings = new InfluxDBSettings(database, uri),
                            ReportInterval = TimeSpan.FromSeconds(5)
                        });
                  })
               .AddMetricsMiddleware(options => options.IgnoredHttpStatusCodes = new[] { 404 });

            services.AddMvc(options => options.AddMetricsResourceFilter());

            /*services.AddMetrics(
                options =>
            {
                options.DefaultContextLabel = "Mvc.Sample";
                options.WithGlobalTags((globalTags, envInfo) =>
                {
                    globalTags.Add("host", envInfo.HostName);
                    globalTags.Add("machine_name", envInfo.MachineName);
                    globalTags.Add("app_name", envInfo.EntryAssemblyName);
                    globalTags.Add("app_version", envInfo.EntryAssemblyVersion);
                });
            }
            )
            .AddJsonSerialization()
            .AddHealthChecks()
            .AddMetricsMiddleware(Configuration.GetSection("AspNetMetrics"));*/
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime lifetime)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            app.UseMetrics();
            app.UseMetricsReporting(lifetime);
            app.UseMvc();


            /* loggerFactory.AddConsole(Configuration.GetSection("Logging"));
             loggerFactory.AddDebug();

             if (env.IsDevelopment())
             {
                 app.UseDeveloperExceptionPage();
                 app.UseBrowserLink();
             }
             else
             {
                 app.UseExceptionHandler("/Home/Error");
             }

             app.UseStaticFiles();

             app.UseMetrics();


             app.UseMvc(routes =>
             {
                 routes.MapRoute(
                     name: "default",
                     template: "{controller=Home}/{action=Index}/{id?}");
             });*/

        }
    }
}
