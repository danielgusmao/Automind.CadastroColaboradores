# Automind.CadastroColaboradores

ASP.NET Core MVC / .NET 10 para cadastro e pré-validação de colaboradores Automind.

## Estado da entrega

Integrações ativas:

- login real no Active Directory;
- autorização inicial pelo grupo AD configurado;
- importação de chamado TOPdesk via Automind TOPdesk Bridge;
- leitura real do Active Directory para OUs, usuários, superior imediato, identidade e grupos;
- sugestão real de grupos por `Title + Department`;
- pré-validação real de login, UPN, e-mail/SMTP, superior, OU e grupos;
- prévia do objeto AD sem executar escrita.

## Escrita no AD

`Automind:Mode=PilotWrite` está ativo somente para criação de usuário na OU piloto `07.Outros`. Na v0.1.10, `GroupWritesEnabled=true` apenas para a allowlist de grupos de escrita, atualmente limitada a `_CriaMovePastas` em `07.Outros`. O fluxo de criação pode gravar a membership selecionada antes de habilitar a conta, com releitura, auditoria e rollback somente das memberships criadas pela operação. `proxyAddresses`, `pwdLastSet` e Microsoft 365 continuam fora desta etapa.

## Configuração principal

`appsettings.json`:

- `Automind:Ad:Server` - controlador/endpoint AD usado nas consultas;
- `Automind:Ad:BaseDn` - DN do domínio;
- `Automind:Ad:PeopleSearchBase` - base para usuários/OUs;
- `Automind:Ad:AllowedOuDns` - allowlist de OUs pelo DN completo;
- `Automind:Ad:SuggestionExcludedOuDns` - OUs cujos usuários não entram na coorte de referência de grupos;
- `Automind:Ad:ProtectedGroupNames` - nomes privilegiados que nunca são pré-selecionados;
- `Automind:EmailDomain` - `automind.com.br`;
- `Automind:PrimarySmtpDomain` - `automind.co`;
- `Automind:JobTitleTranslations` - traduções de cargo aprovadas.

Não inserir usuário/senha administrativa no arquivo de configuração.

## Fluxo validado

1. Login AD.
2. Criar/obter chamado TOPdesk.
3. Importar pelo Bridge.
4. Conferir dados e selecionar OU.
5. Consultar grupos reais.
6. Clicar em `Validar no AD`.
7. Conferir a prévia.
8. Em `PilotWrite`, a criação do usuário só pode ocorrer em `07.Outros`; memberships são gravadas somente para grupos explicitamente autorizados em `GroupWriteAllowedDns`.

## Publicação

Branch atual: `release`.

Nesta fase não há tag Git automática. Os pacotes usam versão numérica curta; fazer commit e push para os remotes existentes `origin` (GitHub) e `azure` (Azure DevOps). O pipeline/release existente não deve ser alterado.

Consulte `Docs/10-FLUXO-GIT-E-PUBLICACAO.md` e `Docs/CHECKPOINT.md`.

## Pacote atual

- Versao do pacote: `0.1.10`
- Nome curto: `CadColab-v0.1.10.zip`
- Estado: `PilotWrite` em `07.Outros`; memberships integradas ao fluxo normal somente para grupos presentes em `GroupWriteAllowedDns` (atualmente apenas `_CriaMovePastas`)
- Versionamento de pacote nao cria tag Git automaticamente.


## Estado da linha piloto - v0.1.7

`Automind:Mode=PilotWrite` permanece ativo. A escrita de criação de usuário continua restrita a `07.Outros`; `GroupWritesEnabled=false`. A funcionalidade da v0.1.4 amplia a descoberta/seleção de grupos (comuns, exceções e busca manual), sem adicionar o usuário a grupos. Microsoft 365, Teams, `proxyAddresses` e `pwdLastSet` continuam bloqueados.


## Documentacao cumulativa

A v0.1.7 endurece a coorte de referência: além de excluir o próprio colaborador, usuários localizados em `SuggestionExcludedOuDns` (atualmente `07.Outros`) não participam da estatística de grupos. Isso impede que contas piloto/teste distorçam futuros cargos. O checkpoint principal e cumulativo, com as informacoes mais novas no topo. Documentos/checkpoints redundantes podem ser unificados e removidos somente depois que todo o conteudo for incorporado, sem perda de informacao. Consulte `Docs/CHECKPOINT.md` e `Docs/CONTEXTO-ATUAL.md`.

## v0.1.8

O projeto agora declara `net10.0-windows`, coerente com IIS + Active Directory. Isso elimina os avisos CA1416 sem suprimi-los artificialmente. A documentacao de checkpoint foi consolidada em `Docs/CHECKPOINT.md` (unico, cumulativo, mais novo primeiro).


## v0.1.10

- teste isolado de membership da v0.1.9 validado com sucesso no AD;
- membership integrada ao fluxo normal de criação;
- `GroupWritesEnabled=true` apenas para a allowlist `GroupWriteAllowedDns`;
- allowlist piloto contém somente `_CriaMovePastas` em `07.Outros`;
- grupos fora da allowlist bloqueiam a pré-validação;
- memberships são gravadas antes do enable final e confirmadas por releitura;
- em falha, rollback remove apenas memberships adicionadas pela própria operação;
- painel isolado de membership desativado após validação do teste técnico.

## v0.1.9

Teste real de membership isolado. O painel `MEMBERSHIP PILOTO` grava somente `teste.cadcolab` em `_CriaMovePastas`, usando a identidade `AUTOMIND\gMSA_CadColab$`, com releitura, auditoria e tentativa de rollback da associacao criada se houver falha. O fluxo normal de criacao permanece com `GroupWritesEnabled=false`.
