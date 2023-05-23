using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteIlia.Servicos.DTOs
{
    public record RelatorioMensalDePonto(string mes, string horasTrabalhadas, string horasExcedentes, string horasDevidas, IList<PontoDoDia> pontosDosDias);
}
