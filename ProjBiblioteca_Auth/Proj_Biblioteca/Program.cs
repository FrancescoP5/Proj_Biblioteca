using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Proj_Biblioteca.Data;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "LoginSession";
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.IsEssential = true;
    options.Cookie.MaxAge = TimeSpan.FromMinutes(10);
});


builder.Services.AddHttpContextAccessor();

var app = builder.Build();
 
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

app.UseAuthorization();

app.UseSession();

Database database = Database.GetInstance();

app.MapControllerRoute(name: "default", pattern: "{controller=Libro}/{action=Elenco}/{id?}");

app.Run();

