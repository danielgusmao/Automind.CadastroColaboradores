const TOPDESK_BASE = "https://automind.topdesk.net";
const LOGIN_URL = `${TOPDESK_BASE}/tas/secure/login/saml`;

async function fetchIncident(ticket) {
  const normalized = String(ticket || "").trim().toUpperCase();

  if (!/^I\d{4}-\d{4}$/.test(normalized)) {
    return {
      ok: false,
      type: "validation",
      status: 0,
      message: "Número de chamado inválido. Use o formato I2609-0223."
    };
  }

  const url = `${TOPDESK_BASE}/tas/api/incidents/number/${encodeURIComponent(normalized)}`;

  try {
    const response = await fetch(url, {
      method: "GET",
      credentials: "include",
      headers: {
        "Accept": "application/json"
      },
      cache: "no-store"
    });

    const text = await response.text();
    let data = null;

    try {
      data = text ? JSON.parse(text) : null;
    } catch {
      data = null;
    }

    if (response.status === 401) {
      return {
        ok: false,
        type: "authentication",
        status: 401,
        needsLogin: true,
        loginUrl: LOGIN_URL,
        message: "Sessão do TOPdesk não encontrada ou expirada."
      };
    }

    if (!response.ok) {
      return {
        ok: false,
        type: "http",
        status: response.status,
        message: data?.errors?.[0]?.errorMessage || text || `TOPdesk respondeu HTTP ${response.status}.`
      };
    }

    return {
      ok: true,
      type: "success",
      status: response.status,
      ticket: normalized,
      incident: data
    };
  } catch (error) {
    return {
      ok: false,
      type: "network",
      status: 0,
      message: error?.message || String(error)
    };
  }
}

async function openTopdeskLogin() {
  try {
    const win = await chrome.windows.create({
      url: LOGIN_URL,
      type: "popup",
      width: 980,
      height: 760,
      focused: true
    });

    return {
      ok: true,
      windowId: win?.id ?? null,
      loginUrl: LOGIN_URL
    };
  } catch (error) {
    const tab = await chrome.tabs.create({
      url: LOGIN_URL,
      active: true
    });

    return {
      ok: true,
      tabId: tab?.id ?? null,
      loginUrl: LOGIN_URL
    };
  }
}

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (!message || typeof message !== "object") {
    return false;
  }

  if (message.type === "AUTOMIND_TOPDESK_FETCH") {
    fetchIncident(message.ticket).then(sendResponse);
    return true;
  }

  if (message.type === "AUTOMIND_TOPDESK_LOGIN") {
    openTopdeskLogin().then(sendResponse);
    return true;
  }

  if (message.type === "AUTOMIND_TOPDESK_PING") {
    sendResponse({
      ok: true,
      type: "pong",
      version: chrome.runtime.getManifest().version
    });
    return false;
  }

  return false;
});
