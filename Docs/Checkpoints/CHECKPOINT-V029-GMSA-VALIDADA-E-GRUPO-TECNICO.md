# CHECKPOINT V029 - gMSA validada e grupo tecnico

Data: 23/09/2026

## Resultado

No servidor `SV052022-6121`, `Test-ADServiceAccount -Identity gMSA_CadColab` retornou `True` apos a instalacao local.

No AD:

- `OU=06.Grupos-Gerais,OU=Automind,DC=automind,DC=com,DC=br` existe;
- nao existem grupos `SG_*` no levantamento atual;
- `_CriaMovePastas`: `Global / Security`;
- `_Informatica`: `Global / Security`.

## Decisao provisoria

Manter como candidato:

`SG_CadastroColaboradores_AD_Writer`

Configuracao proposta:

- `GroupCategory=Security`;
- `GroupScope=Global`;
- OU `06.Grupos-Gerais`;
- uso exclusivo para delegacao tecnica do CadastroColaboradores;
- membro inicial futuro: somente `gMSA_CadColab$`.

`_Informatica` nao recebe permissao tecnica de escrita no AD. Ela permanece somente como autorizacao do operador no sistema.

## Proxima etapa

Somente leitura/simulacao:

1. baseline da ACL da OU `06.Grupos-Gerais`;
2. confirmar que `SG_CadastroColaboradores_AD_Writer` nao existe;
3. executar `New-ADGroup -WhatIf`.

Nenhuma nova escrita foi executada neste checkpoint.
