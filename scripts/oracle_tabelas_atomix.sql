-- =====================================================================
--  ATOMIX - Challenge FIAP 2026 (2TDSPA) - Marinha do Brasil / CTMSP
--  Disciplina : Mastering Relational and Non-Relational Database
--  Entrega    : Sprint 1 (Sprint Flash - ideacao)
--  Arquivo    : ATOMIX_Script_DDL_DML.sql
--  Conteudo   : PARTE 1 - DDL: sequences, tabelas, PK, FK, NOT NULL,
--                         UNIQUE, CHECK, indices e comentarios
--               PARTE 2 - DML: insercao dos dados de teste
--               PARTE 3 - Consultas com JOIN que respondem perguntas da solucao
--  Grupo      : ATOMIX
--               Gianluca K. Araujo - RM 566765
--               Ruan Ribeiro       - RM 567693
--  SGBD       : Oracle Database 19c ou superior (testado no Oracle 23ai Free)
--  Como rodar : executar o arquivo inteiro como script
--               SQL Developer: abrir o arquivo e pressionar F5 (Executar Script)
--               SQL*Plus / SQLcl: @ATOMIX_Script_DDL_DML.sql
--               O script roda do inicio ao fim sem erros e pode ser executado
--               de novo: ele apaga e recria somente os objetos TB_ATX_* e SQ_ATX_*.
--  Observacao : os textos estao sem acentuacao de proposito, para o resultado
--               ser o mesmo em qualquer cliente e codificacao de arquivo.
-- =====================================================================


-- #####################################################################
--  PARTE 1 - DDL (criacao dos objetos)
-- #####################################################################

-- ---------------------------------------------------------------------
-- 0. LIMPEZA SEGURA (permite reexecutar o script sem nenhum erro)
--    Apaga apenas os objetos do projeto que ja existirem no schema.
--    Na primeira execucao nao encontra nada e segue normalmente.
-- ---------------------------------------------------------------------
BEGIN
    FOR t IN (SELECT table_name
                FROM user_tables
               WHERE table_name IN ('TB_ATX_JOGADOR_CONQUISTA', 'TB_ATX_CONQUISTA',
                                    'TB_ATX_JOGADOR', 'TB_ATX_PERGUNTA',
                                    'TB_ATX_APLICACAO', 'TB_ATX_AREA')) LOOP
        EXECUTE IMMEDIATE 'DROP TABLE ' || t.table_name || ' CASCADE CONSTRAINTS PURGE';
    END LOOP;

    FOR s IN (SELECT sequence_name
                FROM user_sequences
               WHERE sequence_name IN ('SQ_ATX_AREA', 'SQ_ATX_APLICACAO', 'SQ_ATX_PERGUNTA',
                                       'SQ_ATX_JOGADOR', 'SQ_ATX_CONQUISTA')) LOOP
        EXECUTE IMMEDIATE 'DROP SEQUENCE ' || s.sequence_name;
    END LOOP;
END;
/

-- ---------------------------------------------------------------------
-- 1. SEQUENCES (geracao das chaves primarias substitutas)
--    NOCACHE garante IDs sequenciais sem saltos apos restart do banco
--    (IDs 1, 2, 3...), o que deixa os dados de teste previsiveis.
-- ---------------------------------------------------------------------
CREATE SEQUENCE SQ_ATX_AREA      START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE SQ_ATX_APLICACAO START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE SQ_ATX_PERGUNTA  START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE SQ_ATX_JOGADOR   START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE SQ_ATX_CONQUISTA START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;


-- ---------------------------------------------------------------------
-- 2. TABELAS
-- ---------------------------------------------------------------------

-- 2.1 AREA - as areas da "cidade nuclear" do app
CREATE TABLE TB_ATX_AREA (
    id_area      NUMBER(10)     NOT NULL,
    nome         VARCHAR2(50)   NOT NULL,
    descricao    VARCHAR2(400)  NOT NULL,
    ativa        CHAR(1)        DEFAULT 'S' NOT NULL,
    CONSTRAINT PK_ATX_AREA        PRIMARY KEY (id_area),
    CONSTRAINT UK_ATX_AREA_NOME   UNIQUE (nome),
    CONSTRAINT CK_ATX_AREA_ATIVA  CHECK (ativa IN ('S', 'N'))
);

-- 2.2 APLICACAO - aplicacoes reais da tecnologia nuclear (usada pela API Java)
CREATE TABLE TB_ATX_APLICACAO (
    id_aplicacao NUMBER(10)     NOT NULL,
    id_area      NUMBER(10)     NOT NULL,
    titulo       VARCHAR2(120)  NOT NULL,
    descricao    VARCHAR2(1000) NOT NULL,
    beneficio    VARCHAR2(500)  NOT NULL,
    dt_cadastro  DATE           DEFAULT SYSDATE NOT NULL,
    CONSTRAINT PK_ATX_APLICACAO         PRIMARY KEY (id_aplicacao),
    CONSTRAINT UK_ATX_APLIC_AREA_TITULO UNIQUE (id_area, titulo),
    CONSTRAINT FK_ATX_APLIC_AREA        FOREIGN KEY (id_area)
        REFERENCES TB_ATX_AREA (id_area)
);

