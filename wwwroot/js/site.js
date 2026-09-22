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

    document.querySelectorAll('[data-topdesk-import]').forEach((form) => {
        form.addEventListener('submit', (event) => {
            event.preventDefault();

            const input = form.querySelector('input[name="chamado"]');
            const chamado = input?.value.trim().toUpperCase();

            if (!chamado) {
                mostrarStatus(form, 'Informe o número do chamado TOPdesk.', 'error');
                return;
            }

            mostrarStatus(form, `Consultando ${chamado} no TOPdesk...`, 'info');

            let respondeu = false;
            const timeout = window.setTimeout(() => {
                if (respondeu) return;
                mostrarStatus(
                    form,
                    'A extensão Automind TOPdesk Bridge não respondeu. Instale ou habilite a extensão e tente novamente.',
                    'error');
            }, 3500);

            const onMessage = (messageEvent) => {
                if (messageEvent.source !== window || messageEvent.origin !== window.location.origin) return;

                const data = messageEvent.data;
                if (!data || data.source !== 'AUTOMIND_TOPDESK_BRIDGE') return;

                if (!['TOPDESK_RESULT', 'TOPDESK_LOGIN_REQUIRED', 'TOPDESK_ERROR'].includes(data.type)) return;

                respondeu = true;
                window.clearTimeout(timeout);
                window.removeEventListener('message', onMessage);

                if (data.type === 'TOPDESK_RESULT' && data.incident) {
                    mostrarStatus(form, `Chamado ${data.incident.number || chamado} localizado. Importando dados...`, 'success');
                    enviarJsonParaServidor(form, data.incident);
                    return;
                }

                if (data.type === 'TOPDESK_LOGIN_REQUIRED') {
                    mostrarStatus(form, 'Sua sessão TOPdesk não está ativa. Abrindo o login SAML; conclua o login e clique em Buscar chamado novamente.', 'warning');
                    window.postMessage({
                        source: 'AUTOMIND_CADASTRO',
                        type: 'TOPDESK_LOGIN'
                    }, window.location.origin);
                    return;
                }

                mostrarStatus(form, data.message || 'Falha ao consultar o TOPdesk.', 'error');
            };

            window.addEventListener('message', onMessage);

            window.postMessage({
                source: 'AUTOMIND_CADASTRO',
                type: 'TOPDESK_FETCH',
                ticket: chamado
            }, window.location.origin);
        });
    });

    document.querySelectorAll('.page-enter').forEach((element) => {
        requestAnimationFrame(() => element.classList.add('is-visible'));
    });
});
