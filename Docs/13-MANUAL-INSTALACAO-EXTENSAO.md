# Manual rápido - Automind TOPdesk Bridge

## Pasta padrão no computador do operador

```text
%USERPROFILE%\Automind\Extensoes\TopdeskBridge\
```

Manter uma das subpastas:

```text
Brave\
Chrome\
```

A pasta não deve ser removida nem movida depois de usar **Carregar sem compactação**, pois o navegador referencia os arquivos diretamente.

## Brave

1. Baixar o pacote Brave pelo Cadastro de Colaboradores.
2. Extrair para `%USERPROFILE%\Automind\Extensoes\TopdeskBridge\Brave`.
3. Abrir `brave://extensions`.
4. Ativar **Modo do desenvolvedor**.
5. Clicar em **Carregar sem compactação**.
6. Selecionar a pasta `Brave`.
7. Entrar normalmente no TOPdesk pelo SAML.

## Chrome

1. Baixar o pacote Chrome pelo Cadastro de Colaboradores.
2. Extrair para `%USERPROFILE%\Automind\Extensoes\TopdeskBridge\Chrome`.
3. Abrir `chrome://extensions`.
4. Ativar **Modo do desenvolvedor**.
5. Clicar em **Carregar sem compactação**.
6. Selecionar a pasta `Chrome`.
7. Entrar normalmente no TOPdesk pelo SAML.

## Regra do sistema

A página apenas informa que a extensão é necessária para a importação TOPdesk. O cadastro manual continua independente da extensão.

## Comportamento de importação

- sessão TOPdesk ativa: a extensão recebe HTTP 200 e envia o JSON ao Cadastro de Colaboradores;
- sessão TOPdesk ausente/expirada: recebe HTTP 401 e abre o login SAML;
- sem extensão: o site informa que a extensão deve ser instalada ou habilitada;
- nenhuma escrita no TOPdesk é realizada.
