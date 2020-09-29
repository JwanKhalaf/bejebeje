namespace Bejebeje.Mvc
{
  using System.IdentityModel.Tokens.Jwt;
  using Bejebeje.Services.Services;
  using Bejebeje.Services.Services.Interfaces;
  using DataAccess.Context;
  using Microsoft.AspNetCore.Authentication;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.HttpOverrides;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Microsoft.IdentityModel.Logging;
  using Microsoft.IdentityModel.Tokens;
  using Services.Config;

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
      IdentityModelEventSource.ShowPII = true;
      string authority = Configuration["IdentityServerConfiguration:Authority"];
      string clientId = Configuration["IdentityServerConfiguration:ClientId"];
      string clientSecret = Configuration["IdentityServerConfiguration:ClientSecret"];
      string connectionString = Configuration["ConnectionString"];

      services
        .Configure<DatabaseOptions>(Configuration);

      services
        .AddDbContext<BbContext>(options => options
          .UseNpgsql(connectionString)
          .UseSnakeCaseNamingConvention());

      services
        .AddScoped<IImagesService, ImagesService>();

      services
        .AddScoped<IArtistSlugsService, ArtistSlugsService>();

      services
        .AddScoped<IArtistsService, ArtistsService>();

      services
        .AddScoped<ILyricsService, LyricsService>();

      services
        .AddScoped<ISitemapService, SitemapService>();

      JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

      JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

      services.AddAuthentication(options =>
        {
          options.DefaultScheme = "Cookies";
          options.DefaultChallengeScheme = "oidc";
        })
        .AddCookie("Cookies")
        .AddOpenIdConnect("oidc", options =>
        {
          options.Authority = authority;
          options.RequireHttpsMetadata = false;
          options.ClientId = clientId;
          options.ClientSecret = clientSecret;
          options.ResponseType = "code";
          options.SaveTokens = true;
          options.GetClaimsFromUserInfoEndpoint = true;
          options.Scope.Clear();
          options.Scope.Add("openid");
          options.ClaimActions.MapUniqueJsonKey("role", "role");
          options.TokenValidationParameters = new TokenValidationParameters { RoleClaimType = "role" };
        });

      services
        .AddControllersWithViews();
    }

    // this method gets called by the runtime.
    // yse this method to configure the http request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      ForwardedHeadersOptions forwardedHeadersOptions = new ForwardedHeadersOptions
      {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
      };

      forwardedHeadersOptions.KnownNetworks.Clear();
      forwardedHeadersOptions.KnownProxies.Clear();

      app.UseForwardedHeaders(forwardedHeadersOptions);

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
        .UseAuthentication();

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
