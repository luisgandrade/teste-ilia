using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteIlia.CrossCutting;
using TesteIlia.Servicos.DTOs;

namespace TesteIlia.Servicos.RelatorioDePonto
{
    public interface IGeradorDeRelatorioDePonto
    {

        Task<ResultadoOperacao<RelatorioMensalDePonto>> GerarRelatorioDeFolhaDoMes(string mesAno);
    }
}
