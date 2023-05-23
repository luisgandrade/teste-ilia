
namespace TesteIlia.Persistencia.Repositorio
{
    public class RegistroDeBatidaRepositorioMemoria : IRegistroDeBatidaRepositorio
    {

        private readonly ISet<DateTime> _registrosDeBatidas= new HashSet<DateTime>();

        public Task<IList<DateTime>> BuscarRegistrosNoIntervalo(DateTime inicio, DateTime fim)
        {
            IList<DateTime> registros = _registrosDeBatidas.Where(reg => reg  >= inicio && reg <= fim).ToList();
            return Task.FromResult(registros);
        }

        public Task Inserir(DateTime registroDeBatida)
        {
            if (_registrosDeBatidas.Contains(registroDeBatida))
                throw new InvalidOperationException("Registo já existente para o dia");
            _registrosDeBatidas.Add(registroDeBatida);
            return Task.CompletedTask;
        }

    }
}
