using Microsoft.AspNetCore.Authentication.Cookies;
using PokeWiki.Web.ApiClients;

var builder = WebApplication.CreateBuilder(args);

var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl");

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<AuthApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl!));
builder.Services.AddHttpClient<PokemonApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl!));
builder.Services.AddHttpClient<MovesApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl!));
builder.Services.AddHttpClient<ObjectsApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl!));
builder.Services.AddHttpClient<ForumApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl!));

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddSession();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();