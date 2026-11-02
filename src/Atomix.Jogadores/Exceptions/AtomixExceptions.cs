namespace Atomix.Jogadores.Exceptions;

/// <summary>Dados de entrada inválidos (nome vazio, e-mail mal formatado, etc.).</summary>
public class ValidacaoException : Exception
{
    public IReadOnlyList<string> Erros { get; }

    public ValidacaoException(IEnumerable<string> erros)
        : this(erros.ToList()) { }

    private ValidacaoException(List<string> erros)
        : base(string.Join(" | ", erros))
    {
        Erros = erros;
    }

    public ValidacaoException(string erro) : this(new List<string> { erro }) { }
}

/// <summary>Operação válida tecnicamente, mas proibida pelas regras do jogo (e-mail repetido, jogador inexistente...).</summary>
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}

/// <summary>Falha ao ler ou gravar os dados (arquivo JSON ou banco Oracle).</summary>
public class PersistenciaException : Exception
{
    public PersistenciaException(string mensagem, Exception? interna = null) : base(mensagem, interna) { }
}

/// <summary>A entrada padrão foi encerrada (Ctrl+Z / Ctrl+D ou fim de arquivo redirecionado).</summary>
public class EntradaEncerradaException : Exception
{
    public EntradaEncerradaException() : base("Entrada encerrada.") { }
}
