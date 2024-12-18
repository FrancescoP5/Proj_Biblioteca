using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Proj_Biblioteca.Data;
using Proj_Biblioteca_Test.IntegrationTests.Helpers;
using System.Net;
using Xunit;

namespace Proj_Biblioteca_Test.IntegrationTests
{
    public class UtentiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _applicationFactory;
        private readonly HttpClient _client;

        private LibreriaContext context;

        public UtentiTests(CustomWebApplicationFactory<Program> applicationFactory)
        {
            _applicationFactory = applicationFactory;

            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            string connectionString = config.GetConnectionString("LibreriaContext") ?? "";

            var options = new DbContextOptionsBuilder<LibreriaContext>().UseSqlServer(connectionString)
                                                                        .Options;
            context = new LibreriaContext(options);

            _client = _applicationFactory.CreateClient();
        }

        public static HttpClient GetAuthClient()
        {
            return new CustomWebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(defaultScheme: "TestScheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "TestScheme", options => { });
                });
            }).CreateClient();
        }

        [Fact]
        public async Task IntegritaUtenti()
        {
            bool integrita = await context.Users.AsNoTracking().AnyAsync();
        }        

        [Fact]
        public async Task IntegritaRuoli()
        {
            bool integrita = await context.Roles.AsNoTracking().AnyAsync();
        }

        [Theory]
        [InlineData("/Utenti/AccountPage")]
        public async Task PublicViews_ReturnTesting_ContentTypeTesting(string url)
        {
            var response = await _client.GetAsync(url);

            response.EnsureSuccessStatusCode();
            
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(response.Content.Headers.ContentType);

            Xunit.Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("/Utenti/GestioneRuoli")]
        public async Task Unauthenticated_ProtectedViews_ReturnTesting(string url)
        {
            var response = await _client.GetAsync(url);

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(response.StatusCode, HttpStatusCode.NotFound);
        }        
        //[Theory]
        //[InlineData("/Utenti/GestioneRuoli")]
        //public async Task Authenticated_ProtectedViews_ReturnTesting(string url)
        //{
        //    var client = GetAuthClient();
        //    var response = await GetAuthClient().GetAsync(url);

        //    response.EnsureSuccessStatusCode();
        //}
    }
}
