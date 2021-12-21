using Microsoft.AspNetCore.Builder;
using Bejebeje.Services.Services;
using Bejebeje.Services.Services.Interfaces;
using Bejebeje.DataAccess.Context;
using System.IdentityModel.Tokens.Jwt;
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
using Bejebeje.Services.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
IdentityModelEventSource.ShowPII = true;
string authority = builder.Configuration["IdentityServerConfiguration:Authority"];
string clientId = builder.Configuration["IdentityServerConfiguration:ClientId"];
string clientSecret = builder.Configuration["IdentityServerConfiguration:ClientSecret"];
string connectionString = builder.Configuration["ConnectionString"];

builder.Services.Configure<DatabaseOptions>(builder.Configuration);

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

builder.Services.AddControllersWithViews();

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var app = builder.Build();

ForwardedHeadersOptions forwardedHeadersOptions = new ForwardedHeadersOptions
{
  ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};

forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

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
