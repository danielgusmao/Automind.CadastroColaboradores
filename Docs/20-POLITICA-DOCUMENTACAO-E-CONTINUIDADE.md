# Politica de documentacao e continuidade

## Regra vigente

- `Docs/CHECKPOINT.md` e o unico checkpoint cumulativo, com informacoes novas no topo.
- documentos tematicos evoluem no proprio arquivo; nao criar copias por versao.
- arquivos redundantes podem ser removidos depois que todo o conteudo relevante for incorporado ao checkpoint ou documento tematico correto.
- preservar **informacao**, nao quantidade de arquivos.
- nunca reduzir o nivel de contexto necessario para handoff a humano ou outra LLM.
- registrar pedidos, decisoes, codigo/configuracao, testes, resultados, versoes, comandos relevantes, AD/IIS/gMSA, grupos/OUs/DNs, rollback, Git/deploy, pendencias e itens proibidos.
- em conflito entre secao historica e estado atual, a secao mais nova explicitamente marcada como vigente prevalece.

## Antes de entregar um novo pacote

1. partir da ultima versao completa;
2. atualizar `CHECKPOINT.md` e `CONTEXTO-ATUAL.md`;
3. evoluir os documentos tematicos afetados;
4. consolidar/remover redundancias somente apos incorporar o conteudo;
5. regenerar `DOC-MANIFEST-SHA256.txt`;
6. confirmar que o pacote permite continuidade sem depender do chat.
