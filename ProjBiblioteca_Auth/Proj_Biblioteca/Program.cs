using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<LibreriaContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("LibreriaContext")));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<Utente,Role>().AddEntityFrameworkStores<LibreriaContext>();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
});
builder.Services.Configure<SecurityStampValidatorOptions>(options => { 
    options.ValidationInterval = TimeSpan.FromSeconds(1);
});

builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<LibreriaContext>();
    context.Database.EnsureCreated();


    var UserManager = services.GetRequiredService<UserManager<Utente>>();

    DbInitializer.Initialize(context,UserManager);
}

app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Libro}/{action=Elenco}/{id?}");

app.Run();

