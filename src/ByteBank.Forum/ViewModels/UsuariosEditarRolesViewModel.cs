using ByteBank.Forum.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ByteBank.Forum.ViewModels
{
    public class UsuariosEditarRolesViewModel
    {
        public string UserName { get; set; }
        public string NomeCompleto { get; set; }
        public string Email { get; set; }

        public List<RoleViewModel> Roles { get; set; }

        public UsuariosEditarRolesViewModel() { }
        public UsuariosEditarRolesViewModel(UsuarioAplicacao usuario, RoleManager<IdentityRole> roleManager)
        {
            UserName = usuario.UserName;
            NomeCompleto = usuario.NomeCompleto;
            Email = usuario.Email;

            var todasAsRoles = roleManager.Roles.ToList();

            Roles = 
                todasAsRoles
                    .Select(role => new RoleViewModel { Nome = role.Name, Id = role.Id })
                    .ToList();

            foreach (var item in usuario.Roles)
            {
                var roleViewModel = Roles.Find(role => role.Id == item.RoleId);
                roleViewModel.Selecionado = true;
            }
        }

    }

    public class RoleViewModel
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public bool Selecionado { get; set; }
    }

}