-- 2.3 PERGUNTA - perguntas do quiz, cada uma sobre uma aplicacao
CREATE TABLE TB_ATX_PERGUNTA (
    id_pergunta      NUMBER(10)    NOT NULL,
    id_aplicacao     NUMBER(10)    NOT NULL,
    enunciado        VARCHAR2(500) NOT NULL,
    alternativa_a    VARCHAR2(200) NOT NULL,
    alternativa_b    VARCHAR2(200) NOT NULL,
    alternativa_c    VARCHAR2(200) NOT NULL,
    alternativa_d    VARCHAR2(200) NOT NULL,
    resposta_correta CHAR(1)       NOT NULL,
    pontos           NUMBER(4)     DEFAULT 10 NOT NULL,
    CONSTRAINT PK_ATX_PERGUNTA          PRIMARY KEY (id_pergunta),
    CONSTRAINT CK_ATX_PERG_RESPOSTA     CHECK (resposta_correta IN ('A', 'B', 'C', 'D')),
    CONSTRAINT CK_ATX_PERG_PONTOS       CHECK (pontos BETWEEN 1 AND 1000),
    CONSTRAINT FK_ATX_PERG_APLICACAO    FOREIGN KEY (id_aplicacao)
        REFERENCES TB_ATX_APLICACAO (id_aplicacao)
);

-- 2.4 JOGADOR - usuario cadastrado no app (tela de cadastro / perfil)
CREATE TABLE TB_ATX_JOGADOR (
    id_jogador   NUMBER(10)    NOT NULL,
    nome         VARCHAR2(100) NOT NULL,
    email        VARCHAR2(120) NOT NULL,
    senha        VARCHAR2(100) NOT NULL,
    pontuacao    NUMBER(8)     DEFAULT 0 NOT NULL,
    dt_cadastro  DATE          DEFAULT SYSDATE NOT NULL,
    CONSTRAINT PK_ATX_JOGADOR           PRIMARY KEY (id_jogador),
    CONSTRAINT UK_ATX_JOGADOR_EMAIL     UNIQUE (email),
    CONSTRAINT CK_ATX_JOG_EMAIL         CHECK (email LIKE '_%@_%._%'),
    CONSTRAINT CK_ATX_JOG_PONTUACAO     CHECK (pontuacao >= 0)
);

-- 2.5 CONQUISTA - medalhas desbloqueadas por pontuacao
--     id_area e opcional: conquistas gerais nao pertencem a uma area.
CREATE TABLE TB_ATX_CONQUISTA (
    id_conquista       NUMBER(10)    NOT NULL,
    id_area            NUMBER(10),
    nome               VARCHAR2(60)  NOT NULL,
    descricao          VARCHAR2(300) NOT NULL,
    pontos_necessarios NUMBER(8)     NOT NULL,
    CONSTRAINT PK_ATX_CONQUISTA         PRIMARY KEY (id_conquista),
    CONSTRAINT UK_ATX_CONQUISTA_NOME    UNIQUE (nome),
    CONSTRAINT CK_ATX_CONQ_PONTOS       CHECK (pontos_necessarios >= 0),
    CONSTRAINT FK_ATX_CONQ_AREA         FOREIGN KEY (id_area)
        REFERENCES TB_ATX_AREA (id_area) ON DELETE SET NULL
);

-- 2.6 JOGADOR_CONQUISTA - entidade associativa (resolve o N:N
--     entre JOGADOR e CONQUISTA) e registra quando houve o desbloqueio.
CREATE TABLE TB_ATX_JOGADOR_CONQUISTA (
    id_jogador     NUMBER(10) NOT NULL,
    id_conquista   NUMBER(10) NOT NULL,
    dt_desbloqueio DATE       DEFAULT SYSDATE NOT NULL,
    CONSTRAINT PK_ATX_JOGADOR_CONQUISTA PRIMARY KEY (id_jogador, id_conquista),
    CONSTRAINT FK_ATX_JC_JOGADOR        FOREIGN KEY (id_jogador)
        REFERENCES TB_ATX_JOGADOR (id_jogador) ON DELETE CASCADE,
    CONSTRAINT FK_ATX_JC_CONQUISTA      FOREIGN KEY (id_conquista)
        REFERENCES TB_ATX_CONQUISTA (id_conquista) ON DELETE CASCADE
);


-- ---------------------------------------------------------------------
-- 3. INDICES nas chaves estrangeiras (evitam full scan nos JOINs e
--    lock de tabela filha ao excluir registros da tabela pai)
-- ---------------------------------------------------------------------
CREATE INDEX IX_ATX_APLIC_AREA      ON TB_ATX_APLICACAO (id_area);
CREATE INDEX IX_ATX_PERG_APLICACAO  ON TB_ATX_PERGUNTA (id_aplicacao);
CREATE INDEX IX_ATX_CONQ_AREA       ON TB_ATX_CONQUISTA (id_area);
CREATE INDEX IX_ATX_JC_CONQUISTA    ON TB_ATX_JOGADOR_CONQUISTA (id_conquista);


-- ---------------------------------------------------------------------
-- 4. DICIONARIO DE DADOS (COMMENTS)
-- ---------------------------------------------------------------------
COMMENT ON TABLE  TB_ATX_AREA                     IS 'Areas da cidade nuclear do app ATOMIX (Saude, Agro, Industria, Energia, Pesquisa...)';
COMMENT ON COLUMN TB_ATX_AREA.id_area             IS 'Chave primaria (SQ_ATX_AREA)';
COMMENT ON COLUMN TB_ATX_AREA.nome                IS 'Nome unico da area';
COMMENT ON COLUMN TB_ATX_AREA.descricao           IS 'Texto de apresentacao da area no app';
COMMENT ON COLUMN TB_ATX_AREA.ativa               IS 'S = visivel na cidade do app; N = area futura (em breve)';

