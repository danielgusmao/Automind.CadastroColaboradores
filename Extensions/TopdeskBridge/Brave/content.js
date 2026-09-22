(() => {
  const VERSION = chrome.runtime.getManifest().version;
  const MARKER_ATTR = 'data-automind-topdesk-bridge-version';

  function markReady() {
    if (document.documentElement) {
      document.documentElement.setAttribute(MARKER_ATTR, VERSION);
    }
  }

  function showStatus(form, message, type = 'info') {
    const scope = form.closest('.panel') || document;
    let status = scope.querySelector('[data-topdesk-status]');

    if (!status) {
      status = document.createElement('div');
      status.dataset.topdeskStatus = '';
      form.insertAdjacentElement('afterend', status);
    }

    status.hidden = false;
    status.className = `topdesk-import-status is-${type}`;
    status.textContent = message;
  }

  function normalizeTicket(value) {
    return String(value || '').trim().toUpperCase().replace(/\s+/g, '');
  }

  function postIncidentToApplication(form, incident) {
    const importUrl = form.dataset.importUrl;
    const antiforgery = form.querySelector('input[name="__RequestVerificationToken"]')?.value;

    if (!importUrl || !antiforgery) {
      showStatus(form, 'Não foi possível preparar a importação do chamado.', 'error');
      return;
    }

    const postForm = document.createElement('form');
    postForm.method = 'post';
    postForm.action = importUrl;
    postForm.style.display = 'none';

    const tokenInput = document.createElement('input');
    tokenInput.type = 'hidden';
    tokenInput.name = '__RequestVerificationToken';
    tokenInput.value = antiforgery;

    const jsonInput = document.createElement('input');
    jsonInput.type = 'hidden';
    jsonInput.name = 'incidentJson';
    jsonInput.value = JSON.stringify(incident);

    postForm.append(tokenInput, jsonInput);
    document.body.appendChild(postForm);
    postForm.submit();
  }

  async function handleTopdeskSubmit(event) {
    const form = event.target;
    if (!(form instanceof HTMLFormElement)) return;
    if (!form.matches('[data-topdesk-import]')) return;

    // A extensão assume integralmente esta submissão. Isso evita depender de
    // window.postMessage/CustomEvent entre o JavaScript da página e o mundo
    // isolado do content script do Chromium.
    event.preventDefault();
    event.stopPropagation();
    event.stopImmediatePropagation();

    const input = form.querySelector('input[name="chamado"]');
    const ticket = normalizeTicket(input?.value);

    if (!ticket) {
      showStatus(form, 'Informe o número do chamado TOPdesk.', 'error');
      return;
    }

    if (!/^I\d{4}-\d{4}$/.test(ticket)) {
      showStatus(form, 'Número de chamado inválido. Use o formato I2609-0223.', 'error');
      return;
    }

    if (input) input.value = ticket;
    showStatus(form, `Consultando ${ticket} no TOPdesk...`, 'info');

    try {
      const response = await chrome.runtime.sendMessage({
        type: 'AUTOMIND_TOPDESK_FETCH',
        ticket
      });

      if (response?.ok && response?.incident) {
        showStatus(
          form,
          `Chamado ${response.incident.number || ticket} localizado. Importando dados...`,
          'success'
        );
        postIncidentToApplication(form, response.incident);
        return;
      }

      if (response?.needsLogin) {
        showStatus(
          form,
          'Sua sessão TOPdesk não está ativa. O login SAML será aberto; conclua o login e clique em Buscar chamado novamente.',
          'warning'
        );

        try {
          await chrome.runtime.sendMessage({ type: 'AUTOMIND_TOPDESK_LOGIN' });
        } catch {
          // A mensagem na tela já orienta o operador.
        }
        return;
      }

      showStatus(
        form,
        response?.message || 'Falha ao consultar o TOPdesk.',
        'error'
      );
    } catch (error) {
      showStatus(
        form,
        `Falha na extensão Automind TOPdesk Bridge: ${error?.message || String(error)}`,
        'error'
      );
    }
  }

  markReady();
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', markReady, { once: true });
  }

  // Captura a submissão antes do JavaScript da aplicação.
  document.addEventListener('submit', handleTopdeskSubmit, true);
})();
