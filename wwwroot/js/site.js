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
    const manualGroupsUrl = form.dataset.manualGroupsUrl;
    const validationUrl = form.dataset.validationUrl;
    const createUrl = form.dataset.createUrl;
    const m365LicenseUrl = form.dataset.m365LicenseUrl;
    const pilotMembershipUrl = form.dataset.pilotMembershipUrl;
    const writeEnabled = form.dataset.writeEnabled === 'true';
    const writeTargetOu = (form.dataset.writeTargetOu || '').trim();
    const groupWritesEnabled = form.dataset.groupWritesEnabled === 'true';
    const m365LicenseWritesEnabled = form.dataset.m365LicenseWritesEnabled === 'true';
    const m365SyncPollSeconds = Math.max(5, Number(form.dataset.m365SyncPollSeconds || 10));
    const m365SyncMaxWaitSeconds = Math.max(30, Number(form.dataset.m365SyncMaxWaitSeconds || 180));
    const groupsBody = form.querySelector('[data-groups-body]');
    const groupsTable = form.querySelector('[data-groups-table]');
    const groupsSummary = form.querySelector('[data-groups-summary]');
    const groupsEmpty = form.querySelector('[data-groups-empty]');
    const groupsStatus = form.querySelector('[data-groups-status]');
    const manualGroupQuery = form.querySelector('[data-manual-group-query]');
    const manualGroupResults = form.querySelector('[data-manual-group-results]');
    const manualGroupSelected = form.querySelector('[data-manual-group-selected]');
    const manualGroupChips = form.querySelector('[data-manual-group-chips]');
    const manualGroupsStatus = form.querySelector('[data-manual-groups-status]');
    const validationStatus = form.querySelector('[data-validation-status]');
    const preview = form.querySelector('[data-ad-preview]');
    const createButton = form.querySelector('[data-create-user]');
    const createStatus = form.querySelector('[data-create-status]');
    const passwordPanel = form.querySelector('[data-temporary-password]');
    const passwordValue = form.querySelector('[data-password-value]');
    const pilotMembershipButton = form.querySelector('[data-apply-pilot-membership]');
    const pilotMembershipStatus = form.querySelector('[data-pilot-membership-status]');
    const m365ProvisionStatus = form.querySelector('[data-m365-provision-status]');
    const retryM365Button = form.querySelector('[data-retry-m365]');
    const ticketSource = document.querySelector('[data-ticket-source]');
    const ticketHidden = form.querySelector('[name="Chamado"]');
    const loginInput = form.querySelector('[data-samaccountname]');
    const loginCounter = form.querySelector('[data-login-counter]');
    const loginLengthWarning = form.querySelector('[data-login-length-warning]');
    const loginMaxLength = 20;
    let lastValidationValid = false;
    let lastM365Request = null;

    const fieldValue = (name) => form.querySelector(`[name="${name}"]`)?.value?.trim() || '';
    const checked = (name) => Boolean(form.querySelector(`[name="${name}"]`)?.checked);

    const updateLoginLengthState = () => {
        if (!loginInput) return;
        const length = (loginInput.value || '').length;
        const exceeded = length > loginMaxLength;
        if (loginCounter) {
            loginCounter.textContent = `${length} / ${loginMaxLength}`;
            loginCounter.classList.toggle('is-over-limit', exceeded);
            loginCounter.classList.toggle('is-at-limit', length === loginMaxLength);
        }
        if (loginLengthWarning) {
            loginLengthWarning.hidden = !exceeded;
            loginLengthWarning.textContent = exceeded
                ? `O login possui ${length} caracteres e excede o limite do sAMAccountName em ${length - loginMaxLength}. Reduza para no máximo ${loginMaxLength} caracteres antes de validar.`
                : '';
        }
        loginInput.classList.toggle('is-over-limit', exceeded);
    };

    loginInput?.addEventListener('input', updateLoginLengthState);
    updateLoginLengthState();

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

    const selectedLicenseSkuIds = () => Array.from(form.querySelectorAll('.license-checkbox:checked:not(:disabled)'))
        .map((checkbox) => checkbox.value)
        .filter(Boolean);

    const selectedLicenseNames = () => Array.from(form.querySelectorAll('.license-checkbox:checked:not(:disabled)'))
        .map((checkbox) => checkbox.dataset.licenseName || checkbox.value)
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
        selectedLicenseSkuIds: selectedLicenseSkuIds(),
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
        createButton.disabled = !(lastValidationValid && scopeOk);

        if (!lastValidationValid) return;
        if (!scopeOk) {
            setStatus(createStatus, 'A pré-validação passou, mas a OU selecionada não pertence ao escopo de escrita do piloto.', 'warning');
        } else {
            const groupNote = !groupWritesEnabled && selectedGroupDns().length > 0
                ? ' Os grupos selecionados permanecem somente como prévia e o usuário não será adicionado a eles.'
                : '';
            setStatus(createStatus, `Pré-validação íntegra e escopo piloto confirmado. A criação permanece dependente do clique e da confirmação explícita.${groupNote}`, 'success');
        }
    };

    const updateGroupSummary = (groups) => {
        const common = groups.filter((g) => !g.protegido && g.totalComparados > 0 && g.encontradoEm === g.totalComparados).length;
        const exceptions = groups.filter((g) => !g.protegido && (g.totalComparados === 0 || g.encontradoEm !== g.totalComparados)).length;
        const protectedCount = groups.filter((g) => g.protegido).length;

        const commonEl = form.querySelector('[data-common-count]');
        const exceptionEl = form.querySelector('[data-exception-count]');
        const protectedEl = form.querySelector('[data-protected-count]');
        if (commonEl) commonEl.textContent = common;
        if (exceptionEl) exceptionEl.textContent = exceptions;
        if (protectedEl) protectedEl.textContent = protectedCount;
    };

    const appendGroupSection = (title, groups, type) => {
        if (!groupsBody || groups.length === 0) return;

        const section = document.createElement('tr');
        section.className = 'group-section-row';
        section.innerHTML = `<td colspan="4">${escapeHtml(title)}</td>`;
        groupsBody.appendChild(section);

        groups.forEach((g) => {
            const common = type === 'common';
            const protectedGroup = type === 'protected';
            const rowClass = protectedGroup ? 'row-protected' : (common ? 'row-common' : '');
            const statusClass = protectedGroup ? 'status-pill status-pill-danger' : (common ? 'status-pill status-pill-success' : 'status-pill');
            const statusText = protectedGroup ? 'Protegido' : (common ? 'Comum ao cargo' : 'Exceção');
            const indirect = Array.isArray(g.efeitosIndiretos) && g.efeitosIndiretos.length > 0
                ? `<small class="group-indirect">Acesso indireto: ${escapeHtml(g.efeitosIndiretos.join(', '))}</small>`
                : '';
            const disabled = protectedGroup ? 'disabled' : '';
            const isChecked = g.selecionado && !protectedGroup ? 'checked' : '';

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

    const updateManualSelectedVisibility = () => {
        if (!manualGroupSelected || !manualGroupChips) return;
        manualGroupSelected.hidden = manualGroupChips.querySelectorAll('[data-manual-group-dn]').length === 0;
    };

    const suggestedDns = () => new Set(Array.from(groupsBody?.querySelectorAll('.group-checkbox') || [])
        .map((checkbox) => checkbox.value)
        .filter(Boolean));

    const manualSelectedDns = () => new Set(Array.from(manualGroupChips?.querySelectorAll('[data-manual-group-dn]') || [])
        .map((element) => element.dataset.manualGroupDn)
        .filter(Boolean));

    const reconcileManualGroups = () => {
        if (!manualGroupChips) return;
        const suggestions = suggestedDns();
        manualGroupChips.querySelectorAll('[data-manual-group-dn]').forEach((chip) => {
            const dn = chip.dataset.manualGroupDn || '';
            if (!suggestions.has(dn)) return;
            const checkbox = Array.from(groupsBody?.querySelectorAll('.group-checkbox') || [])
                .find((item) => item.value === dn && !item.disabled);
            if (checkbox) checkbox.checked = true;
            chip.remove();
        });
        updateManualSelectedVisibility();
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

        const common = groups.filter((g) => !g.protegido && g.totalComparados > 0 && g.encontradoEm === g.totalComparados);
        const exceptions = groups.filter((g) => !g.protegido && (g.totalComparados === 0 || g.encontradoEm !== g.totalComparados));
        const protectedGroups = groups.filter((g) => g.protegido);

        appendGroupSection('Grupos comuns ao cargo', common, 'common');
        appendGroupSection('Exceções encontradas', exceptions, 'exception');
        appendGroupSection('Grupos protegidos', protectedGroups, 'protected');
        reconcileManualGroups();
    };

    const addManualGroup = (group) => {
        if (!manualGroupChips || !group || group.protegido || !group.distinguishedName) return;
        const dn = group.distinguishedName;
        if (suggestedDns().has(dn)) {
            const checkbox = Array.from(groupsBody?.querySelectorAll('.group-checkbox') || [])
                .find((item) => item.value === dn && !item.disabled);
            if (checkbox) checkbox.checked = true;
            invalidateValidation();
            setStatus(manualGroupsStatus, 'O grupo já estava nas sugestões e foi selecionado.', 'success');
            return;
        }
        if (manualSelectedDns().has(dn)) {
            setStatus(manualGroupsStatus, 'Esse grupo já foi adicionado manualmente.', 'warning');
            return;
        }

        const chip = document.createElement('div');
        chip.className = 'manual-group-chip';
        chip.dataset.manualGroupDn = dn;

        const checkbox = document.createElement('input');
        checkbox.type = 'checkbox';
        checkbox.className = 'group-checkbox manual-group-checkbox';
        checkbox.value = dn;
        checkbox.checked = true;
        checkbox.hidden = true;

        const text = document.createElement('span');
        const name = document.createElement('strong');
        name.textContent = group.nome || dn;
        const meta = document.createElement('small');
        meta.textContent = `${group.categoria || 'Grupo'} · ${group.escopo || ''}`.replace(/ · $/, '');
        text.append(name, meta);

        const remove = document.createElement('button');
        remove.type = 'button';
        remove.className = 'manual-group-remove';
        remove.setAttribute('aria-label', `Remover ${group.nome || 'grupo'}`);
        remove.textContent = '×';
        remove.addEventListener('click', () => {
            chip.remove();
            updateManualSelectedVisibility();
            invalidateValidation();
        });

        chip.append(checkbox, text, remove);
        manualGroupChips.appendChild(chip);
        updateManualSelectedVisibility();
        invalidateValidation();
        setStatus(manualGroupsStatus, 'Grupo adicionado manualmente à seleção.', 'success');
    };

    const renderManualResults = (groups) => {
        if (!manualGroupResults) return;
        manualGroupResults.innerHTML = '';
        const list = Array.isArray(groups) ? groups : [];
        if (list.length === 0) {
            manualGroupResults.hidden = true;
            return;
        }

        const knownSuggested = suggestedDns();
        const knownManual = manualSelectedDns();
        list.forEach((group) => {
            const item = document.createElement('div');
            item.className = `manual-group-result${group.protegido ? ' is-protected' : ''}`;

            const copy = document.createElement('div');
            const name = document.createElement('strong');
            name.textContent = group.nome || group.distinguishedName;
            const meta = document.createElement('small');
            meta.textContent = `${group.categoria || 'Grupo'} · ${group.escopo || ''}`.replace(/ · $/, '');
            copy.append(name, meta);
            if (Array.isArray(group.efeitosIndiretos) && group.efeitosIndiretos.length > 0) {
                const indirect = document.createElement('small');
                indirect.className = 'group-indirect';
                indirect.textContent = `Acesso indireto: ${group.efeitosIndiretos.join(', ')}`;
                copy.appendChild(indirect);
            }

            const button = document.createElement('button');
            button.type = 'button';
            button.className = 'btn btn-secondary btn-small';
            const alreadyListed = knownSuggested.has(group.distinguishedName);
            const alreadyManual = knownManual.has(group.distinguishedName);
            button.disabled = Boolean(group.protegido || alreadyListed || alreadyManual);
            button.textContent = group.protegido ? 'Protegido' : (alreadyListed ? 'Já sugerido' : (alreadyManual ? 'Já adicionado' : 'Selecionar'));
            if (!button.disabled) button.addEventListener('click', () => addManualGroup(group));

            item.append(copy, button);
            manualGroupResults.appendChild(item);
        });
        manualGroupResults.hidden = false;
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

    form.querySelectorAll('.license-checkbox').forEach((checkbox) => {
        const syncLicenseRow = () => checkbox.closest('[data-license-row]')?.classList.toggle('is-selected', checkbox.checked);
        checkbox.addEventListener('change', syncLicenseRow);
        syncLicenseRow();
    });

    const sleep = (milliseconds) => new Promise((resolve) => window.setTimeout(resolve, milliseconds));

    const markAssignedLicenses = (skuIds) => {
        (skuIds || []).forEach((skuId) => {
            const row = Array.from(form.querySelectorAll('[data-license-row]')).find((item) => (item.dataset.skuId || '').toLowerCase() === String(skuId).toLowerCase());
            if (!row) return;
            row.classList.add('is-assigned');
            const checkbox = row.querySelector('.license-checkbox');
            if (checkbox) checkbox.disabled = true;
            const state = row.querySelector('[data-license-state]');
            if (state) state.innerHTML = '<span class="status-pill status-pill-success">Atribuída</span>';
            const count = row.querySelector('[data-license-count]');
            const available = Number(row.dataset.availableUnits || 0);
            const enabled = Number(row.dataset.enabledUnits || 0);
            if (count && enabled < 1000000 && available > 0) {
                const nextAvailable = Math.max(0, available - 1);
                row.dataset.availableUnits = String(nextAvailable);
                count.innerHTML = `<b>${nextAvailable}</b> de ${enabled} licenças disponíveis`;
            }
        });
    };

    const applySelectedM365Licenses = async (request) => {
        if (!m365LicenseUrl || !m365LicenseWritesEnabled || !request || !Array.isArray(request.selectedLicenseSkuIds) || request.selectedLicenseSkuIds.length === 0)
            return true;

        lastM365Request = request;
        if (retryM365Button) retryM365Button.hidden = true;
        const startedAt = Date.now();

        while ((Date.now() - startedAt) / 1000 < m365SyncMaxWaitSeconds) {
            try {
                const result = await postJson(m365LicenseUrl, request);
                if (result.pendingSynchronization) {
                    const elapsed = Math.floor((Date.now() - startedAt) / 1000);
                    setStatus(m365ProvisionStatus, `Usuário criado no AD. Aguardando sincronização com o Entra (${elapsed}s/${m365SyncMaxWaitSeconds}s)... Nenhuma licença foi gravada ainda.`, 'warning');
                    await sleep(Math.max(5, Number(result.retryAfterSeconds || m365SyncPollSeconds)) * 1000);
                    continue;
                }

                if (!result.success) {
                    setStatus(m365ProvisionStatus, result.message || 'Não foi possível concluir o Microsoft 365.', result.requiresManualReview ? 'error' : 'warning');
                    if (retryM365Button && !result.requiresManualReview) retryM365Button.hidden = false;
                    return false;
                }

                markAssignedLicenses(result.addedSkuIds || []);
                setStatus(m365ProvisionStatus, result.message || 'Licenças Microsoft 365 atribuídas e confirmadas.', 'success');
                lastM365Request = null;
                return true;
            } catch (error) {
                setStatus(m365ProvisionStatus, `Falha ao chamar o Microsoft 365 (${error.message}). O AD já foi criado; revise antes de repetir.`, 'error');
                if (retryM365Button) retryM365Button.hidden = false;
                return false;
            }
        }

        setStatus(m365ProvisionStatus, `Usuário criado no AD, mas não apareceu no Entra dentro de ${m365SyncMaxWaitSeconds}s. Nenhuma licença foi atribuída. Use Repetir atribuição M365 após a sincronização.`, 'warning');
        if (retryM365Button) retryM365Button.hidden = false;
        return false;
    };

    ticketSource?.addEventListener('input', () => {
        syncTicket();
        invalidateValidation();
    });
    syncTicket();

    form.addEventListener('input', (event) => {
        if (event.target.matches('[data-manual-group-query]')) return;
        if (event.target.matches('input:not([type="hidden"]), select')) invalidateValidation();
    });
    form.addEventListener('change', (event) => {
        if (event.target.matches('[data-manual-group-query]')) return;
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
            const result = await postJson(groupsUrl, {
                cargo,
                departamento,
                login: fieldValue('Login'),
                nomeCompleto: fieldValue('NomeCompleto'),
                ouDistinguishedName: fieldValue('OuDistinguishedName')
            });
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

    const searchManualGroups = async () => {
        const termo = (manualGroupQuery?.value || '').trim();
        clearStatus(manualGroupsStatus);
        if (manualGroupResults) manualGroupResults.hidden = true;
        if (termo.length < 2) {
            setStatus(manualGroupsStatus, 'Digite pelo menos 2 caracteres para pesquisar.', 'warning');
            return;
        }

        const button = form.querySelector('[data-search-manual-groups]');
        const original = button?.textContent || 'Buscar no AD';
        if (button) {
            button.disabled = true;
            button.textContent = 'Buscando...';
        }
        try {
            const result = await postJson(manualGroupsUrl, { termo });
            if (!result.success) {
                setStatus(manualGroupsStatus, result.message || 'Não foi possível pesquisar os grupos.', 'error');
                return;
            }
            renderManualResults(result.groups || []);
            setStatus(manualGroupsStatus,
                (result.groups || []).length > 0 ? `${result.groups.length} grupo(s) encontrado(s) no AD.` : 'Nenhum grupo encontrado para esse nome.',
                (result.groups || []).length > 0 ? 'success' : 'warning');
        } catch (error) {
            setStatus(manualGroupsStatus, `Falha ao pesquisar o Active Directory (${error.message}).`, 'error');
        } finally {
            if (button) {
                button.disabled = false;
                button.textContent = original;
            }
        }
    };

    form.querySelector('[data-search-manual-groups]')?.addEventListener('click', searchManualGroups);
    manualGroupQuery?.addEventListener('keydown', (event) => {
        if (event.key !== 'Enter') return;
        event.preventDefault();
        searchManualGroups();
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
        button.textContent = 'Validando AD + M365...';
        try {
            const result = await postJson(validationUrl, buildRequest(false));
            if (!result.success) {
                setStatus(validationStatus, result.message || 'Não foi possível concluir a pré-validação.', 'error');
                return;
            }

            renderGroups(result.groups || []);
            updateChecks(result.checks || []);
            lastValidationValid = Boolean(result.valid);
            setStatus(validationStatus, result.message, result.valid ? 'success' : 'warning');
            if (result.valid && result.preview) renderPreview(result.preview);
            updateCreateEligibility();
        } catch (error) {
            setStatus(validationStatus, `Falha na pré-validação (${error.message}).`, 'error');
        } finally {
            button.disabled = false;
            button.innerHTML = original;
        }
    });

    createButton?.addEventListener('click', async () => {
        syncTicket();
        if (createButton.disabled || !lastValidationValid) return;

        const groups = selectedGroupDns();
        const licenses = selectedLicenseNames();
        const groupText = groups.length > 0 ? `\nGrupos selecionados: ${groups.length}` : '\nNenhum grupo selecionado.';
        const licenseText = licenses.length > 0
            ? `\nLicenças Microsoft 365: ${licenses.join(', ')}\nApós o AD, o sistema aguardará a sincronização com o Entra para atribuí-las.`
            : '\nNenhuma licença Microsoft 365 selecionada.';
        const message = `CONFIRMA a criação do usuário piloto no Active Directory?\n\nA conta será criada desabilitada, receberá atributos, senha, manager e somente memberships autorizadas, será relida e habilitada ao final.${groupText}${licenseText}`;
        if (!window.confirm(message)) return;

        clearStatus(createStatus);
        clearStatus(m365ProvisionStatus);
        hidePassword();
        if (retryM365Button) retryM365Button.hidden = true;
        const original = createButton.textContent;
        createButton.disabled = true;
        createButton.textContent = 'Criando usuário no AD...';

        try {
            const creationRequest = buildRequest(true);
            const result = await postJson(createUrl, creationRequest);
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

            if (creationRequest.selectedLicenseSkuIds.length > 0 && m365LicenseWritesEnabled) {
                createButton.textContent = 'Aguardando Microsoft 365...';
                await applySelectedM365Licenses({
                    chamado: creationRequest.chamado,
                    userPrincipalName: creationRequest.email,
                    selectedLicenseSkuIds: creationRequest.selectedLicenseSkuIds,
                    confirmacao: true
                });
            }
        } catch (error) {
            setStatus(createStatus, `Falha na chamada de criação (${error.message}). Interrompa o fluxo e revise o AD antes de tentar novamente.`, 'error');
        } finally {
            createButton.textContent = original;
            createButton.disabled = true;
        }
    });

    retryM365Button?.addEventListener('click', async () => {
        if (!lastM365Request) return;
        retryM365Button.disabled = true;
        try {
            await applySelectedM365Licenses(lastM365Request);
        } finally {
            retryM365Button.disabled = false;
        }
    });


    pilotMembershipButton?.addEventListener('click', async () => {
        if (!pilotMembershipUrl) return;
        const message = 'CONFIRMA a inclusão do usuário piloto existente no grupo _CriaMovePastas?\n\nEsta operação grava uma membership real no Active Directory usando a identidade técnica do aplicativo.';
        if (!window.confirm(message)) return;

        clearStatus(pilotMembershipStatus);
        const original = pilotMembershipButton.textContent;
        pilotMembershipButton.disabled = true;
        pilotMembershipButton.textContent = 'Aplicando membership...';
        try {
            const result = await postJson(pilotMembershipUrl, { confirmacao: true });
            setStatus(pilotMembershipStatus, result.message || 'Operação concluída.', result.success ? 'success' : 'error');
        } catch (error) {
            setStatus(pilotMembershipStatus, `Falha na chamada de membership (${error.message}).`, 'error');
        } finally {
            pilotMembershipButton.textContent = original;
            pilotMembershipButton.disabled = false;
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
