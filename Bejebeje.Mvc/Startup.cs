using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bejebeje.Mvc
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // this method gets called by the runtime.
    // use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services
        .AddControllersWithViews();
    }

    // this method gets called by the runtime.
    // yse this method to configure the http request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app
          .UseDeveloperExceptionPage();
      }
      else
      {
        app
          .UseExceptionHandler("/Home/Error");

        // the default hsts value is 30 days.
        // you may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app
          .UseHsts();
      }

      app
        .UseHttpsRedirection();

      app
        .UseStaticFiles();

      app
        .UseRouting();

      app
        .UseAuthorization();

      app
        .UseEndpoints(endpoints =>
      {
        endpoints
          .MapControllerRoute(
            "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
      });
    }
  }
}
