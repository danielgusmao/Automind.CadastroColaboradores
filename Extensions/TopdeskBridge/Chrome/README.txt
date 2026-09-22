AUTOMIND TOPDESK SESSION BRIDGE - Chrome
Versao 1.0.0

OBJETIVO
-------
Permitir que o Cadastro de Colaboradores consulte:
https://automind.topdesk.net/tas/api/incidents/number/<CHAMADO>

usando a sessao SAML do proprio operador ja autenticada no navegador.

A extensao:
- faz somente GET no TOPdesk;
- nao armazena senha;
- nao usa Application Token;
- nao usa conta tecnica;
- nao altera chamados;
- ao receber HTTP 401 pode abrir a tela SAML do TOPdesk;
- possui ponte pronta para o site http://cadastro.automind.com.br.

INSTALACAO - CHROME
-------------------------
1. Extraia esta pasta.
2. Abra a pagina de extensoes do navegador.
3. Ative o Modo do desenvolvedor.
4. Clique em Carregar sem compactacao.
5. Selecione esta pasta.
6. Fixe a extensao na barra se desejar.
7. Deixe o TOPdesk autenticado no mesmo perfil do navegador.
8. Clique no icone da extensao e teste I2609-0223.

BRAVE:
brave://extensions

CHROME:
chrome://extensions

INTEGRACAO COM O CADASTRO DE COLABORADORES
------------------------------------------
A extensao injeta content.js apenas em:
- http://cadastro.automind.com.br/*
- https://cadastro.automind.com.br/*

O site podera solicitar um chamado com:

window.postMessage({
  source: "AUTOMIND_CADASTRO",
  type: "TOPDESK_FETCH",
  ticket: "I2609-0223"
}, window.location.origin);

A extensao responde via window.postMessage com:
- TOPDESK_RESULT
- TOPDESK_LOGIN_REQUIRED
- TOPDESK_ERROR
- TOPDESK_BRIDGE_READY

Para abrir o login TOPdesk:

window.postMessage({
  source: "AUTOMIND_CADASTRO",
  type: "TOPDESK_LOGIN"
}, window.location.origin);

SEGURANCA
---------
A extensao tem host_permissions somente para:
- automind.topdesk.net
- cadastro.automind.com.br

Ela nao tenta ler outros sites.

OBSERVACAO
----------
Chrome e Brave usam Chromium/Manifest V3. Os pacotes sao tecnicamente
equivalentes; foram separados para facilitar instalacao, distribuicao e
documentacao interna.
