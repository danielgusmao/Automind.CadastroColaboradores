document.addEventListener('DOMContentLoaded', () => {
    const ticketInputs = document.querySelectorAll('input[name="chamado"]');

    ticketInputs.forEach((ticketInput) => {
        ticketInput.addEventListener('input', () => {
            ticketInput.value = ticketInput.value.toUpperCase().replace(/\s+/g, '');
        });
    });

    const somenteDigitos = (valor) => valor.replace(/\D/g, '');

    const removerCodigoPais = (digitos) => {
        if (digitos.length > 11 && digitos.startsWith('55')) {
            return digitos.slice(2);
        }
        return digitos;
    };

    const formatarTelefone = (valor, finalizar = false) => {
        let digitos = removerCodigoPais(somenteDigitos(valor)).slice(0, 11);

        if (finalizar && digitos.length === 10) {
            digitos = `${digitos.slice(0, 2)}9${digitos.slice(2)}`;
        }

        if (digitos.length === 0) return '';
        if (digitos.length <= 2) return `(${digitos}`;

        const ddd = digitos.slice(0, 2);
        const primeiro = digitos.slice(2, 3);
        const meio = digitos.slice(3, 7);
        const fim = digitos.slice(7, 11);

        let resultado = `(${ddd})`;
        if (primeiro) resultado += ` ${primeiro}`;
        if (meio) resultado += ` ${meio}`;
        if (fim) resultado += `-${fim}`;

        return resultado;
    };

    document.querySelectorAll('[data-phone-br]').forEach((input) => {
        input.value = formatarTelefone(input.value, true);

        input.addEventListener('input', () => {
            input.value = formatarTelefone(input.value, false);
        });

        input.addEventListener('blur', () => {
            input.value = formatarTelefone(input.value, true);
        });
    });

    const mostrarStatus = (form, mensagem, tipo = 'info') => {
        const scope = form.closest('.panel') || document;
        let status = scope.querySelector('[data-topdesk-status]');

        if (!status) {
            status = document.createElement('div');
            status.dataset.topdeskStatus = '';
            form.insertAdjacentElement('afterend', status);
        }

        status.hidden = false;
        status.className = `topdesk-import-status is-${tipo}`;
        status.textContent = mensagem;
    };

    const enviarJsonParaServidor = (form, incident) => {
        const importUrl = form.dataset.importUrl;
        const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;

        if (!importUrl || !token) {
            mostrarStatus(form, 'Não foi possível preparar a importação do chamado.', 'error');
            return;
        }

        const postForm = document.createElement('form');
        postForm.method = 'post';
        postForm.action = importUrl;
        postForm.style.display = 'none';

        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = token;

        const jsonInput = document.createElement('input');
        jsonInput.type = 'hidden';
        jsonInput.name = 'incidentJson';
        jsonInput.value = JSON.stringify(incident);

        postForm.append(tokenInput, jsonInput);
        document.body.appendChild(postForm);
        postForm.submit();
    };

    const REQUEST_EVENT = 'automind:topdesk:request';
    const RESPONSE_EVENT = 'automind:topdesk:response';
    const SOURCE_APP = 'AUTOMIND_CADASTRO';

    const novoRequestId = () => {
        if (window.crypto?.randomUUID) {
            return window.crypto.randomUUID();
        }

        return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
    };

    const enviarParaBridge = (tipo, payload = {}, timeoutMs = 5000) => {
        return new Promise((resolve, reject) => {
            const requestId = novoRequestId();
            let finalizado = false;

            const cleanup = () => {
                document.removeEventListener(RESPONSE_EVENT, onResponse);
            };

            const timer = window.setTimeout(() => {
                if (finalizado) return;
                finalizado = true;
                cleanup();
                reject(new Error('A extensão Automind TOPdesk Bridge não respondeu.'));
            }, timeoutMs);

            const onResponse = (event) => {
                let data;

                try {
                    data = typeof event.detail === 'string'
                        ? JSON.parse(event.detail)
                        : event.detail;
                } catch {
                    return;
                }

                if (!data || data.requestId !== requestId) return;

                finalizado = true;
                window.clearTimeout(timer);
                cleanup();
                resolve(data);
            };

            document.addEventListener(RESPONSE_EVENT, onResponse);

            document.dispatchEvent(new CustomEvent(REQUEST_EVENT, {
                detail: JSON.stringify({
                    source: SOURCE_APP,
                    requestId,
                    type: tipo,
                    ...payload
                })
            }));
        });
    };

    document.querySelectorAll('[data-topdesk-import]').forEach((form) => {
        form.addEventListener('submit', async (event) => {
            event.preventDefault();

            const input = form.querySelector('input[name="chamado"]');
            const chamado = input?.value.trim().toUpperCase();

            if (!chamado) {
                mostrarStatus(form, 'Informe o número do chamado TOPdesk.', 'error');
                return;
            }

            mostrarStatus(form, `Consultando ${chamado} no TOPdesk...`, 'info');

            try {
                const data = await enviarParaBridge('fetch', { ticket: chamado }, 5000);

                if (data.type === 'result' && data.incident) {
                    mostrarStatus(
                        form,
                        `Chamado ${data.incident.number || chamado} localizado. Importando dados...`,
                        'success');

                    enviarJsonParaServidor(form, data.incident);
                    return;
                }

                if (data.type === 'login-required') {
                    mostrarStatus(
                        form,
                        'Sua sessão TOPdesk não está ativa. Abrindo o login SAML; conclua o login e clique em Buscar chamado novamente.',
                        'warning');

                    try {
                        await enviarParaBridge('login', {}, 5000);
                    } catch {
                        // A mensagem principal ja orienta o operador.
                    }
                    return;
                }

                mostrarStatus(form, data.message || 'Falha ao consultar o TOPdesk.', 'error');
            } catch (error) {
                const marker = document.documentElement.getAttribute('data-automind-topdesk-bridge-version');

                if (marker) {
                    mostrarStatus(
                        form,
                        `A extensão Automind TOPdesk Bridge ${marker} está carregada, mas a ponte com a página não respondeu. Recarregue a extensão e tente novamente.`,
                        'error');
                } else {
                    mostrarStatus(
                        form,
                        'A extensão Automind TOPdesk Bridge não respondeu. Instale ou habilite a extensão e tente novamente.',
                        'error');
                }
            }
        });
    });

    document.querySelectorAll('.page-enter').forEach((element) => {
        requestAnimationFrame(() => element.classList.add('is-visible'));
    });
});
