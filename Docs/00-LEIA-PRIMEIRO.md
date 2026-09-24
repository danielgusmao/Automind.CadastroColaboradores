# LEIA PRIMEIRO - continuidade integral do CadColab

Este pacote foi preparado para permitir continuidade por outra pessoa ou LLM sem depender do chat original.

## Regra primordial de documentacao

A documentacao do projeto e cumulativa.

- Nunca remover checkpoints, decisoes, resultados de testes ou documentos anteriores ao gerar nova versao.
- Nunca substituir o historico detalhado por um resumo menor.
- Resumos e consolidacoes podem ser adicionados, mas nunca usados para apagar os arquivos historicos originais.
- Toda interacao relevante deve atualizar o checkpoint corrente e, quando aplicavel, o historico detalhado.
- Antes de entregar novo ZIP, comparar com o pacote anterior e confirmar que nenhum documento anterior desapareceu.
- O pacote completo deve ser autossuficiente para humanos e LLMs: estado atual, historico, regras, arquitetura, testes, resultados, seguranca, Git/release, pendencias e proximos passos.

## Onde ler

1. `Docs/CONTEXTO-ATUAL.md` - estado operacional mais recente e proximos passos.
2. `Docs/CHECKPOINT.md` - checkpoint corrente acumulado.
3. `Docs/CP-HIST.md` - historico consolidado dos checkpoints.
4. `Docs/Checkpoints/` - arquivos individuais dos checkpoints historicos; preservados novamente a partir desta versao.
5. `Docs/21-REGISTRO-INTERACOES-24-09-2026.md` - registro das interacoes e correcoes recentes que levaram ao estado atual.
6. `Docs/03-DECISOES.md` - decisoes tecnicas e operacionais.
7. `Docs/17-ROLLBACK-AD.md` - regras e procedimentos de rollback.
8. `Docs/10-FLUXO-GIT-E-PUBLICACAO.md` - fluxo de versao, Git e publicacao.

## Fonte historica restaurada

O pacote `Automind.CadastroColaboradores-23-09-2026-checkpoint-v0.0.50.zip` continha arquivos individuais de checkpoint ate V054, apesar do nome externo do ZIP. Esses arquivos foram restaurados em `Docs/Checkpoints/`.

Depois de V054 existiram checkpoints mais novos: V055, V056, V057, V058, V059, V060 e V061. Eles foram preservados no historico consolidado e agora tambem existem como arquivos individuais.

A partir desta versao, a documentacao nunca mais deve diminuir.
