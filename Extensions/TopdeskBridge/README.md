# Automind TOPdesk Session Bridge

Versao atual: **1.0.0**

Extensao auxiliar do projeto `Automind.CadastroColaboradores` para consultar incidentes do TOPdesk utilizando a **sessao SAML ja autenticada do operador no navegador**.

## Decisao de arquitetura

O sistema Cadastro de Colaboradores **nao armazena senha do TOPdesk**, **nao usa Application Token** e **nao depende de conta tecnica TOPdesk** para esse fluxo.

O operador continua se autenticando diretamente no TOPdesk pelo SAML corporativo. A extensao faz a leitura do endpoint de incidente usando a sessao ja existente no navegador.

Fluxo validado em 22/09/2026:

1. Operador logado no TOPdesk.
2. Extensao consulta `/tas/api/incidents/number/{numero}`.
3. Sessao valida: TOPdesk responde `HTTP 200` com JSON.
4. Sessao inexistente/expirada: TOPdesk responde `HTTP 401`.
5. A extensao pode abrir `https://automind.topdesk.net/tas/secure/login/saml`.
6. Depois do login, uma nova consulta retorna `HTTP 200`.

## Estrutura

- `Brave/` - pacote fonte para Brave.
- `Chrome/` - pacote fonte para Google Chrome.
- `Build-Packages.ps1` - gera ZIPs de distribuicao a partir dessas pastas.
- `CHANGELOG.md` - historico das alteracoes da extensao.

Brave e Chrome usam Chromium/Manifest V3. Nesta versao os codigos sao equivalentes, mas ficam separados para facilitar instalacao e futuras alteracoes especificas por navegador.

## Instalacao em ambiente de teste

### Brave

1. Abra `brave://extensions`.
2. Ative **Modo do desenvolvedor**.
3. Clique em **Carregar sem compactacao**.
4. Selecione a pasta:
   `Extensions/TopdeskBridge/Brave`
5. Mantenha essa pasta no mesmo local. O Brave referencia os arquivos diretamente.
6. Fixe a extensao na barra, se desejar.

### Chrome

1. Abra `chrome://extensions`.
2. Ative **Modo do desenvolvedor**.
3. Clique em **Carregar sem compactacao**.
4. Selecione a pasta:
   `Extensions/TopdeskBridge/Chrome`
5. Mantenha essa pasta no mesmo local. O Chrome referencia os arquivos diretamente.
6. Fixe a extensao na barra, se desejar.

## Importante sobre a pasta

Enquanto a extensao estiver sendo instalada com **Carregar sem compactacao**, a pasta e necessaria e nao deve ser apagada ou movida. O navegador executa os arquivos diretamente dela.

O ZIP serve apenas para transporte/backup. Ele deve ser extraido antes de usar `Carregar sem compactacao`.

Para uma distribuicao definitiva para varios colaboradores, deve ser estudada posteriormente uma instalacao corporativa/gerenciada, evitando que cada operador precise manter uma pasta manualmente.

## Atualizacao durante desenvolvimento

Depois de substituir/alterar arquivos da extensao:

1. Abra `brave://extensions` ou `chrome://extensions`.
2. Localize **Automind TOPdesk Bridge**.
3. Clique no botao de **Recarregar** da extensao.
4. Atualize a pagina `cadastro.automind.com.br`.
5. Repita o teste.

## Endpoints utilizados

Somente leitura:

`GET https://automind.topdesk.net/tas/api/incidents/number/{CHAMADO}`

Login SAML:

`https://automind.topdesk.net/tas/secure/login/saml`

## Restricoes de seguranca acordadas

- Nenhuma senha TOPdesk e armazenada pela extensao.
- Nenhum Application Token e armazenado pela extensao.
- Nenhuma conta tecnica e usada neste fluxo.
- A extensao nao cria nem altera incidentes.
- A extensao utiliza a sessao do proprio operador.
- Permissoes de host limitadas ao TOPdesk Automind e ao Cadastro de Colaboradores.
- O sistema principal continua validando acesso pelo Active Directory e grupo `_informatica`.

## Integracao com Cadastro de Colaboradores

A extensao possui `content.js` preparado para a pagina:

- `http://cadastro.automind.com.br/*`
- `https://cadastro.automind.com.br/*`

O site pode solicitar a consulta assim:

```javascript
window.postMessage({
    source: "AUTOMIND_CADASTRO",
    type: "TOPDESK_FETCH",
    ticket: "I2609-0223"
}, window.location.origin);
```

Possiveis respostas:

- `TOPDESK_RESULT`
- `TOPDESK_LOGIN_REQUIRED`
- `TOPDESK_ERROR`
- `TOPDESK_BRIDGE_READY`

Para abrir o login TOPdesk:

```javascript
window.postMessage({
    source: "AUTOMIND_CADASTRO",
    type: "TOPDESK_LOGIN"
}, window.location.origin);
```

## Casos validados

- `I2609-0223`
- `I2508-0393`
- `I2603-0141`

Os tres retornaram JSON ao abrir o endpoint com sessao TOPdesk valida.
