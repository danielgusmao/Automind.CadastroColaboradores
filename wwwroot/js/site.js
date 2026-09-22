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

    document.querySelectorAll('[data-topdesk-import]').forEach((form) => {
        // Fallback. Quando a extensão está ativa, o content script captura o
        // submit em capture=true antes deste listener e faz a importação.
        form.addEventListener('submit', (event) => {
            event.preventDefault();

            const versao = document.documentElement.getAttribute('data-automind-topdesk-bridge-version');

            if (versao) {
                mostrarStatus(
                    form,
                    `A extensão Automind TOPdesk Bridge ${versao} está carregada, mas não capturou a solicitação. Recarregue a extensão em brave://extensions ou chrome://extensions e atualize esta página.`,
                    'error'
                );
                return;
            }

            mostrarStatus(
                form,
                'A extensão Automind TOPdesk Bridge não está instalada ou habilitada. Instale a extensão para importar chamados do TOPdesk.',
                'error'
            );
        });
    });

    document.querySelectorAll('.page-enter').forEach((element) => {
        requestAnimationFrame(() => element.classList.add('is-visible'));
    });
});
