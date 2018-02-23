using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ByteBank.Forum.ViewModels
{
    public class ContaAutenticacaoDeDoisFatoresViewModel
    {
        [Required]
        public string Codigo { get; set; }

        [Display(Name ="Continuar conectado")]
        public bool ContinuarLogado { get; set; }

        [Display(Name ="Não pedir código neste navegador")]
        public bool LembrarNavegador { get; set; }
    }
}