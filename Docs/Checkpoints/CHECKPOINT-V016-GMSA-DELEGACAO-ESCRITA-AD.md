# CHECKPOINT V016 - gMSA, autorizacao humana e delegacao minima para escrita no AD

Data: 23/09/2026

## Decisao principal

Foi aprovado separar completamente:

1. quem esta autorizado a acionar o provisionamento no sistema;
2. qual identidade tecnica executa a operacao no Active Directory.

## Autorizacao humana

- operador autentica com sua propria conta;
- `_informatica` continua sendo o grupo de autorizacao administrativa do CadastroColaboradores;
- um membro de `_informatica` nao precisa ser Domain Admin, Account Operator nem possuir ACL individual nas OUs;
- `_informatica` nao deve receber permissoes de escrita no AD por causa desta aplicacao.

Motivo: se a permissao LDAP fosse concedida diretamente a `_informatica`, seus membros poderiam potencialmente utilizar ADUC/PowerShell fora do fluxo, contornando validacoes e auditoria da aplicacao. Alem disso, o diagnostico anterior mostrou que `_informatica` possui associacoes privilegiadas no dominio e nao deve ser reutilizado como grupo tecnico da aplicacao.

## Identidade tecnica

Identidade alvo: gMSA exclusiva do CadastroColaboradores.

Nome sugerido: `gMSA_CadColab$`.

O nome curto substitui a sugestao inicial longa `gmsa_CadastroColaboradores$`, pois a documentacao do `New-ADServiceAccount` recomenda `sAMAccountName` de conta de servico com 15 caracteres ou menos para compatibilidade; o simbolo `$` e acrescentado quando necessario.

A gMSA devera:

- ter senha administrada automaticamente pelo AD;
- nao possuir senha em `appsettings.json`, codigo ou pipeline;
- ser utilizavel somente pelo host IIS `SV052022-6121` / `10.1.2.21` (diretamente ou por grupo de hosts dedicado);
- executar as operacoes LDAP da aplicacao.

## Contexto atual do IIS

Hoje o App Pool esta em `ApplicationPoolIdentity`. Para acessos de rede, esse modelo usa a conta da maquina do servidor, identificada nos testes como:

`AUTOMIND\\SV052022-6121$`

Os testes manuais no AD foram executados pelo usuario elevado apenas para descoberta/validacao. Isso nao representa a identidade desejada da aplicacao em producao.

## Grupo tecnico de delegacao

Criar um grupo de seguranca dedicado, sugerido:

`SG_CadastroColaboradores_AD_Writer`

Esse grupo recebera ACLs delegadas e nao deve conter operadores humanos.

A identidade tecnica da aplicacao sera associada a esse mecanismo de delegacao. Nenhuma permissao administrativa ampla deve ser concedida.

## Delegacao nas OUs

Somente OUs aprovadas na allowlist poderao receber delegacao.

Permissoes funcionais planejadas:

- criar objeto User;
- preencher givenName;
- sn;
- displayName;
- description;
- physicalDeliveryOfficeName;
- telephoneNumber quando autorizado;
- mail;
- title;
- department;
- company;
- manager;
- userPrincipalName;
- sAMAccountName;
- definir senha;
- habilitar conta;
- forcar troca de senha no primeiro logon somente se essa politica for confirmada.

Nao conceder Domain Admin, Enterprise Admin, Account Operators ou Full Control do dominio.

## Delegacao de grupos

Grupos sao uma permissao separada da OU. Para associar um usuario, a operacao altera `member` no objeto de grupo.

Somente grupos explicitamente aprovados poderao receber permissao de alteracao de membros.

Exemplos estudados, ainda dependentes de aprovacao final de escrita:

- `_Todos`;
- `_Todos SSA`;
- `_Engenharia`;
- `_Tecnica SSA`;
- `Dist_Engenharia`.

Proibido conceder escrita automatica em grupos administrativos/protegidos, `_informatica`, `Domain Admins` ou equivalentes.

## Auditoria

O AD vera a gMSA como identidade executora. A aplicacao precisa registrar o contexto humano:

- usuario solicitante;
- chamado TOPdesk;
- identidade tecnica;
- objeto criado;
- OU;
- grupos;
- data/hora;
- resultado.

A senha inicial nunca sera persistida.

## Fluxo alvo

`membro de _informatica`

-> login no CadastroColaboradores

-> autorizacao humana validada

-> operador confirma criacao

-> backend executa como `gMSA_CadColab$`

-> AD valida apenas ACLs delegadas a identidade tecnica

-> aplicacao registra operador + chamado + resultado.

## Sequencia aprovada para implantacao

1. validar nomes/objetos tecnicos existentes;
2. criar gMSA exclusiva;
3. autorizar uso somente no `10.1.2.21`;
4. instalar/testar gMSA no servidor;
5. configurar IIS/App Pool para identidade tecnica;
6. criar grupo tecnico de delegacao;
7. delegar permissoes minimas nas OUs aprovadas;
8. delegar Write Members somente nos grupos aprovados;
9. testar leitura;
10. criar usuario ficticio;
11. validar senha;
12. validar manager;
13. validar grupos;
14. remover usuario ficticio, se decidido.

## Estado deste checkpoint

Nenhum objeto AD, grupo, ACL, gMSA ou configuracao IIS foi criado/alterado por este checkpoint. Esta versao e somente documental e prepara os testes/implantacao controlada.
