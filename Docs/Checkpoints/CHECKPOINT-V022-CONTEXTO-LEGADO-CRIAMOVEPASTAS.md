# CHECKPOINT V022 - Contexto legado CriaMovePastas

Data: 23/09/2026

## Objetivo

Registrar o contexto do sistema legado `CriaMovePastas` sem misturá-lo com a identidade técnica do `Automind.CadastroColaboradores`.

## Estado confirmado

- existe um projeto legado `CriaMovePastas` hospedado em IIS;
- o Application Pool `CriaMovePastas` foi configurado historicamente com a conta pessoal elevada `AUTOMIND\daniel.gusmao.d`;
- essa configuração é dívida técnica e será corrigida em trabalho separado, com baseline e rollback próprios;
- não reutilizar essa conta, o pool ou a conta AD `CriaMovePastas` no `CadastroColaboradores`;
- o worker real de `CadastroColaboradores` continua confirmado como `IIS APPPOOL\CadastroColaboradores`.

## Script legado

Arquivo informado: `C:\CriaMovePastas\Script\CriarEstrutura.ps1`.

O script:

- cria estruturas de pastas para `Projeto` ou `SAAS`;
- copia modelos com `robocopy` sem copiar ACL de origem;
- remove herança em pastas criadas;
- remove `Domain Users` e `_Informatica` das ACLs tratadas;
- concede Full Control a `Administrators` e `Domain Admins`;
- em pontos específicos concede Full Control nominal a `daniel.gusmao` e `fernando.menezes`;
- aplica permissões `RX` e `M` aos grupos `_GAP1` a `_GAP7` conforme o tipo/subpasta;
- grava log e progresso em `C:\CriaMovePastas\Logs`.

O script altera sistema de arquivos/ACL NTFS. Ele não deve ser usado como modelo de permissões do Active Directory.

## Decisão

O `CadastroColaboradores` seguirá arquitetura própria:

- `_informatica`: autorização humana dentro da aplicação;
- gMSA dedicada: identidade técnica do backend;
- grupo técnico dedicado: delegação mínima no AD;
- nenhuma conta pessoal elevada na identidade do App Pool;
- nenhuma reutilização do legado `CriaMovePastas`.

## Segurança

Credenciais vistas em telas/comandos não são registradas neste checkpoint.

## Estado da mudança

Nenhuma configuração de IIS, AD, ACL ou conta foi alterada neste checkpoint.
