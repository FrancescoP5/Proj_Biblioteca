using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.ViewModels;

namespace Proj_Biblioteca.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ILogger<BaseController> _logger;
        protected readonly IHttpContextAccessor _contextAccessor;

        protected readonly LibreriaContext _libreria;

        protected readonly UserManager<Utente> _userManager;
        protected readonly SignInManager<Utente> _signInManager;
        protected readonly RoleManager<Role> _roleManager;

        public BaseController(IHttpContextAccessor contextAccessor, ILogger<BaseController> logger,
            LibreriaContext Dbcontext,
            UserManager<Utente> userManager, SignInManager<Utente> signInManager, RoleManager<Role> roleManager)
        {
            _contextAccessor = contextAccessor;
            _logger = logger;

            _libreria = Dbcontext;

            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<UtenteViewModel?> GetUser()
        {
            var id = User.Claims.FirstOrDefault()?.Value;


            if (id == null)
            {

                _logger.LogInformation("Nessun UserId trovato nella sessione");
                return null;
            }

            var user = await UtenteViewModel.GetViewModel(_libreria, id);

            return user;
        }
    }
}
