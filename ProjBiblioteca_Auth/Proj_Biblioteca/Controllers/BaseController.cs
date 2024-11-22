using Microsoft.AspNetCore.Mvc;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;

namespace Proj_Biblioteca.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ILogger<BaseController> _logger;
        protected readonly IHttpContextAccessor _contextAccessor;

        public BaseController(IHttpContextAccessor contextAccessor, ILogger<BaseController> logger)
        {
            _contextAccessor = contextAccessor;
            _logger = logger;
        }
       
        public async Task<Utente?> GetUser(string path="NoPath")
        {
            var httpContext = _contextAccessor.HttpContext;

            if (httpContext == null)
                return null;

            var session = httpContext.Session;

            if (session == null) 
                return null;

            int? id =  session.GetInt32("UserId");



            if (id == null)
            {
                _logger.LogInformation("Nessun UserId trovato nella sessione");
                return null;
            }

            var user = (Utente)await DAOUtente.GetInstance().Find((int)id);

            return user; 
        }

        public void SetUser(int? userId, string path = "NoPath")
        {
            var httpContext = _contextAccessor.HttpContext;

            if (httpContext == null) 
                return;

            var session = httpContext.Session;

            if(session == null) 
                return;

            if (userId == null)
            {
                session.Clear();
            }
            else
            {
                session.SetInt32("UserId", userId.Value);

            }
        }
    }
}
