# ATOMIX ⚛ — Jogadores e Conquistas (.NET)

Aplicação console em **C# / .NET 8** do projeto **ATOMIX**, desenvolvida para a disciplina **Advanced Business Development with .NET** no Challenge FIAP 2026 (2TDSPA) com a **Marinha do Brasil — CTMSP (Programa Nuclear da Marinha)**.

## 👥 Integrantes

| Nome | RM |
|---|---|
| Gianluca K. Araujo | RM 566765 |
| Ruan Ribeiro | RM 567693 |

## 💡 A solução: ATOMIX

O desafio da Marinha é aproximar a sociedade da tecnologia nuclear, mostrando de forma acessível suas aplicações em saúde, agricultura, indústria, energia e pesquisa.

O **ATOMIX** é um app gamificado em que o usuário explora uma **cidade nuclear** com 5 áreas — **Saúde, Agro, Indústria, Energia e Pesquisa**. Em cada área ele descobre aplicações reais da tecnologia nuclear, lê curiosidades e responde quizzes, ganhando pontos e desbloqueando conquistas.

O projeto completo é formado por:

- **App Flutter** — a experiência do usuário (cidade, áreas, quiz, perfil e conquistas);
- **API REST Java + Spring** — porta de entrada dos dados: CRUD principal de **Aplicações Nucleares** (com busca por texto) e os cadastros que o banco exige, gravando tudo pelas procedures do Oracle;
- **Console .NET (este repositório)** — **motor de gamificação** dos **Jogadores e Conquistas**: pontuação dos quizzes, níveis e desbloqueio por pontos;
- **Banco Oracle** — tabelas TB_ATX_AREA, TB_ATX_APLICACAO, TB_ATX_PERGUNTA, TB_ATX_JOGADOR, TB_ATX_CONQUISTA e TB_ATX_JOGADOR_CONQUISTA;
- **Docker + Azure** — deploy da API Java e do banco;
- **IoT (conceitual)** — totens com NFC/sensor de presença em museus e escolas que liberam missões no app.

## 🎯 O que foi desenvolvido nesta Sprint (escopo do módulo .NET)

Entidade principal: **Jogador**, relacionada às **Conquistas** do jogo. A funcionalidade é **complementar** à da API Java: a API concentra o conteúdo educativo (CRUD de Aplicações Nucleares) e os cadastros, enquanto este console implementa regras que a API não tem — a pontuação do quiz com bônus, a progressão de nível com aviso, o progresso até a próxima conquista e as buscas por nível e por pontuação.

| Funcionalidade | Detalhes |
|---|---|
| **Cadastro** | Nome, e-mail e senha (digitada com `*`), com validação e e-mail único; a senha é gravada só como hash **BCrypt**, igual à API Java |
| **Listagem** | Tabela com ID, nome, e-mail, pontos, nível, nº de conquistas e data de cadastro |
| **Busca por critério** | Por parte do nome (ignora maiúsculas e acentos: "joao" encontra "João"), por e-mail, por nível ou por pontuação mínima |
| **Registrar quiz** | Informa a área da cidade e os acertos; o sistema calcula os pontos e desbloqueia conquistas |
| **Conquistas do jogador** | Conquistas desbloqueadas com data e quanto falta para a próxima |
| **Catálogo de conquistas** | Todas as conquistas e os pontos necessários |
| **Ranking** | Top 10 por pontos (desempate por nº de conquistas e data de cadastro) |
| **Editar / remover** | Edição de nome e e-mail; remoção com confirmação |

### Regras de negócio

1. **Pontuação do quiz:** cada quiz de área tem 5 perguntas. Cada acerto vale **10 pontos**; acertar as 5 dá **+20 de bônus** (máximo de 70 pontos por quiz).
2. **Desbloqueio de conquistas por pontos:** no cadastro e sempre que o jogador ganha pontos, o sistema desbloqueia automaticamente **todas** as conquistas cuja pontuação necessária foi atingida. Cada conquista é desbloqueada uma única vez e registra a data. O catálogo é o mesmo do banco Oracle do projeto e do app:

   | Conquista | Pontos necessários |
   |---|---|
   | Primeiro Átomo | 0 (liberada no cadastro) |
   | Explorador da Saúde | 50 |
   | Guardião do Agro | 100 |
   | Engenheiro da Indústria | 150 |
   | Energia Limpa | 200 |
   | Mente Científica | 300 |
   | Mestre do Quiz | 400 |
   | Cidadão Nuclear | 500 |
   | Submarinista | 750 |
   | Mestre Nuclear | 1000 |

3. **Nível do jogador** (calculado a partir dos pontos, mesma escala do ranking do banco): Aprendiz Atômico (0–99), Explorador Nuclear (100–299), Cientista do Átomo (300–599), Especialista CTMSP (600–999) e Mestre Nuclear (1000+). O sistema avisa quando o jogador sobe de nível.
4. **E-mail único:** não é possível cadastrar nem editar um jogador para um e-mail que já está em uso (comparação sem diferenciar maiúsculas).

### Validações

- Nome obrigatório, de 3 a 80 caracteres, só letras (com acento), espaços, hífen ou apóstrofo; espaços extras são removidos.
- E-mail obrigatório, formato válido, até 120 caracteres; salvo em minúsculas.
- Senha de 6 a 50 caracteres, com pelo menos uma letra e um número; digitada com `*` e gravada só como hash BCrypt.
- Acertos entre 0 e 5; área entre as 5 existentes; IDs numéricos positivos.
- Todos os erros de um cadastro são mostrados juntos.

### Tratamento de erros

