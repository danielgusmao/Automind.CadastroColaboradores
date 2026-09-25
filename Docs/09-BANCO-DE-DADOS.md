# Complemento 25/09/2026 - Historico sem banco na v0.1.14

O menu `Historico` passa a ser funcional sem instalar banco de dados: le o arquivo `ProvisioningAudit.jsonl` ja usado pelo provisionamento e consulta o Microsoft Graph para o estado atual de licencas. Persistencia SQL continua adiada ate decisao explicita de infraestrutura.

---

# Banco de dados

SQL Server ainda não definido.

Não instalar SQL Server no servidor IIS antes de verificar se existe instância corporativa disponível.

Dados previstos:
- auditoria/histórico;
- configurações administrativas;
- traduções de cargos;
- destinatários Teams;
- grupos protegidos;
- perfis/regras aprovadas.
