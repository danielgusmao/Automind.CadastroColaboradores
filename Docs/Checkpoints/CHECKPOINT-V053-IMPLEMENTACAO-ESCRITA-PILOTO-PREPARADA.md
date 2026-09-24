# CHECKPOINT V053 - Implementacao de escrita piloto preparada

Data: 24/09/2026

## Estado

Foi preparada no codigo a primeira implementacao de provisionamento AD, mas ela permanece **inativa** por configuracao.

`Automind:Mode=ReadOnly` continua sendo o valor entregue no `appsettings.json`.

Nenhuma publicacao, mudanca de configuracao no servidor ou criacao de usuario foi executada nesta etapa.

## Escopo preparado

- unica OU autorizada em `WriteAllowedOuDns`: `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- `07.Outros` adicionada tambem a allowlist de leitura do codigo para permitir pre-validacao;
- identidade tecnica obrigatoria: `AUTOMIND\gMSA_CadColab$`;
- operador humano revalidado em `_informatica` imediatamente antes da escrita;
- grupos continuam bloqueados;
- M365 continua desabilitado;
- `proxyAddresses` e `pwdLastSet` continuam fora do contrato.

## Fluxo implementado

`pre-validacao -> auditoria -> CreateChild desabilitado -> atributos -> senha -> manager -> releitura -> habilitar por ultimo -> releitura final -> auditoria`

Em falha depois do CreateChild:

- interromper imediatamente;
- tentar confirmar conta desabilitada;
- nao excluir automaticamente;
- exigir revisao manual.

## Auditoria

Arquivo configurado:

`C:\Automind.CadastroColaboradores\Logs\ProvisioningAudit.jsonl`

A senha temporaria nao integra o modelo de auditoria e nao e persistida pela aplicacao. Ela somente e retornada ao operador na resposta bem-sucedida da criacao.

## Arquivos principais novos/alterados

- `Services/IAdProvisioningWriteService.cs`;
- `Services/WindowsAdProvisioningWriteService.cs`;
- `Services/IProvisioningAuditService.cs`;
- `Services/FileProvisioningAuditService.cs`;
- `Models/AdProvisioningWriteModels.cs`;
- `Controllers/ColaboradoresController.cs`;
- `Services/IAdAuthenticationService.cs`;
- `Services/WindowsAdAuthenticationService.cs`;
- `Services/AdConnectionFactory.cs`;
- `Models/AdDirectoryModels.cs`;
- `Program.cs`;
- `Views/Colaboradores/Novo.cshtml`;
- `Views/Home/Index.cshtml`;
- `wwwroot/js/site.js`;
- `wwwroot/css/automind.css`;
- `appsettings.json`;
- documentacao do projeto.

## Validacoes realizadas no ambiente de geracao

- `appsettings.json` parseado com sucesso;
- `node --check wwwroot/js/site.js` sem erro;
- `Automind:Mode` confirmado como `ReadOnly`;
- `WriteAllowedOuDns` confirmado contendo somente `07.Outros`;
- `GroupWritesEnabled=false`;
- `Microsoft365.Enabled=false`.

Nao foi executado `dotnet build`: o ambiente de geracao nao possui .NET SDK.

## Regra operacional mantida

Testes somente leitura/simulacao no mesmo local podem ser consolidados em uma unica linha e conter mais de 3 verificacoes. Alteracoes reais continuam uma por vez, com efeito esperado, rollback e validacao preparados antes.

## Proximo passo

1. entregar o projeto completo com a implementacao preparada;
2. executar build em maquina com .NET 10 SDK;
3. corrigir qualquer erro de compilacao antes de publicacao;
4. publicar inicialmente ainda em `ReadOnly`;
5. validar site, leitura AD, OU piloto e caminho de auditoria;
6. somente depois preparar e autorizar a mudanca real para `PilotWrite`.