Cada opção do menu roda dentro de `try/catch` com exceções próprias (`ValidacaoException`, `RegraNegocioException`, `PersistenciaException`). Qualquer erro é exibido e o programa **volta ao menu sem encerrar**. Entradas numéricas usam `TryParse` (digitar texto onde se espera número só pede de novo). Se o arquivo JSON estiver corrompido, é feita uma cópia de segurança e um novo arquivo é criado. Se o Oracle estiver inacessível, o app avisa e continua usando o JSON.

## 🧱 Arquitetura

```
Menu do console (UI) ──► Services (regras: pontuação, níveis, conquistas, validação)
                               │
                               ▼
                     IAtomixRepository
                      ├── JsonRepository   → dados/atomix-jogadores.json (padrão)
                      └── OracleRepository → EF Core + Oracle (TB_ATX_JOGADOR, TB_ATX_CONQUISTA,
                                             TB_ATX_JOGADOR_CONQUISTA), ligado pela variável ATOMIX_ORACLE_CONNECTION
```

## 🗂️ Estrutura

```
├── Atomix.sln
├── scripts/
│   └── oracle_tabelas_atomix.sql   # Script de Banco de Dados: tabelas + dados de teste (modo Oracle)
└── src/Atomix.Jogadores/
    ├── Program.cs                  # Inicialização e tratamento global de erros
    ├── Models/                     # Jogador, Conquista, JogadorConquista, AreaNuclear, NivelJogador
    ├── Services/                   # JogadorService, ConquistaService (regras), JogadorValidator
    ├── Data/                       # IAtomixRepository, JsonRepository, OracleRepository, AtomixDbContext
    ├── Exceptions/                 # Exceções de validação, regra de negócio e persistência
    └── UI/                         # Menu do console e helpers de entrada/saída
```

## ▶️ Como executar

### Pré-requisito

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0) ou superior (o projeto também compila e roda com o SDK 9 ou 10)
  - macOS (Homebrew): `brew install --cask dotnet-sdk` (instala a versão mais recente)
  - Windows (winget): `winget install Microsoft.DotNet.SDK.8`

Confira com `dotnet --version`.

### Rodar (modo padrão — arquivo JSON)

```bash
git clone <url-deste-repositorio>
cd <pasta-do-repositorio>
dotnet run --project src/Atomix.Jogadores
```

Os dados ficam em `dados/atomix-jogadores.json`, criado automaticamente na primeira execução (com o catálogo de conquistas). Não precisa de banco nem de credenciais.

### Rodar com Oracle (EF Core) — opcional

1. As tabelas são as do projeto de Banco de Dados. Se o banco ainda não existe, rode `scripts/oracle_tabelas_atomix.sql` no seu schema (SQL Developer, F5): é o mesmo script da entrega de Banco, que cria as tabelas e já cadastra áreas, conquistas e jogadores de teste. Se o banco já foi criado pela entrega de Banco ou pelo Docker da API, pule este passo (o script recria as tabelas e apagaria os dados). Se a tabela de conquistas estiver vazia, o console avisa ao iniciar.
2. Defina a connection string **numa variável de ambiente** (ela nunca vai para o repositório):

   **macOS / Linux**
   ```bash
   export ATOMIX_ORACLE_CONNECTION="User Id=SEU_RM;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/ORCL"
   dotnet run --project src/Atomix.Jogadores
   ```

   **Windows (PowerShell)**
   ```powershell
   $env:ATOMIX_ORACLE_CONNECTION="User Id=SEU_RM;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/ORCL"
   dotnet run --project src/Atomix.Jogadores
   ```

Para usar o Oracle do Docker da API Java (`docker compose up -d oracle-db` no repositório da API), a connection string é `User Id=ATOMIX;Password=<DB_PASSWORD do .env>;Data Source=localhost:1521/FREEPDB1`.

Ao iniciar, o app mostra onde os dados estão sendo salvos. O arquivo `.env.example` traz apenas um modelo com valores fictícios.

### Roteiro rápido de teste

1. `1` → cadastre um jogador (tente também um e-mail inválido, um e-mail repetido e uma senha curta para ver as validações).
   O cadastro já libera a conquista **Primeiro Átomo**.
2. `4` → registre um quiz com 5 acertos: +70 pontos e a conquista **Explorador da Saúde**.
3. `4` → registre outro quiz com 4 acertos: chega a 110 pontos, sobe para **Explorador Nuclear** e desbloqueia **Guardião do Agro**.
4. `3` → busque por nome sem acento, por nível ou por pontuação mínima.
5. `5`, `6` e `7` → veja conquistas do jogador, catálogo e ranking.

## 🔧 Como o .NET se aplica à solução

No ATOMIX, o .NET é responsável pela camada de **regras de negócio da gamificação**: é ele que guarda os jogadores, transforma o desempenho nos quizzes da cidade nuclear em pontos e decide, de forma centralizada e consistente, quando cada conquista é desbloqueada e quando o jogador sobe de nível. Com C# e .NET 8 organizamos o código em camadas (modelos, serviços, repositórios e interface), usamos LINQ para as buscas e o ranking, exceções personalizadas com `try/catch` para que o sistema nunca pare por erro de entrada, e o **Entity Framework Core** com o provider Oracle para mapear as tabelas TB_ATX_JOGADOR, TB_ATX_CONQUISTA e TB_ATX_JOGADOR_CONQUISTA — as mesmas do banco compartilhado com a API Java, com IDs gerados pelas mesmas sequences. Assim, enquanto o app Flutter cuida da experiência e a API Java do conteúdo educativo sobre as aplicações nucleares, o .NET garante a motivação do usuário a continuar aprendendo, com regras de progressão confiáveis e fáceis de evoluir.
