# CHECKPOINT V044 - Validacao no servidor interrompida por tipo de HostName

Data: 23/09/2026

## Escopo

Servidor de aplicacao: `10.1.2.21` / `SV052022-6121`.

Objetivo: confirmar, somente por leitura, que o DC descoberto pelo servidor apresenta a ACL piloto da OU `07.Outros` e a membership da `gMSA_CadColab$` no grupo tecnico.

## Resultado

O DC descoberto foi exibido como:

`SV062022-6158.automind.com.br`

Entretanto, a propriedade `HostName` retornada por `Get-ADDomainController -Discover -Service ADWS` foi mantida no PowerShell como `Microsoft.ActiveDirectory.Management.ADPropertyValueCollection`. Ao repassa-la diretamente para `-Server`, os cmdlets `Get-ADObject`, `Get-ADGroup` e `Get-ADGroupMember` falharam no binding do parametro.

As linhas finais `ACE TOTAL: 0`, `ACE GRUPO TECNICO: 0` e `GMSA NO GRUPO: False` nao representam o estado do AD; sao consequencia das consultas anteriores terem falhado e as variaveis nao terem sido populadas.

## Impacto

- nenhuma escrita no AD;
- nenhuma alteracao no IIS;
- nenhuma alteracao no servidor;
- nenhuma evidência de problema de replicacao;
- ACL piloto da OU `07.Outros` permanece como validada no V043.

## Correcao

Repetir o teste somente leitura convertendo explicitamente o primeiro valor de `HostName` para `System.String` antes de usa-lo em `-Server`.

## Estado

Parar antes de qualquer mudanca no App Pool. Somente prosseguir depois de obter `ACE TOTAL=56`, `ACE GRUPO TECNICO=16` e `GMSA NO GRUPO=True` a partir do servidor `10.1.2.21`.
