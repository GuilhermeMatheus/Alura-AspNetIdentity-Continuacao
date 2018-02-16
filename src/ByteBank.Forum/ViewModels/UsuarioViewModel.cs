using ByteBank.Forum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ByteBank.Forum.ViewModels
{
    public class UsuarioViewModel
    {
        public string NomeCompleto { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
    }
}