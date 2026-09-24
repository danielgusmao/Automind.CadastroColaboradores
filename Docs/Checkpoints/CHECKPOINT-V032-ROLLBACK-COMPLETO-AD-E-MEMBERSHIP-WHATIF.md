# CHECKPOINT V032 - Rollback completo AD e simulacao de membership

Data: 23/09/2026
Projeto: Automind.CadastroColaboradores

## Resultado da rodada

- `SG_CadastroColaboradores_AD_Writer` confirmado sem membros;
- `gMSA_CadColab$` confirmada sem `MemberOf`;
- `Add-ADGroupMember -WhatIf` confirmou o grupo tecnico como alvo;
- nenhuma membership foi criada pelo `-WhatIf`;
- criada a documentacao `Docs/17-ROLLBACK-AD.md`;
- o rollback completo passa a ser requisito obrigatorio antes de cada nova escrita.

## Estado atual

- gMSA criada no AD;
- gMSA instalada e validada no `SV052022-6121`;
- grupo tecnico criado e vazio;
- gMSA ainda nao pertence ao grupo tecnico;
- nenhuma delegacao de OU ou grupo;
- IIS ainda nao usa a gMSA;
- escrita de usuarios continua bloqueada.

## Regra para continuidade

Antes de qualquer proxima mudanca, o checkpoint deve informar comando, efeito esperado, rollback e validacao. Uma alteracao por vez.
