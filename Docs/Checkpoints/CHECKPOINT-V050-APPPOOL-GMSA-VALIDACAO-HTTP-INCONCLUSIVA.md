# CHECKPOINT V050 - App Pool gMSA / validacao HTTP inicial inconclusiva

Data: 24/09/2026
Servidor: `SV052022-6121` / `10.1.2.21`

## Alteracao real executada

O App Pool `CadastroColaboradores` foi configurado para:

- `IdentityType=SpecificUser`;
- `UserName=AUTOMIND\gMSA_CadColab$`;
- `LogonType=LogonBatch`.

A gMSA continuou valida no servidor: `Test-ADServiceAccount=True`.

## Resultado da primeira validacao

- pool: `Started`;
- requisicao enviada para `http://127.0.0.1/` com cabecalho `Host=cadastro.automind.com.br`;
- retorno: HTTP 404;
- worker do pool nao encontrado apos a requisicao;
- sem eventos WAS/W3SVC exibidos nos cinco minutos consultados.

## Interpretacao

O binding confirmado anteriormente e `10.1.2.21:80:cadastro.automind.com.br`. Portanto, testar contra `127.0.0.1` nao prova que a aplicacao vinculada a `10.1.2.21` foi acionada. O 404 nao deve ser usado isoladamente para concluir falha da gMSA nem para disparar rollback.

## Bloqueios

Ate a proxima validacao:

- nao mudar ACL NTFS;
- nao conceder direitos locais extras;
- nao executar `iisreset`;
- nao fazer nova alteracao no AD;
- nao restaurar backup integral do IIS.

## Rollback mantido preparado

Se a ativacao real do pool falhar quando o site correto for acionado, o rollback primario continua restrito ao pool:

`ApplicationPoolIdentity`

O backup `CadColab-Pre-gMSA-20260924-082229` permanece apenas como contingencia integral.
