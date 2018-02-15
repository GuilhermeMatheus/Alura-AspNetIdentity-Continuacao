using ByteBank.Forum.Models;
using ByteBank.Forum.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

namespace ByteBank.Forum.Controllers
{
    public class ContaController : Controller
    {
        private UserManager<UsuarioAplicacao> _userManager;
        public UserManager<UsuarioAplicacao> UserManager
        {
            get
            {
                if(_userManager == null)
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

        private SignInManager<UsuarioAplicacao, string> _signInManager;
        public SignInManager<UsuarioAplicacao, string> SignInManager
        {
            get
            {
                if (_signInManager == null)
                {
                    var contextOwin = HttpContext.GetOwinContext();
                    _signInManager = contextOwin.GetUserManager<SignInManager<UsuarioAplicacao, string>>();
                }
                return _signInManager;
            }
            set
            {
                _signInManager = value;
            }
        }

        public IAuthenticationManager AuthenticationManager
        {
            get
            {
                var contextoOwin = Request.GetOwinContext();
                return contextoOwin.Authentication;
            }
        }

        public ActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Registrar(ContaRegistrarViewModel modelo)
        {
            if(ModelState.IsValid)
            {
                var novoUsuario = new UsuarioAplicacao();

                novoUsuario.Email = modelo.Email;
                novoUsuario.UserName = modelo.UserName;
                novoUsuario.NomeCompleto = modelo.NomeCompleto;

                var usuario = await UserManager.FindByEmailAsync(modelo.Email);
                var usuarioJaExiste = usuario != null;

                if (usuarioJaExiste)
                    return View("AguardandoConfirmacao");

                var resultado = await UserManager.CreateAsync(novoUsuario, modelo.Senha);

                if (resultado.Succeeded)
                {
                    // Enviar o email de confirmação
                    await EnviarEmailDeConfirmacaoAsync(novoUsuario);
                    return View("AguardandoConfirmacao");
                }
                else
                {
                    AdicionaErros(resultado);
                }
            }

            // Alguma coisa de errado aconteceu!
            return View(modelo);
        }

        private async Task EnviarEmailDeConfirmacaoAsync(UsuarioAplicacao usuario)
        {
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(usuario.Id);

            var linkDeCallback =
                Url.Action(
                    "ConfirmacaoEmail",
                    "Conta",
                    new { usuarioId = usuario.Id, token = token },
                    Request.Url.Scheme);

            await UserManager.SendEmailAsync(
                usuario.Id,
                "Fórum ByteBank - Confirmação de Email",
                $"Bem vindo ao fórum ByteBank, clique aqui {linkDeCallback} para confirmar seu email!");
        }

        public async Task<ActionResult> ConfirmacaoEmail(string usuarioId, string token)
        {
            if (usuarioId == null || token == null)
                return View("Error");

            var resultado = await UserManager.ConfirmEmailAsync(usuarioId, token);

            if (resultado.Succeeded)
                return RedirectToAction("Index", "Home");
            else
                return View("Error");
        }
        
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(ContaLoginViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByEmailAsync(modelo.Email);

                if (usuario == null)
                    return SenhaOuUsuarioInvalidos();

                var signInResultado =
                    await SignInManager.PasswordSignInAsync(
                        usuario.UserName,
                        modelo.Senha,
                        isPersistent: modelo.ContinuarLogado,
                        shouldLockout: true);

                switch (signInResultado)
                {
                    case SignInStatus.Success:
                        if (!usuario.EmailConfirmed)
                        {
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            return View("AguardandoConfirmacao");
                        }
                        return RedirectToAction("Index", "Home");
                    case SignInStatus.LockedOut:
                        var senhaCorreta = 
                            await UserManager.CheckPasswordAsync(
                                usuario,
                                modelo.Senha);

                        if (senhaCorreta)
                            ModelState.AddModelError("", "A conta está bloqueada!");
                        else
                            return SenhaOuUsuarioInvalidos();
                        break;
                    default:
                        return SenhaOuUsuarioInvalidos();
                }
            }

            // Algo de errado aconteceu
            return View(modelo);
        }

        public ActionResult RedefinirSenha()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> RedefinirSenha(ContaEsqueciMinhaSenhaViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByEmailAsync(modelo.Email);
                if (usuario != null)
                {
                    var token = await UserManager.GeneratePasswordResetTokenAsync(usuario.Id);
                    var linkDeCallback =
                        Url.Action(
                            "ConfirmacaoRedefinirSenha",
                            "Conta",
                            new { usuarioId = usuario.Id, token = token },
                            Request.Url.Scheme);

                    await UserManager.SendEmailAsync(
                        usuario.Id,
                        "Fórum ByteBank - Redefinir senha",
                        $"Clique aqui {linkDeCallback} para mudar sua senha no fórum ByteBank!");
                }

                return View("EmailRedefinirSenhaEnviado");
            }

            return View(modelo);
        }

