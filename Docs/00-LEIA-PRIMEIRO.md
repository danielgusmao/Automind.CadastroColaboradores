# LEIA PRIMEIRO - continuidade do CadColab

Este pacote e autossuficiente para continuidade por humano ou LLM.

## Ordem de leitura

1. `Docs/CONTEXTO-ATUAL.md` - estado operacional e proximo teste.
2. `Docs/CHECKPOINT.md` - historico cumulativo, mais novo primeiro.
3. `Docs/07-MICROSOFT-365.md` - identidade Graph, testes reais e rollback.
4. `Docs/03-DECISOES.md` - decisoes tecnicas e operacionais.
5. `Docs/16-CONTRATO-ESCRITA-AD.md` - contrato vigente de escrita no AD.
6. `Docs/17-ROLLBACK-AD.md` - rollback AD.
7. `Docs/10-FLUXO-GIT-E-PUBLICACAO.md` - build, Git e deploy.

## Regra documental

- `Docs/CHECKPOINT.md` e o unico checkpoint cumulativo.
- documentos tematicos evoluem no proprio arquivo; nao criar copias por versao;
- consolidar/remover redundancia somente depois de incorporar todo o conteudo relevante;
- preservar informacao, nao quantidade de arquivos;
- informacoes mais novas prevalecem quando uma secao historica descrever estado antigo.

## Estado resumido - v0.1.13

- fase Active Directory do piloto concluida;
- Microsoft Graph App-only por certificado validado;
- v0.1.12 foi publicada/testada e o inventario M365 apareceu corretamente no servidor;
- teste manual real de `UsageLocation=BR` + Business Standard + remocao concluido em `lucas.costa`;
- v0.1.13 habilita selecao de SKUs disponiveis e atribuicao M365 controlada apos o usuario sincronizar com o Entra;
- `LicenseWritesEnabled=true` somente dentro do escopo piloto e com revalidacao backend;
- rollback automatico remove somente licencas adicionadas pela tentativa.

Proximo gate: build/deploy da v0.1.13 e teste ponta a ponta com um NOVO usuario piloto em `07.Outros`.
