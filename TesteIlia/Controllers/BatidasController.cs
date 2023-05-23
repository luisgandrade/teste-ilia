using Microsoft.AspNetCore.Mvc;
using TesteIlia.CrossCutting;
using TesteIlia.DTOs;
using TesteIlia.Servicos.Ponto;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TesteIlia.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class BatidasController : ControllerBase
    {

        private readonly IBatedorDePonto _batedorDePonto;

        public BatidasController(IBatedorDePonto batedorDePonto)
        {
            _batedorDePonto = batedorDePonto;
        }

        // GET v1/<BatidasController>/5
        [HttpGet("{dia}")]
        public async Task<IActionResult> Get(DateTime dia)
        {
            var resultadoConsultaPontoDoDia = await _batedorDePonto.ObterRegistrosDoDia(dia);
            if (resultadoConsultaPontoDoDia.Falha)
                return NotFound();
            return Ok(resultadoConsultaPontoDoDia.Retorno);
        }

        // POST v1/<BatidasController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Momento momento)
        {
            if (momento is null)
                return BadRequest("Momento não informado");

            var resultadoDoRegistroDePonto = await _batedorDePonto.BaterPonto(momento.dataHora);
            if (resultadoDoRegistroDePonto.Falha)
                return StatusCode((int)resultadoDoRegistroDePonto.CodigoErro, new Mensagem(resultadoDoRegistroDePonto.Mensagem));
            return CreatedAtAction(nameof(Get), new { dia = resultadoDoRegistroDePonto.Retorno.dia }, resultadoDoRegistroDePonto.Retorno);
        }
    }
}
