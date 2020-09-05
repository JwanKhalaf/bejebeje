namespace Bejebeje.Api
{
    using Bejebeje.DataAccess.Configuration;
    using Bejebeje.DataAccess.Context;
    using Bejebeje.Services.Config;
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

    public void ConfigureServices(IServiceCollection services)
    {
      string connectionString = Configuration["ConnectionString"];

      services
        .Configure<DatabaseOptions>(Configuration);

      services
        .AddDbContext<BbContext>(options => options
          .UseNpgsql(connectionString)
          .UseSnakeCaseNamingConvention());

      services
        .AddScoped<IDataSeederService, DataSeederService>();

      services
        .Configure<InitialSeedConfiguration>(c =>
        {
          c.ConnectionString = connectionString;
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

    public void Configure(
      IApplicationBuilder app,
      BbContext context,
      IWebHostEnvironment env)
    {
      context.Database.Migrate();

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
