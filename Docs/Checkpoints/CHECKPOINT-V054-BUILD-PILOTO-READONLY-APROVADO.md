# CHECKPOINT V054 - Build do piloto ReadOnly aprovado

Data: 24/09/2026

## Resultado do build

Build executado na maquina de desenvolvimento com .NET 10 SDK:

- `Automind.CadastroColaboradores net10.0`: sucesso;
- 0 erros;
- 15 avisos `CA1416` em `Services/AdConnectionFactory.cs`;
- os avisos correspondem ao uso de APIs Windows-only de `System.DirectoryServices`.

O projeto permanece destinado ao servidor Windows/IIS, portanto os avisos nao impediram a continuidade do piloto.

## Validacoes de seguranca apos o build

- `Automind:Mode=ReadOnly`;
- `AllowedOuDns`: 23 entradas;
- `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br` presente na allowlist de leitura;
- `WriteAllowedOuDns`: 1 entrada;
- unica OU de escrita: `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- `GroupWritesEnabled=False`;
- `Microsoft365.Enabled=False`;
- servico de escrita AD presente;
- servico de auditoria presente.

## Estado operacional

Nenhuma alteracao foi feita no servidor nesta etapa.

O App Pool de producao continua executando como `AUTOMIND\gMSA_CadColab$`, e a versao atualmente implantada permanece separada deste build local ate nova autorizacao de deploy.

## Regra para a proxima mudanca

A publicacao da nova versao sera uma **ALTERACAO REAL** no servidor e deve ocorrer mantendo `Automind:Mode=ReadOnly`.

Antes da publicacao devem estar preparados:

1. comando/processo de deploy;
2. efeito esperado;
3. rollback para a versao anterior;
4. validacao pos-deploy;
5. confirmacao de que `PilotWrite` continua desabilitado.

Nenhuma ativacao de escrita AD deve ocorrer junto com o deploy.
