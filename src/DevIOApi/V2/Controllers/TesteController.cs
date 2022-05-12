using DevIO.Business.Intefaces;
using DevIOApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DevIOApi.V2.Controllers
{
    [ApiVersion("2.0")]
    //[Route("api/v{version:apiVersion}/teste")]
    [Route("api/v2/teste")]

    public class TesteController : MainController
    {
        public TesteController(INotificador notificador, IUser appUser) : base(notificador, appUser)
        {
        }

        [HttpGet]
        public string Valor()
        {
            return "Sou a V1";
        }
    }
}
