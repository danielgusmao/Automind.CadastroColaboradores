# 12 - Extensao TOPdesk Session Bridge

## Objetivo

Permitir que o `Automind.CadastroColaboradores` importe dados de chamados de criacao de usuario do TOPdesk utilizando a sessao SAML ja aberta pelo proprio operador no Brave ou Chrome.

## Motivo da extensao

Foi validado que:

1. Abrir diretamente no navegador autenticado o endpoint `https://automind.topdesk.net/tas/api/incidents/number/I2609-0223` retorna o JSON do incidente.
2. Sem sessao TOPdesk, o mesmo endpoint retorna `HTTP 401`.
3. Um `fetch` executado diretamente por `cadastro.automind.com.br` para `automind.topdesk.net` foi bloqueado por CORS/origem cruzada.
4. Uma extensao Chromium com `host_permissions` para o TOPdesk conseguiu consultar o endpoint usando a sessao ja autenticada e retornou `HTTP 200`.
5. Apos logout, a extensao recebeu `HTTP 401`.
6. Apos abrir o login SAML e autenticar novamente, a extensao voltou a receber `HTTP 200`.

Portanto, a extensao e a ponte entre o sistema interno e a sessao web individual do TOPdesk.

## Arquitetura aprovada

```text
Usuario
  |
  +--> Cadastro de Colaboradores
  |      |
  |      +--> login AD
  |      +--> valida grupo _informatica
  |      +--> solicita chamado TOPdesk
  |                 |
  |                 v
  |          Extensao TOPdesk Bridge
  |                 |
  |                 v
  |       Sessao SAML do operador
  |                 |
  |                 v
  |       GET /tas/api/incidents/number/{numero}
  |            |                  |
  |          200                401
  |            |                  |
  |          JSON          solicitar login SAML
  |            |                  |
  +------------+------------------+
```

## Decisoes que nao devem ser alteradas sem nova aprovacao

- Login principal do Cadastro de Colaboradores continua sendo Active Directory.
- Acesso ao sistema continua restrito ao grupo AD `_informatica`.
- A extensao nao substitui a autenticacao do Cadastro.
- A extensao nao guarda senha TOPdesk.
- A extensao nao guarda Application Token.
- Nao sera usada conta tecnica TOPdesk neste fluxo atual.
- Cada operador utiliza a propria sessao TOPdesk/SAML.
- A integracao TOPdesk desta fase e somente leitura.
- Nenhuma operacao de escrita em TOPdesk foi autorizada.
- Nenhuma operacao de escrita em Active Directory foi autorizada nesta fase.

## Instalacao em modo de desenvolvimento

Brave:

`brave://extensions`

Chrome:

`chrome://extensions`

Ativar `Modo do desenvolvedor`, escolher `Carregar sem compactacao` e apontar respectivamente para:

- `Extensions/TopdeskBridge/Brave`
- `Extensions/TopdeskBridge/Chrome`

A pasta deve permanecer no mesmo local enquanto a extensao estiver instalada dessa forma.

## Atualizacoes futuras

As extensoes ficam versionadas junto com o projeto em:

`Extensions/TopdeskBridge/`

Qualquer alteracao deve atualizar:

- `manifest.json` quando houver mudanca de versao/permissoes;
- `CHANGELOG.md`;
- esta documentacao quando houver mudanca de arquitetura;
- pacotes de Brave e Chrome.

Depois de atualizar os arquivos durante desenvolvimento, recarregar a extensao na pagina de extensoes do navegador.

## Distribuicao futura

O modo `Carregar sem compactacao` e adequado para desenvolvimento e homologacao. Para uso por varios operadores, deve ser avaliada uma distribuicao corporativa/gerenciada para Brave/Chrome, sem exigir que cada usuario mantenha manualmente uma pasta da extensao.

Isso sera tratado como etapa separada e nao deve ser implementado sem aprovacao.

## Chamados usados nos testes

- I2609-0223 - Gabriel Luis Lima Silva.
- I2508-0393 - Arthur Martim Santana de Oliveira.
- I2603-0141 - Adriano Chagas de Lima.

Os JSONs confirmaram que o campo `request` contem os dados do formulario de criacao de usuario necessarios para o parser inicial.
