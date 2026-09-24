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

// Active Directory - leitura real e piloto de escrita controlado por configuração server-side.
document.addEventListener('DOMContentLoaded', () => {
    const form = document.querySelector('[data-provisioning-form]');
    if (!form) return;

    const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    const groupsUrl = form.dataset.groupsUrl;
    const validationUrl = form.dataset.validationUrl;
    const createUrl = form.dataset.createUrl;
    const writeEnabled = form.dataset.writeEnabled === 'true';
    const writeTargetOu = (form.dataset.writeTargetOu || '').trim();
    const groupWritesEnabled = form.dataset.groupWritesEnabled === 'true';
    const groupsBody = form.querySelector('[data-groups-body]');
    const groupsTable = form.querySelector('[data-groups-table]');
    const groupsSummary = form.querySelector('[data-groups-summary]');
    const groupsEmpty = form.querySelector('[data-groups-empty]');
    const groupsStatus = form.querySelector('[data-groups-status]');
    const validationStatus = form.querySelector('[data-validation-status]');
    const preview = form.querySelector('[data-ad-preview]');
    const createButton = form.querySelector('[data-create-user]');
    const createStatus = form.querySelector('[data-create-status]');
    const passwordPanel = form.querySelector('[data-temporary-password]');
    const passwordValue = form.querySelector('[data-password-value]');
    const ticketSource = document.querySelector('[data-ticket-source]');
    const ticketHidden = form.querySelector('[name="Chamado"]');
    let lastValidationValid = false;

    const fieldValue = (name) => form.querySelector(`[name="${name}"]`)?.value?.trim() || '';
    const checked = (name) => Boolean(form.querySelector(`[name="${name}"]`)?.checked);

    const escapeHtml = (value) => String(value ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');

    const setStatus = (element, message, type = 'info') => {
        if (!element) return;
        element.hidden = false;
        element.className = `topdesk-import-status is-${type}`;
        element.textContent = message;
    };

    const clearStatus = (element) => {
        if (!element) return;
        element.hidden = true;
        element.textContent = '';
    };

    const hidePassword = () => {
        if (passwordPanel) passwordPanel.hidden = true;
        if (passwordValue) passwordValue.textContent = '';
    };

    const syncTicket = () => {
        if (!ticketSource || !ticketHidden) return;
        ticketHidden.value = (ticketSource.value || '').trim().toUpperCase();
    };

    const selectedGroupDns = () => Array.from(form.querySelectorAll('.group-checkbox:checked:not(:disabled)'))
        .map((checkbox) => checkbox.value)
        .filter(Boolean);

    const buildRequest = (confirmacao = false) => ({
        chamado: fieldValue('Chamado'),
        nomeCompleto: fieldValue('NomeCompleto'),
        login: fieldValue('Login'),
        email: fieldValue('Email'),
        telefoneCelular: fieldValue('TelefoneCelular'),
        divulgarContato: checked('DivulgarContato'),
        localTrabalho: fieldValue('LocalTrabalho'),
        superiorImediato: fieldValue('SuperiorImediato'),
        cargoIngles: fieldValue('CargoIngles'),
        departamento: fieldValue('Departamento'),
        perfilUsuario: fieldValue('PerfilUsuario'),
        ouDistinguishedName: fieldValue('OuDistinguishedName'),
        selectedGroupDns: selectedGroupDns(),
        confirmacao
    });

    const invalidateValidation = () => {
        lastValidationValid = false;
        if (createButton) createButton.disabled = true;
        hidePassword();
    };

    const updateCreateEligibility = () => {
        if (!createButton) return;
        const selectedOu = fieldValue('OuDistinguishedName');
        const scopeOk = writeEnabled && writeTargetOu && selectedOu.localeCompare(writeTargetOu, undefined, { sensitivity: 'accent' }) === 0;
        const groupsOk = groupWritesEnabled || selectedGroupDns().length === 0;
        createButton.disabled = !(lastValidationValid && scopeOk && groupsOk);

        if (!lastValidationValid) return;
        if (!scopeOk) {
            setStatus(createStatus, 'A pré-validação passou, mas a OU selecionada não pertence ao escopo de escrita do piloto.', 'warning');
        } else if (!groupsOk) {
            setStatus(createStatus, 'A pré-validação passou, mas o piloto não grava grupos. Desmarque os grupos e valide novamente antes de criar.', 'warning');
        } else {
            setStatus(createStatus, 'Pré-validação íntegra e escopo piloto confirmado. A criação permanece dependente do clique e da confirmação explícita.', 'success');
        }
    };

    const updateGroupSummary = (groups) => {
        const common = groups.filter((g) => !g.protegido && g.encontradoEm === g.totalComparados).length;
        const exceptions = groups.filter((g) => !g.protegido && g.encontradoEm !== g.totalComparados).length;
        const protectedCount = groups.filter((g) => g.protegido).length;

        const commonEl = form.querySelector('[data-common-count]');
        const exceptionEl = form.querySelector('[data-exception-count]');
        const protectedEl = form.querySelector('[data-protected-count]');
        if (commonEl) commonEl.textContent = common;
        if (exceptionEl) exceptionEl.textContent = exceptions;
        if (protectedEl) protectedEl.textContent = protectedCount;
    };

    const renderGroups = (groups) => {
        if (!groupsBody) return;
        groupsBody.innerHTML = '';

        if (!Array.isArray(groups) || groups.length === 0) {
            if (groupsTable) groupsTable.hidden = true;
            if (groupsSummary) groupsSummary.hidden = true;
            if (groupsEmpty) groupsEmpty.hidden = false;
            return;
        }

        updateGroupSummary(groups);
        if (groupsTable) groupsTable.hidden = false;
        if (groupsSummary) groupsSummary.hidden = false;
        if (groupsEmpty) groupsEmpty.hidden = true;

        groups.forEach((g) => {
            const common = !g.protegido && g.encontradoEm === g.totalComparados;
            const rowClass = g.protegido ? 'row-protected' : (common ? 'row-common' : '');
            const statusClass = g.protegido ? 'status-pill status-pill-danger' : (common ? 'status-pill status-pill-success' : 'status-pill');
            const statusText = g.protegido ? 'Protegido' : (common ? 'Comum ao cargo' : 'Exceção');
            const indirect = Array.isArray(g.efeitosIndiretos) && g.efeitosIndiretos.length > 0
                ? `<small class="group-indirect">Acesso indireto: ${escapeHtml(g.efeitosIndiretos.join(', '))}</small>`
                : '';
            const disabled = g.protegido ? 'disabled' : '';
            const isChecked = g.selecionado && !g.protegido ? 'checked' : '';

            const tr = document.createElement('tr');
            tr.className = rowClass;
            tr.innerHTML = `
                <td><input class="group-checkbox" type="checkbox" value="${escapeHtml(g.distinguishedName)}" ${isChecked} ${disabled} /></td>
                <td>
                    <strong>${escapeHtml(g.nome)}</strong>
                    <small class="group-meta">${escapeHtml(g.categoria)} · ${escapeHtml(g.escopo)}</small>
                    ${indirect}
                </td>
                <td><span class="incidence">${g.encontradoEm} / ${g.totalComparados}</span></td>
                <td><span class="${statusClass}">${statusText}</span></td>`;
            groupsBody.appendChild(tr);
        });
    };

    const postJson = async (url, body) => {
        const response = await fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            cache: 'no-store',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(body)
        });

        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        return response.json();
    };

    const updateChecks = (checks) => {
        form.querySelectorAll('[data-ad-check]').forEach((element) => {
            element.classList.remove('is-pass', 'is-fail');
        });

        (checks || []).forEach((check) => {
            const element = form.querySelector(`[data-ad-check="${check.key}"]`);
            if (!element) return;
            element.classList.add(check.passed ? 'is-pass' : 'is-fail');
            element.textContent = `${check.passed ? '✓' : '✕'} ${check.label}`;
            element.title = check.message || '';
        });
    };

    const renderPreview = (data) => {
        if (!preview || !data) return;
        preview.querySelectorAll('[data-preview]').forEach((element) => {
            const key = element.dataset.preview;
            const value = data[key];
            element.textContent = Array.isArray(value) ? (value.length ? value.join('; ') : '—') : (value || '—');
        });
        preview.hidden = false;
        preview.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    };

    ticketSource?.addEventListener('input', () => {
        syncTicket();
        invalidateValidation();
    });
    syncTicket();

    form.addEventListener('input', (event) => {
        if (event.target.matches('input:not([type="hidden"]), select')) invalidateValidation();
    });
    form.addEventListener('change', (event) => {
        if (event.target.matches('input, select')) invalidateValidation();
    });

    form.querySelector('[data-search-groups]')?.addEventListener('click', async (event) => {
        const button = event.currentTarget;
        clearStatus(groupsStatus);
        clearStatus(createStatus);
        if (preview) preview.hidden = true;
        invalidateValidation();

        const cargo = fieldValue('CargoIngles');
        const departamento = fieldValue('Departamento');
        if (!cargo || !departamento) {
            setStatus(groupsStatus, 'Informe Cargo em inglês e Departamento antes de consultar os grupos.', 'warning');
            return;
        }

        const original = button.textContent;
        button.disabled = true;
        button.textContent = 'Consultando AD...';
        try {
            const result = await postJson(groupsUrl, { cargo, departamento });
            if (!result.success) {
                setStatus(groupsStatus, result.message || 'Não foi possível consultar os grupos.', 'error');
                return;
            }

            renderGroups(result.groups || []);
            setStatus(groupsStatus,
                (result.groups || []).length > 0
                    ? 'Grupos consultados diretamente no Active Directory.'
                    : 'Nenhum usuário ativo equivalente foi encontrado para cargo + departamento.',
                (result.groups || []).length > 0 ? 'success' : 'warning');
        } catch (error) {
            setStatus(groupsStatus, `Falha ao consultar o Active Directory (${error.message}).`, 'error');
        } finally {
            button.disabled = false;
            button.textContent = original;
        }
    });

    form.querySelector('[data-validate-ad]')?.addEventListener('click', async (event) => {
        const button = event.currentTarget;
        syncTicket();
        clearStatus(validationStatus);
        clearStatus(createStatus);
        hidePassword();
        if (preview) preview.hidden = true;
        lastValidationValid = false;
        if (createButton) createButton.disabled = true;

        const original = button.innerHTML;
        button.disabled = true;
        button.textContent = 'Validando no AD...';
        try {
            const result = await postJson(validationUrl, buildRequest(false));
            if (!result.success) {
                setStatus(validationStatus, result.message || 'Não foi possível validar o Active Directory.', 'error');
                return;
            }

            renderGroups(result.groups || []);
            updateChecks(result.checks || []);
            lastValidationValid = Boolean(result.valid);
            setStatus(validationStatus, result.message, result.valid ? 'success' : 'warning');
            if (result.valid && result.preview) renderPreview(result.preview);
            updateCreateEligibility();
        } catch (error) {
            setStatus(validationStatus, `Falha ao validar o Active Directory (${error.message}).`, 'error');
        } finally {
            button.disabled = false;
            button.innerHTML = original;
        }
    });

    createButton?.addEventListener('click', async () => {
        syncTicket();
        if (createButton.disabled || !lastValidationValid) return;

        const message = 'CONFIRMA a criação do usuário piloto no Active Directory?\\n\\nO backend criará a conta inicialmente desabilitada, definirá atributos, senha e manager, validará por releitura e habilitará somente ao final. Nenhum grupo será gravado.';
        if (!window.confirm(message)) return;

        clearStatus(createStatus);
        hidePassword();
        const original = createButton.textContent;
        createButton.disabled = true;
        createButton.textContent = 'Criando usuário no AD...';

        try {
            const result = await postJson(createUrl, buildRequest(true));
            lastValidationValid = false;

            if (!result.success) {
                const type = result.requiresManualReview ? 'error' : 'warning';
                const suffix = result.distinguishedName ? ` DN: ${result.distinguishedName}` : '';
                setStatus(createStatus, `${result.message || 'Criação não concluída.'}${suffix}`, type);
                return;
            }

            setStatus(createStatus, `${result.message} DN: ${result.distinguishedName}`, 'success');
            if (result.temporaryPassword && passwordPanel && passwordValue) {
                passwordValue.textContent = result.temporaryPassword;
                passwordPanel.hidden = false;
            }
        } catch (error) {
            setStatus(createStatus, `Falha na chamada de criação (${error.message}). Interrompa o fluxo e revise o AD antes de tentar novamente.`, 'error');
        } finally {
            createButton.textContent = original;
            createButton.disabled = true;
        }
    });

    form.querySelector('[data-copy-password]')?.addEventListener('click', async (event) => {
        const value = passwordValue?.textContent || '';
        if (!value) return;
        const button = event.currentTarget;
        try {
            if (navigator.clipboard && window.isSecureContext) {
                await navigator.clipboard.writeText(value);
            } else {
                const input = document.createElement('textarea');
                input.value = value;
                input.setAttribute('readonly', '');
                input.style.position = 'fixed';
                input.style.opacity = '0';
                document.body.appendChild(input);
                input.select();
                document.execCommand('copy');
                input.remove();
            }
            button.textContent = 'Copiada';
            setTimeout(() => { button.textContent = 'Copiar senha'; }, 1500);
        } catch {
            setStatus(createStatus, 'Não foi possível copiar automaticamente. Copie a senha exibida manualmente.', 'warning');
        }
    });
});
