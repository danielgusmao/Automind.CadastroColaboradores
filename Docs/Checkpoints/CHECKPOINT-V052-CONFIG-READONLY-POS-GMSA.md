# CHECKPOINT V052 - Configuracao ReadOnly apos gMSA

Data: 24/09/2026

## Validacao executada

Local: servidor `10.1.2.21`.
Tipo: somente leitura.

Resultado:

- `MODE: ReadOnly`;
- `AD SERVER: 10.1.2.1`;
- `PEOPLE SEARCH BASE: OU=Automind,DC=automind,DC=com,DC=br`;
- `AUTHORIZED GROUP: _informatica`;
- `OU ALLOWLIST TOTAL: 22`;
- `07.OUTROS NA ALLOWLIST: False`;
- `M365 ENABLED: False`;
- `APPSETTINGS SHA256: 9C6DB1F3CE04EB96C99157DA8E2A99CAC0808B83448F32D933507E315DAB441A`;
- pool `Started`;
- identidade `SpecificUser`;
- usuario `AUTOMIND\\gMSA_CadColab$`;
- `Test-ADServiceAccount=True`;
- HTTP `200`.

## Conclusao

A infraestrutura IIS/gMSA esta operacional, mas a aplicacao continua explicitamente bloqueada para escrita. A OU piloto `07.Outros` ainda nao esta disponivel na allowlist da aplicacao.

Revisao da linha de codigo atual mostrou apenas interfaces/servicos de leitura do AD. Nao existe servico de escrita registrado nem endpoint/action de criacao de usuario. Logo, mudar somente o modo/configuracao nao deve ser usado como atalho para habilitar provisionamento.

## Proximo gate

Antes do piloto real de criacao de usuario:

1. implementar escrita AD explicita e separada;
2. restringir server-side o primeiro destino a `07.Outros`;
3. manter M365 desabilitado;
4. preparar rollback de codigo/configuracao;
5. buildar e revisar;
6. publicar somente apos autorizacao;
7. criar somente usuario ficticio de teste no primeiro write end-to-end;
8. uma alteracao real por vez.

## Regra operacional de testes

Testes somente leitura/simulacao no mesmo local podem ser consolidados em uma unica linha de PowerShell, inclusive mais de tres verificacoes. Escritas reais permanecem isoladas, cada uma com efeito esperado, rollback e validacao preparados.
