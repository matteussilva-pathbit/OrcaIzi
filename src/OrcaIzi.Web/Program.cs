using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using OrcaIzi.Web.Interfaces;
using OrcaIzi.Web.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
    });

// Configure HttpClient for ApiService
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    // Adjust the base address to match your API port (check launchSettings.json of WebAPI)
    client.BaseAddress = new Uri("http://localhost:5012/"); 
});

var app = builder.Build();

// Configure Localization
var defaultCulture = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = new List<CultureInfo> { new CultureInfo("pt-BR") },
    SupportedUICultures = new List<CultureInfo> { new CultureInfo("pt-BR") }
};
app.UseRequestLocalization(defaultCulture);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
