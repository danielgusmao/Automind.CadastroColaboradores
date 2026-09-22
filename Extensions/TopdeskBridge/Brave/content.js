(() => {
  const VERSION = chrome.runtime.getManifest().version;
  const REQUEST_EVENT = 'automind:topdesk:request';
  const RESPONSE_EVENT = 'automind:topdesk:response';
  const READY_EVENT = 'automind:topdesk:ready';
  const SOURCE_APP = 'AUTOMIND_CADASTRO';
  const SOURCE_EXT = 'AUTOMIND_TOPDESK_BRIDGE';

  function safeParse(value) {
    if (!value) return null;
    if (typeof value === 'object') return value;
    try { return JSON.parse(value); } catch { return null; }
  }

  function dispatchResponse(payload) {
    document.dispatchEvent(new CustomEvent(RESPONSE_EVENT, {
      detail: JSON.stringify({
        source: SOURCE_EXT,
        ...payload
      })
    }));
  }

  function markReady() {
    if (document.documentElement) {
      document.documentElement.setAttribute('data-automind-topdesk-bridge-version', VERSION);
    }

    document.dispatchEvent(new CustomEvent(READY_EVENT, {
      detail: JSON.stringify({
        source: SOURCE_EXT,
        version: VERSION
      })
    }));
  }

  async function processRequest(request) {
    if (!request || request.source !== SOURCE_APP || !request.requestId) {
      return;
    }

    try {
      if (request.type === 'ping') {
        const response = await chrome.runtime.sendMessage({
          type: 'AUTOMIND_TOPDESK_PING'
        });

        dispatchResponse({
          requestId: request.requestId,
          type: 'pong',
          ...response
        });
        return;
      }

      if (request.type === 'fetch') {
        const response = await chrome.runtime.sendMessage({
          type: 'AUTOMIND_TOPDESK_FETCH',
          ticket: request.ticket
        });

        if (response?.ok) {
          dispatchResponse({
            requestId: request.requestId,
            type: 'result',
            ...response
          });
        } else if (response?.needsLogin) {
          dispatchResponse({
            requestId: request.requestId,
            type: 'login-required',
            ...response
          });
        } else {
          dispatchResponse({
            requestId: request.requestId,
            type: 'error',
            ...(response || { message: 'Falha desconhecida ao consultar o TOPdesk.' })
          });
        }
        return;
      }

      if (request.type === 'login') {
        const response = await chrome.runtime.sendMessage({
          type: 'AUTOMIND_TOPDESK_LOGIN'
        });

        dispatchResponse({
          requestId: request.requestId,
          type: 'login-opened',
          ...response
        });
      }
    } catch (error) {
      dispatchResponse({
        requestId: request.requestId,
        type: 'error',
        message: error?.message || String(error)
      });
    }
  }

  document.addEventListener(REQUEST_EVENT, (event) => {
    processRequest(safeParse(event.detail));
  });

  // Compatibilidade com a primeira implementacao baseada em postMessage.
  // Nao utiliza event.source === window, pois content scripts Chromium executam
  // em um mundo isolado e essa verificacao pode impedir a ponte com a pagina.
  window.addEventListener('message', async (event) => {
    if (event.origin !== window.location.origin) return;

    const data = event.data;
    if (!data || data.source !== SOURCE_APP) return;

    const requestId = data.requestId || `legacy-${Date.now()}-${Math.random()}`;

    if (data.type === 'TOPDESK_FETCH') {
      await processRequest({
        source: SOURCE_APP,
        requestId,
        type: 'fetch',
        ticket: data.ticket
      });
    } else if (data.type === 'TOPDESK_LOGIN') {
      await processRequest({
        source: SOURCE_APP,
        requestId,
        type: 'login'
      });
    } else if (data.type === 'TOPDESK_PING') {
      await processRequest({
        source: SOURCE_APP,
        requestId,
        type: 'ping'
      });
    }
  });

  markReady();
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', markReady, { once: true });
  }
})();
