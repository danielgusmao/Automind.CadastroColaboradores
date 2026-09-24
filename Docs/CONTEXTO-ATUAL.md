# ATUALIZACAO v0.1.8 - 24/09/2026

- Target framework agora `net10.0-windows` para refletir o destino real IIS/Windows/AD e eliminar CA1416 corretamente.
- Build v0.1.7 recebido: 0 erros, 15 CA1416; proximo build esperado: 0 CA1416.
- Checkpoint historico agora e unico: `Docs/CHECKPOINT.md`, mais novo primeiro.
- Checkpoints/CP-HIST/contexto historico redundantes foram incorporados ao checkpoint unico e removidos como arquivos separados.
- Estado funcional permanece: PilotWrite, somente 07.Outros, GroupWritesEnabled=false, sem memberships reais.

---

# CadColab - contexto atual para continuidade

Versao do pacote de continuidade: `0.1.7`
Data: 24/09/2026
Projeto: `Automind.CadastroColaboradores`

## Regra de precedencia

Este arquivo descreve o estado operacional mais recente. Historicos antigos devem ser preservados, mas, em caso de conflito sobre o estado atual, prevalecem este arquivo e o checkpoint mais novo.

## Estado atual

- codigo funcional atual: `v0.1.7`;
- build local da v0.1.6 foi confirmado com 0 erros e 15 avisos CA1416 esperados;
- a v0.1.7 exclui da coorte de referencia usuarios localizados em `SuggestionExcludedOuDns`; atualmente `07.Outros`, impedindo que contas piloto/teste distorcam novos perfis;
- a exclusao por sAMAccountName e CN + OU da v0.1.6 permanece como defesa adicional;
- `Automind:Mode=PilotWrite`;
- criacao real permitida somente em `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- App Pool `CadastroColaboradores` roda como `AUTOMIND\gMSA_CadColab$`;
- `GroupWritesEnabled=false`;
- `Microsoft365.Enabled=false`;
- `proxyAddresses` e `pwdLastSet` continuam fora da escrita;
- usuario ficticio `teste.cadcolab` ja existe e foi criado com sucesso;
- nenhuma membership de negocio foi gravada nesse usuario;
- auditoria de provisionamento foi confirmada;
- nao excluir/recriar o usuario sem autorizacao explicita.

## Selecionador de grupos

Objetivo aprovado:

1. buscar usuarios ativos com mesmo `Title + Department` em todo o escopo de leitura do AD;
2. excluir da coorte as OUs configuradas em `SuggestionExcludedOuDns` (atualmente `07.Outros`);
3. excluir o proprio colaborador em cadastro por `sAMAccountName` e, quando a OU estiver selecionada, tambem por `CN + OU`;
4. grupos 100% da coorte -> **Comuns ao cargo**, marcados automaticamente;
5. grupos parciais -> **Excecoes encontradas**, desmarcados;
6. campo **Outros grupos** -> busca manual/autocomplete no AD;
7. backend revalida todos os grupos escolhidos e bloqueia protegidos;
8. efeitos indiretos/transitivos permanecem visiveis;
9. enquanto `GroupWritesEnabled=false`, nenhuma membership e gravada.

Perfil de teste atual: `Automation Systems Analyst + ENGENHARIA`, com 5 usuarios de referencia e 8 grupos comuns 5/5.

## Escopo de 07.Outros

`07.Outros` e a OU piloto de escrita do usuario, nao a fonte exclusiva das referencias de cargo. Para inteligencia/sugestao, consultar outras OUs em modo somente leitura e usar usuarios equivalentes reais.

O grupo `_CriaMovePastas` existente em `07.Outros` pertence ao legado e nao deve ser reutilizado.

## Regras operacionais

- testes somente leitura/simulacao no mesmo local: consolidar em uma unica linha PowerShell quando pratico;
- alteracao real: uma por vez;
- antes de qualquer alteracao real: comando, efeito esperado, rollback e validacao do rollback;
- indicar sempre onde executar: AD, servidor `10.1.2.21` ou maquina do Visual Studio;
- PowerShell enviado ao responsavel em uma unica linha;
- nao usar `iisreset`;
- nao reiniciar DC/AD/KDC/Netlogon/servidor por causa do projeto;
- nao restaurar ACL completa cegamente;
- parar no primeiro resultado inesperado;
- nao apagar usuario de teste automaticamente;
- M365 permanece pausado;
- `_informatica` e autorizacao humana, nao principal tecnico de escrita.

## Documentacao - regra primordial

Nunca remover documentacao em novas versoes. O checkpoint principal e `Docs/CHECKPOINT.md`, cumulativo, com as informacoes mais novas sempre no topo. Nao criar novos arquivos individuais de checkpoint. Os arquivos historicos individuais ja existentes permanecem preservados. Todo ZIP deve conter:

- checkpoint cumulativo corrente;
- historico/documentacao anterior preservados;
- decisoes;
- regras de seguranca e rollback;
- fluxo Git/release;
- registro das interacoes relevantes;
- contexto atual para handoff.

Antes de entregar novo pacote, comparar inventario de documentos com a versao anterior.

## Proximo gate tecnico

Build/deploy da `v0.1.7` e um unico teste funcional limpo: 8 grupos comuns `5/5` em verde, 4 excecoes, busca manual em `Outros grupos` e pre-validacao. Nao excluir o usuario piloto. Ao concluir, encerrar a fase de descoberta/selecao e abrir a fase separada de escrita de memberships.
