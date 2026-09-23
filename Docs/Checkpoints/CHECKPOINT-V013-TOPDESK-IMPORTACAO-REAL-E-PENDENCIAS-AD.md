# CHECKPOINT V013 - TOPdesk: chamado ficticio importado; pendencias antes da escrita no AD

Data: 2026-09-23

## Operacao realizada

Foi criado no TOPdesk, pela maquina do usuario/navegador, o chamado ficticio de teste:

- numero: `I2609-0295`;
- objetivo: validar o fluxo controlado TOPdesk -> CadastroColaboradores -> Active Directory;
- o chamado representa apenas um colaborador ficticio e nao corresponde a uma pessoa real.

Nenhuma escrita no Active Directory foi realizada nesta etapa.

## Resultado da importacao no CadastroColaboradores

O chamado `I2609-0295` foi carregado com sucesso pela tela `Colaboradores/ImportarTopdesk` por meio do fluxo TOPdesk/Bridge ja existente.

Os campos exibidos na aplicacao apos a importacao confirmam, entre outros:

- Nome completo: `Teste Provisionamento Automind`;
- Nome de guerra: `Teste Provisionamento`;
- Login gerado: `teste.provisionamento`;
- telefone celular: `(71) 9 9999-0000`;
- local de trabalho: `Salvador`;
- Cargo em portugues: `Analista de Sistemas de Automacao`;
- Departamento: `ENGENHARIA`;
- Empresa: `Automind`;
- Superior imediato: `Edson Neto`;
- existe campo de selecao `OU de destino`, ainda sem OU selecionada na evidencia recebida;
- `Cargo em ingles` permaneceu vazio na importacao e precisa de regra/mapeamento antes da criacao do usuario.

Conclusao: a etapa de criacao do chamado ficticio e a importacao TOPdesk -> CadastroColaboradores foram validadas no primeiro teste ponta a ponta.

## Evidencia importante - sugestao de grupos ainda nao representa o AD real

Na tela importada, a secao `Sugestao automatica pelo cargo` apresentou os grupos:

- `GG_Colaboradores` - 4/4;
- `GG_Engenharia` - 4/4;
- `VPN_Usuarios` - 4/4;
- `Projeto_Temporario` - 1/4;
- `Domain Admins` - 1/4 e classificado como protegido.

Esses nomes nao correspondem ao conjunto real levantado no Active Directory para o perfil `Automation Systems Analyst + ENGENHARIA`, cuja intersecao real 5/5 e:

1. `_Tecnica SSA`;
2. `Dist_Engenharia`;
3. `_Todos SSA`;
4. `_Todos`;
5. `_Engenharia`;
6. `_GA_E-CLIC`;
7. `Dist_Todos`;
8. `Dist_Todos_SSA`.

Portanto, a sugestao de grupos exibida nesta versao da tela nao deve ser usada como base para a primeira criacao real no AD. Antes da escrita, a aplicacao precisa consumir o servico de leitura real do AD ou a camada equivalente aprovada para retornar o perfil real.

## OU de destino

O formulario da aplicacao ja possui campo explicito `OU de destino`, com texto informando que a lista definitiva sera lida diretamente do Active Directory.

Regra mantida:

- a OU nao deve ser inferida somente por `Department`;
- a aplicacao deve exibir apenas OUs permitidas;
- internamente deve ser persistido/usado o `DistinguishedName` completo;
- para este teste, a OU candidata continua sendo `OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`;
- nenhum `New-ADUser` deve ocorrer antes de a OU ser validada e selecionada explicitamente.

## Pre-validacao exibida

A tela possui o bloco `Validar antes de criar`, com indicadores para:

- Login disponivel;
- UPN / e-mail disponivel;
- Superior localizado;
- OU valida;
- Grupos existentes;
- Licencas M365 disponiveis.

Decisao atual:

- Microsoft 365/Entra permanece pausado;
- a verificacao `Licencas M365 disponiveis` nao deve bloquear o primeiro teste de criacao no AD;
- neste momento, o proximo objetivo e tornar reais as validacoes de AD e somente depois habilitar a escrita controlada.

## Estado do primeiro teste ponta a ponta

Concluido:

1. dados ficticios definidos;
2. disponibilidade da identidade `teste.provisionamento` validada em leitura no AD;
3. perfil real de grupos `Automation Systems Analyst + ENGENHARIA` levantado;
4. cadeias transitivas de `_Engenharia` e `_Tecnica SSA` mapeadas;
5. chamado ficticio criado no TOPdesk: `I2609-0295`;
6. chamado importado com sucesso pelo CadastroColaboradores.

Pendente antes de qualquer escrita no AD:

1. substituir/ativar a leitura real de OUs no formulario;
2. substituir/ativar a sugestao real de grupos por cargo + departamento;
3. resolver `Superior imediato` para o DN real do usuario `Edson Neto`;
4. validar login, UPN/e-mail, OU e grupos usando o AD real;
5. definir/mapejar `Cargo em ingles` para `Automation Systems Analyst`;
6. manter Microsoft 365 fora do bloqueio desta fase;
7. mostrar a pre-visualizacao final do objeto AD e pedir autorizacao explicita antes de `New-ADUser`;
8. pedir autorizacao explicita separada antes de adicionar grupos.

## Proximo passo recomendado

O fluxo TOPdesk esta suficientemente validado para esta fase. O proximo trabalho deve ocorrer no projeto/aplicacao e permanecer inicialmente apenas em modo leitura:

1. ligar o formulario ao servico real de leitura de AD para OUs, superior, disponibilidade e grupos;
2. aplicar a allowlist de OUs aprovada/configuravel;
3. reproduzir na tela o perfil real 5/5 em vez dos grupos simulados;
4. repetir `Validar no AD - somente leitura` com o chamado `I2609-0295`;
5. somente depois disso preparar a rotina de criacao do usuario, ainda bloqueada ate autorizacao explicita.

Nenhum codigo deve ser alterado sem autorizacao explicita do usuario.

## Regra de local dos testes

Toda solicitacao futura deve indicar claramente um dos tres locais:

- **AD**;
- **servidor 10.1.2.21**;
- **maquina do usuario**.