COMMENT ON TABLE  TB_ATX_APLICACAO                IS 'Aplicacoes reais da tecnologia nuclear exibidas em cada area';
COMMENT ON COLUMN TB_ATX_APLICACAO.id_aplicacao   IS 'Chave primaria (SQ_ATX_APLICACAO)';
COMMENT ON COLUMN TB_ATX_APLICACAO.id_area        IS 'FK - area a que a aplicacao pertence';
COMMENT ON COLUMN TB_ATX_APLICACAO.titulo         IS 'Titulo da aplicacao (unico dentro da area)';
COMMENT ON COLUMN TB_ATX_APLICACAO.descricao      IS 'Explicacao de como a tecnologia nuclear e aplicada';
COMMENT ON COLUMN TB_ATX_APLICACAO.beneficio      IS 'Beneficio para a sociedade';
COMMENT ON COLUMN TB_ATX_APLICACAO.dt_cadastro    IS 'Data de cadastro do registro';

COMMENT ON TABLE  TB_ATX_PERGUNTA                 IS 'Perguntas do quiz; cada pergunta e sobre uma aplicacao';
COMMENT ON COLUMN TB_ATX_PERGUNTA.id_pergunta     IS 'Chave primaria (SQ_ATX_PERGUNTA)';
COMMENT ON COLUMN TB_ATX_PERGUNTA.id_aplicacao    IS 'FK - aplicacao sobre a qual a pergunta trata';
COMMENT ON COLUMN TB_ATX_PERGUNTA.enunciado       IS 'Texto da pergunta';
COMMENT ON COLUMN TB_ATX_PERGUNTA.alternativa_a   IS 'Texto da alternativa A';
COMMENT ON COLUMN TB_ATX_PERGUNTA.alternativa_b   IS 'Texto da alternativa B';
COMMENT ON COLUMN TB_ATX_PERGUNTA.alternativa_c   IS 'Texto da alternativa C';
COMMENT ON COLUMN TB_ATX_PERGUNTA.alternativa_d   IS 'Texto da alternativa D';
COMMENT ON COLUMN TB_ATX_PERGUNTA.resposta_correta IS 'Letra da alternativa correta (A, B, C ou D)';
COMMENT ON COLUMN TB_ATX_PERGUNTA.pontos          IS 'Pontos ganhos ao acertar a pergunta';

COMMENT ON TABLE  TB_ATX_JOGADOR                  IS 'Jogadores cadastrados no app';
COMMENT ON COLUMN TB_ATX_JOGADOR.id_jogador       IS 'Chave primaria (SQ_ATX_JOGADOR)';
COMMENT ON COLUMN TB_ATX_JOGADOR.nome             IS 'Nome do jogador';
COMMENT ON COLUMN TB_ATX_JOGADOR.email            IS 'E-mail unico, usado no login';
COMMENT ON COLUMN TB_ATX_JOGADOR.senha            IS 'Hash da senha (gerado pela API, ex.: BCrypt)';
COMMENT ON COLUMN TB_ATX_JOGADOR.pontuacao        IS 'Pontuacao acumulada no jogo';
COMMENT ON COLUMN TB_ATX_JOGADOR.dt_cadastro      IS 'Data de cadastro do jogador';

COMMENT ON TABLE  TB_ATX_CONQUISTA                IS 'Conquistas (medalhas) desbloqueaveis por pontuacao';
COMMENT ON COLUMN TB_ATX_CONQUISTA.id_conquista   IS 'Chave primaria (SQ_ATX_CONQUISTA)';
COMMENT ON COLUMN TB_ATX_CONQUISTA.id_area        IS 'FK opcional - area tematica da conquista (NULL = conquista geral)';
COMMENT ON COLUMN TB_ATX_CONQUISTA.nome           IS 'Nome unico da conquista';
COMMENT ON COLUMN TB_ATX_CONQUISTA.descricao      IS 'Descricao exibida no perfil';
COMMENT ON COLUMN TB_ATX_CONQUISTA.pontos_necessarios IS 'Pontuacao minima do jogador para desbloquear';

COMMENT ON TABLE  TB_ATX_JOGADOR_CONQUISTA        IS 'Associativa JOGADOR x CONQUISTA - conquistas desbloqueadas';
COMMENT ON COLUMN TB_ATX_JOGADOR_CONQUISTA.id_jogador     IS 'PK/FK - jogador';
COMMENT ON COLUMN TB_ATX_JOGADOR_CONQUISTA.id_conquista   IS 'PK/FK - conquista';
COMMENT ON COLUMN TB_ATX_JOGADOR_CONQUISTA.dt_desbloqueio IS 'Data em que a conquista foi desbloqueada';


-- #####################################################################
--  PARTE 2 - DML (insercao dos dados de teste)
--  Conteudo real do Programa Nuclear e das aplicacoes nucleares no Brasil.
--  As chaves estrangeiras sao buscadas pelas chaves de negocio (nome da area,
--  titulo da aplicacao, e-mail do jogador), sem depender de IDs fixos.
-- #####################################################################

-- ---------------------------------------------------------------------
-- 5.1 AREA - 10 areas: 5 ativas na cidade do app e 5 futuras ("em breve")
-- ---------------------------------------------------------------------
INSERT INTO TB_ATX_AREA (id_area, nome, descricao, ativa)
VALUES (SQ_ATX_AREA.NEXTVAL, 'Saude',
        'Como a radiacao ajuda a diagnosticar e tratar doencas, de exames de imagem ao tratamento do cancer.', 'S');
