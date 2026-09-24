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

**Continua bloqueada nesta entrega.**

O projeto contém a rotina de provisionamento piloto preparada, mas `Automind:Mode=ReadOnly` mantém a criação de usuários desabilitada. Escrita de grupos, `proxyAddresses`, `pwdLastSet` e Microsoft 365 continuam fora desta etapa.

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
8. Parar: nenhuma escrita é executada nesta versão.

## Publicação

Branch atual: `release`.

Nesta fase não há tag nem versionamento numérico. Fazer apenas commit e push para os remotes existentes `origin` (GitHub) e `azure` (Azure DevOps). O pipeline/release existente não deve ser alterado.

Consulte `Docs/10-FLUXO-GIT-E-PUBLICACAO.md` e `Docs/CHECKPOINT.md`.

## Pacote atual

- Versao do pacote: `0.1.2`
- Nome curto: `CadColab-v0.1.2.zip`
- Estado: `ReadOnly`
- Versionamento de pacote nao cria tag Git automaticamente.
