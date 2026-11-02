using Atomix.Jogadores.Exceptions;
using Atomix.Jogadores.Models;
using Microsoft.EntityFrameworkCore;

namespace Atomix.Jogadores.Data;

/// <summary>
/// Persistência no Oracle via Entity Framework Core.
/// Ativada quando a variável de ambiente ATOMIX_ORACLE_CONNECTION está definida.
/// </summary>
public sealed class OracleRepository : IAtomixRepository
{
    private readonly AtomixDbContext _db;

    public OracleRepository(string connectionString)
    {
        _db = new AtomixDbContext(connectionString);

        try
        {
            if (!_db.Database.CanConnect())
                throw new PersistenciaException("Não foi possível conectar ao Oracle com a connection string informada.");

            // No Oracle o catálogo de conquistas é cadastrado pela API Java (coleção Postman do
            // projeto de Banco). O console não grava conquistas para não duplicar os dados da carga.
            if (!_db.Conquistas.Any())
                AvisoCarregamento = "O banco ainda não tem conquistas cadastradas (TB_ATX_CONQUISTA vazia). " +
                                    "Carregue os dados pela API Java (coleção Postman) para o desbloqueio funcionar.";
        }
        catch (PersistenciaException)
        {
            _db.Dispose();
            throw;
        }
        catch (Exception ex)
        {
            _db.Dispose();
            throw new PersistenciaException($"Falha ao inicializar o Oracle: {ex.Message}", ex);
        }
    }

    public string Descricao => "banco Oracle (EF Core)";

    /// <summary>Aviso exibido ao iniciar (ex.: tabela de conquistas vazia).</summary>
    public string? AvisoCarregamento { get; private set; }

    public IReadOnlyList<Jogador> ListarJogadores() => Executar(() =>
        _db.Jogadores
            .AsNoTracking()
            .Include(j => j.Conquistas)
            .OrderBy(j => j.IdJogador)
            .ToList());

    public Jogador? ObterJogador(int idJogador) => Executar(() =>
        _db.Jogadores
            .Include(j => j.Conquistas)
            .FirstOrDefault(j => j.IdJogador == idJogador));

    public void AdicionarJogador(Jogador jogador) => Gravar(() =>
    {
        // Mesma sequence usada pela procedure PRC_ATX_INS_JOGADOR do projeto de Banco
        jogador.IdJogador = ProximoIdJogador();
        _db.Jogadores.Add(jogador);
    });

    public void AtualizarJogador(Jogador jogador) => Gravar(() =>
    {
        if (_db.Entry(jogador).State == EntityState.Detached)
            _db.Jogadores.Update(jogador);
    });

    public void RemoverJogador(int idJogador) => Gravar(() =>
    {
        var jogador = _db.Jogadores.Find(idJogador);
        if (jogador is not null)
            _db.Jogadores.Remove(jogador);
    });

    public IReadOnlyList<Conquista> ListarConquistas() => Executar(() =>
        _db.Conquistas.AsNoTracking().OrderBy(c => c.PontosNecessarios).ToList());

    private int ProximoIdJogador() =>
        _db.Database
            .SqlQuery<int>($"SELECT CAST(SQ_ATX_JOGADOR.NEXTVAL AS NUMBER(10)) AS \"Value\" FROM DUAL")
            .AsEnumerable()
            .First();

    private T Executar<T>(Func<T> consulta)
    {
        try
        {
            return consulta();
        }
        catch (Exception ex) when (ex is not PersistenciaException)
        {
            throw new PersistenciaException($"Erro ao consultar o Oracle: {ex.Message}", ex);
        }
    }

    private void Gravar(Action alteracao)
    {
        try
        {
            alteracao();
            _db.SaveChanges();
        }
        catch (Exception ex)
        {
            // Descarta as alterações pendentes para não "sujar" as próximas operações
            _db.ChangeTracker.Clear();
            throw new PersistenciaException($"Erro ao gravar no Oracle: {ex.InnerException?.Message ?? ex.Message}", ex);
        }
    }

    public void Dispose() => _db.Dispose();
}
