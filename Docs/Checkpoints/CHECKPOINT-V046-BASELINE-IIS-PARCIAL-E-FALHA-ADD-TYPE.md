# CHECKPOINT V046 - Baseline IIS parcial e falha de Add-Type

Data: 23/09/2026

## Escopo

Servidor: `10.1.2.21` / `SV052022-6121`.

Objetivo: capturar o estado do App Pool `CadastroColaboradores` antes de qualquer troca de identidade para a `gMSA_CadColab$`.

## Resultado recebido

A tentativa de carregar `Microsoft.Web.Administration` pelo nome simples com `Add-Type -AssemblyName Microsoft.Web.Administration` retornou erro de assembly nao encontrado.

Apesar disso, a sequencia posterior retornou informacoes do App Pool:

- pool: `CadastroColaboradores`;
- estado: `Started`;
- `IdentityType`: `ApplicationPoolIdentity`;
- `UserName` configurado: `AUTOMIND\\CriaMovePastas`;
- `LoadUserProfile`: `True`;
- `StartMode`: `OnDemand`;
- runtime vazio, consistente com aplicacao ASP.NET Core hospedada no IIS;
- pipeline: `Integrated`;
- `Test-ADServiceAccount gMSA_CadColab`: `True`;
- nenhum worker ativo foi encontrado naquele instante.

## Interpretacao segura

1. Nenhuma alteracao foi feita no IIS.
2. O erro de `Add-Type` e de carregamento, nao de configuracao do App Pool.
3. Como a sequencia conseguiu retornar propriedades do pool mesmo apos o erro, nao sera usado esse caminho ambiguo como baseline definitivo.
4. O baseline definitivo deve ser repetido por `WebAdministration`/`appcmd`, sem ler ou exibir senha.
5. `ApplicationPoolIdentity` continua sendo a identidade efetiva configurada; o `UserName` residual `AUTOMIND\\CriaMovePastas` nao deve ser interpretado como identidade efetiva enquanto `IdentityType` permanecer `ApplicationPoolIdentity`.
6. Ausencia de worker e compativel com pool `OnDemand` sem requisicao ativa e nao deve ser tratada como falha.

## Estado de seguranca

- nenhuma troca de identidade autorizada ainda;
- nenhuma alteracao de ACL NTFS autorizada ainda;
- nao executar `iisreset`;
- nao reiniciar servidor;
- nao reciclar o pool antes de existir comando de mudanca + rollback + validacao;
- nao registrar ou exibir senha residual do App Pool.

## Proximo passo

Executar apenas leitura no servidor para:

1. confirmar o baseline do App Pool usando `WebAdministration`/`appcmd` sem `Add-Type` e sem senha;
2. identificar aplicacao/site vinculados ao pool e eventual worker ativo;
3. revisar ACL NTFS da pasta da aplicacao e diretorios potencialmente gravaveis antes de qualquer mudanca para gMSA.
