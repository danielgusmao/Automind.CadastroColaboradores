# CHECKPOINT V024 — KDS, níveis funcionais e container de gMSA

Data: 23/09/2026

## Resultado

Testes somente leitura.

- `DomainMode`: `Windows2016Domain`.
- `ForestMode`: `Windows2008R2Forest`.
- `Get-KdsRootKey` voltou a não exibir dados na projeção usada. Esse retorno não será tratado como ausência da chave, porque o projeto já confirmou anteriormente um objeto `msKds-ProvRootKey` no Configuration Naming Context.
- Container de contas de serviço: `CN=Managed Service Accounts,DC=automind,DC=com,DC=br`.
- Owner do container: `AUTOMIND\Domain Admins`.
- Herança de ACL habilitada (`AreAccessRulesProtected=False`).
- 36 regras observadas no baseline.
- Contas existentes: `provAgentgMSA` / `pGMSA_c5fa29e8$` e `ADSyncMSA82bf3$`.

## Decisão

- Não executar `Add-KdsRootKey` / `New-KdsRootKey`.
- Não elevar nível funcional de domínio ou floresta como parte deste projeto.
- Antes da criação da gMSA do CadastroColaboradores, reconfirmar diretamente o objeto KDS já existente e testar sua configuração com `Test-KdsRootKey`.
- A divergência entre `Get-KdsRootKey` vazio e o objeto KDS existente será tratada como comportamento a investigar, sem alteração de infraestrutura.
- Nenhuma alteração foi executada.

## Próxima etapa

Somente leitura no AD:

1. consultar diretamente `CN=Master Root Keys,CN=Group Key Distribution Service,CN=Services,CN=Configuration,...`;
2. obter o GUID da chave encontrada;
3. executar `Test-KdsRootKey -KeyId <GUID>`;
4. parar e documentar o resultado antes de qualquer criação de gMSA.
