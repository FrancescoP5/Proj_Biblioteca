using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proj_Biblioteca.DAL;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.ViewModels;

namespace Proj_Biblioteca.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ILogger<BaseController> _logger;

        protected readonly LibreriaContext _libreria;
        protected readonly UserManager<Utente> _userManager;
        protected readonly SignInManager<Utente> _signInManager;
        protected readonly RoleManager<Role> _roleManager;

        protected readonly IRepoPrenotazioni repoPrenotazioni;
        protected readonly IRepoLibri repoLibri;
        protected readonly IRepoUtenti repoUtenti;

        public BaseController
            (
                ILogger<BaseController> logger,
                LibreriaContext Dbcontext, 
                UserManager<Utente> userManager, 
                SignInManager<Utente> signInManager, 
                RoleManager<Role> roleManager
            )
        {
            _logger = logger;
            _libreria = Dbcontext;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;

            repoPrenotazioni = new RepoPrenotazioni(_libreria);
            repoLibri = new RepoLibri(_libreria);
            repoUtenti = new RepoUtenti(_libreria);
        }

        public async Task<UtenteViewModel?> GetUser()
        {
            var id = User.Claims.FirstOrDefault()?.Value;


            if (id == null)
            {
                return null;
            }

            var user = await UtenteViewModel.GetViewModel(_libreria, id);

            return user;
        }
    }
}
