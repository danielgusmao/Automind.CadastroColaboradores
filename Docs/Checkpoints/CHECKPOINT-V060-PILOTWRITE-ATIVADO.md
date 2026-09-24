# CHECKPOINT V060 - CadColab v0.1.3 - ativacao controlada do PilotWrite

Data: 24/09/2026.


Autorizacao explicita do responsavel recebida para avancar de `ReadOnly` para `PilotWrite`.

Estado preparado no pacote:

- `Automind:Mode=PilotWrite`;
- `WriteAllowedOuDns` = somente `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- `GroupWritesEnabled=false`;
- grupos comuns continuam selecionados visualmente como sugestao, mas nenhuma membership e enviada ao writer (`GroupDns=[]`);
- `Microsoft365.Enabled=false`;
- `proxyAddresses` e `pwdLastSet` permanecem fora da escrita;
- `VERSION.txt` = `CadColab 0.1.3`;
- `Version` do projeto = `0.1.3`;
- `VERSION.txt` configurado para `CopyToOutputDirectory` e `CopyToPublishDirectory`, corrigindo a ausencia constatada no servidor;
- rollback de contencao: voltar `Mode=ReadOnly` e publicar;
- pacote: `CadColab-v0.1.3.zip`;
- sem tag Git automatica.

O pre-check imediatamente anterior confirmou no servidor: `Mode=ReadOnly`, unica OU de escrita `07.Outros`, App Pool Started, identidade `AUTOMIND\gMSA_CadColab$`, `Test-ADServiceAccount=True`, OU existente, gMSA no grupo tecnico e HTTP 200. O diretorio de auditoria ainda nao existia; isso e fail-safe porque o writer tenta iniciar a auditoria antes da primeira escrita AD e deve abortar se nao conseguir.

