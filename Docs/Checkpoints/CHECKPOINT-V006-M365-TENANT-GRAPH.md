# CHECKPOINT V006 - Microsoft 365 / Entra: Tenant ID e endpoint Graph validados

> Registro somente de diagnostico. Nenhum codigo, modulo, App Registration, credencial, permissao Graph, IIS ou configuracao de servidor foi criado ou alterado.

## 2026-09-23 - Resultados recebidos no servidor 10.1.2.21

### Tenant Microsoft Entra

Consulta ao documento publico OIDC executada em sessao PowerShell limpa.

Resultado:
- Issuer: `https://login.microsoftonline.com/9ab05ca8-1779-410b-ae61-82dbd20810f3/v2.0`
- Tenant ID: `9ab05ca8-1779-410b-ae61-82dbd20810f3`
- Token endpoint: `https://login.microsoftonline.com/9ab05ca8-1779-410b-ae61-82dbd20810f3/oauth2/v2.0/token`

Conclusao factual:
- o dominio `automind.com.br` resolve corretamente para o tenant Microsoft Entra acima;
- nenhuma autenticacao de usuario foi necessaria para obter esses metadados publicos.

### Endpoint Microsoft Graph - subscribedSkus

Teste executado sem token:

`GET https://graph.microsoft.com/v1.0/subscribedSkus`

Resultado:
- HTTP Status: `401`.

Interpretacao:
- a comunicacao HTTPS com o endpoint Microsoft Graph esta funcional;
- `/subscribedSkus` exige cabecalho `Authorization: Bearer {token}`;
- portanto HTTP 401 sem token e coerente com um endpoint protegido e nao indica falha de rede;
- a consulta autenticada ainda nao foi executada.

## Permissao minima documentada para a proxima etapa

Para `GET /subscribedSkus`, a permissao menos privilegiada documentada pelo Microsoft Graph e:
- `LicenseAssignment.Read.All`

Ela existe tanto no modelo Delegated quanto no modelo Application. Para o desenho futuro do CadastroColaboradores, a avaliacao continua orientada a identidade tecnica/App-only, sem uso da conta administrativa pessoal do responsavel.

Nenhuma permissao foi concedida ate este checkpoint.

## Convencao do pacote de checkpoint

A pedido do responsavel, enquanto houver apenas atualizacao documental/checkpoint e nenhuma entrega de codigo alterado, o ZIP deve ser identificado explicitamente como checkpoint:

`Automind.CadastroColaboradores-AAAA-MM-DD-checkpoint-vX.Y.Z.zip`

Primeira versao nesse padrao:

`Automind.CadastroColaboradores-23-09-2026-checkpoint-v0.0.1.zip`

Quando houver uma entrega autorizada do projeto completo, o checkpoint deve acompanhar o projeto dentro do pacote completo e o nome do projeto sera versionado separadamente.

## Estado atual do bloco Microsoft 365

Validado:
- saida TCP/443 para Entra e Microsoft Graph;
- HTTPS funcional via `curl.exe` e via Windows PowerShell 5.1 em sessao limpa;
- causa do erro da sessao original isolada a um callback customizado presente naquela sessao;
- Tenant ID confirmado: `9ab05ca8-1779-410b-ae61-82dbd20810f3`;
- endpoint `/v1.0/subscribedSkus` acessivel e retornando HTTP 401 sem token, conforme comportamento de recurso protegido;
- Microsoft Graph PowerShell SDK continua ausente.

Ainda nao realizado/autorizado:
- instalacao/atualizacao de PowerShellGet, Microsoft.Graph ou PowerShell 7;
- criacao de App Registration;
- criacao de secret ou certificado;
- concessao de `LicenseAssignment.Read.All`;
- autenticacao App-only;
- consulta autenticada das licencas/SKUs.

## Proximo passo

Antes de criar qualquer recurso no tenant, definir e validar qual mecanismo de autenticacao sera usado pela aplicacao para Microsoft Graph (preferencialmente identidade tecnica/App-only) e quais pre-requisitos ja existem no tenant. Qualquer criacao ou alteracao depende de consentimento explicito do responsavel.
