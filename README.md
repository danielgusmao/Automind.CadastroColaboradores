# Automind.CadastroColaboradores

ASP.NET Core MVC / .NET 10 Windows para cadastro e provisionamento controlado de colaboradores Automind.

## Estado atual - v0.1.14


### v0.1.14 - recuperacao operacional

- corrige traducao de `Estagiaria`/`Estagiario` para `Intern`;
- evita erro JSON quando a sessao expira durante polling M365;
- habilita `SlidingExpiration` da autenticacao;
- transforma `Historico` em painel real baseado no `ProvisioningAudit.jsonl` + leitura ao vivo do Graph;
- mostra AD, Entra, `UsageLocation` e licencas atuais;
- permite concluir/repetir licenciamento M365 de usuario ja criado sem recriar o AD;
- registra pendencia M365 e UPN nas novas auditorias.

Caso de recuperacao inicial: `I2609-0317` / `safira.gusmao@automind.com.br`.

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
- atribuicao e remocao controlada de Microsoft 365 Business Standard em `lucas.costa`;
- v0.1.12 validada no servidor exibindo o inventario real e quantidades do tenant.

A v0.1.13 habilita o proximo passo:
- checkboxes somente em SKUs disponiveis;
- SKUs sem vagas/suspensos permanecem bloqueados;
- selecao revalidada no backend imediatamente antes do fluxo;
- apos criar o usuario no AD, a tela consulta o Entra ate o Cloud Sync publicar o usuario;
- quando o usuario aparecer, `UsageLocation=BR` e aplicado se estiver vazio;
- somente as licencas selecionadas sao atribuidas;
- readback confirma a atribuicao;
- falha apos atribuicao aciona rollback apenas dos SKUs adicionados pela operacao;
- auditoria M365 usa o mesmo JSONL do provisionamento;
- o endpoint de escrita M365 continua restrito ao operador autorizado e a usuarios dentro de `WriteAllowedOuDns`.

Configuracao desta entrega:
- `Microsoft365.Enabled=true`;
- `LicenseInventoryEnabled=true`;
- `LicenseWritesEnabled=true`;
- `UsageLocation=BR`;
- `SyncPollSeconds=10`;
- `SyncMaxWaitSeconds=180`.

Nao existe client secret. A autenticacao usa o certificado do servidor.

## Seguranca

- certificado privado permanece em `LocalMachine\My` no servidor;
- gMSA possui somente leitura na chave privada;
- autorizacao humana continua por `_informatica`;
- M365 write piloto aceita somente usuario AD localizado dentro da OU de escrita autorizada;
- disponibilidade de SKU e revalidada no Graph antes da escrita;
- rollback remove somente licencas ausentes antes da operacao e adicionadas pelo CadColab;
- `UsageLocation=BR` nao e revertido automaticamente para vazio;
- nenhuma senha inicial e gravada em banco/log/historico;
- `proxyAddresses` e `pwdLastSet` continuam fora da escrita.

## Build e publicacao

Branch: `release`.

Validacao local:

```powershell
git branch --show-current;dotnet build -c Release;git status --short
```

Depois do build aprovado:

```powershell
git add .;git commit -m "feat: adiciona historico e recuperacao M365 v0.1.14";git push origin release;git push azure release
```

Nao criar tag Git automaticamente.

## Teste da v0.1.14

Depois do deploy:
1. abrir `Novo colaborador`;
2. confirmar `Graph conectado` e quantidades atuais;
3. confirmar que apenas SKUs disponiveis podem ser marcados;
4. selecionar uma licenca disponivel para um novo usuario piloto;
5. executar `Validar AD + M365` e confirmar `Licencas Microsoft 365 validas`;
6. criar o usuario piloto;
7. acompanhar `Aguardando sincronizacao com o Entra`;
8. confirmar atribuicao M365 e readback;
9. conferir portal Microsoft 365 e auditoria JSONL;
10. testar rollback somente em uma operacao controlada, se necessario.

Se o usuario nao aparecer no Entra em ate 180 segundos, nenhuma licenca e gravada e a tela oferece `Repetir atribuicao M365`.

## Documentacao

- `Docs/CONTEXTO-ATUAL.md` - estado operacional atual;
- `Docs/CHECKPOINT.md` - checkpoint cumulativo, mais novo primeiro;
- `Docs/07-MICROSOFT-365.md` - identidade, fluxo, testes, escrita e rollback M365;
- `Docs/16-CONTRATO-ESCRITA-AD.md` - contrato AD;
- `Docs/17-ROLLBACK-AD.md` - rollback AD;
- `Docs/20-POLITICA-DOCUMENTACAO-E-CONTINUIDADE.md` - politica documental.
