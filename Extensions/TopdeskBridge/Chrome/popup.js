const ticket = document.getElementById("ticket");
const testar = document.getElementById("testar");
const login = document.getElementById("login");
const saida = document.getElementById("saida");

function show(text) {
  saida.textContent = text;
}

async function consultar() {
  const numero = ticket.value.trim();

  if (!numero) {
    show("Informe o número do chamado.");
    return;
  }

  show("Consultando TOPdesk...");

  try {
    const result = await chrome.runtime.sendMessage({
      type: "AUTOMIND_TOPDESK_FETCH",
      ticket: numero
    });

    if (result?.ok) {
      const incident = result.incident || {};
      show(
        `HTTP ${result.status}\n` +
        `Número: ${incident.number || ""}\n` +
        `Descrição: ${incident.briefDescription || ""}\n` +
        `Operador: ${incident.operator?.name || ""}\n` +
        `Request presente: ${Boolean(incident.request)}\n\n` +
        `SUCESSO: sessão TOPdesk válida.`
      );
      return;
    }

    if (result?.needsLogin) {
      show(
        `HTTP 401\n\n` +
        `Sessão TOPdesk necessária.\n` +
        `Clique em "Entrar no TOPdesk", conclua o login e teste novamente.`
      );
      return;
    }

    show(
      `${result?.status ? `HTTP ${result.status}\n\n` : ""}` +
      `${result?.message || "Falha ao consultar o TOPdesk."}`
    );
  } catch (error) {
    show(`ERRO\n${error?.message || String(error)}`);
  }
}

async function abrirLogin() {
  show("Abrindo login do TOPdesk...");

  try {
    await chrome.runtime.sendMessage({
      type: "AUTOMIND_TOPDESK_LOGIN"
    });

    show(
      `Login do TOPdesk aberto.\n\n` +
      `Conclua o SAML e depois clique novamente em "Testar leitura".`
    );
  } catch (error) {
    show(`ERRO\n${error?.message || String(error)}`);
  }
}

testar.addEventListener("click", consultar);
login.addEventListener("click", abrirLogin);

ticket.addEventListener("keydown", (event) => {
  if (event.key === "Enter") {
    consultar();
  }
});
