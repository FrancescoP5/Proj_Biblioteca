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

        private static ISession _session; 
       
        public async Task<Utente?> GetUser()
        {
            if (_session == null)
                return null;

            int? id =  _session.GetInt32("UserId");

            return id != null ? (Utente)await DAOUtente.GetInstance().Find((int)id) : null; 
        }

        public void SetUser(int? userId)
        {
            if (userId == null)
            {
                _session.Clear();
                _session = null;
            }
            else
            {
                _session = HttpContext.Session;
                _session.SetInt32("UserId", userId.Value);
            }
        }
    }
}
