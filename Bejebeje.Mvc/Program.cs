using System;
using System.Threading.RateLimiting;
using Amazon.CognitoIdentityProvider;
using Amazon.S3;
using Amazon.SimpleEmailV2;
using Bejebeje.DataAccess.Context;
using Bejebeje.Services.Config;
using Bejebeje.Services.Services;
using Bejebeje.Mvc.Auth;
using Bejebeje.Services.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// add services to the container.
string connectionString = builder.Configuration["ConnectionString"];

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

builder.Services.AddAWSService<IAmazonCognitoIdentityProvider>();

builder.Services.AddAWSService<IAmazonSimpleEmailServiceV2>();

builder.Services.AddAWSService<IAmazonS3>();

builder.Services.Configure<DatabaseOptions>(builder.Configuration);

builder.Services.Configure<BbPointsOptions>(builder.Configuration.GetSection("BbPoints"));

builder.Services.Configure<CognitoOptions>(builder.Configuration.GetSection("Cognito"));

builder.Logging.AddSeq();

builder.Services.AddDbContext<BbContext>(options => options
      .UseNpgsql(connectionString)
      .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<IImagesService, ImagesService>();

builder.Services.AddScoped<IArtistSlugsService, ArtistSlugsService>();

builder.Services.AddScoped<IArtistsService, ArtistsService>();

builder.Services.AddScoped<ILyricsService, LyricsService>();

builder.Services.AddScoped<ISitemapService, SitemapService>();

builder.Services.AddScoped<ICognitoService, CognitoService>();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IAuthorService, AuthorService>();

builder.Services.AddScoped<ILyricReportsService, LyricReportsService>();

builder.Services.AddScoped<IBbPointsService, BbPointsService>();

builder.Services.AddScoped<IAuthService, AuthService>();

// cookie-only authentication (replaces oidc middleware)
builder.Services.AddAuthentication(options =>
    {
      options.DefaultScheme = "Cookies";
      options.DefaultChallengeScheme = "Cookies";
    })
    .AddCookie("Cookies", options =>
    {
      options.LoginPath = "/login";
      options.Cookie.HttpOnly = true;
      options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
      options.Cookie.SameSite = SameSiteMode.Lax;
      options.Events.OnSigningIn = async context =>
      {
        var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;

        if (identity != null)
        {
          var handler = new OnSigningInHandler(
            context.HttpContext.RequestServices.GetRequiredService<IBbPointsService>(),
            context.HttpContext.RequestServices.GetRequiredService<ICognitoService>(),
            context.HttpContext.RequestServices.GetRequiredService<ILogger<OnSigningInHandler>>());

          await handler.HandleAsync(identity);
        }
      };
    });

// rate limiting for auth endpoints
builder.Services.AddRateLimiter(options =>
{
  options.RejectionStatusCode = 429;

  options.AddFixedWindowLimiter("auth-login", limiterOptions =>
  {
    limiterOptions.PermitLimit = 2;
    limiterOptions.Window = TimeSpan.FromMinutes(1);
  });

  options.AddFixedWindowLimiter("auth-signup", limiterOptions =>
  {
    limiterOptions.PermitLimit = 2;
    limiterOptions.Window = TimeSpan.FromMinutes(1);
  });

  options.OnRejected = async (context, cancellationToken) =>
  {
    context.HttpContext.Response.StatusCode = 429;
    await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);
  };
});

builder.Services.AddControllersWithViews();

WebApplication app = builder.Build();

ForwardedHeadersOptions forwardedHeadersOptions = new ForwardedHeadersOptions
{
  ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};

forwardedHeadersOptions.KnownNetworks.Clear();

forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

// configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  Console.WriteLine("We are in a Production environment!!!");

  // handle unhandled exceptions with a friendly error page
  app.UseExceptionHandler("/server-error");

  // handle http status codes (404, etc.) with friendly pages
  app.UseStatusCodePagesWithReExecute("/status/{0}");

  // the default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}
else
{
  Console.WriteLine("We are in Development environment!!!");

  // handle unhandled exceptions with a friendly error page
  app.UseExceptionHandler("/server-error");

  // handle http status codes (404, etc.) with friendly pages
  app.UseStatusCodePagesWithReExecute("/status/{0}");
}

app.UseHttpsRedirection();

app.MapStaticAssets();

app.UseRouting();

app.UseRateLimiter();

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
