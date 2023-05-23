using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteIlia.CrossCutting;
using TesteIlia.Persistencia.Repositorio;
using TesteIlia.Servicos.DTOs;

namespace TesteIlia.Servicos.Ponto
{
    public class BatedorDePonto : IBatedorDePonto
    {

        private readonly IRegistroDeBatidaRepositorio _registroDeBatidaRepositorio;

        public BatedorDePonto(IRegistroDeBatidaRepositorio registroDeBatidaRepositorio)
        {
            _registroDeBatidaRepositorio = registroDeBatidaRepositorio;
        }

        private PontoDoDia MapearParaDto(IList<DateTime> registros) => new PontoDoDia(
            dia: registros[0].ToString("yyyy-MM-dd"),
            horarios: registros.OrderBy(reg => reg).Select(reg => reg.ToString("HH:mm:ss")).ToList());        

        public async Task<ResultadoOperacao<PontoDoDia>> ObterRegistrosDoDia(DateTime data) 
        {
            var registrosDoDia = await _registroDeBatidaRepositorio.BuscarRegistrosNoIntervalo(data.Date, data.Date.AddDays(1));
            if (!registrosDoDia.Any())
                return ResultadoOperacao<PontoDoDia>.CriarResultadoDeFalha(CodigoErro.NotFound, "Não encontrado");
            return ResultadoOperacao<PontoDoDia>.CriarResultadoDeSucesso(MapearParaDto(registrosDoDia));
        }

        public async Task<ResultadoOperacao<PontoDoDia>> BaterPonto(string horario)
        {
            if (string.IsNullOrWhiteSpace(horario))
                return ResultadoOperacao<PontoDoDia>.CriarResultadoDeFalha(CodigoErro.BadRequest, "Campo obrigatório não informado");
            if (!DateTime.TryParse(horario, out var horarioComoDatetime))
                return ResultadoOperacao<PontoDoDia>.CriarResultadoDeFalha(CodigoErro.BadRequest, "Horário com formato inválido");
            if(horarioComoDatetime.DayOfWeek == DayOfWeek.Sunday || horarioComoDatetime.DayOfWeek == DayOfWeek.Saturday)
                return ResultadoOperacao<PontoDoDia>.CriarResultadoDeFalha(CodigoErro.Forbidden, "Sábado e domingo não são permitidos como dia de trabalho");
            
            var registrosDePontoNoDia = await _registroDeBatidaRepositorio.BuscarRegistrosNoIntervalo(horarioComoDatetime.Date, horarioComoDatetime.Date.AddDays(1));

            if (registrosDePontoNoDia.Contains(horarioComoDatetime))
                return ResultadoOperacao<PontoDoDia>.CriarResultadoDeFalha(CodigoErro.Conflict, "Horário já registrado");
            if(registrosDePontoNoDia.Count >= 4)
                return ResultadoOperacao<PontoDoDia>.CriarResultadoDeFalha(CodigoErro.Forbidden, "Apenas 4 horários podem ser registrados por dia");

            var registrosDePontoOrdenadosPorData = registrosDePontoNoDia.OrderBy(reg => reg).ToList();

            if (registrosDePontoOrdenadosPorData.Any() && horarioComoDatetime < registrosDePontoOrdenadosPorData.Last())
                return ResultadoOperacao<PontoDoDia>.CriarResultadoDeFalha(CodigoErro.Forbidden, "Não é permitido registro retroativo de ponto");

            if (registrosDePontoOrdenadosPorData.Count == 2 && horarioComoDatetime - registrosDePontoOrdenadosPorData[1] < TimeSpan.FromHours(1))
                return ResultadoOperacao<PontoDoDia>.CriarResultadoDeFalha(CodigoErro.Forbidden, "Deve haver no mínimo 1 hora de almoço");

            await _registroDeBatidaRepositorio.Inserir(horarioComoDatetime);
            registrosDePontoOrdenadosPorData.Add(horarioComoDatetime);


            return ResultadoOperacao<PontoDoDia>.CriarResultadoDeSucesso(MapearParaDto(registrosDePontoOrdenadosPorData));
        }
    }
}
