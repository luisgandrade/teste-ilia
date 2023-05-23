using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteIlia.Servicos.DTOs
{
    public record PontoDoDia(string dia, IList<string> horarios);    
}
