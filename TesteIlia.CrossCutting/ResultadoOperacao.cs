namespace TesteIlia.CrossCutting
{
    public class ResultadoOperacao
    {
        public bool Sucesso { get; protected set; }
        public bool Falha => !Sucesso;

        public CodigoErro CodigoErro { get; protected set; }

        public string? Mensagem { get; protected set; }

        protected ResultadoOperacao()
        {
            Sucesso = true;
        }

        protected ResultadoOperacao(CodigoErro codigoErro, string mensagem)
        {
            CodigoErro = codigoErro;
            Mensagem = mensagem;
        }

        public static ResultadoOperacao CriarResultadoDeSucesso() => new ResultadoOperacao();

        public static ResultadoOperacao CriarResultadoDeFalha(CodigoErro codigoErro, string mensagem) => new ResultadoOperacao(codigoErro, mensagem);
    }

    public class ResultadoOperacao<T> : ResultadoOperacao
    {
        
        public T? Retorno { get; private set; }

        private ResultadoOperacao(T retorno)
            : base()
        {
            Retorno = retorno;
        }

        private ResultadoOperacao(CodigoErro codigoErro, string mensagem)
            : base(codigoErro, mensagem)
        {
        }

        public static ResultadoOperacao<T> CriarResultadoDeSucesso(T retorno) => new ResultadoOperacao<T>(retorno);

        public static new ResultadoOperacao<T> CriarResultadoDeFalha(CodigoErro codigoErro, string mensagem) => new ResultadoOperacao<T>(codigoErro, mensagem);
    }
}