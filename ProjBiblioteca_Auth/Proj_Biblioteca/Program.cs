using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.DAL;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.Service;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<IRepoUtenti, RepoUtenti>();
builder.Services.AddScoped<IRepoLibri, RepoLibri>();
builder.Services.AddScoped<IRepoPrenotazioni, RepoPrenotazioni>();

builder.Services.AddScoped<ILibreriaManager, LibreriaManager>();

builder.Services.AddRazorPages();

builder.Services.Configure<AntiforgeryOptions>(opts => 
{ 
    opts.Cookie.HttpOnly = true; 
    opts.Cookie.SameSite = SameSiteMode.None;
    opts.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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



    var UserManager = services.GetRequiredService<UserManager<Utente>>();
    var SignInManager = services.GetRequiredService<SignInManager<Utente>>();
    var RoleManager = services.GetRequiredService<RoleManager<Role>>();

    DbInitializer.Initialize(context,UserManager);
}


app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();

app.UseAuthorization();

app.UseSession();

Database database = Database.GetInstance();

app.MapControllerRoute(name: "default", pattern: "{controller=Libro}/{action=Elenco}/{id?}");

app.Run();

