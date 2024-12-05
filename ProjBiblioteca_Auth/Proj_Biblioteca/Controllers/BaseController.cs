using Microsoft.AspNetCore.Mvc;
using Proj_Biblioteca.Service;
using Proj_Biblioteca.ViewModels;

namespace Proj_Biblioteca.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ILogger<BaseController> _logger;
        protected readonly ILibreriaManager _libreriaManager;

        public BaseController
            (
                ILogger<BaseController> logger,
                ILibreriaManager libreriaManager
            )
        {
            _logger = logger;
            _libreriaManager = libreriaManager;
        }

    }
}
