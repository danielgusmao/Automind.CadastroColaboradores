# CHECKPOINT

## Estado atual
Starter MVC .NET 10 criado para testes locais.

## Implementado no starter
- identidade visual Automind inicial;
- tela de login mock;
- dashboard;
- formulário editável de colaborador;
- cenário exemplo I2609-0223;
- sugestão visual de grupos por cargo (mock);
- área administrativa inicial;
- histórico placeholder;
- documentação das decisões.

## Ainda NÃO implementado
- autenticação AD real;
- leitura AD real;
- TOPdesk real;
- SQL Server;
- Teams real;
- Microsoft Graph;
- qualquer escrita no AD/M365/TOPdesk/Teams.

## Próximo passo recomendado
Validar o projeto localmente e aprovar o fluxo/telas. Depois implementar autenticação e leitura real do AD, mantendo escrita bloqueada.

## Ajuste aprovado - telefone celular
- padrão obrigatório de exibição: `(DD) 9 XXXX-XXXX`;
- exemplo aprovado: `(71) 9 8169-6721`;
- telefone com 10 dígitos recebe automaticamente o nono dígito após o DDD;
- código de país `55`, quando presente, é removido para a apresentação interna;
- máscara aplicada no formulário e normalização aplicada no ViewModel.
