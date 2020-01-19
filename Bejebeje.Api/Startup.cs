namespace Bejebeje.Api
{
  using Bejebeje.DataAccess.Configuration;
  using Bejebeje.DataAccess.Context;
  using Bejebeje.Services.Services;
  using Bejebeje.Services.Services.Interfaces;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Microsoft.OpenApi.Models;

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
      string databaseConnectionString = Configuration["Database:DefaultConnectionString"];

      services
        .AddDbContext<BbContext>(options => options
          .UseNpgsql(databaseConnectionString)
          .UseSnakeCaseNamingConvention());

      services
        .AddScoped<IDataSeederService, DataSeederService>();

      services
        .Configure<InitialSeedConfiguration>(c =>
        {
          c.ConnectionString = databaseConnectionString;
        });

      services
        .AddAuthentication("Bearer")
        .AddIdentityServerAuthentication(options =>
        {
          options.Authority = Configuration["Authority"];
          options.RequireHttpsMetadata = false;
          options.ApiName = Configuration["ApiName"];
        });

      services
        .AddCors(options =>
        {
          // this defines a CORS policy called "default"
          options.AddPolicy("default", policy =>
          {
            policy.WithOrigins(Configuration["FrontendCorsOrigin"])
              .AllowAnyHeader()
              .AllowAnyMethod();
          });
        });

      services
        .AddScoped<IArtistsService, ArtistsService>();

      services
        .AddScoped<IArtistSlugsService, ArtistSlugsService>();

      services
        .AddScoped<ILyricsService, LyricsService>();

      services
        .AddScoped<IAuthorService, AuthorService>();

      services
        .AddScoped<IImagesService, ImagesService>();

      services
        .AddMvc()
        .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

      services
        .AddSwaggerGen(c =>
        {
          c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bejebeje API", Version = "v1" });
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();

        app.UseSwagger();

        app.UseSwaggerUI(c =>
        {
          c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bejebeje API V1");
        });
      }
      else
      {
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseRouting();

      app.UseCors("default");

      app.UseAuthentication();

      app.UseHttpsRedirection();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
      });
    }
  }
}
