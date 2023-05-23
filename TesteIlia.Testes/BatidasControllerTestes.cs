using Microsoft.AspNetCore.Mvc;
using Moq;
using TesteIlia.Controllers;
using TesteIlia.CrossCutting;
using TesteIlia.DTOs;
using TesteIlia.Servicos.DTOs;
using TesteIlia.Servicos.Ponto;

namespace TesteIlia.Testes
{
    public class BatidasControllerTestes
    {
        private readonly Mock<IBatedorDePonto> _batedorDePonto;
        private readonly BatidasController _batidasController;

        public BatidasControllerTestes()
        {
            _batedorDePonto = new();
            _batidasController = new(_batedorDePonto.Object);
        }

        [Fact]
        public async void ObterRegistrosDoDiaRetornaNotFoundSeBuscaDeRegistroFalhar()
        {
            var data = DateTime.Now;
            var resultadoBuscaEsperado = ResultadoOperacao<PontoDoDia>.CriarResultadoDeFalha(CodigoErro.NotFound, "");

            _batedorDePonto.Setup(bp => bp.ObterRegistrosDoDia(data)).ReturnsAsync(resultadoBuscaEsperado);

            var resultadoAction = await _batidasController.Get(data);

            Assert.IsType<NotFoundResult>(resultadoAction);
        }

        [Fact]
        public async void ObterRegistrosDoDiaRetornaOkComPontoDoDiaSeEncontrarResultados()
        {
            var data = DateTime.Now;
            var pontoDoDia = new PontoDoDia("", new List<string>());
            var resultadoBuscaEsperado = ResultadoOperacao<PontoDoDia>.CriarResultadoDeSucesso(pontoDoDia);

            _batedorDePonto.Setup(bp => bp.ObterRegistrosDoDia(data)).ReturnsAsync(resultadoBuscaEsperado);

            var resultadoAction = await _batidasController.Get(data);

            var resultadoActionComoOkObjectResult = resultadoAction as OkObjectResult;
            Assert.NotNull(resultadoActionComoOkObjectResult);
            Assert.Equal(pontoDoDia, resultadoActionComoOkObjectResult.Value);
        }

        [Theory]
        [InlineData(CodigoErro.BadRequest)]
        [InlineData(CodigoErro.Forbidden)]
        [InlineData(CodigoErro.Conflict)]
        public async void BaterPontoRetornaPropagaCodigoDeErroComMensagemSeOperacaoFalhar(CodigoErro codigoErro)
        {
            var data = DateTime.Now;
            var mensagemErroEsperada = "mensagem";
            var momento = new Momento(data.ToString("s"));
            var resultadoBuscaEsperado = ResultadoOperacao<PontoDoDia>.CriarResultadoDeFalha(codigoErro, mensagemErroEsperada);            

            _batedorDePonto.Setup(bp => bp.BaterPonto(momento.dataHora)).ReturnsAsync(resultadoBuscaEsperado);

            var resultadoAction = await _batidasController.Post(momento);

            var resultadoActionComoStatusCodeObjectResult = resultadoAction as ObjectResult;
            Assert.NotNull(resultadoActionComoStatusCodeObjectResult);
            Assert.Equal((int)codigoErro, resultadoActionComoStatusCodeObjectResult.StatusCode);
            var objetoRetornoComoMensagem = resultadoActionComoStatusCodeObjectResult.Value as Mensagem;
            Assert.Equal(mensagemErroEsperada, objetoRetornoComoMensagem.mensagem);
        }

        [Fact]
        public async void BaterPontoRetornaRetornarOkComOPontoDoDia()
        {
            var data = DateTime.Now;
            var pontoDoDia = new PontoDoDia("", new List<string>());            
            var momento = new Momento(data.ToString("s"));
            var resultadoBuscaEsperado = ResultadoOperacao<PontoDoDia>.CriarResultadoDeSucesso(pontoDoDia);

            _batedorDePonto.Setup(bp => bp.BaterPonto(momento.dataHora)).ReturnsAsync(resultadoBuscaEsperado);

            var resultadoAction = await _batidasController.Post(momento);

            var resultadoActionComoCreatedObjectResult = resultadoAction as CreatedAtActionResult;
            Assert.NotNull(resultadoActionComoCreatedObjectResult);
            Assert.Equal(nameof(_batidasController.Get), resultadoActionComoCreatedObjectResult.ActionName);
            Assert.Equal(pontoDoDia, resultadoActionComoCreatedObjectResult.Value);
        }
    }
}