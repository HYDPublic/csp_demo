﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using Joonasw.AspNetCore.SecurityHeaders;

namespace DotNetCoreSqlDb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddCsp(nonceByteAmount: 32);
            // Add framework services.
            services.AddMvc();

            // Use SQL Database if in Azure, otherwise, use SQLite
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
                services.AddDbContext<MyDatabaseContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("MyDbConnection")));
            else
                services.AddDbContext<MyDatabaseContext>(options =>
                    options.UseSqlite("Data Source=MvcMovie.db"));

            // Automatically perform database migration
            services.BuildServiceProvider().GetService<MyDatabaseContext>().Database.Migrate();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
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
            
            app.UseCsp(csp =>
            {
                csp.AllowScripts
                    .FromSelf()
                    .From("ajax.aspnetcdn.com");
                    
                csp.AllowStyles
                    .FromSelf()
                    .From("ajax.aspnetcdn.com");

                csp.ReportViolationsTo("https://prod-02.australiasoutheast.logic.azure.com:443/workflows/903685ffe04142749e603448cd9f12cf/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=S7h0xrxAQwtWYhW0s1iJq2pLnvTuAxZJWODTZYOnnRY");
            });
            
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Todos}/{action=Index}/{id?}");
            });
        }
    }
}
