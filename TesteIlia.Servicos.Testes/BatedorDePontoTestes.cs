using Moq;
using TesteIlia.CrossCutting;
using TesteIlia.Persistencia.Repositorio;
using TesteIlia.Servicos.Ponto;

namespace TesteIlia.Servicos.Testes
{
    public class BatedorDePontoTestes
    {
        private readonly Mock<IRegistroDeBatidaRepositorio> _batidaRepositorioMock;
        private readonly BatedorDePonto _batedorDePonto;

        public BatedorDePontoTestes()
        {
            _batidaRepositorioMock = new();
            _batedorDePonto = new(_batidaRepositorioMock.Object);
        }


        [Fact]
        public async Task ObterRegistrosDoDiaDeveRetornar404SeNaoHouverRegistros()
        {
            var horario = DateTime.Now;
            var codigoErroEsperado = CodigoErro.NotFound;

            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(horario.Date, horario.Date.AddDays(1))).ReturnsAsync(new List<DateTime>());

            var resultado = await _batedorDePonto.ObterRegistrosDoDia(horario);

            Assert.True(resultado.Falha);
            Assert.Equal(codigoErroEsperado, resultado.CodigoErro);
        }

        [Fact]
        public async Task ObterRegistrosDoDiaDeveRetornarPontoDoDiaSeHouverRegistros()
        {
            var agora = DateTime.Now;
            var horarioEntrada = new DateTime(agora.Year, agora.Month, agora.Day, 8, 0, 0);
            var horarioSaidaAlmoco = new DateTime(agora.Year, agora.Month, agora.Day, 12, 0, 0);
            var horarioRetornoAlmoco = new DateTime(agora.Year, agora.Month, agora.Day, 13, 0, 0);
            var horarioSaida = new DateTime(agora.Year, agora.Month, agora.Day, 17, 0, 0);
            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(horarioEntrada.Date, horarioEntrada.Date.AddDays(1)))
                .ReturnsAsync(new[] { horarioEntrada, horarioSaidaAlmoco, horarioRetornoAlmoco, horarioSaida });

            var horariosEsperadoNoRetorno = new[]
            {
                horarioEntrada.ToString("HH:mm:ss"),
                horarioSaidaAlmoco.ToString("HH:mm:ss"),
                horarioRetornoAlmoco.ToString("HH:mm:ss"),
                horarioSaida.ToString("HH:mm:ss")
            };

            var resultado = await _batedorDePonto.ObterRegistrosDoDia(agora);

            Assert.True(resultado.Sucesso);
            Assert.Equal(agora.ToString("yyyy-MM-dd"), resultado.Retorno.dia);
            Assert.True(resultado.Retorno.horarios.SequenceEqual(horariosEsperadoNoRetorno));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task BaterPontoDeveRetornar400ComMensagemSeHorarioForNulo(string horario)
        {
            var mensagemEsperada = "Campo obrigatório não informado";
            var codigoErroEsperado = CodigoErro.BadRequest;

            var resultado = await _batedorDePonto.BaterPonto(horario);

            Assert.True(resultado.Falha);
            Assert.Equal(codigoErroEsperado, resultado.CodigoErro);
            Assert.Equal(mensagemEsperada, resultado.Mensagem);
        }

        [Fact]
        public async Task BaterPontoDeveRetornar400ComMensagemSeHorarioNaoForConversivelParaData()
        {
            var horario = "kashjnisdauihfwe";
            var mensagemEsperada = "Horário com formato inválido";
            var codigoErroEsperado = CodigoErro.BadRequest;

            var resultado = await _batedorDePonto.BaterPonto(horario);

            Assert.True(resultado.Falha);
            Assert.Equal(codigoErroEsperado, resultado.CodigoErro);
            Assert.Equal(mensagemEsperada, resultado.Mensagem);
        }

        [Theory]
        [InlineData("2023-05-20T08:00:00")]
        [InlineData("2023-05-21T08:00:00")]
        public async Task BaterPontoDeveRetornar403ComMensagemSeDataForFimDeSemana(string horario)
        {
            
            var mensagemEsperada = "Sábado e domingo não são permitidos como dia de trabalho";
            var codigoErroEsperado = CodigoErro.Forbidden;

            var resultado = await _batedorDePonto.BaterPonto(horario);

            Assert.True(resultado.Falha);
            Assert.Equal(codigoErroEsperado, resultado.CodigoErro);
            Assert.Equal(mensagemEsperada, resultado.Mensagem);
        }

        [Fact]
        public async Task BaterPontoDeveRetornar409ComMensagemSeHorarioJaFoiInserido()
        {
            var horarioComoString = DateTime.Now.ToString("s");
            var horario = DateTime.Parse(horarioComoString);
            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(horario.Date, horario.Date.AddDays(1))).ReturnsAsync(new[] { horario });

            var mensagemEsperada = "Horário já registrado";
            var codigoErroEsperado = CodigoErro.Conflict;

            var resultado = await _batedorDePonto.BaterPonto(horarioComoString);

