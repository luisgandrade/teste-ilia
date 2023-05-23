using TesteIlia.Persistencia.Repositorio;

namespace TesteIlia.Pesistencia.Testes
{
    public class RegistroDeBatidaRepositorioMemoriaTeste
    {
        private readonly RegistroDeBatidaRepositorioMemoria _registroDeBatidaRepositorioMemoria = new();

        [Fact]
        public async Task InserirRegistroDeveInserirRegistroDePonto()
        {
            var horario = DateTime.Now;
            await _registroDeBatidaRepositorioMemoria.Inserir(horario);
            var horarioInserido = await _registroDeBatidaRepositorioMemoria.BuscarRegistrosNoIntervalo(horario.Date, horario.Date.AddDays(1));

            Assert.NotEmpty(horarioInserido);
            Assert.Equal(horario, horarioInserido.Single());
        }


        [Fact]
        public async Task InserirRegistroDeveLancarExcecaoSeRegistroNoHorarioJaFoiInseridoAnteriormente()
        {
            var horario = DateTime.Now;

            await _registroDeBatidaRepositorioMemoria.Inserir(horario);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _registroDeBatidaRepositorioMemoria.Inserir(horario));
        }


        [Fact]
        public async Task BuscarRegistrosNoIntervaloDeveRetornarApenasRegistrosDentroDoIntervalo()
        {
            var horarioEntradaOntem = DateTime.Now.AddDays(-1);
            var horarioEntradaHoje = DateTime.Now;
            var horarioSaidaAlmocoHoje = DateTime.Now.AddHours(2);

            await _registroDeBatidaRepositorioMemoria.Inserir(horarioEntradaOntem);
            await _registroDeBatidaRepositorioMemoria.Inserir(horarioEntradaHoje);
            await _registroDeBatidaRepositorioMemoria.Inserir(horarioSaidaAlmocoHoje);

            var horariosInseridos = await _registroDeBatidaRepositorioMemoria.BuscarRegistrosNoIntervalo(horarioEntradaHoje.Date, horarioEntradaHoje.Date.AddDays(1));

            Assert.Contains(horarioEntradaHoje, horariosInseridos);
            Assert.Contains(horarioSaidaAlmocoHoje, horariosInseridos);
            Assert.DoesNotContain(horarioEntradaOntem, horariosInseridos);
        }
    }
}