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

// Active Directory - consultas reais em modo somente leitura.
document.addEventListener('DOMContentLoaded', () => {
    const form = document.querySelector('[data-provisioning-form]');
    if (!form) return;

    const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    const groupsUrl = form.dataset.groupsUrl;
    const validationUrl = form.dataset.validationUrl;
    const groupsBody = form.querySelector('[data-groups-body]');
    const groupsTable = form.querySelector('[data-groups-table]');
    const groupsSummary = form.querySelector('[data-groups-summary]');
    const groupsEmpty = form.querySelector('[data-groups-empty]');
    const groupsStatus = form.querySelector('[data-groups-status]');
    const validationStatus = form.querySelector('[data-validation-status]');
    const preview = form.querySelector('[data-ad-preview]');

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
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(body)
        });

        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        return response.json();
    };

    const selectedGroupDns = () => Array.from(form.querySelectorAll('.group-checkbox:checked:not(:disabled)'))
        .map((checkbox) => checkbox.value)
        .filter(Boolean);

    form.querySelector('[data-search-groups]')?.addEventListener('click', async (event) => {
        const button = event.currentTarget;
        clearStatus(groupsStatus);
        if (preview) preview.hidden = true;

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
            element.textContent = Array.isArray(value) ? value.join('; ') : (value || '—');
        });
        preview.hidden = false;
        preview.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    };

    form.querySelector('[data-validate-ad]')?.addEventListener('click', async (event) => {
        const button = event.currentTarget;
        clearStatus(validationStatus);
        if (preview) preview.hidden = true;

        const request = {
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
            selectedGroupDns: selectedGroupDns()
        };

        const original = button.innerHTML;
        button.disabled = true;
        button.textContent = 'Validando no AD...';
        try {
            const result = await postJson(validationUrl, request);
            if (!result.success) {
                setStatus(validationStatus, result.message || 'Não foi possível validar o Active Directory.', 'error');
                return;
            }

            renderGroups(result.groups || []);
            updateChecks(result.checks || []);
            setStatus(validationStatus, result.message, result.valid ? 'success' : 'warning');
            if (result.valid && result.preview) renderPreview(result.preview);
        } catch (error) {
            setStatus(validationStatus, `Falha ao validar o Active Directory (${error.message}).`, 'error');
        } finally {
            button.disabled = false;
            button.innerHTML = original;
        }
    });
});
