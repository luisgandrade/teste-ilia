
namespace TesteIlia.Persistencia.Repositorio
{
    public interface IRegistroDeBatidaRepositorio
    {

        Task Inserir(DateTime registroDeBatida);
        Task<IList<DateTime>> BuscarRegistrosNoIntervalo(DateTime inicio, DateTime fim);

    }
}
