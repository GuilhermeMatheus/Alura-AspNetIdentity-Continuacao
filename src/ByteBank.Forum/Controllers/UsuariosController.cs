using ByteBank.Forum.Models;
using ByteBank.Forum.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ByteBank.Forum.Controllers
{
    [Authorize(Roles =RolesAplicacao.ADMINISTRADOR)]
    public class UsuariosController : Controller
    {
        private UserManager<UsuarioAplicacao> _userManager;
        public UserManager<UsuarioAplicacao> UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    var contextOwin = HttpContext.GetOwinContext();
                    _userManager = contextOwin.GetUserManager<UserManager<UsuarioAplicacao>>();
                }
                return _userManager;
            }
            set
            {
                _userManager = value;
            }
        }

        private RoleManager<IdentityRole> _roleManager;
        public RoleManager<IdentityRole> RoleManager
        {
            get
            {
                if (_roleManager == null)
                {
                    var contextOwin = HttpContext.GetOwinContext();
                    _roleManager = contextOwin.GetUserManager<RoleManager<IdentityRole>>();
                }
                return _roleManager;
            }
            set
            {
                _roleManager = value;
            }
        }

        public ActionResult Index()
        {
            var usuarios = UserManager.Users.ToList();
            var modelo =
                from usuario in usuarios
                select new UsuarioViewModel
                {
                    Email = usuario.Email,
                    NomeCompleto = usuario.NomeCompleto,
                    Username = usuario.UserName,
                    Id = usuario.Id
                };
            
            return View(modelo);
        }
        
        public async Task<ActionResult> EditarRoles(string id)
        {
            var usuario = await UserManager.FindByIdAsync(id);

            var viewModel = new UsuariosEditarRolesViewModel(usuario, RoleManager);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> EditarRoles(UsuariosEditarRolesViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var usuario = UserManager.FindByName(modelo.UserName);

                var todasAsRoles = await UserManager.GetRolesAsync(usuario.Id);
                await UserManager.RemoveFromRolesAsync(usuario.Id, todasAsRoles.ToArray());

                var novasRoles = modelo.Roles.Where(funcao => funcao.Selecionado).Select(funcao => funcao.Nome);
                await UserManager.AddToRolesAsync(usuario.Id, novasRoles.ToArray());

                return RedirectToAction("Index");
            }
            return View();
        }

    }
}