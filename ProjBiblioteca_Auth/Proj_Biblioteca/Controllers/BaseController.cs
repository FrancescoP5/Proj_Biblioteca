using Microsoft.AspNetCore.Mvc;
using Proj_Biblioteca.Service;

namespace Proj_Biblioteca.Controllers
{
    public class BaseController(
            ILogger<BaseController> logger,
            ILibreriaManager libreriaManager
            ) : Controller
    {
        protected readonly ILogger<BaseController> _logger = logger;
        protected readonly ILibreriaManager _libreriaManager = libreriaManager;
    }
}
