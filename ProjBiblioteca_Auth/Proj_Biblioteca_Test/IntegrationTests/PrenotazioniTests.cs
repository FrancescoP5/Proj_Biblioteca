using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca_Test.IntegrationTests.Helpers;
using Xunit;

namespace Proj_Biblioteca_Test.IntegrationTests
{
    public class PrenotazioniTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _applicationFactory;
        private readonly HttpClient _client;

        private LibreriaContext context;

        public PrenotazioniTests(CustomWebApplicationFactory<Program> applicationFactory)
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
        public async Task IntegritaPrenotazioni()
        {
            bool integrita = await context.Prenotazioni.AsNoTracking().AnyAsync();
        }

        [Theory]
        [InlineData("/Prenotazioni/Prenota/1")]
        public async Task RedirectToLogin_Unauthorized(string url)
        {
            var client = _applicationFactory.CreateClient();

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(response.Content.Headers.ContentType);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(response.RequestMessage);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(response.RequestMessage.RequestUri);

            Xunit.Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());

            Xunit.Assert.StartsWith("http://localhost/Utenti/AccountPage",response.RequestMessage.RequestUri.OriginalString);
        }

        //[Theory]
        //[InlineData("/Prenotazioni/Prenota/1")]
        //public async Task Authorized_ProtectedViews_ReturnTesting_TypeTesting(string url)
        //{
        //    var client = _applicationFactory.CreateClient();

        //    var response = await client.GetAsync(url);

        //    response.EnsureSuccessStatusCode();

        //    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(response.Content.Headers.ContentType);
        //    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(response.RequestMessage);
        //    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(response.RequestMessage.RequestUri);

        //    Xunit.Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());

        //    Xunit.Assert.StartsWith("http://localhost/Prenotazioni/Prenota/1", response.RequestMessage.RequestUri.OriginalString);
        //}

    }
}
