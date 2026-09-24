# CHECKPOINT V027 - gMSA criada e contrato de escrita AD

Data: 23/09/2026

## Alteracao executada

Foi criada no Active Directory a identidade tecnica exclusiva do CadastroColaboradores:

- `gMSA_CadColab$`
- habilitada;
- DNS `gMSA_CadColab.automind.com.br`;
- DN `CN=gMSA_CadColab,CN=Managed Service Accounts,DC=automind,DC=com,DC=br`;
- somente `SV052022-6121$` autorizado a recuperar a senha gerenciada.

## Nao alterado

Nesta etapa nao foram executados:

- `Install-ADServiceAccount` no servidor;
- alteracao de App Pool;
- criacao do grupo tecnico de delegacao;
- delegacao de OU;
- delegacao de `Write Members` em grupos;
- criacao/modificacao de usuario de colaborador;
- restart de AD, servidor ou IIS.

## Contrato de escrita

Foi criado `Docs/16-CONTRATO-ESCRITA-AD.md` para definir de forma objetiva:

- identidade tecnica;
- atributos que o sistema podera escrever;
- atributos bloqueados;
- regra de `manager` por DN;
- OU por allowlist de DN;
- grupos por allowlist separada;
- criacao inicial desabilitada e habilitacao somente ao final;
- auditoria;
- rollback;
- bloqueios obrigatorios.

## Estado

A gMSA existe, mas ainda nao possui delegacao nem esta ativa no IIS. A escrita de usuarios continua bloqueada.