INSERT INTO TB_ATX_AREA (id_area, nome, descricao, ativa)
VALUES (SQ_ATX_AREA.NEXTVAL, 'Agro',
        'Tecnicas nucleares que protegem lavouras, controlam pragas e criam variedades mais resistentes.', 'S');
INSERT INTO TB_ATX_AREA (id_area, nome, descricao, ativa)
VALUES (SQ_ATX_AREA.NEXTVAL, 'Industria',
        'Inspecao de pecas, esterilizacao e controle de qualidade com fontes radioativas.', 'S');
INSERT INTO TB_ATX_AREA (id_area, nome, descricao, ativa)
VALUES (SQ_ATX_AREA.NEXTVAL, 'Energia',
        'Geracao de eletricidade em usinas nucleares e o ciclo do combustivel nuclear.', 'S');
INSERT INTO TB_ATX_AREA (id_area, nome, descricao, ativa)
VALUES (SQ_ATX_AREA.NEXTVAL, 'Pesquisa',
        'Reatores de pesquisa, datacao de materiais e producao de radioisotopos para a ciencia.', 'S');
INSERT INTO TB_ATX_AREA (id_area, nome, descricao, ativa)
VALUES (SQ_ATX_AREA.NEXTVAL, 'Meio Ambiente',
        'Tracadores radioativos para estudar agua, solo e poluicao.', 'N');
INSERT INTO TB_ATX_AREA (id_area, nome, descricao, ativa)
VALUES (SQ_ATX_AREA.NEXTVAL, 'Alimentacao',
        'Irradiacao de alimentos para aumentar a durabilidade e a seguranca alimentar.', 'N');
INSERT INTO TB_ATX_AREA (id_area, nome, descricao, ativa)
VALUES (SQ_ATX_AREA.NEXTVAL, 'Patrimonio Cultural',
        'Analises nucleares nao destrutivas que ajudam a preservar obras de arte e fosseis.', 'N');
INSERT INTO TB_ATX_AREA (id_area, nome, descricao, ativa)
VALUES (SQ_ATX_AREA.NEXTVAL, 'Propulsao Naval',
        'A propulsao nuclear de submarinos desenvolvida pelo Programa Nuclear da Marinha.', 'N');
INSERT INTO TB_ATX_AREA (id_area, nome, descricao, ativa)
VALUES (SQ_ATX_AREA.NEXTVAL, 'Espaco',
        'Fontes nucleares de energia que alimentam sondas e missoes espaciais.', 'N');

