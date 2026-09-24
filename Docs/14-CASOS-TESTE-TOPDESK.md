# Casos de teste - importacao TOPdesk

Objetivo: validar a extensao ativa e o parser do Cadastro de Colaboradores sem qualquer escrita no AD ou TOPdesk.

## I2609-0223

Esperado:
- Nome completo: `Gabriel Luis Lima Silva` (mantendo acentos quando presentes no JSON)
- Nome de guerra normalizado para 2 nomes: `Gabriel Silva`
- Login: `gabriel.silva`
- E-mail: `gabriel.silva@automind.com.br`
- Telefone: `(71) 9 8169-6721`
- Local: `Salvador - Sede`
- Superior: `Edson Neto`
- Cargo PT: `Analista de Sistemas de Automacao` (mantendo acentos na tela)
- Cargo EN: `Automation Systems Analyst`
- Departamento: `ENGENHARIA`

Regra especial: `Nome do cargo:` e `Em Ingles:` existentes em Observacao prevalecem sobre a descricao generica do cargo.

## I2508-0393

Esperado:
- Nome completo: `Arthur Martim Santana de Oliveira`
- Nome de guerra: `Arthur Oliveira`
- Login: `arthur.oliveira`
- E-mail: `arthur.oliveira@automind.com.br`
- Telefone recebido com 10 digitos deve ser normalizado para `(71) 9 9973-2603`
- Local: `salvador` (valor importado permanece editavel)
- Superior: `Luis Lopes`
- Cargo PT: `Estagiario` (mantendo acento na tela)
- Cargo EN: vazio nesta fase, pois nao foi informado no chamado
- Departamento: `BACKOFFICE`

## I2603-0141

Esperado:
- Nome completo: `Adriano Chagas de Lima`
- Nome de guerra: `Adriano Chagas`
- Login: `adriano.chagas`
- E-mail: `adriano.chagas@automind.com.br`
- Telefone: `(22) 9 9888-6150`
- Local: `Macae` (mantendo acento se vier no JSON)
- Superior: `Sanderson Caldas` (ponto final removido para futura busca no AD)
- Cargo PT: `Tecnico de Instrumentacao V` (mantendo acentos na tela)
- Cargo EN: vazio nesta fase
- Departamento: `GEMED`

## Resultado minimo para aprovar esta etapa

1. A extensao retorna HTTP 200 com a sessao SAML ativa.
2. O botao Buscar chamado do Cadastro importa o JSON sem abrir API Account/token.
3. O formulario Novo colaborador e preenchido com os dados esperados.
4. Telefone segue o padrao `(DD) 9 XXXX-XXXX`.
5. Nenhuma escrita em AD ou TOPdesk ocorre.


## Caso - login acima do limite do sAMAccountName

Objetivo: validar a indicacao visual e a protecao backend para login com mais de 20 caracteres.

Exemplo observado no piloto:

- login importado: `teste.provisionamento`;
- resultado esperado: contador acima de `20 / 20`, campo destacado e mensagem informando o excesso;
- a pre-validacao deve reprovar `Login valido e disponivel`;
- operador deve escolher conscientemente um login com no maximo 20 caracteres, sem truncamento automatico;
- exemplo usado no teste: `teste.cadcolab`.
