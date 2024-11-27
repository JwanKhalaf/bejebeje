using System;
using System.IdentityModel.Tokens.Jwt;
using Bejebeje.DataAccess.Context;
using Bejebeje.Services.Config;
using Bejebeje.Services.Services;
using Bejebeje.Services.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// add services to the container.
IdentityModelEventSource.ShowPII = true;

string authority = builder.Configuration["Cognito:Authority"];

string clientId = builder.Configuration["Cognito:ClientId"];

string clientSecret = builder.Configuration["Cognito:ClientSecret"];

string connectionString = builder.Configuration["ConnectionString"];

builder.Services.Configure<DatabaseOptions>(builder.Configuration);

builder.WebHost.UseSentry();

builder.Services.AddDbContext<BbContext>(options => options
      .UseNpgsql(connectionString)
      .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<IImagesService, ImagesService>();

builder.Services.AddScoped<IArtistSlugsService, ArtistSlugsService>();

builder.Services.AddScoped<IArtistsService, ArtistsService>();

builder.Services.AddScoped<ILyricsService, LyricsService>();

builder.Services.AddScoped<ISitemapService, SitemapService>();

builder.Services.AddAuthentication(options =>
    {
      options.DefaultScheme = "Cookies";
      options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie("Cookies")
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
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
      options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "cognito:user", RoleClaimType = "cognito:groups" };
    });

builder.Services.AddControllersWithViews();

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

WebApplication app = builder.Build();

ForwardedHeadersOptions forwardedHeadersOptions = new ForwardedHeadersOptions
{
  ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};

forwardedHeadersOptions.KnownNetworks.Clear();

forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseSentryTracing();

// configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  Console.WriteLine("We are in a Production environment!!!");

  app.UseExceptionHandler("/Error");

  // the default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}
else
{
  Console.WriteLine("We are in Development environment!!!");
}

app.UseHttpsRedirection();

app.MapStaticAssets();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
  endpoints
    .MapControllerRoute(
      "default",
  pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
