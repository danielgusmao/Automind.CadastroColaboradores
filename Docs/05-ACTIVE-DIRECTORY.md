# Active Directory

## Estado atual - leitura real habilitada

A aplicação usa o Active Directory real para:

- autenticar usuário/senha no login;
- restringir o acesso inicial ao grupo configurado em `Automind:Ad:AuthorizedGroup`;
- listar OUs existentes diretamente no AD;
- filtrar OUs por `Automind:Ad:AllowedOuDns`, usando o DistinguishedName completo;
- localizar superior imediato entre usuários ativos;
- validar disponibilidade de `sAMAccountName`, UPN, `mail` e `proxyAddresses`;
- consultar usuários ativos por `Title + Department`;
- ler grupos diretos por `memberOf`;
- validar existência dos grupos selecionados;
- consultar grupos ancestrais para exibir efeitos indiretos de grupos de segurança.

As consultas de cadastro utilizam a identidade do processo IIS, sem senha administrativa armazenada na aplicação. Em `ApplicationPoolIdentity`, o acesso de rede deve ser validado com as permissões efetivas da identidade do servidor/aplicação.

## OUs

- Base de pesquisa: `Automind:Ad:PeopleSearchBase`.
- A lista é lida do AD em tempo real.
- Quando `AllowedOuDns` possui valores, somente DNs existentes na allowlist são exibidos.
- O valor submetido/validado é o DistinguishedName completo, nunca apenas o nome amigável.
- A aplicação valida novamente a OU antes de montar a prévia.

## Disponibilidade de identidade

Antes de qualquer futura criação, a pré-validação pesquisa no domínio inteiro colisões em:

- `sAMAccountName`;
- `userPrincipalName`;
- `mail` para os domínios corporativos envolvidos;
- `proxyAddresses` para o SMTP primário `@automind.co` e o alias `@automind.com.br`.

A busca não é limitada a objetos `user`, pois grupos/contatos também podem possuir atributos de e-mail conflitantes.

## SMTP

Regra confirmada no ambiente:

- UPN/mail normalmente: `usuario@automind.com.br`;
- `SMTP:` primário: `usuario@automind.co`;
- `smtp:` secundário: `usuario@automind.com.br`.

A prévia mostra ambos, mas nenhuma escrita é feita nesta entrega.

## Escrita continua proibida nesta entrega

Não existe rotina ativa para:

- `New-ADUser`;
- definir/redefinir senha;
- `Add-ADGroupMember` / remover membros;
- alterar `manager`;
- mover objetos;
- alterar `mail` ou `proxyAddresses`.

A próxima fase de escrita deverá ser apresentada separadamente e só poderá ser implementada/ativada após autorização explícita e definição da identidade técnica com permissão mínima necessária.