            Assert.True(resultado.Falha);
            Assert.Equal(codigoErroEsperado, resultado.CodigoErro);
            Assert.Equal(mensagemEsperada, resultado.Mensagem);
        }

        [Fact]
        public async Task BaterPontoDeveRetornar403ComMensagemSeJaHouver4RegistrosNoDia()
        {
            var agora = DateTime.Now;
            var horarioEntrada = new DateTime(agora.Year, agora.Month, agora.Day, 8, 0, 0);
            var horarioSaidaAlmoco = new DateTime(agora.Year, agora.Month, agora.Day, 12, 0, 0);
            var horarioRetornoAlmoco = new DateTime(agora.Year, agora.Month, agora.Day, 13, 0, 0);
            var horarioSaida = new DateTime(agora.Year, agora.Month, agora.Day, 17, 0, 0);
            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(horarioEntrada.Date, horarioEntrada.Date.AddDays(1)))
                .ReturnsAsync(new[] { horarioEntrada, horarioSaidaAlmoco, horarioRetornoAlmoco, horarioSaida });

            var mensagemEsperada = "Apenas 4 horários podem ser registrados por dia";
            var codigoErroEsperado = CodigoErro.Forbidden;

            var resultado = await _batedorDePonto.BaterPonto(agora.ToString("s"));

            Assert.True(resultado.Falha);
            Assert.Equal(codigoErroEsperado, resultado.CodigoErro);
            Assert.Equal(mensagemEsperada, resultado.Mensagem);
        }

        [Fact]
        public async Task BaterPontoDeveRetornar403ComMensagemSeHorarioDoRegistroForMenorQueHorariosRegistrados()
        {
            var agora = DateTime.Now;
            var horarioEntrada = new DateTime(agora.Year, agora.Month, agora.Day, 8, 0, 0);
            var horarioSaidaAlmoco = new DateTime(agora.Year, agora.Month, agora.Day, 12, 0, 0);
            var horarioRetornoAlmoco = new DateTime(agora.Year, agora.Month, agora.Day, 13, 0, 0);
            var novoHorario = new DateTime(agora.Year, agora.Month, agora.Day, 11, 0, 0);
            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(horarioEntrada.Date, horarioEntrada.Date.AddDays(1)))
                .ReturnsAsync(new[] { horarioEntrada, horarioSaidaAlmoco, horarioRetornoAlmoco });

            var mensagemEsperada = "Não é permitido registro retroativo de ponto";
            var codigoErroEsperado = CodigoErro.Forbidden;

            var resultado = await _batedorDePonto.BaterPonto(novoHorario.ToString("s"));

            Assert.True(resultado.Falha);
            Assert.Equal(codigoErroEsperado, resultado.CodigoErro);
            Assert.Equal(mensagemEsperada, resultado.Mensagem);
        }

        [Fact]
        public async Task BaterPontoDeveRetornar403ComMensagemSeHorarioDeAlmocoForMenorQue1Hora()
        {
            var agora = DateTime.Now;
            var horarioEntrada = new DateTime(agora.Year, agora.Month, agora.Day, 8, 0, 0);
            var horarioSaidaAlmoco = new DateTime(agora.Year, agora.Month, agora.Day, 12, 0, 0);
            var horarioRetornoAlmoco = new DateTime(agora.Year, agora.Month, agora.Day, 12, 30, 0);
            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(horarioEntrada.Date, horarioEntrada.Date.AddDays(1)))
                .ReturnsAsync(new[] { horarioEntrada, horarioSaidaAlmoco });

            var mensagemEsperada = "Deve haver no mínimo 1 hora de almoço";
            var codigoErroEsperado = CodigoErro.Forbidden;

            var resultado = await _batedorDePonto.BaterPonto(horarioRetornoAlmoco.ToString("s"));

            Assert.True(resultado.Falha);
            Assert.Equal(codigoErroEsperado, resultado.CodigoErro);
            Assert.Equal(mensagemEsperada, resultado.Mensagem);
        }

        [Fact]
        public async Task BaterPontoDeveInserirNoRepositorioERetornarRegistrosDoDiaEmCasoDeSucesso()
        {
            var agora = DateTime.Now;
            var horarioEntrada = new DateTime(agora.Year, agora.Month, agora.Day, 8, 0, 0);
            var horarioSaidaAlmoco = new DateTime(agora.Year, agora.Month, agora.Day, 12, 0, 0);
            var horarioRetornoAlmoco = new DateTime(agora.Year, agora.Month, agora.Day, 13, 0, 0);
            var horarioSaida = new DateTime(agora.Year, agora.Month, agora.Day, 17, 0, 0);
            _batidaRepositorioMock.Setup(reg => reg.BuscarRegistrosNoIntervalo(horarioEntrada.Date, horarioEntrada.Date.AddDays(1)))
                .ReturnsAsync(new[] { horarioEntrada, horarioSaidaAlmoco, horarioRetornoAlmoco});

            var horariosEsperadoNoRetorno = new[]
            {
                horarioEntrada.ToString("HH:mm:ss"),
                horarioSaidaAlmoco.ToString("HH:mm:ss"),
                horarioRetornoAlmoco.ToString("HH:mm:ss"),
                horarioSaida.ToString("HH:mm:ss")
            };

            var resultado = await _batedorDePonto.BaterPonto(horarioSaida.ToString("s"));

            Assert.True(resultado.Sucesso);
            Assert.Equal(agora.ToString("yyyy-MM-dd"), resultado.Retorno.dia);
            Assert.True(resultado.Retorno.horarios.SequenceEqual(horariosEsperadoNoRetorno));

            _batidaRepositorioMock.Verify(reg => reg.Inserir(horarioSaida), Times.Once());
        }
    }
}