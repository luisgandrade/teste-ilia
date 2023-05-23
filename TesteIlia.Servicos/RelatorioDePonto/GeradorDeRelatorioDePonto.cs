using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TesteIlia.CrossCutting;
using TesteIlia.Persistencia.Repositorio;
using TesteIlia.Servicos.DTOs;

namespace TesteIlia.Servicos.RelatorioDePonto
{
    public class GeradorDeRelatorioDePonto : IGeradorDeRelatorioDePonto
    {

        private readonly IRegistroDeBatidaRepositorio _registroDeBatidaRepositorio;

        public GeradorDeRelatorioDePonto(IRegistroDeBatidaRepositorio registroDeBatidaRepositorio)
        {
            _registroDeBatidaRepositorio = registroDeBatidaRepositorio;
        }

        private int QuantidadeHorasUteisNoMes(DateTime data) => Enumerable.Range(1, DateTime.DaysInMonth(data.Year, data.Month))
            .Select(d => new DateTime(data.Year, data.Month, d))
            .Where(d => d.DayOfWeek != DayOfWeek.Sunday && d.DayOfWeek != DayOfWeek.Saturday)
            .Count() * 8;

        private long QuantidadeDeTicksTrabalhadosNoDia(IList<DateTime> registrosNoDia)
        {
            var registrosOrdenados = registrosNoDia.OrderBy(reg => reg);
            var timestampFimDoDia = new DateTime(registrosNoDia[0].Year, registrosNoDia[0].Month, registrosNoDia[0].Day, 23, 59, 59);

            return registrosNoDia.Count switch
            {
                4 => (registrosNoDia[3] - registrosNoDia[0]).Ticks - (registrosNoDia[2] - registrosNoDia[1]).Ticks,
                3 => (timestampFimDoDia - registrosNoDia[0]).Ticks - (registrosNoDia[2] - registrosNoDia[1]).Ticks,
                2 => (registrosNoDia[1] - registrosNoDia[0]).Ticks,
                1 => (timestampFimDoDia - registrosNoDia[0]).Ticks,
                _ => throw new InvalidOperationException("Quantidade inválida de registros no dia")
            };
        }

        private PontoDoDia MapearParaDto(IList<DateTime> registros) => new PontoDoDia(
            dia: registros[0].ToString("yyyy-MM-dd"),
            horarios: registros.OrderBy(reg => reg).Select(reg => reg.ToString("HH:mm:ss")).ToList());

        private string FormatarQuantidadeHoras(TimeSpan timespan)
        {
            var totalHoras = (int)timespan.TotalHours;
            var totalMinutos = (int)timespan.TotalMinutes - totalHoras * 60;
            var totalSegundos = (int)timespan.TotalSeconds - (int)timespan.TotalMinutes * 60;

            return $"PT{totalHoras}H{totalMinutos}M{totalSegundos}S";
        }

        public async Task<ResultadoOperacao<RelatorioMensalDePonto>> GerarRelatorioDeFolhaDoMes(string mesAno)
        {
            if (string.IsNullOrWhiteSpace(mesAno))
                return ResultadoOperacao<RelatorioMensalDePonto>.CriarResultadoDeFalha(CodigoErro.BadRequest, "Mês não informado");
            if(!DateTime.TryParseExact(mesAno, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                return ResultadoOperacao<RelatorioMensalDePonto>.CriarResultadoDeFalha(CodigoErro.BadRequest, "Mês ano informado em formato não válido");

            var dataInicio = new DateTime(data.Year, data.Month, 1, 0, 0, 0);
            var dataFinal = dataInicio.AddMonths(1);
            var registrosNoMes = await _registroDeBatidaRepositorio.BuscarRegistrosNoIntervalo(dataInicio, dataFinal);

            var registrosOrdenadosAgrupadosPorDia = registrosNoMes
                .OrderBy(reg => reg)
                .GroupBy(reg => reg.Date)
                .Select(reg => new
                {
                    dia = reg.Key,
                    registros = reg.ToList(),
                    quantidadeMinutosTrabalhadosNoDia = QuantidadeDeTicksTrabalhadosNoDia(reg.ToList())
                }).ToList();

            var horasTrabalhadosNoMes = TimeSpan.FromTicks(registrosOrdenadosAgrupadosPorDia.Sum(reg => reg.quantidadeMinutosTrabalhadosNoDia));
            var horasEsperadasDeTrabalhoNoMes = TimeSpan.FromHours(QuantidadeHorasUteisNoMes(data));
            
            var horasExcedentes = TimeSpan.FromTicks(Math.Max(horasTrabalhadosNoMes.Ticks - horasEsperadasDeTrabalhoNoMes.Ticks, 0));
            var horasDevidas = TimeSpan.FromTicks(Math.Max(horasEsperadasDeTrabalhoNoMes.Ticks - horasTrabalhadosNoMes.Ticks, 0));

            var relatorioMensalDePonto = new RelatorioMensalDePonto(
                mes: mesAno,
                horasTrabalhadas: FormatarQuantidadeHoras(horasTrabalhadosNoMes),
                horasExcedentes: FormatarQuantidadeHoras(horasExcedentes),
                horasDevidas: FormatarQuantidadeHoras(horasDevidas),
                pontosDosDias: registrosOrdenadosAgrupadosPorDia.Select(reg => MapearParaDto(reg.registros)).ToList());

            return ResultadoOperacao<RelatorioMensalDePonto>.CriarResultadoDeSucesso(relatorioMensalDePonto);
        }
    }
}
