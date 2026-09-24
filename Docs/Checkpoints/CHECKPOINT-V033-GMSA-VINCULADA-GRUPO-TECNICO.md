# CHECKPOINT V033 - gMSA vinculada ao grupo tecnico

Data: 23/09/2026

## Mudanca executada

Foi adicionada somente a `gMSA_CadColab$` ao grupo `SG_CadastroColaboradores_AD_Writer`.

## Validacao

`Get-ADGroupMember` retornou:

- `Name`: `gMSA_CadColab`
- `SamAccountName`: `gMSA_CadColab$`
- `ObjectClass`: `msDS-GroupManagedServiceAccount`

`Get-ADServiceAccount -Properties MemberOf` confirmou:

`CN=SG_CadastroColaboradores_AD_Writer,OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`

## Impacto

A associacao so vincula a identidade tecnica ao grupo de delegacao. Como nenhuma ACE foi concedida ao grupo, esta etapa nao adiciona por si so permissoes de criacao/alteracao de usuarios.

Nenhuma alteracao foi feita no IIS, OUs, usuarios ou ACLs.

## Rollback

```powershell
$svc=Get-ADServiceAccount "gMSA_CadColab";Remove-ADGroupMember -Identity "SG_CadastroColaboradores_AD_Writer" -Members $svc -Confirm:$false
```

Validar com `Get-ADGroupMember` e `MemberOf` da gMSA.

## Proxima etapa

Somente leitura:

1. confirmar que o grupo tecnico ainda nao possui ACEs no dominio/OU piloto;
2. mapear GUIDs de classe, atributos e direitos estendidos necessarios;
3. preparar delegacao e rollback exato antes de qualquer escrita de ACL.
