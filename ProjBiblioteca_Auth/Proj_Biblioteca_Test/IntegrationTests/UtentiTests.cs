using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Proj_Biblioteca.Data;
using Proj_Biblioteca_Test.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Proj_Biblioteca_Test.IntegrationTests
{
    public class UtentiTests : IClassFixture<CustomWebApplicationFactory<Program>>, IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _applicationFactory;
        private readonly CustomWebApplicationFactory<Program> _applicationFactoryAuthenticated;

        private readonly HttpClient _client;
        private readonly HttpClient _authClient;

        public LibreriaContext context;

        public UtentiTests(WebApplicationFactory<Program> applicationFactory, CustomWebApplicationFactory<Program> applicationFactoryAuthenticated)
        {
            _applicationFactory = applicationFactory;
            _applicationFactoryAuthenticated = applicationFactoryAuthenticated;

            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            string connectionString = config.GetConnectionString("LibreriaContext") ?? "";

            var options = new DbContextOptionsBuilder<LibreriaContext>().UseSqlServer(connectionString)
                                                                        .Options;
            context = new LibreriaContext(options);

            _client = _applicationFactory.CreateClient();

            _authClient = _applicationFactoryAuthenticated.CreateClient();
            _authClient.BaseAddress = new Uri("https://localhost/");
            _authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        }

        public static HttpClient GetAuthClient(CustomWebApplicationFactory<Program> factory)
        {
            var client = factory.CreateClient();

            client.BaseAddress = new Uri("https://localhost/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
            return client;
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
        public async Task Unauthenticated_ProtectedViews_GetsRedirected(string url)
        {
            var client = _applicationFactory.CreateClient(new WebApplicationFactoryClientOptions()
            {
                AllowAutoRedirect = false
            });
            var response = await client.GetAsync(url);

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(response.Headers.Location);
            Xunit.Assert.StartsWith("http://localhost/Utenti/AccountPage", response.Headers.Location.OriginalString);
        }

        [Theory]
        [InlineData("/Utenti/GestioneRuoli")]
        public async Task Authenticated_ProtectedViews_ReturnsSuccessfully(string url)
        {
            _authClient.DefaultRequestHeaders.Add(TestAuthHandler.UserId, "eb9512d8-dea5-467b-9256-f17a03532392");
            var response = await _authClient.GetAsync(url);

            response.EnsureSuccessStatusCode();
        }
    }
}
