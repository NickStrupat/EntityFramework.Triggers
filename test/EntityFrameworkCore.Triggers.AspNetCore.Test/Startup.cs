using System;
using System.Diagnostics;
using EntityFrameworkCore.Triggers.Tests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkCore.Triggers.AspNetCore.Test
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddDbContext<Context>();
            services.AddTriggers();
            services.AddSingleton<String>("What");
            services.AddSingleton<Object>("Okay");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseTriggers(builder =>
            {
                builder.Triggers().Inserted.Add(entry => Debug.WriteLine(entry.Entity.ToString()));
                builder.Triggers<Person, Context>().Inserted.Add(entry => Debug.WriteLine(entry.Entity.FirstName));
                builder.Triggers<Person, Context>().Inserted.Add<String>(entry => Debug.WriteLine(entry.Service));
                builder.Triggers<Person, Context>().Inserted.Add<(String Text, Object Object)>(entry => Debug.WriteLine($"{entry.Service.Text} {entry.Service.Object}"));
            });
        }
    }
}
