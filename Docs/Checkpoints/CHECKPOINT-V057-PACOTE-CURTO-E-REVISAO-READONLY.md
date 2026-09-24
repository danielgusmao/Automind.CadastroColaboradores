# CHECKPOINT V057 - Pacote curto/versionado e revisao da tela ReadOnly

Data: 24/09/2026.

Nova regra de distribuicao:

- pacote completo atual passa a `CadColab-v0.1.0.zip`;
- proximos ajustes desta linha: `v0.1.1`, `v0.1.2`, ...;
- `VERSION.txt` registra a versao;
- nao criar tag Git automaticamente; manter commits na branch `release`;
- historico detalhado dos checkpoints foi consolidado em `Docs/CP-HIST.md` para reduzir caminhos longos;
- codigo funcional nao foi alterado nesta rodada de reorganizacao/nomenclatura.

Revisao da tela publicada em 24/09/2026:

- banner confirma `AD REAL - SOMENTE LEITURA` e `Automind:Mode=ReadOnly`;
- chamado `I2609-0295` carregado;
- OU selecionada `07.Outros`;
- sugestao de grupos retornou 8 comuns, 4 excecoes e 0 protegidos;
- o piloto continua sem gravacao de memberships;
- pre-validacao apresentou uma unica pendencia: `Login valido e disponivel`;
- o login de teste exibido e `teste.provisionamento`, que ultrapassa o limite de 20 caracteres do `sAMAccountName` ja validado pelo projeto;
- nenhuma escrita foi executada.

Antes de ativar `PilotWrite`, fechar a pre-validacao com login de ate 20 caracteres e revisar o comportamento dos checkboxes de grupos, pois a tela mostra os grupos comuns selecionados mesmo com escrita de memberships desabilitada.
