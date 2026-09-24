# CadColab - contexto atual para continuidade

Versao do pacote de continuidade: `0.1.5`
Data: 24/09/2026
Projeto: `Automind.CadastroColaboradores`

## Regra de precedencia

Este arquivo descreve o estado operacional mais recente. Historicos antigos devem ser preservados, mas, em caso de conflito sobre o estado atual, prevalecem este arquivo e o checkpoint mais novo.

## Estado atual

- ultimo codigo funcional: linha `v0.1.4`;
- pacote atual `v0.1.5` apenas restaura/expande documentacao e preserva o codigo funcional da `v0.1.4`;
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
2. excluir o proprio colaborador em cadastro da coorte por `sAMAccountName`;
3. grupos 100% da coorte -> **Comuns ao cargo**, marcados automaticamente;
4. grupos parciais -> **Excecoes encontradas**, desmarcados;
5. campo **Outros grupos** -> busca manual/autocomplete no AD;
6. backend revalida todos os grupos escolhidos e bloqueia protegidos;
7. efeitos indiretos/transitivos permanecem visiveis;
8. enquanto `GroupWritesEnabled=false`, nenhuma membership e gravada.

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

Nunca remover documentacao em novas versoes. Todo ZIP deve conter:

- checkpoint corrente;
- historico consolidado;
- checkpoints individuais anteriores;
- decisoes;
- regras de seguranca e rollback;
- fluxo Git/release;
- registro das interacoes relevantes;
- contexto atual para handoff.

Antes de entregar novo pacote, comparar inventario de documentos com a versao anterior.

## Proximo gate tecnico

A `v0.1.4` funcional ainda precisa do build local .NET 10 na maquina do Visual Studio e dos testes visuais/funcionais da nova selecao de grupos. Escrita real de memberships continua fora do escopo ate delegacao/allowlist de grupos e autorizacao explicita posterior.
