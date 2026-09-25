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

## Estado resumido - v0.1.12

- fase Active Directory do piloto concluida;
- Microsoft Graph App-only por certificado validado;
- `lucas.costa` recebeu e teve removida Microsoft 365 Business Standard com sucesso; estado final: 0 licencas, `UsageLocation=BR`;
- tela `Novo colaborador` passa a exibir inventario real de licencas/quantidades;
- `LicenseWritesEnabled=false`: nenhuma escrita M365 integrada ao sistema nesta versao.

Proximo gate: build/deploy da v0.1.12 e validacao visual do inventario M365 no servidor `10.1.2.21`.
