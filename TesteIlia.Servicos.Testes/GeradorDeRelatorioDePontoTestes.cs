using Microsoft.Win32;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteIlia.CrossCutting;
using TesteIlia.Persistencia.Repositorio;
using TesteIlia.Servicos.DTOs;
using TesteIlia.Servicos.Ponto;
using TesteIlia.Servicos.RelatorioDePonto;
using Xunit.Sdk;

namespace TesteIlia.Servicos.Testes
{
    public class GeradorDeRelatorioDePontoTestes
    {

        private readonly Mock<IRegistroDeBatidaRepositorio> _batidaRepositorioMock;
        private readonly GeradorDeRelatorioDePonto _geradorDeRealatorioDePonto;

        public GeradorDeRelatorioDePontoTestes()
        {
            _batidaRepositorioMock = new();
            _geradorDeRealatorioDePonto = new(_batidaRepositorioMock.Object);
        }


        private IList<DateTime> DiasUteisDoMes(DateTime referencia) => Enumerable.Range(1, DateTime.DaysInMonth(referencia.Year, referencia.Month))
            .Select(d => new DateTime(referencia.Year, referencia.Month, d, 0, 0, 0))
            .Where(d => d.DayOfWeek != DayOfWeek.Sunday && d.DayOfWeek != DayOfWeek.Saturday)
            .ToList();

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task GerarRelatorioDeveRetornar400SeMesAnoForNulo(string mesAno)
        {
            var codigoErroEsperado = CodigoErro.BadRequest;
            var mensagemErroEsperada = "Mês não informado";

            var resultadoGeracaoRelatorio = await _geradorDeRealatorioDePonto.GerarRelatorioDeFolhaDoMes(mesAno);

            Assert.True(resultadoGeracaoRelatorio.Falha);
            Assert.Equal(codigoErroEsperado, resultadoGeracaoRelatorio.CodigoErro);
            Assert.Equal(mensagemErroEsperada, resultadoGeracaoRelatorio.Mensagem);
        }

        [Fact]
        public async Task GerarRelatorioDeveRetornar400SeDataNaoEstiverNoFormatoEsperado()
        {
            var mesAno = "sdanioasjna";
            var codigoErroEsperado = CodigoErro.BadRequest;
            var mensagemErroEsperada = "Mês ano informado em formato não válido";

            var resultadoGeracaoRelatorio = await _geradorDeRealatorioDePonto.GerarRelatorioDeFolhaDoMes(mesAno);

            Assert.True(resultadoGeracaoRelatorio.Falha);
            Assert.Equal(codigoErroEsperado, resultadoGeracaoRelatorio.CodigoErro);
            Assert.Equal(mensagemErroEsperada, resultadoGeracaoRelatorio.Mensagem);
        }

        [Fact]
        public async Task GerarRelatorioDeveRetornarRegistrosZeradosSeNaoHouverNenhumRegistro()
        {
            var dataReferencia = new DateTime(2023, 05, 01);
            var mesAno = dataReferencia.ToString("yyyy-MM");
            var inicioIntervalo = new DateTime(dataReferencia.Year, dataReferencia.Month, 1, 0, 0, 0);
            var fimIntervalo = inicioIntervalo.AddMonths(1);

            var horasTrabalhadasEsperado = "PT0H0M0S";
            var horasExcedentesEsperado = "PT0H0M0S";
            var horasDevidasEsperado = "PT184H0M0S"; //mês tem 23 dias úteis (desconsiderando feriados)

            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(inicioIntervalo, fimIntervalo))
                .ReturnsAsync(new List<DateTime>());

            var resultadoGeracaoRelatorio = await _geradorDeRealatorioDePonto.GerarRelatorioDeFolhaDoMes(mesAno);

            Assert.True(resultadoGeracaoRelatorio.Sucesso);
            Assert.Equal(mesAno, resultadoGeracaoRelatorio.Retorno.mes);
            Assert.Equal(horasTrabalhadasEsperado, resultadoGeracaoRelatorio.Retorno.horasTrabalhadas);
            Assert.Equal(horasExcedentesEsperado, resultadoGeracaoRelatorio.Retorno.horasExcedentes);
            Assert.Equal(horasDevidasEsperado, resultadoGeracaoRelatorio.Retorno.horasDevidas);
            Assert.Empty(resultadoGeracaoRelatorio.Retorno.pontosDosDias);
        }

        [Fact]
        public async Task GerarRelatorioDeveConsiderarHorasTrabalhadasEntreEntradaESaidaDescontandoAlmocoSeTodosOsRegistrosEstiveremPresentes()
        {
            var dataReferencia = new DateTime(2023, 05, 01, 0, 0, 0);
            var mesAno = dataReferencia.ToString("yyyy-MM");
            var registros = new[]
            {
                dataReferencia.AddHours(8),
                dataReferencia.AddHours(12),
                dataReferencia.AddHours(13),
                dataReferencia.AddHours(17),
            };

            var inicioIntervalo = new DateTime(dataReferencia.Year, dataReferencia.Month, 1, 0, 0, 0);
            var fimIntervalo = inicioIntervalo.AddMonths(1);

            var horasTrabalhadasEsperado = "PT8H0M0S";

            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(inicioIntervalo, fimIntervalo))
                .ReturnsAsync(registros);

