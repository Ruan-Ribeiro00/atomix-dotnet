using Atomix.Jogadores.Exceptions;

namespace Atomix.Jogadores.Data;

/// <summary>
/// Escolhe onde os dados ficam:
///  - Se ATOMIX_ORACLE_CONNECTION estiver definida → Oracle via EF Core;
///  - Caso contrário (ou se o Oracle falhar) → arquivo JSON local.
/// A connection string NUNCA fica no código nem no repositório.
/// </summary>
public static class RepositoryFactory
{
    public const string VariavelOracle = "ATOMIX_ORACLE_CONNECTION";
    public const string VariavelArquivoJson = "ATOMIX_JSON_PATH";

    public static IAtomixRepository Criar(out List<string> avisos)
    {
        avisos = new List<string>();
        var connectionString = Environment.GetEnvironmentVariable(VariavelOracle);

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            try
            {
                var oracle = new OracleRepository(connectionString);
                if (oracle.AvisoCarregamento is not null)
                    avisos.Add(oracle.AvisoCarregamento);
                return oracle;
            }
            catch (PersistenciaException ex)
            {
                avisos.Add($"{ex.Message} Usando o arquivo JSON local no lugar.");
            }
        }

        var caminho = Environment.GetEnvironmentVariable(VariavelArquivoJson);
        if (string.IsNullOrWhiteSpace(caminho))
            caminho = Path.Combine(Directory.GetCurrentDirectory(), "dados", "atomix-jogadores.json");

        var json = new JsonRepository(caminho);
        if (json.AvisoCarregamento is not null)
            avisos.Add(json.AvisoCarregamento);
        return json;
    }
}
