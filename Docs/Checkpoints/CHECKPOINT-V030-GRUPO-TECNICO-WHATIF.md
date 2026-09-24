# CHECKPOINT V030 - baseline e simulacao do grupo tecnico

Data: 23/09/2026

## Baseline

OU alvo:

`OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`

Estado observado:

- owner: `AUTOMIND\\Domain Admins`;
- heranca nao bloqueada;
- 40 ACEs;
- nenhuma colisao para `SG_CadastroColaboradores_AD_Writer`.

## Simulacao

Foi executado `New-ADGroup -WhatIf` para:

- nome: `SG_CadastroColaboradores_AD_Writer`;
- `sAMAccountName`: `SG_CadastroColaboradores_AD_Writer`;
- categoria: `Security`;
- escopo: `Global`;
- OU: `06.Grupos-Gerais`;
- descricao: `Delegacao tecnica AD do Automind.CadastroColaboradores`.

O `-WhatIf` confirmou o alvo:

`CN=SG_CadastroColaboradores_AD_Writer,OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br`

Nenhuma escrita ocorreu.

## Proxima mudanca proposta

Criar somente o grupo tecnico. Nessa mesma etapa nao adicionar a gMSA ao grupo, nao delegar OUs, nao alterar grupos de acesso e nao alterar IIS.

Validacao imediata apos a criacao:

- confirmar DN, escopo, categoria e descricao;
- confirmar que o grupo esta vazio.

Rollback preparado:

`Remove-ADGroup -Identity "SG_CadastroColaboradores_AD_Writer"`

O rollback so pode ser usado apos confirmar que o objeto alvo e o grupo criado por este projeto e que ainda nao recebeu dependencias adicionais.
