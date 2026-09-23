# Checkpoint operacional - login e leitura real do Active Directory

Data de consolidação: 23/09/2026

## Login

- autenticação real via Active Directory;
- acesso inicial restrito ao grupo configurado em `Automind:Ad:AuthorizedGroup` (atualmente `_informatica`);
- credenciais usadas somente durante a validação e não persistidas;
- cookie autenticado e rate limit no endpoint de login.

## Consultas de cadastro

A partir da entrega completa de 23/09/2026, os mocks de OU e grupos deixam de ser registrados no `Program.cs`.

Serviços reais:

- `WindowsAdReadOnlyService`;
- `WindowsAccessSuggestionService`;
- `AdConnectionFactory`.

Capacidades ligadas à interface:

- OUs reais e allowlist por DistinguishedName;
- disponibilidade de login/UPN/mail/proxyAddresses;
- resolução de superior imediato;
- grupos reais por `Title + Department`;
- grupos ancestrais/efeitos indiretos;
- botão `Validar no AD` funcional em modo somente leitura;
- prévia completa do objeto que seria criado.

## Segurança

Nenhuma escrita no AD é implementada nesta entrega. Não existe `New-ADUser`, definição de senha ou inclusão em grupos.

A aplicação não armazena credencial administrativa para consultas. As leituras usam a identidade do processo IIS; a permissão efetiva deve ser confirmada após publicação no servidor `10.1.2.21`.

Microsoft 365/Entra permanece pausado e não participa da pré-validação atual.