-- ---------------------------------------------------------------------
-- 5.2 APLICACAO - 10 aplicacoes reais, duas em cada area ativa
-- ---------------------------------------------------------------------
INSERT INTO TB_ATX_APLICACAO (id_aplicacao, id_area, titulo, descricao, beneficio)
VALUES (SQ_ATX_APLICACAO.NEXTVAL,
        (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Saude'),
        'Radioterapia',
        'Feixes de radiacao ionizante sao direcionados ao tumor para danificar o DNA das celulas cancerigenas, poupando ao maximo os tecidos saudaveis ao redor.',
        'Tratamento de cancer usado em boa parte dos pacientes oncologicos, com maior chance de cura e preservacao de orgaos.');
INSERT INTO TB_ATX_APLICACAO (id_aplicacao, id_area, titulo, descricao, beneficio)
VALUES (SQ_ATX_APLICACAO.NEXTVAL,
        (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Saude'),
        'Medicina Nuclear Diagnostica',
        'Radiofarmacos como o tecnecio-99m sao administrados ao paciente e emitem radiacao captada por equipamentos de cintilografia e PET, mostrando o funcionamento dos orgaos.',
        'Diagnostico precoce de doencas cardiacas, cancer e problemas na tireoide, com exames rapidos e pouco invasivos.');
INSERT INTO TB_ATX_APLICACAO (id_aplicacao, id_area, titulo, descricao, beneficio)
VALUES (SQ_ATX_APLICACAO.NEXTVAL,
        (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Agro'),
        'Tecnica do Inseto Esteril',
        'Machos de insetos-praga sao criados em laboratorio e esterilizados com radiacao; ao serem soltos, cruzam com femeas selvagens sem gerar descendentes.',
        'Controle de pragas como a mosca-das-frutas e mosquitos sem uso intensivo de agrotoxicos.');
INSERT INTO TB_ATX_APLICACAO (id_aplicacao, id_area, titulo, descricao, beneficio)
VALUES (SQ_ATX_APLICACAO.NEXTVAL,
        (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Agro'),
        'Melhoramento por Mutacao Induzida',
        'Sementes sao expostas a radiacao para induzir mutacoes, e os pesquisadores selecionam as plantas com caracteristicas desejaveis.',
        'Variedades de plantas mais produtivas e resistentes a secas e doencas.');
INSERT INTO TB_ATX_APLICACAO (id_aplicacao, id_area, titulo, descricao, beneficio)
VALUES (SQ_ATX_APLICACAO.NEXTVAL,
        (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Industria'),
        'Gamagrafia Industrial',
        'Fontes de radiacao gama atravessam pecas metalicas e geram imagens que revelam trincas e falhas internas em soldas e tubulacoes.',
        'Inspecao nao destrutiva que aumenta a seguranca de oleodutos, navios, pontes e aeronaves.');
INSERT INTO TB_ATX_APLICACAO (id_aplicacao, id_area, titulo, descricao, beneficio)
VALUES (SQ_ATX_APLICACAO.NEXTVAL,
        (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Industria'),
        'Esterilizacao por Radiacao Gama',
        'Produtos medicos ja embalados passam por irradiacao gama, que elimina micro-organismos sem aquecer o material nem torna-lo radioativo.',
        'Seringas, luvas e implantes estereis em larga escala e com seguranca.');
INSERT INTO TB_ATX_APLICACAO (id_aplicacao, id_area, titulo, descricao, beneficio)
VALUES (SQ_ATX_APLICACAO.NEXTVAL,
        (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Energia'),
        'Usinas Nucleares de Angra',
        'A fissao do uranio aquece agua e gera vapor que movimenta turbinas, produzindo eletricidade nas usinas de Angra dos Reis (RJ).',
        'Energia eletrica continua com baixissima emissao de gases de efeito estufa durante a geracao.');
INSERT INTO TB_ATX_APLICACAO (id_aplicacao, id_area, titulo, descricao, beneficio)
VALUES (SQ_ATX_APLICACAO.NEXTVAL,
        (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Energia'),
        'Enriquecimento de Uranio por Ultracentrifugacao',
        'Ultracentrifugas giram o uranio em alta velocidade para aumentar a concentracao do isotopo U-235; tecnologia desenvolvida no Brasil pelo Programa Nuclear da Marinha.',
        'Autonomia tecnologica do Brasil na producao do combustivel nuclear.');
INSERT INTO TB_ATX_APLICACAO (id_aplicacao, id_area, titulo, descricao, beneficio)
VALUES (SQ_ATX_APLICACAO.NEXTVAL,
        (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Pesquisa'),
        'Datacao por Carbono-14',
        'A medicao do carbono-14 restante em materiais organicos permite estimar ha quanto tempo o organismo morreu.',
        'Datacao de fosseis e achados arqueologicos de ate cerca de 50 mil anos.');
INSERT INTO TB_ATX_APLICACAO (id_aplicacao, id_area, titulo, descricao, beneficio)
VALUES (SQ_ATX_APLICACAO.NEXTVAL,
        (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Pesquisa'),
        'Reator Multiproposito Brasileiro',
        'Reator de pesquisa em implantacao em Ipero (SP) destinado a producao de radioisotopos, testes de materiais e pesquisa com feixes de neutrons.',
        'Producao nacional de radiofarmacos e menor dependencia de importacao.');

-- ---------------------------------------------------------------------
-- 5.3 PERGUNTA - 10 perguntas de quiz, uma sobre cada aplicacao
-- ---------------------------------------------------------------------
INSERT INTO TB_ATX_PERGUNTA (id_pergunta, id_aplicacao, enunciado, alternativa_a, alternativa_b,
                             alternativa_c, alternativa_d, resposta_correta, pontos)
VALUES (SQ_ATX_PERGUNTA.NEXTVAL,
        (SELECT id_aplicacao FROM TB_ATX_APLICACAO WHERE titulo = 'Radioterapia'),
        'Qual e o principal objetivo da radioterapia?',
        'Destruir ou controlar celulas de tumores', 'Aquecer o corpo do paciente',
        'Gerar energia eletrica no hospital', 'Esterilizar instrumentos cirurgicos',
        'A', 10);
INSERT INTO TB_ATX_PERGUNTA (id_pergunta, id_aplicacao, enunciado, alternativa_a, alternativa_b,
                             alternativa_c, alternativa_d, resposta_correta, pontos)
VALUES (SQ_ATX_PERGUNTA.NEXTVAL,
        (SELECT id_aplicacao FROM TB_ATX_APLICACAO WHERE titulo = 'Medicina Nuclear Diagnostica'),
        'Qual radioisotopo e o mais usado em exames de cintilografia?',
        'Uranio-235', 'Tecnecio-99m',
        'Carbono-14', 'Plutonio-239',
        'B', 15);
INSERT INTO TB_ATX_PERGUNTA (id_pergunta, id_aplicacao, enunciado, alternativa_a, alternativa_b,
                             alternativa_c, alternativa_d, resposta_correta, pontos)
VALUES (SQ_ATX_PERGUNTA.NEXTVAL,
        (SELECT id_aplicacao FROM TB_ATX_APLICACAO WHERE titulo = 'Tecnica do Inseto Esteril'),
        'Na tecnica do inseto esteril, o que a radiacao faz com os insetos machos?',
        'Deixa os insetos maiores', 'Mata todos os insetos',
        'Torna os insetos estereis', 'Muda a cor dos insetos',
        'C', 10);
INSERT INTO TB_ATX_PERGUNTA (id_pergunta, id_aplicacao, enunciado, alternativa_a, alternativa_b,
                             alternativa_c, alternativa_d, resposta_correta, pontos)
VALUES (SQ_ATX_PERGUNTA.NEXTVAL,
        (SELECT id_aplicacao FROM TB_ATX_APLICACAO WHERE titulo = 'Melhoramento por Mutacao Induzida'),
        'O que o melhoramento por mutacao induzida busca obter?',
        'Plantas radioativas', 'Variedades mais produtivas e resistentes',
        'Alimentos mais caros', 'Plantas que nao precisam de agua',
        'B', 15);
INSERT INTO TB_ATX_PERGUNTA (id_pergunta, id_aplicacao, enunciado, alternativa_a, alternativa_b,
                             alternativa_c, alternativa_d, resposta_correta, pontos)
VALUES (SQ_ATX_PERGUNTA.NEXTVAL,
        (SELECT id_aplicacao FROM TB_ATX_APLICACAO WHERE titulo = 'Gamagrafia Industrial'),
        'Para que serve a gamagrafia industrial?',
        'Pintar pecas metalicas', 'Medir a temperatura de motores',
        'Soldar tubulacoes', 'Encontrar falhas internas em soldas sem destruir a peca',
        'D', 20);
INSERT INTO TB_ATX_PERGUNTA (id_pergunta, id_aplicacao, enunciado, alternativa_a, alternativa_b,
                             alternativa_c, alternativa_d, resposta_correta, pontos)
VALUES (SQ_ATX_PERGUNTA.NEXTVAL,
        (SELECT id_aplicacao FROM TB_ATX_APLICACAO WHERE titulo = 'Esterilizacao por Radiacao Gama'),
        'Por que a radiacao gama e usada para esterilizar seringas?',
        'Elimina micro-organismos mesmo com a embalagem fechada', 'Deixa a seringa radioativa',
        'Derrete o plastico', 'Deixa a seringa mais barata',
        'A', 15);
INSERT INTO TB_ATX_PERGUNTA (id_pergunta, id_aplicacao, enunciado, alternativa_a, alternativa_b,
                             alternativa_c, alternativa_d, resposta_correta, pontos)
VALUES (SQ_ATX_PERGUNTA.NEXTVAL,
        (SELECT id_aplicacao FROM TB_ATX_APLICACAO WHERE titulo = 'Usinas Nucleares de Angra'),
        'Qual e uma vantagem ambiental das usinas nucleares durante a geracao de energia?',
        'Nao precisam de agua', 'Quase nao emitem gases de efeito estufa',
        'Funcionam apenas com sol', 'Nao produzem nenhum residuo',
        'B', 20);
INSERT INTO TB_ATX_PERGUNTA (id_pergunta, id_aplicacao, enunciado, alternativa_a, alternativa_b,
                             alternativa_c, alternativa_d, resposta_correta, pontos)
VALUES (SQ_ATX_PERGUNTA.NEXTVAL,
        (SELECT id_aplicacao FROM TB_ATX_APLICACAO WHERE titulo = 'Enriquecimento de Uranio por Ultracentrifugacao'),
        'Qual tecnologia de enriquecimento de uranio foi desenvolvida pelo Programa Nuclear da Marinha?',
        'Difusao gasosa', 'Laser',
        'Ultracentrifugacao', 'Fusao a frio',
        'C', 25);
INSERT INTO TB_ATX_PERGUNTA (id_pergunta, id_aplicacao, enunciado, alternativa_a, alternativa_b,
                             alternativa_c, alternativa_d, resposta_correta, pontos)
VALUES (SQ_ATX_PERGUNTA.NEXTVAL,
        (SELECT id_aplicacao FROM TB_ATX_APLICACAO WHERE titulo = 'Datacao por Carbono-14'),
        'Ate aproximadamente quantos anos a datacao por carbono-14 consegue estimar?',
        '500 anos', '5 mil anos',
        '50 mil anos', '5 milhoes de anos',
        'C', 20);
INSERT INTO TB_ATX_PERGUNTA (id_pergunta, id_aplicacao, enunciado, alternativa_a, alternativa_b,
                             alternativa_c, alternativa_d, resposta_correta, pontos)
VALUES (SQ_ATX_PERGUNTA.NEXTVAL,
        (SELECT id_aplicacao FROM TB_ATX_APLICACAO WHERE titulo = 'Reator Multiproposito Brasileiro'),
        'Qual e um dos objetivos do Reator Multiproposito Brasileiro?',
        'Gerar energia para Sao Paulo', 'Produzir radioisotopos para a medicina',
        'Mover submarinos', 'Dessalinizar a agua do mar',
        'B', 25);

-- ---------------------------------------------------------------------
-- 5.4 JOGADOR - 10 jogadores de teste
--     senha = hash BCrypt de "Atomix@2026" (o banco nunca guarda a senha em
--     texto puro; nas APIs o hash e gerado pela aplicacao)
-- ---------------------------------------------------------------------
INSERT INTO TB_ATX_JOGADOR (id_jogador, nome, email, senha, pontuacao)
VALUES (SQ_ATX_JOGADOR.NEXTVAL, 'Ana Beatriz Souza', 'ana.souza@email.com',
        '$2a$10$Rf47dja5/Bh8eqHbI/81/O8cB6QwjUNyoN/KPkMq6p4nYYVv9zkXe', 1050);
INSERT INTO TB_ATX_JOGADOR (id_jogador, nome, email, senha, pontuacao)
VALUES (SQ_ATX_JOGADOR.NEXTVAL, 'Bruno Henrique Lima', 'bruno.lima@email.com',
        '$2a$10$kq4FLBoQog12iGwQvbpxGOaRG1XL52BwJc6mVsNTo972M4ihKZX..', 820);
INSERT INTO TB_ATX_JOGADOR (id_jogador, nome, email, senha, pontuacao)
VALUES (SQ_ATX_JOGADOR.NEXTVAL, 'Carla Mendes Rocha', 'carla.rocha@email.com',
        '$2a$10$bsUcYD.zzlSf3AUCWHyKSOqvVz1AmQMAGgRfCK5tgvll2xT7/dmt.', 640);
INSERT INTO TB_ATX_JOGADOR (id_jogador, nome, email, senha, pontuacao)
VALUES (SQ_ATX_JOGADOR.NEXTVAL, 'Diego Ferreira Alves', 'diego.alves@email.com',
        '$2a$10$G3HzkjpBXYgi/8z6LJZAu.FBw49pTOCnRyt2SKpDcqvh1CDu/BdZ.', 510);
INSERT INTO TB_ATX_JOGADOR (id_jogador, nome, email, senha, pontuacao)
VALUES (SQ_ATX_JOGADOR.NEXTVAL, 'Eduarda Costa Nunes', 'eduarda.nunes@email.com',
        '$2a$10$9gFNE8Bba5DrTzVrK5PgjOJita7zAC6iFuLXmNWmhQwgSjM4Siod2', 430);
INSERT INTO TB_ATX_JOGADOR (id_jogador, nome, email, senha, pontuacao)
VALUES (SQ_ATX_JOGADOR.NEXTVAL, 'Felipe Araujo Martins', 'felipe.martins@email.com',
        '$2a$10$HeeP60zoMnevFSqevkM7TelDpCjsCQl2/tfZWcF0szzBc3d4G5g4K', 320);
INSERT INTO TB_ATX_JOGADOR (id_jogador, nome, email, senha, pontuacao)
VALUES (SQ_ATX_JOGADOR.NEXTVAL, 'Gabriela Ribeiro Dias', 'gabriela.dias@email.com',
        '$2a$10$HcAZEC9lJd4uj.1kMiawaOgloCX0Z3ZxCxcQr9fxxQtMq8MCbsvxa', 260);
INSERT INTO TB_ATX_JOGADOR (id_jogador, nome, email, senha, pontuacao)
VALUES (SQ_ATX_JOGADOR.NEXTVAL, 'Henrique Santos Pires', 'henrique.pires@email.com',
        '$2a$10$RUuKI5N/6M4ZFsu22M7XEu55vzgClGesENGBpNIESRvVRzYSeFheK', 150);
INSERT INTO TB_ATX_JOGADOR (id_jogador, nome, email, senha, pontuacao)
VALUES (SQ_ATX_JOGADOR.NEXTVAL, 'Isabela Moraes Teixeira', 'isabela.teixeira@email.com',
        '$2a$10$PxD6Zd8IZIWbhFglf5pyseSRsjgufm2/PdOVjP/zFgcDScDQtosLm', 90);
INSERT INTO TB_ATX_JOGADOR (id_jogador, nome, email, senha, pontuacao)
VALUES (SQ_ATX_JOGADOR.NEXTVAL, 'Joao Pedro Cardoso', 'joao.cardoso@email.com',
        '$2a$10$RPXc3iEOOjM4TybDasNm1.5nN4W/7hWX5mkl/B0.54WF.IJGVje2u', 30);

-- ---------------------------------------------------------------------
-- 5.5 CONQUISTA - 10 conquistas; id_area NULL = conquista geral
-- ---------------------------------------------------------------------
INSERT INTO TB_ATX_CONQUISTA (id_conquista, id_area, nome, descricao, pontos_necessarios)
VALUES (SQ_ATX_CONQUISTA.NEXTVAL, NULL,
        'Primeiro Atomo', 'Criou a conta e comecou a explorar a cidade nuclear.', 0);
INSERT INTO TB_ATX_CONQUISTA (id_conquista, id_area, nome, descricao, pontos_necessarios)
VALUES (SQ_ATX_CONQUISTA.NEXTVAL, (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Saude'),
        'Explorador da Saude', 'Descobriu como a radiacao salva vidas na medicina.', 50);
INSERT INTO TB_ATX_CONQUISTA (id_conquista, id_area, nome, descricao, pontos_necessarios)
VALUES (SQ_ATX_CONQUISTA.NEXTVAL, (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Agro'),
        'Guardiao do Agro', 'Aprendeu como a tecnologia nuclear protege as lavouras.', 100);
INSERT INTO TB_ATX_CONQUISTA (id_conquista, id_area, nome, descricao, pontos_necessarios)
VALUES (SQ_ATX_CONQUISTA.NEXTVAL, (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Industria'),
        'Engenheiro da Industria', 'Dominou as aplicacoes nucleares na industria.', 150);
INSERT INTO TB_ATX_CONQUISTA (id_conquista, id_area, nome, descricao, pontos_necessarios)
VALUES (SQ_ATX_CONQUISTA.NEXTVAL, (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Energia'),
        'Energia Limpa', 'Entendeu como a energia nuclear gera eletricidade com baixa emissao.', 200);
INSERT INTO TB_ATX_CONQUISTA (id_conquista, id_area, nome, descricao, pontos_necessarios)
VALUES (SQ_ATX_CONQUISTA.NEXTVAL, (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Pesquisa'),
        'Mente Cientifica', 'Explorou os reatores de pesquisa e a datacao de materiais.', 300);
INSERT INTO TB_ATX_CONQUISTA (id_conquista, id_area, nome, descricao, pontos_necessarios)
VALUES (SQ_ATX_CONQUISTA.NEXTVAL, NULL,
        'Mestre do Quiz', 'Acumulou pontos respondendo os quizzes de todas as areas.', 400);
INSERT INTO TB_ATX_CONQUISTA (id_conquista, id_area, nome, descricao, pontos_necessarios)
VALUES (SQ_ATX_CONQUISTA.NEXTVAL, NULL,
        'Cidadao Nuclear', 'Conhece as principais aplicacoes nucleares do dia a dia.', 500);
INSERT INTO TB_ATX_CONQUISTA (id_conquista, id_area, nome, descricao, pontos_necessarios)
VALUES (SQ_ATX_CONQUISTA.NEXTVAL, (SELECT id_area FROM TB_ATX_AREA WHERE nome = 'Propulsao Naval'),
        'Submarinista', 'Conheceu o Programa Nuclear da Marinha e a propulsao naval.', 750);
INSERT INTO TB_ATX_CONQUISTA (id_conquista, id_area, nome, descricao, pontos_necessarios)
VALUES (SQ_ATX_CONQUISTA.NEXTVAL, NULL,
        'Mestre Nuclear', 'Alcancou o nivel maximo do ATOMIX.', 1000);

-- ---------------------------------------------------------------------
-- 5.6 JOGADOR_CONQUISTA - desbloqueios
--     Aplica a regra do jogo: cada jogador tem todas as conquistas cuja
--     pontuacao minima ja alcancou (pontos_necessarios <= pontuacao).
--     O INSERT ... SELECT com JOIN gera os registros de uma vez.
-- ---------------------------------------------------------------------
INSERT INTO TB_ATX_JOGADOR_CONQUISTA (id_jogador, id_conquista, dt_desbloqueio)
SELECT j.id_jogador, c.id_conquista, SYSDATE
  FROM TB_ATX_JOGADOR j
 INNER JOIN TB_ATX_CONQUISTA c
    ON c.pontos_necessarios <= j.pontuacao;

COMMIT;

-- ---------------------------------------------------------------------
-- 6. CONFERENCIA - quantidade de registros por tabela (minimo de 5)
-- ---------------------------------------------------------------------
SELECT 'TB_ATX_AREA' AS tabela, COUNT(*) AS registros FROM TB_ATX_AREA
UNION ALL SELECT 'TB_ATX_APLICACAO', COUNT(*) FROM TB_ATX_APLICACAO
UNION ALL SELECT 'TB_ATX_PERGUNTA', COUNT(*) FROM TB_ATX_PERGUNTA
UNION ALL SELECT 'TB_ATX_JOGADOR', COUNT(*) FROM TB_ATX_JOGADOR
UNION ALL SELECT 'TB_ATX_CONQUISTA', COUNT(*) FROM TB_ATX_CONQUISTA
UNION ALL SELECT 'TB_ATX_JOGADOR_CONQUISTA', COUNT(*) FROM TB_ATX_JOGADOR_CONQUISTA;


-- #####################################################################
--  PARTE 3 - CONSULTAS COM JOIN
-- #####################################################################

-- ---------------------------------------------------------------------
-- CONSULTA 1 - "Que conteudo cada area da cidade oferece ao jogador?"
--   Para cada area ativa: quantas aplicacoes ela apresenta, quantas
--   perguntas de quiz existem e quantos pontos o quiz da area vale.
--   INNER JOIN AREA x APLICACAO e LEFT JOIN com PERGUNTA (uma aplicacao
--   ainda sem pergunta continuaria aparecendo, com 0 perguntas).
-- ---------------------------------------------------------------------
SELECT a.nome                          AS area,
       COUNT(DISTINCT ap.id_aplicacao) AS aplicacoes,
       COUNT(p.id_pergunta)            AS perguntas,
       NVL(SUM(p.pontos), 0)           AS pontos_quiz
  FROM TB_ATX_AREA a
 INNER JOIN TB_ATX_APLICACAO ap ON ap.id_area      = a.id_area
  LEFT JOIN TB_ATX_PERGUNTA  p  ON p.id_aplicacao  = ap.id_aplicacao
 WHERE a.ativa = 'S'
 GROUP BY a.nome
 ORDER BY a.nome;

-- ---------------------------------------------------------------------
-- CONSULTA 2 - "Quais conquistas cada jogador ja desbloqueou e em que
--               nivel ele esta?" (o progresso exibido no perfil do app)
--   INNER JOIN JOGADOR x JOGADOR_CONQUISTA x CONQUISTA, com COUNT e
--   LISTAGG por jogador e o nivel calculado pela pontuacao.
-- ---------------------------------------------------------------------
SELECT j.nome                AS jogador,
       j.pontuacao,
       CASE
           WHEN j.pontuacao >= 1000 THEN 'Mestre Nuclear'
           WHEN j.pontuacao >= 600  THEN 'Especialista CTMSP'
           WHEN j.pontuacao >= 300  THEN 'Cientista do Atomo'
           WHEN j.pontuacao >= 100  THEN 'Explorador Nuclear'
           ELSE 'Aprendiz Atomico'
       END                   AS nivel,
       COUNT(c.id_conquista) AS qtd_conquistas,
       LISTAGG(c.nome, ', ') WITHIN GROUP (ORDER BY c.pontos_necessarios) AS conquistas
  FROM TB_ATX_JOGADOR j
 INNER JOIN TB_ATX_JOGADOR_CONQUISTA jc ON jc.id_jogador   = j.id_jogador
 INNER JOIN TB_ATX_CONQUISTA         c  ON c.id_conquista  = jc.id_conquista
 GROUP BY j.id_jogador, j.nome, j.pontuacao
 ORDER BY j.pontuacao DESC, j.nome;

-- Fim do script
