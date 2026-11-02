using System.Text.Encodings.Web;
using System.Text.Json;
using Atomix.Jogadores.Exceptions;
using Atomix.Jogadores.Models;

namespace Atomix.Jogadores.Data;

/// <summary>
/// Persistência em arquivo JSON (modo padrão — não precisa de banco nem de credenciais).
/// </summary>
public sealed class JsonRepository : IAtomixRepository
{
    private sealed class ArquivoDados
    {
        public int ProximoIdJogador { get; set; } = 1;
        public List<Jogador> Jogadores { get; set; } = new();
        public List<Conquista> Conquistas { get; set; } = new();
    }

    private static readonly JsonSerializerOptions Opcoes = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // mantém acentos legíveis no arquivo
    };

    private readonly string _caminho;
    private ArquivoDados _dados;

    /// <summary>Mensagem informativa gerada no carregamento (ex.: arquivo corrompido recuperado).</summary>
    public string? AvisoCarregamento { get; private set; }

    public JsonRepository(string caminhoArquivo)
    {
        _caminho = Path.GetFullPath(caminhoArquivo);
        _dados = Carregar();
    }

    public string Descricao => $"arquivo JSON ({_caminho})";

    public IReadOnlyList<Jogador> ListarJogadores() => _dados.Jogadores.OrderBy(j => j.IdJogador).ToList();

    public Jogador? ObterJogador(int idJogador) => _dados.Jogadores.FirstOrDefault(j => j.IdJogador == idJogador);

    public void AdicionarJogador(Jogador jogador)
    {
        jogador.IdJogador = _dados.ProximoIdJogador++;
        _dados.Jogadores.Add(jogador);
        Salvar();
    }

    public void AtualizarJogador(Jogador jogador) => Salvar();

    public void RemoverJogador(int idJogador)
    {
        _dados.Jogadores.RemoveAll(j => j.IdJogador == idJogador);
        Salvar();
    }

    public IReadOnlyList<Conquista> ListarConquistas() =>
        _dados.Conquistas.OrderBy(c => c.PontosNecessarios).ToList();

    private ArquivoDados Carregar()
    {
        try
        {
            if (!File.Exists(_caminho))
            {
                var novo = new ArquivoDados { Conquistas = DadosIniciais.Conquistas() };
                _dados = novo;
                Salvar();
                return novo;
            }

            var json = File.ReadAllText(_caminho);
            var dados = JsonSerializer.Deserialize<ArquivoDados>(json, Opcoes) ?? new ArquivoDados();

            if (dados.Conquistas.Count == 0)
                dados.Conquistas = DadosIniciais.Conquistas();

            // Garante que o próximo ID nunca colida com um já existente
            var maiorId = dados.Jogadores.Count == 0 ? 0 : dados.Jogadores.Max(j => j.IdJogador);
            dados.ProximoIdJogador = Math.Max(dados.ProximoIdJogador, maiorId + 1);
            return dados;
        }
        catch (JsonException)
        {
            // Arquivo corrompido: guarda uma cópia e recomeça, em vez de derrubar o programa
            var backup = _caminho + $".corrompido-{DateTime.Now:yyyyMMddHHmmss}";
            File.Move(_caminho, backup);
            AvisoCarregamento = $"O arquivo de dados estava corrompido. Uma cópia foi salva em {backup} e um novo arquivo foi criado.";
            var novo = new ArquivoDados { Conquistas = DadosIniciais.Conquistas() };
            _dados = novo;
            Salvar();
            return novo;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new PersistenciaException($"Não foi possível ler o arquivo de dados: {ex.Message}", ex);
        }
    }

    private void Salvar()
    {
        try
        {
            var pasta = Path.GetDirectoryName(_caminho);
            if (!string.IsNullOrEmpty(pasta))
                Directory.CreateDirectory(pasta);

            // Grava num arquivo temporário e depois substitui, para não corromper o original se algo falhar
            var temporario = _caminho + ".tmp";
            File.WriteAllText(temporario, JsonSerializer.Serialize(_dados, Opcoes));
            File.Move(temporario, _caminho, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new PersistenciaException($"Não foi possível salvar o arquivo de dados: {ex.Message}", ex);
        }
    }

    public void Dispose() { }
}
