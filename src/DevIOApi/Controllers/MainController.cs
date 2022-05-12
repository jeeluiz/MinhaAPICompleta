using DevIO.Business.Intefaces;
using DevIO.Business.Notificacoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DevIOApi.Controllers
{
    [Route("[controller]")]
    public abstract class MainController : ControllerBase
    {
        private readonly INotificador _notificador;
        private readonly IUser AppUser;
        private INotificador notificador;

        protected Guid UsuarioId { get; set; }
        protected bool UsuarioAutenticado { get; set; }

        protected MainController(INotificador notificador, 
            IUser appUser)
        {
            _notificador = notificador;
            AppUser = appUser;

            if (appUser.IsAuthenticated())
            {
                UsuarioId = appUser.GetUserId();
                UsuarioAutenticado = true;
            }
        }

        protected MainController(INotificador notificador)
        {
            this.notificador = notificador;
        }

        protected bool OperacaoValida()
        {
            return !_notificador.TemNotificacao();
        }

        protected ActionResult CustomResponse(object result = null)
        {
            if (OperacaoValida())
            {
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            return BadRequest(new
            {
                success = false,
                errors = _notificador.ObterNotificacoes().Select(n => n.Mensagem)
            });

        }


        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) NotificarErrosModelInvalida(modelState);
            return CustomResponse();

        }
        protected void NotificarErrosModelInvalida(ModelStateDictionary modelState)
        {
            var erros = modelState.Values.SelectMany(e => e.Errors);
            foreach (var erro in erros)
            {
                var errosMsg = erro.Exception == null ? erro.ErrorMessage : erro.ErrorMessage;
                NotificarErro(errosMsg);
            }
        }

        protected void NotificarErro(string mensagem)
        {
            _notificador.Handle(new Notificacao(mensagem));
        }
    }
}