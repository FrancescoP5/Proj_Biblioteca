using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca_Test.IntegrationTests.Helpers;
using System.Net;
using Xunit;

namespace Proj_Biblioteca_Test.IntegrationTests
{
    public class LibriTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _applicationFactory;
        private readonly HttpClient _client;

        public LibreriaContext context;
        public UserManager<Utente> userManager;
        public SignInManager<Utente> signInManager;
        public RoleManager<Role> roleManager;

        public LibriTests(CustomWebApplicationFactory<Program> applicationFactory)
        {
            _applicationFactory = applicationFactory;

            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            string connectionString = config.GetConnectionString("LibreriaContext") ?? "";

            var options = new DbContextOptionsBuilder<LibreriaContext>().UseSqlServer(connectionString)
                                                                        .Options;
            context = new LibreriaContext(options);

            _client = _applicationFactory.CreateClient();
        }

        [Fact]
        public async Task IntegritaLibri()
        {
            bool integrita = await context.Libri.AsNoTracking().AnyAsync();
        }

        [Theory]
        [InlineData("/Libro/Elenco")]
        //[InlineData("/Libro/Modifica")] ADMIN
        //[InlineData("/Libro/AggiungiLibro")] ADMIN
        public async Task PublicViews_ReturnTesting_ContentTypeTesting(string url)
        {

            var response = await _client.GetAsync(url);


            response.EnsureSuccessStatusCode();

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(response.Content.Headers.ContentType);

            Xunit.Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("/Libro/Modifica")]
        [InlineData("/Libro/AggiungiLibro")] 
        public async Task Unauthenticated_ProtectedViews_ReturnTesting(string url)
        {
            var response = await _client.GetAsync(url);

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(response.StatusCode,HttpStatusCode.NotFound);
        }


    }
}
