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
    public class PrenotazioniTests : IClassFixture<CustomWebApplicationFactory<Program>>, IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _applicationFactory;
        private readonly CustomWebApplicationFactory<Program> _applicationFactoryAuthenticated;

        private readonly HttpClient _client;
        private readonly HttpClient _authClient;

        public LibreriaContext context;

        public PrenotazioniTests(WebApplicationFactory<Program> applicationFactory, CustomWebApplicationFactory<Program> applicationFactoryAuthenticated)
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

        [Fact]
        public async Task IntegritaPrenotazioni()
        {
            bool integrita = await context.Prenotazioni.AsNoTracking().AnyAsync();
        }

        [Theory]
        [InlineData("/Prenotazioni/Prenota/1")]
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
        [InlineData("/Prenotazioni/Prenota/1")]
        public async Task Authenticated_ProtectedViews_ReturnsSuccessfully(string url)
        {
            _authClient.DefaultRequestHeaders.Add(TestAuthHandler.UserId, "eb9512d8-dea5-467b-9256-f17a03532392");
            var response = await _authClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(response.Content.Headers.ContentType);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(response.RequestMessage);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(response.RequestMessage.RequestUri);

            Xunit.Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());

            Xunit.Assert.StartsWith("https://localhost/Prenotazioni/Prenota/1", response.RequestMessage.RequestUri.OriginalString);
        }

    }
}
