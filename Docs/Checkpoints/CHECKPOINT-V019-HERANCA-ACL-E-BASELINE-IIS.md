# CHECKPOINT V019 — Herança de ACL e baseline IIS

Data: 23/09/2026

## Resultado dos testes

### Active Directory

A conta existente `AUTOMIND\pGMSA_c5fa29e8$` possui ACEs herdadas nas OUs:

- `OU=Automind,DC=automind,DC=com,DC=br`
- `OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`
- `OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`

Em todos os resultados analisados, `IsInherited = True`. Portanto, essas permissões não foram definidas diretamente em Engenharia nem em 03.UDN. Como também aparecem herdadas em `OU=Automind`, a origem está acima dessa OU e deve ser localizada antes de qualquer nova delegação.

Decisão: não reutilizar nem alterar `pGMSA_c5fa29e8$` ou suas ACEs.

### Servidor da aplicação

Os cmdlets `Install-ADServiceAccount` e `Test-ADServiceAccount` estão disponíveis.

O baseline de IIS não foi concluído porque:

- o módulo `WebAdministration` não foi encontrado;
- o drive `IIS:` não existe nessa sessão;
- `C:\Automind.CadastroColaboradores` não existe no host onde o comando foi executado.

Nenhuma alteração foi realizada.

## Próximos testes — somente leitura

1. localizar a origem das ACEs da `pGMSA_c5fa29e8$` no nível do domínio;
2. confirmar hostname, presença do IIS, App Pool/site e caminho físico reais no servidor `10.1.2.21`.

## Regra editorial

Documentação e interação devem usar vocabulário técnico simples, texto objetivo e sem repetição desnecessária.
