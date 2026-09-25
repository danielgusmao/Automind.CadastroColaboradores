# Automind.CadastroColaboradores

ASP.NET Core MVC / .NET 10 Windows para cadastro e provisionamento controlado de colaboradores Automind.

## Estado atual - v0.1.12

### Active Directory

Piloto concluido ponta a ponta:
- login/autorizacao AD;
- importacao TOPdesk Bridge;
- consultas reais de OU, identidade, manager e grupos;
- sugestao por `Title + Department`;
- comuns, excecoes, grupos manuais e bloqueio de protegidos;
- criacao em `07.Outros`;
- atributos, senha e manager;
- membership limitada por `GroupWriteAllowedDns`;
- readback, enable final e auditoria.

### Microsoft 365 / Entra

Validado no tenant real:
- App-only por certificado;
- leitura de `subscribedSkus`;
- leitura de usuarios/licencas;
- `UsageLocation=BR` em usuario sincronizado;
- atribuicao e remocao controlada de Microsoft 365 Business Standard em `lucas.costa`.

A v0.1.12 adiciona ao formulario `Novo colaborador` uma lista somente leitura das licencas do tenant no formato do Microsoft 365 Admin Center, por exemplo:

`Microsoft 365 Business Standard - 17 de 166 licencas disponiveis`

Configuracao da entrega:
- `Microsoft365.Enabled=true`;
- `LicenseInventoryEnabled=true`;
- `LicenseWritesEnabled=false`.

Nenhuma atribuicao/remocao de licenca foi integrada ao fluxo normal do CadColab nesta versao.

## Seguranca

- sem client secret;
- certificado privado permanece em `LocalMachine\My` no servidor;
- gMSA possui somente leitura na chave privada;
- nenhuma senha inicial e gravada em banco/log/historico;
- `proxyAddresses` e `pwdLastSet` continuam fora da escrita;
- M365 write permanece bloqueado no codigo/configuracao da v0.1.12.

## Build e publicacao

Branch: `release`.

Validacao local:

```powershell
git branch --show-current;dotnet build -c Release;git status --short
```

Depois do build aprovado:

```powershell
git add .;git commit -m "CadColab v0.1.12 - inventario Microsoft 365";git push origin release;git push azure release
```

Nao criar tag Git automaticamente.

## Teste da v0.1.12

Depois do deploy:
1. abrir `Novo colaborador`;
2. localizar `04 - MICROSOFT 365`;
3. confirmar `Graph conectado`;
4. comparar quantidades com o Microsoft 365 Admin Center;
5. confirmar checkboxes desabilitados;
6. validar que TOPdesk/AD continuam funcionando normalmente.

## Documentacao

- `Docs/CONTEXTO-ATUAL.md` - estado operacional atual;
- `Docs/CHECKPOINT.md` - checkpoint cumulativo, mais novo primeiro;
- `Docs/07-MICROSOFT-365.md` - identidade, testes, implementacao e rollback M365;
- `Docs/16-CONTRATO-ESCRITA-AD.md` - contrato AD;
- `Docs/17-ROLLBACK-AD.md` - rollback AD;
- `Docs/20-POLITICA-DOCUMENTACAO-E-CONTINUIDADE.md` - politica documental.
