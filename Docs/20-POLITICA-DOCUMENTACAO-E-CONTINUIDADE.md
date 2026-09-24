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
