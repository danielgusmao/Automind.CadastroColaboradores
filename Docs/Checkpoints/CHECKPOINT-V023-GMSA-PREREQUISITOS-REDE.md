# CHECKPOINT V023 — gMSA: pré-requisitos de rede e AD

Data: 23/09/2026

## Resultado

Os testes continuam somente leitura.

### Contas de serviço existentes

Foram encontradas:

- `provAgentgMSA` / `pGMSA_c5fa29e8$`, habilitada, localizada em `CN=Managed Service Accounts`; somente os controladores `SV062022-9947` e `SV062022-6158` podem recuperar sua senha gerenciada.
- `ADSyncMSA82bf3$`, habilitada, sem `PrincipalsAllowedToRetrieveManagedPassword` retornado.

Conclusão: nenhuma conta existente será reutilizada pelo CadastroColaboradores.

### Servidor da aplicação

No `SV052022-6121`:

- `Test-ComputerSecureChannel` retornou `True`;
- domínio: `automind.com.br`;
- DC localizado: `SV062022-6158.automind.com.br` / `10.1.2.1`;
- site AD: `Site-Salvador`;
- DC com LDAP/KDC/DNS e escrita disponíveis;
- horário sincronizado com `SV062022-6158.automind.com.br`;
- última sincronização de horário bem-sucedida no teste.

Conclusão: canal de domínio, descoberta de DC e sincronismo de horário estão adequados para continuar os testes de prontidão da gMSA.

## Decisão

A identidade planejada continua sendo uma gMSA exclusiva do projeto, atualmente sugerida como `gMSA_CadColab$`, autorizada somente para o servidor `SV052022-6121$`.

Nenhuma gMSA, grupo, ACL ou configuração IIS foi criada/alterada nesta etapa.

## Próxima etapa

Antes da primeira escrita:

1. confirmar KDS root key e níveis funcionais por leitura;
2. registrar baseline do container `CN=Managed Service Accounts`;
3. preparar o comando exato de criação e o rollback, sem executar;
4. solicitar autorização explícita antes da criação.
