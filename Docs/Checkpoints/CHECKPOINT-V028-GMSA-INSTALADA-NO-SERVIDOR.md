# CHECKPOINT V028 - gMSA instalada no servidor da aplicacao

Data: 23/09/2026

## Alteracao executada

No servidor `10.1.2.21` (`SV052022-6121`) foi executado:

`Install-ADServiceAccount gMSA_CadColab`

Antes da instalacao:

- `Test-ADServiceAccount -Identity gMSA_CadColab` retornou `True`;
- `Install-ADServiceAccount -Identity gMSA_CadColab -WhatIf` apontou somente para `CN=gMSA_CadColab,CN=Managed Service Accounts,DC=automind,DC=com,DC=br`.

A instalacao real terminou sem erro.

## Estado atual

- gMSA existe no AD;
- somente `SV052022-6121$` pode recuperar a senha gerenciada;
- gMSA foi instalada/localmente preparada no servidor da aplicacao;
- App Pool `CadastroColaboradores` ainda nao foi alterado;
- grupo tecnico ainda nao foi criado;
- nenhuma ACL de OU/grupo foi alterada;
- nenhuma criacao/modificacao de colaborador foi executada.

## Rollback desta etapa

Se for necessario desfazer somente a instalacao local:

`Uninstall-ADServiceAccount -Identity gMSA_CadColab`

Nao remover o objeto da gMSA do AD como rollback desta etapa, pois a criacao do objeto pertence a etapa anterior.

## Proxima etapa

Antes de criar o grupo tecnico, levantar o padrao de grupos de seguranca existente e definir onde o grupo `SG_CadastroColaboradores_AD_Writer` sera criado. Permanecer em leitura ate essa definicao.
