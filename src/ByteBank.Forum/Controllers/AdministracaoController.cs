using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ByteBank.Forum.Controllers
{
    [Authorize(Roles ="Administrador")]
    public class AdministracaoController : Controller
    {
        public ActionResult Index()
        {
            System.Security.Claims.ClaimTypes.Name
            return View();
        }
    }
}