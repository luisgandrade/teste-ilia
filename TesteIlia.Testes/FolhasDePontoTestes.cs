using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteIlia.Controllers;
using TesteIlia.CrossCutting;
using TesteIlia.DTOs;
using TesteIlia.Servicos.DTOs;
using TesteIlia.Servicos.Ponto;
using TesteIlia.Servicos.RelatorioDePonto;

namespace TesteIlia.Testes
{
    public class FolhasDePontoTestes
    {
        private readonly Mock<IGeradorDeRelatorioDePonto> _geradorRelatorioPonto;
        private readonly FolhasDePontoController _folhaPontoController;

        public FolhasDePontoTestes()
        {
            _geradorRelatorioPonto = new();
            _folhaPontoController = new(_geradorRelatorioPonto.Object);
        }

        [Theory]
        [InlineData(CodigoErro.BadRequest)]
        [InlineData(CodigoErro.Forbidden)]
        [InlineData(CodigoErro.NotFound)]
        [InlineData(CodigoErro.Conflict)]
        public async Task GerarRelatorioDevePropagarCodigoDeErroSeGeracaoDeRelatorioFalhar(CodigoErro codigoErro)
        {
            var mes = "2023-05";
            var mensagemErro = "mensagem";
            var resultadoOperacao = ResultadoOperacao<RelatorioMensalDePonto>.CriarResultadoDeFalha(codigoErro, mensagemErro);

            _geradorRelatorioPonto.Setup(grp => grp.GerarRelatorioDeFolhaDoMes(mes)).ReturnsAsync(resultadoOperacao);

            var resultadoAction = await _folhaPontoController.Get(mes);

            var resultadoActionComoStatusCodeObjectResult = resultadoAction as ObjectResult;
            Assert.NotNull(resultadoActionComoStatusCodeObjectResult);
            Assert.Equal((int)codigoErro, resultadoActionComoStatusCodeObjectResult.StatusCode);
            var objetoRetornoComoMensagem = resultadoActionComoStatusCodeObjectResult.Value as Mensagem;
            Assert.Equal(mensagemErro, objetoRetornoComoMensagem.mensagem);
        }

        [Fact]
        public async Task GerarRelatorioDeveRetornarORelatorioDeFolhaGerado()
        {
            var mes = "2023-05";
            var relatorio = new RelatorioMensalDePonto("", "", "", "", new List<PontoDoDia>());
            var resultadoOperacao = ResultadoOperacao<RelatorioMensalDePonto>.CriarResultadoDeSucesso(relatorio);

            _geradorRelatorioPonto.Setup(grp => grp.GerarRelatorioDeFolhaDoMes(mes)).ReturnsAsync(resultadoOperacao);

            var resultadoAction = await _folhaPontoController.Get(mes);

            var resultadoActionComoStatusOkObjectResult = resultadoAction as OkObjectResult;
            Assert.NotNull(resultadoActionComoStatusOkObjectResult);
            Assert.Equal(relatorio, resultadoActionComoStatusOkObjectResult.Value);
        }
    }
}
