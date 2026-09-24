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

`Automind:Mode=PilotWrite` está ativo somente para criação de usuário na OU piloto `07.Outros`. Escrita de grupos permanece bloqueada por `GroupWritesEnabled=false`; `proxyAddresses`, `pwdLastSet` e Microsoft 365 continuam fora desta etapa.

## Configuração principal

`appsettings.json`:

- `Automind:Ad:Server` - controlador/endpoint AD usado nas consultas;
- `Automind:Ad:BaseDn` - DN do domínio;
- `Automind:Ad:PeopleSearchBase` - base para usuários/OUs;
- `Automind:Ad:AllowedOuDns` - allowlist de OUs pelo DN completo;
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
8. Em `PilotWrite`, a criação do usuário só pode ocorrer em `07.Outros`; memberships de grupos continuam sem escrita.

## Publicação

Branch atual: `release`.

Nesta fase não há tag Git automática. Os pacotes usam versão numérica curta; fazer commit e push para os remotes existentes `origin` (GitHub) e `azure` (Azure DevOps). O pipeline/release existente não deve ser alterado.

Consulte `Docs/10-FLUXO-GIT-E-PUBLICACAO.md` e `Docs/CHECKPOINT.md`.

## Pacote atual

- Versao do pacote: `0.1.5`
- Nome curto: `CadColab-v0.1.5.zip`
- Estado: `PilotWrite` para criação de usuário somente em `07.Outros`; escrita de grupos desabilitada
- Versionamento de pacote nao cria tag Git automaticamente.


## Estado da linha piloto - v0.1.5

`Automind:Mode=PilotWrite` permanece ativo. A escrita de criação de usuário continua restrita a `07.Outros`; `GroupWritesEnabled=false`. A funcionalidade da v0.1.4 amplia a descoberta/seleção de grupos (comuns, exceções e busca manual), sem adicionar o usuário a grupos. Microsoft 365, Teams, `proxyAddresses` e `pwdLastSet` continuam bloqueados.


## Documentacao cumulativa

A v0.1.5 restaura os checkpoints individuais e formaliza a regra de que a documentacao nunca pode diminuir entre pacotes. Consulte `Docs/00-LEIA-PRIMEIRO.md` e `Docs/CONTEXTO-ATUAL.md`.
