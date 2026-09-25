# Escopo atual

Sistema interno Automind para cadastro e provisionamento controlado de colaboradores.

## Entradas
- chamado TOPdesk `CRIAÇÃO DE USUÁRIO`;
- preenchimento manual vinculado obrigatoriamente a chamado TOPdesk.

## Active Directory - fase concluida no piloto

Ja validado em `PilotWrite`:
- autenticacao/autorizacao;
- importacao e pre-validacao;
- criacao de usuario em `07.Outros`;
- atributos, senha e manager;
- sugestao/selecao de grupos;
- membership limitada por allowlist;
- readback, enable final e auditoria.

A escrita permanece restrita a `07.Outros` e, para grupos, somente aos DNs explicitamente autorizados.

## Microsoft 365 / Entra

Validado tecnicamente:
- autenticacao App-only por certificado;
- consulta de SKUs/licencas;
- leitura de usuarios/licencas;
- atualizacao controlada de `UsageLocation`;
- atribuicao e remocao direta de Microsoft 365 Business Standard em usuario piloto.

Escopo da v0.1.12:
- exibir inventario de licencas no formulario;
- mostrar total, consumido/disponivel e status;
- manter selecao/atribuicao de licencas desabilitada no sistema.

## Ainda fora da escrita da aplicacao
- atribuicao/remocao M365 pelo fluxo do CadColab;
- `proxyAddresses`;
- `pwdLastSet`;
- Teams;
- escrita de volta no TOPdesk.
