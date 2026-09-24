# CHECKPOINT V045 - ACL e membership visiveis no servidor da aplicacao

Data: 23/09/2026

## Escopo

Servidor: `10.1.2.21` / `SV052022-6121`.

Objetivo: confirmar por leitura que o DC usado pelo servidor apresenta a delegacao piloto da OU `07.Outros` e a membership da gMSA no grupo tecnico.

## Resultado

- DC consultado: `SV062022-6158.automind.com.br`;
- tipo da variavel usada em `-Server`: `System.String`;
- `ACE TOTAL`: `56`;
- `ACE GRUPO TECNICO`: `16`;
- `GMSA NO GRUPO`: `True`.

## Conclusao

O servidor da aplicacao enxerga o estado esperado do AD:

1. a OU piloto `07.Outros` possui as 16 ACEs tecnicas validadas;
2. `gMSA_CadColab$` e membro de `SG_CadastroColaboradores_AD_Writer`;
3. o DC consultado apresenta o mesmo estado necessario ao piloto.

Nenhuma escrita foi executada nesta validacao.

## Proximo passo

Antes de qualquer mudanca no IIS:

- reler a configuracao atual do App Pool `CadastroColaboradores`;
- confirmar estado e identidade configurada;
- confirmar a identidade efetiva do worker atual;
- validar novamente `Test-ADServiceAccount gMSA_CadColab` no servidor;
- nao exibir, copiar ou registrar senha residual do App Pool;
- preparar comando de mudanca, efeito esperado, rollback e validacao antes de qualquer escrita.
