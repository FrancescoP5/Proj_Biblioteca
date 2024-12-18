using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using System;
using System.Data.Common;

namespace Proj_Biblioteca_Test.IntegrationTests.Helpers
{
    public class CustomWebApplicationFactory<TProgram>
        : WebApplicationFactory<TProgram> where TProgram : class
    {
        //protected override void ConfigureWebHost(IWebHostBuilder builder)
        //{
        //    var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        //    string connectionString = config.GetConnectionString("LibreriaContext") ?? "";

        //    builder.ConfigureServices(services =>
        //    {
        //        services.AddDbContext<LibreriaContext>(options =>
        //        options.UseSqlServer(connectionString));

        //        services.AddIdentity<Utente, Role>().AddEntityFrameworkStores<LibreriaContext>();

        //        services.Configure<IdentityOptions>(options =>
        //        {
        //            options.SignIn.RequireConfirmedAccount = true;
        //            options.User.RequireUniqueEmail = true;
        //        });
        //        services.Configure<SecurityStampValidatorOptions>(options =>
        //        {
        //            options.ValidationInterval = TimeSpan.FromSeconds(1);
        //        });
        //        services.AddAntiforgery(options =>
        //        {
        //            options.Cookie.HttpOnly = true;
        //            options.Cookie.SameSite = SameSiteMode.Lax;
        //            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        //        });


        //        var app = builder.Build();

        //        using (var scope = app.Services.CreateScope())
        //        {
        //            var appServices = scope.ServiceProvider;

        //            context = appServices.GetRequiredService<LibreriaContext>();

        //            userManager = appServices.GetRequiredService<UserManager<Utente>>();
        //            signInManager = appServices.GetRequiredService<SignInManager<Utente>>();
        //            roleManager = appServices.GetRequiredService<RoleManager<Role>>();
        //        }

        //    });
        //    builder.UseEnvironment("Development");
        //}
    }
}