            var resultadoGeracaoRelatorio = await _geradorDeRealatorioDePonto.GerarRelatorioDeFolhaDoMes(mesAno);

            Assert.True(resultadoGeracaoRelatorio.Sucesso);
            Assert.Equal(horasTrabalhadasEsperado, resultadoGeracaoRelatorio.Retorno.horasTrabalhadas);
        }

        [Fact]
        public async Task GerarRelatorioDeveConsiderarHorasTrabalhadasEntreEntradaEFimDoDiaDescontandoAlmocoSe3RegistrosEstaoPresentes()
        {
            var dataReferencia = new DateTime(2023, 05, 01);
            var mesAno = dataReferencia.ToString("yyyy-MM");
            var registros = new[]
            {
                dataReferencia.AddHours(8),
                dataReferencia.AddHours(12),
                dataReferencia.AddHours(13)
            };
            var inicioIntervalo = new DateTime(dataReferencia.Year, dataReferencia.Month, 1, 0, 0, 0);
            var fimIntervalo = inicioIntervalo.AddMonths(1);
            
            var horasTrabalhadasEsperado = "PT14H59M59S";

            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(inicioIntervalo, fimIntervalo))
                .ReturnsAsync(registros);

            var resultadoGeracaoRelatorio = await _geradorDeRealatorioDePonto.GerarRelatorioDeFolhaDoMes(mesAno);

            Assert.True(resultadoGeracaoRelatorio.Sucesso);
            Assert.Equal(horasTrabalhadasEsperado, resultadoGeracaoRelatorio.Retorno.horasTrabalhadas);
        }

        [Fact]
        public async Task GerarRelatorioDeveConsiderarHorasTrabalhadasEntreEntradaESaidaParaAlmocoSeApenas2RegistrosEstaoPresentes()
        {
            var dataReferencia = new DateTime(2023, 05, 01, 0, 0, 0);
            var mesAno = dataReferencia.ToString("yyyy-MM");
            var registros = new[]
            {
                dataReferencia.AddHours(8),
                dataReferencia.AddHours(12)
            };

            var inicioIntervalo = new DateTime(dataReferencia.Year, dataReferencia.Month, 1, 0, 0, 0);
            var fimIntervalo = inicioIntervalo.AddMonths(1);

            var horasTrabalhadasEsperado = "PT4H0M0S";

            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(inicioIntervalo, fimIntervalo))
                .ReturnsAsync(registros);

            var resultadoGeracaoRelatorio = await _geradorDeRealatorioDePonto.GerarRelatorioDeFolhaDoMes(mesAno);

            Assert.True(resultadoGeracaoRelatorio.Sucesso);
            Assert.Equal(horasTrabalhadasEsperado, resultadoGeracaoRelatorio.Retorno.horasTrabalhadas);
        }


        [Fact]
        public async Task GerarRelatorioDeveConsiderarHorasTrabalhadasEntreEntradaEFimDoDiaSe1RegistroEstaPresente()
        {
            var dataReferencia = new DateTime(2023, 05, 01, 0, 0, 0);
            var mesAno = dataReferencia.ToString("yyyy-MM");
            var registros = new[]
            {
                dataReferencia.AddHours(8)
            };

            var inicioIntervalo = new DateTime(dataReferencia.Year, dataReferencia.Month, 1, 0, 0, 0);
            var fimIntervalo = inicioIntervalo.AddMonths(1);

            var horasTrabalhadasEsperado = "PT15H59M59S";

            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(inicioIntervalo, fimIntervalo))
                .ReturnsAsync(registros);

            var resultadoGeracaoRelatorio = await _geradorDeRealatorioDePonto.GerarRelatorioDeFolhaDoMes(mesAno);

            Assert.True(resultadoGeracaoRelatorio.Sucesso);
            Assert.Equal(horasTrabalhadasEsperado, resultadoGeracaoRelatorio.Retorno.horasTrabalhadas);
        }


        [Fact]
        public async Task GerarRelatorioDeveConsolidarHorasDeMultiplosDias()
        {
            var dataReferencia = new DateTime(2023, 05, 01, 0, 0, 0);
            var diaSeguinteDataReferencia = dataReferencia.AddDays(1);
            var mesAno = dataReferencia.ToString("yyyy-MM");
            var registrosDeDataDeReferencia = new[] { dataReferencia.AddHours(8), dataReferencia.AddHours(12), dataReferencia.AddHours(13), dataReferencia.AddHours(17) };
            var registrosDoDiaSeguinte = new[] 
            { 
                diaSeguinteDataReferencia.AddHours(8).AddMinutes(30), diaSeguinteDataReferencia.AddHours(12).AddSeconds(30),
                diaSeguinteDataReferencia.AddHours(13).AddMinutes(15).AddSeconds(15), diaSeguinteDataReferencia.AddHours(17) 
            };            
            //8:30:00 ~ 12:00:30 = 3h30m30s
            //13:15:15 ~ 17:00:00 = 3h44m45s
            var inicioIntervalo = new DateTime(dataReferencia.Year, dataReferencia.Month, 1, 0, 0, 0);
            var fimIntervalo = inicioIntervalo.AddMonths(1);
            //8h + 3h30m30s + 3h44m45s
            var horasTrabalhadasEsperado = "PT15H15M15S";

            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(inicioIntervalo, fimIntervalo))
                .ReturnsAsync(registrosDeDataDeReferencia.Concat(registrosDoDiaSeguinte).ToList());

            var resultadoGeracaoRelatorio = await _geradorDeRealatorioDePonto.GerarRelatorioDeFolhaDoMes(mesAno);

            Assert.True(resultadoGeracaoRelatorio.Sucesso);
            Assert.Equal(mesAno, resultadoGeracaoRelatorio.Retorno.mes);
            Assert.Equal(horasTrabalhadasEsperado, resultadoGeracaoRelatorio.Retorno.horasTrabalhadas);
            Assert.Equal(2, resultadoGeracaoRelatorio.Retorno.pontosDosDias.Count);
            Assert.Contains(resultadoGeracaoRelatorio.Retorno.pontosDosDias,
                ponto => ponto.dia == dataReferencia.ToString("yyyy-MM-dd") && registrosDeDataDeReferencia.All(reg => ponto.horarios.Contains(reg.ToString("HH:mm:ss"))));
            Assert.Contains(resultadoGeracaoRelatorio.Retorno.pontosDosDias,
                ponto => ponto.dia == diaSeguinteDataReferencia.ToString("yyyy-MM-dd") && registrosDoDiaSeguinte.All(reg => ponto.horarios.Contains(reg.ToString("HH:mm:ss"))));
        }

        [Fact]
        public async Task GerarRelatorioDeveCalcularHorasExcedentesCorretamente()
        {
            var dataReferencia = new DateTime(2023, 05, 01, 0, 0, 0);
            var mesAno = dataReferencia.ToString("yyyy-MM");
            var diasUteisDoMes = DiasUteisDoMes(dataReferencia);
            var registrosDoMes = diasUteisDoMes.SelectMany(dia => new[]
            {
                dia.AddHours(8),
                dia.AddHours(12),
                dia.AddHours(13),
                dia.AddHours(18),
            });
            
            var inicioIntervalo = new DateTime(dataReferencia.Year, dataReferencia.Month, 1, 0, 0, 0);
            var fimIntervalo = inicioIntervalo.AddMonths(1);
            
            var horasExcedentesEsperado = $"PT{diasUteisDoMes.Count}H0M0S"; //1h excedente por dia útil

            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(inicioIntervalo, fimIntervalo))
                .ReturnsAsync(registrosDoMes.ToList());

            var resultadoGeracaoRelatorio = await _geradorDeRealatorioDePonto.GerarRelatorioDeFolhaDoMes(mesAno);

            Assert.True(resultadoGeracaoRelatorio.Sucesso);
            Assert.Equal(mesAno, resultadoGeracaoRelatorio.Retorno.mes);
            Assert.Equal(horasExcedentesEsperado, resultadoGeracaoRelatorio.Retorno.horasExcedentes);            
        }

        [Fact]
        public async Task GerarRelatorioDeveCalcularHorasDevidasCorretamente()
        {
            var dataReferencia = new DateTime(2023, 05, 01, 0, 0, 0);
            var mesAno = dataReferencia.ToString("yyyy-MM");
            var diasUteisDoMes = DiasUteisDoMes(dataReferencia);
            var registrosDoMes = diasUteisDoMes.SelectMany(dia => new[]
            {
                dia.AddHours(8),
                dia.AddHours(12),
                dia.AddHours(13),
                dia.AddHours(16),
            });

            var inicioIntervalo = new DateTime(dataReferencia.Year, dataReferencia.Month, 1, 0, 0, 0);
            var fimIntervalo = inicioIntervalo.AddMonths(1);

            var horasDevidasEsperado = $"PT{diasUteisDoMes.Count}H0M0S"; //1h devida por dia útil

            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(inicioIntervalo, fimIntervalo))
                .ReturnsAsync(registrosDoMes.ToList());

            var resultadoGeracaoRelatorio = await _geradorDeRealatorioDePonto.GerarRelatorioDeFolhaDoMes(mesAno);

            Assert.True(resultadoGeracaoRelatorio.Sucesso);
            Assert.Equal(mesAno, resultadoGeracaoRelatorio.Retorno.mes);
            Assert.Equal(horasDevidasEsperado, resultadoGeracaoRelatorio.Retorno.horasDevidas);
        }

    }
}
