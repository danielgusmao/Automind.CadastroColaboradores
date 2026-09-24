# CadColab - contexto atual para continuidade

Versao em preparacao: `0.1.9`  
Data: 24/09/2026  
Projeto: `Automind.CadastroColaboradores`

## Estado operacional atual

- build confirmado da v0.1.8: `net10.0-windows`, 0 erros e 0 warnings;
- `Automind:Mode=PilotWrite`;
- criacao real de usuario continua limitada a `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- App Pool `CadastroColaboradores` executa como `AUTOMIND\gMSA_CadColab$`;
- usuario piloto existente: `teste.cadcolab`;
- descoberta de grupos validada: 8 comuns 5/5, 4 excecoes, busca manual e bloqueio de grupo protegido;
- `GroupWritesEnabled=false` continua valendo para o fluxo normal de criacao;
- existe agora um teste isolado de membership real para o par exato `teste.cadcolab` -> `_CriaMovePastas`;
- Microsoft 365, `proxyAddresses` e `pwdLastSet` continuam fora da escrita.

## Membership piloto autorizada

O responsavel autorizou explicitamente usar:

- usuario: `CN=Teste Provisionamento Automind,OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- grupo: `CN=_CriaMovePastas,OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`.

A ACE `WriteProperty(member)` para `SG_CadastroColaboradores_AD_Writer` foi aplicada nesse grupo e o responsavel confirmou que a alteracao funcionou. Isso nao autoriza reutilizar outras ACLs, contas ou regras do sistema legado CriaMovePastas.

A v0.1.9 adiciona um painel `MEMBERSHIP PILOTO` que executa a inclusao pela propria identidade tecnica do aplicativo, confirma por releitura do atributo `member`, audita e tenta remover apenas a associacao criada pela operacao caso uma falha aconteca depois da inclusao.

## Regra de sugestao de grupos

1. pesquisar usuarios ativos por `Title + Department` em todo o escopo de leitura;
2. excluir `07.Outros` da coorte estatistica por `SuggestionExcludedOuDns`;
3. excluir o proprio colaborador por login e CN+OU quando aplicavel;
4. 100% da coorte -> `Comum ao cargo`, marcado;
5. parcial -> `Excecao`, desmarcado;
6. `Outros grupos` permite busca manual no AD;
7. grupo protegido nunca pode ser selecionado;
8. efeitos indiretos continuam visiveis.

## Regras operacionais

- leitura/simulacao no mesmo local: agrupar testes em uma linha PowerShell quando pratico;
- alteracao real: uma por vez;
- antes da escrita real: efeito, rollback e validacao do rollback;
- indicar onde executar;
- nao usar `iisreset`;
- nao reiniciar AD/DC/KDC/Netlogon/servidor por causa do projeto;
- nao restaurar ACL inteira cegamente;
- nao excluir usuario de teste sem autorizacao separada;
- `_informatica` e autorizacao humana, nao identidade tecnica.

## Documentacao

- `Docs/CHECKPOINT.md` e o unico checkpoint cumulativo;
- secoes novas sempre no topo;
- documentos tematicos sao evoluidos no mesmo arquivo por tema;
- arquivos redundantes podem ser consolidados/removidos somente depois de incorporar integralmente seu conteudo;
- nunca reduzir informacao documental;
- todo pacote deve permitir continuidade por outra LLM ou por humanos sem depender do chat.

## Proximo gate

1. build da v0.1.9;
2. publicar pela branch `release`;
3. clicar `Aplicar membership piloto`;
4. confirmar `teste.cadcolab` em `Member Of -> _CriaMovePastas`;
5. confirmar auditoria `pilot-membership-*`;
6. aprovado o teste, evoluir a mesma logica para o fluxo normal de criacao com allowlist de grupos.

---

## Historico anterior consolidado

O historico detalhado completo permanece em `Docs/CHECKPOINT.md`. Este arquivo descreve apenas o estado corrente e o proximo gate, evitando duplicacao desnecessaria.
