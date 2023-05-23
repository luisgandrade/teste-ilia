using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteIlia.CrossCutting;
using TesteIlia.Servicos.DTOs;

namespace TesteIlia.Servicos.Ponto
{
    public interface IBatedorDePonto
    {
        Task<ResultadoOperacao<PontoDoDia>> ObterRegistrosDoDia(DateTime data);
        Task<ResultadoOperacao<PontoDoDia>> BaterPonto(string horario);
    }
}