        public ActionResult ConfirmacaoRedefinirSenha(string usuarioId, string token)
        {
            return View(new ContaConfirmacaoRedefinirSenhaViewModel { UsuarioId = usuarioId, Token = token });
        }

        [HttpPost]
        public async Task<ActionResult> ConfirmacaoRedefinirSenha(ContaConfirmacaoRedefinirSenhaViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var resultado = await UserManager.ResetPasswordAsync(modelo.UsuarioId, modelo.Token, modelo.NovaSenha);
                if (resultado.Succeeded)
                    return RedirectToAction("Index", "Home");
                else
                    AdicionaErros(resultado);
            }
            
            return View(modelo);
        }

        [HttpPost]
        public ActionResult AutenticacaoExterna(string provider)
        {
            SignInManager.AuthenticationManager.Challenge(new AuthenticationProperties
            {
                RedirectUri = Url.Action("AutenticacaoExternaCallback", new { provider })
            }, provider);

            return new HttpUnauthorizedResult();
        }

        [HttpPost]
        public ActionResult RegistrarComProviderExterno(string provider)
        {
            SignInManager.AuthenticationManager.Challenge(new AuthenticationProperties
            {
                RedirectUri = Url.Action("RegistrarComProviderExternoCallback", new { provider })
            }, provider);

            return new HttpUnauthorizedResult();
        }

        [HttpPost]
        public ActionResult LoginPorAutenticacaoExterna(string provider)
        {
            SignInManager.AuthenticationManager.Challenge(new AuthenticationProperties
            {
                RedirectUri = Url.Action("LoginPorAutenticacaoExternaCallback", new { provider })
            }, provider);

            return new HttpUnauthorizedResult();
        }

        public async Task<ActionResult> LoginPorAutenticacaoExternaCallback(string provider)
        {
            var loginInfo = await SignInManager.AuthenticationManager.GetExternalLoginInfoAsync();
            var resultadoSignIn = await SignInManager.ExternalSignInAsync(loginInfo, true);

            switch (resultadoSignIn)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Index", "Home");
                default:
                    return View("Error");
            }
        }

        public async Task<ActionResult> RegistrarComProviderExternoCallback(string provider)
        {
            var loginInfo = await SignInManager.AuthenticationManager.GetExternalLoginInfoAsync();

            var usuario = await UserManager.FindByEmailAsync(loginInfo.Email);
            var usuarioJaExiste = usuario != null;

            if (usuarioJaExiste)
                return View("Error");

            var novoUsuario = new UsuarioAplicacao();
            
            novoUsuario.Email = loginInfo.Email;
            novoUsuario.UserName = loginInfo.Email;
            novoUsuario.NomeCompleto = loginInfo.ExternalIdentity.FindFirstValue(loginInfo.ExternalIdentity.NameClaimType);
            
            var resultado = await UserManager.CreateAsync(novoUsuario);

            if (resultado.Succeeded)
            {
                var resultadoAddLogin = await UserManager.AddLoginAsync(novoUsuario.Id, loginInfo.Login);
                if (resultadoAddLogin.Succeeded)
                    return RedirectToAction("Index", "Home");
            }

            return View("Error");
        }

        public async Task<ActionResult> AutenticacaoExternaCallback(string provider)
        {
            var loginInfo = await SignInManager.AuthenticationManager.GetExternalLoginInfoAsync();



            var resultadoSignIn = await SignInManager.ExternalSignInAsync(loginInfo, true);

            switch (resultadoSignIn)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Index", "Home");
                default:
                    //TODO: mudar esse fluxo
                    var usuario = await UserManager.FindByEmailAsync(loginInfo.Email);
                    var usuarioJaExiste = usuario != null;

                    if (usuarioJaExiste)
                    {
                        var resultadoAddLogin = await UserManager.AddLoginAsync(usuario.Id, loginInfo.Login);
                        return await AutenticacaoExternaCallback(provider);
                    }
                    break;
            }

            return View("Error");
        }

        [HttpPost]
        public ActionResult Logoff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        private ActionResult SenhaOuUsuarioInvalidos()
        {
            ModelState.AddModelError("", "Credenciais inválidas!");
            return View("Login");
        }

        private void AdicionaErros(IdentityResult resultado)
        {
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError("", erro);
        }
    }
}