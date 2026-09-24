# CHECKPOINT V031 - Grupo tecnico criado

Data: 23/09/2026

## Alteracao executada

Criado no Active Directory:

- Nome: `SG_CadastroColaboradores_AD_Writer`
- `sAMAccountName`: `SG_CadastroColaboradores_AD_Writer`
- Tipo: `Global / Security`
- OU: `OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`
- Descricao: `Delegacao tecnica AD do Automind.CadastroColaboradores`

## Validacao

O grupo foi consultado apos a criacao e retornou com os valores esperados. O campo `Members` esta vazio.

## Estado preservado

Nesta etapa nao foram executados:

- inclusao da gMSA no grupo;
- delegacao em OU;
- permissao `Write Members` em grupos;
- alteracao de identidade do App Pool;
- restart/reboot de AD, servidor ou IIS.

## Rollback da etapa

Se for necessario desfazer esta criacao antes de qualquer delegacao ou inclusao de membros:

`Remove-ADGroup -Identity "SG_CadastroColaboradores_AD_Writer"`

Nao executar sem autorizacao explicita.

## Proxima etapa

Validar o grupo vazio, validar a `gMSA_CadColab$` e simular a inclusao dela como unico membro inicial antes de qualquer escrita.
