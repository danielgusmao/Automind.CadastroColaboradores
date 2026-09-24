# POLITICA ATUAL - DOCUMENTACAO UNIFICADA E EVOLUTIVA

Data: 24/09/2026. Esta secao prevalece sobre orientacoes anteriores em caso de conflito.

- `Docs/CHECKPOINT.md` e o **unico checkpoint cumulativo** do projeto;
- secoes novas entram no topo, com divisao clara por versao/interacao;
- nao criar checkpoint individual por versao;
- documentos historicos redundantes podem ser removidos **depois** de seu conteudo ser incorporado ao checkpoint unico ou ao documento tematico correspondente;
- o requisito e preservar **informacao**, nao a quantidade de arquivos;
- documentos tematicos devem ser evoluidos/consolidados, evitando duplicacao desnecessaria;
- outra LLM ou humano deve conseguir reconstruir estado, decisoes, testes, resultados, seguranca, rollback, Git/release, pendencias e proximos passos apenas com a documentacao do pacote.

---

# ATUALIZACAO DE POLITICA - CHECKPOINT UNICO CUMULATIVO

Data: 24/09/2026. Esta secao prevalece sobre regras antigas abaixo quando houver conflito.

- `Docs/CHECKPOINT.md` passa a ser o checkpoint principal e cumulativo.
- novas informacoes entram sempre no topo, divididas por versao/interacao;
- nao criar novos arquivos individuais em `Docs/Checkpoints/` a cada versao;
- arquivos individuais historicos ja existentes permanecem preservados e nao devem ser apagados;
- nunca reduzir documentacao: apenas acrescentar contexto, evidencias, decisoes, testes, resultados e pendencias;
- o pacote deve continuar compreensivel por outra LLM ou humano sem depender do chat.

---

# Politica de documentacao e continuidade

Data de consolidacao: 24/09/2026.

## Regra obrigatoria

Documentacao e checkpoint sao parte do produto e do pacote de release/teste. Eles nao sao artefatos descartaveis.

Em toda nova versao:

1. partir do pacote mais novo e completo;
2. preservar integralmente todos os documentos existentes;
3. adicionar novas decisoes, resultados, alteracoes e pendencias;
4. nunca apagar checkpoints antigos por causa de consolidacao;
5. manter `Docs/CHECKPOINT.md` atualizado;
6. manter `Docs/CP-HIST.md` como consolidacao cumulativa;
7. manter `Docs/Checkpoints/` com os checkpoints individuais;
8. manter um contexto atual de handoff para outro humano/LLM;
9. registrar mudancas funcionais e tambem mudancas operacionais relevantes;
10. antes de entregar o ZIP, comparar o inventario de documentos com a versao anterior e falhar a entrega se algum documento anterior tiver desaparecido sem autorizacao explicita.

## O que deve ser documentado

- pedidos e correcoes do responsavel;
- funcionalidades aprovadas e rejeitadas;
- alteracoes de codigo e configuracao;
- versoes/pacotes;
- comandos de teste relevantes e onde executa-los;
- resultados reais dos testes;
- decisoes de seguranca;
- regras de AD/IIS/gMSA;
- baselines, hashes e rollbacks conhecidos;
- tabelas, campos, OUs, grupos e DNs relevantes;
- Git, branch, remotes e fluxo de deploy;
- investigacoes e conclusoes;
- pendencias e proximos passos;
- itens que nao devem ser alterados;
- mudancas de prioridade ou de interpretacao do escopo.

## Consolidacao nao significa exclusao

`Docs/CP-HIST.md` pode facilitar leitura e reduzir dependencia de caminhos longos, mas e um indice/consolidado adicional. Ele nao substitui nem autoriza apagar `Docs/Checkpoints/`.

Quando houver necessidade de nomes curtos por limitacao do Windows, reduzir nomes de novos arquivos sem remover conteudo historico. Se necessario reorganizar arquivos antigos, preservar integralmente o conteudo e registrar o mapeamento.
