(() => {
  const SOURCE_APP = "AUTOMIND_CADASTRO";
  const SOURCE_EXT = "AUTOMIND_TOPDESK_BRIDGE";

  function post(type, payload = {}) {
    window.postMessage({
      source: SOURCE_EXT,
      type,
      ...payload
    }, window.location.origin);
  }

  window.addEventListener("message", async (event) => {
    if (event.source !== window) return;
    if (event.origin !== window.location.origin) return;

    const data = event.data;
    if (!data || data.source !== SOURCE_APP) return;

    if (data.type === "TOPDESK_PING") {
      try {
        const response = await chrome.runtime.sendMessage({
          type: "AUTOMIND_TOPDESK_PING"
        });
        post("TOPDESK_PONG", response || {});
      } catch (error) {
        post("TOPDESK_ERROR", {
          message: error?.message || String(error)
        });
      }
      return;
    }

    if (data.type === "TOPDESK_FETCH") {
      try {
        const response = await chrome.runtime.sendMessage({
          type: "AUTOMIND_TOPDESK_FETCH",
          ticket: data.ticket
        });

        if (response?.ok) {
          post("TOPDESK_RESULT", response);
        } else if (response?.needsLogin) {
          post("TOPDESK_LOGIN_REQUIRED", response);
        } else {
          post("TOPDESK_ERROR", response || {
            message: "Falha desconhecida ao consultar o TOPdesk."
          });
        }
      } catch (error) {
        post("TOPDESK_ERROR", {
          message: error?.message || String(error)
        });
      }
      return;
    }

    if (data.type === "TOPDESK_LOGIN") {
      try {
        const response = await chrome.runtime.sendMessage({
          type: "AUTOMIND_TOPDESK_LOGIN"
        });
        post("TOPDESK_LOGIN_OPENED", response || {});
      } catch (error) {
        post("TOPDESK_ERROR", {
          message: error?.message || String(error)
        });
      }
    }
  });

  // Informa ao site que a extensão está disponível.
  post("TOPDESK_BRIDGE_READY", {
    version: chrome.runtime.getManifest().version
  });
})();
