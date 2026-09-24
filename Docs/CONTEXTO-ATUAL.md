# CadColab - contexto atual para continuidade

Versao em preparacao: `0.1.10`  
Data: 24/09/2026  
Projeto: `Automind.CadastroColaboradores`

## Estado operacional atual

- ultimo build confirmado pelo responsavel: v0.1.8/v0.1.9 base em `net10.0-windows`, 0 erros e 0 warnings;
- `Automind:Mode=PilotWrite`;
- criacao real de usuario limitada a `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- App Pool `CadastroColaboradores` executa como `AUTOMIND\gMSA_CadColab$`;
- usuario piloto existente `teste.cadcolab` foi criado pelo CadColab e permanece em `07.Outros`;
- sugestao/seleção de grupos validada: 8 comuns 5/5, 4 excecoes, busca manual e bloqueio de grupo protegido;
- coorte de sugestao exclui `07.Outros` para contas piloto nao distorcerem as frequencias;
- teste real de membership concluido com sucesso: `teste.cadcolab` entrou em `_CriaMovePastas` pela propria aplicacao;
- ADUC confirmou a membership tanto em `Members` do grupo quanto em `Member Of` do usuario;
- ACE `WriteProperty(member)` para `SG_CadastroColaboradores_AD_Writer` no grupo `_CriaMovePastas` esta funcional;
- v0.1.10 evolui para `GroupWritesEnabled=true`, mas somente para `GroupWriteAllowedDns`;
- allowlist de grupos atual: somente `CN=_CriaMovePastas,OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- grupos fora dessa allowlist podem continuar aparecendo como sugestao, mas bloqueiam a pre-validacao se permanecerem selecionados para escrita;
- Microsoft 365, Teams, `proxyAddresses` e `pwdLastSet` continuam fora da escrita.

## Fluxo de escrita v0.1.10

1. operador autorizado confirma a operacao;
2. backend repete toda pre-validacao;
3. exige OU na allowlist de escrita;
4. exige cada grupo selecionado na `GroupWriteAllowedDns`;
5. exige identidade tecnica `AUTOMIND\gMSA_CadColab$`;
6. inicia auditoria antes da primeira escrita;
7. cria usuario desabilitado;
8. grava atributos;
9. define senha;
10. grava manager;
11. adiciona memberships autorizadas no atributo `member` dos grupos;
12. releitura confirma memberships;
13. releitura confirma usuario ainda desabilitado e atributos;
14. habilita a conta por ultimo;
15. releitura final e auditoria de conclusao.

Em falha depois de inclusoes de grupo, o rollback remove somente memberships efetivamente adicionadas pela operacao atual e depois tenta manter a conta desabilitada. Nao existe exclusao automatica de usuario.

## Regra de sugestao de grupos

1. pesquisar usuarios ativos por `Title + Department` no escopo de leitura;
2. excluir OUs configuradas em `SuggestionExcludedOuDns` (atualmente `07.Outros`) da coorte;
3. excluir o proprio colaborador por login/CN quando aplicavel;
4. 100% da coorte -> `Comum ao cargo`;
5. parcial -> `Excecao`;
6. `Outros grupos` permite pesquisa manual no AD;
7. grupos protegidos nao podem ser selecionados;
8. efeitos indiretos continuam visiveis.

## Regras operacionais

- testes somente leitura/simulacao no mesmo local podem ser agrupados em uma linha PowerShell;
- alteracao real: uma por vez;
- antes da escrita real: comando, efeito, rollback e validacao;
- sempre indicar local de execucao;
- nao usar `iisreset`;
- nao reiniciar AD/DC/KDC/Netlogon/servidor por causa do projeto;
- nao restaurar ACL inteira cegamente;
- nao excluir usuario de teste sem autorizacao separada;
- `_informatica` e autorizacao humana, nao identidade tecnica;
- CriaMovePastas e sistema legado separado; apenas o objeto grupo `_CriaMovePastas` foi autorizado como alvo de teste.

## Documentacao

- `Docs/CHECKPOINT.md` e o checkpoint historico cumulativo, mais novo primeiro;
- nao criar checkpoint por versao;
- documentos tematicos sao evoluidos no mesmo arquivo;
- consolidar/remover redundancia somente depois de incorporar integralmente o conteudo;
- nunca reduzir informacao documental.

## Proximo gate

1. build da v0.1.10;
2. publicar pela branch `release`;
3. usar um novo usuario de teste com CN/login/UPN livres em `07.Outros`;
4. deixar desmarcados grupos comuns fora da allowlist;
5. adicionar manualmente `_CriaMovePastas`;
6. pre-validar;
7. criar o usuario;
8. confirmar atributos, conta habilitada, membership e auditoria;
9. manter grupos de producao fora da allowlist nesta fase.

## Historico

Todo o historico de decisoes, comandos, resultados, rollbacks, versoes e investigacoes permanece em `Docs/CHECKPOINT.md`.
