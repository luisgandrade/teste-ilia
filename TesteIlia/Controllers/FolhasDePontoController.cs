using Microsoft.AspNetCore.Mvc;
using TesteIlia.DTOs;
using TesteIlia.Servicos.RelatorioDePonto;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TesteIlia.Controllers
{
    [Route("api/folhas-de-ponto")]
    [ApiController]
    public class FolhasDePontoController : ControllerBase
    {

        private readonly IGeradorDeRelatorioDePonto _geradorRelatorioDePonto;

        public FolhasDePontoController(IGeradorDeRelatorioDePonto geradorRelatorioDePonto)
        {
            _geradorRelatorioDePonto = geradorRelatorioDePonto;
        }


        [HttpGet("{mes}")]
        public async Task<IActionResult> Get(string mes)
        {
            var resultadoRelatorio = await _geradorRelatorioDePonto.GerarRelatorioDeFolhaDoMes(mes);
            if (resultadoRelatorio.Falha)
                return StatusCode((int)resultadoRelatorio.CodigoErro, new Mensagem(resultadoRelatorio.Mensagem));
            return Ok(resultadoRelatorio.Retorno);
        }
        
    }
}
