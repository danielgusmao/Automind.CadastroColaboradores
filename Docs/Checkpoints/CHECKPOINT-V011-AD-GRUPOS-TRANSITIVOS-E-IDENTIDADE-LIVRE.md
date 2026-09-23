# CHECKPOINT V011 - Active Directory: grupos transitivos e identidade ficticia livre

Data: 2026-09-23

## Local dos testes desta rodada

Os comandos desta rodada foram executados **no Active Directory, com usuario de nivel elevado**.

Nenhuma escrita no AD, TOPdesk, IIS ou Microsoft 365 foi realizada.

## Resultado - metadados dos 8 grupos comuns do perfil

Perfil analisado:

- Title = `Automation Systems Analyst`;
- Department = `ENGENHARIA`;
- coorte previamente validada = 5 usuarios;
- 8 grupos comuns = intersecao 5/5.

### Grupos de seguranca

1. `_Tecnica SSA`
   - GroupCategory: Security;
   - GroupScope: Global;
   - adminCount: vazio;
   - isCriticalSystemObject: vazio;
   - membro direto de `_GAP2` e `_Tecnica`.

2. `_Todos SSA`
   - GroupCategory: Security;
   - GroupScope: Global;
   - adminCount: vazio;
   - isCriticalSystemObject: vazio;
   - sem parent group direto retornado.

3. `_Todos`
   - GroupCategory: Security;
   - GroupScope: Global;
   - adminCount: vazio;
   - isCriticalSystemObject: vazio;
   - sem parent group direto retornado.

4. `_Engenharia`
   - GroupCategory: Security;
   - GroupScope: Global;
   - adminCount: vazio;
   - isCriticalSystemObject: vazio;
   - membro direto de `_JumpServer`.

5. `_GA_E-CLIC`
   - GroupCategory: Security;
   - GroupScope: Global;
   - adminCount: vazio;
   - isCriticalSystemObject: vazio;
   - Description: `#I2604-0053`;
   - sem parent group direto retornado.

### Grupos de distribuicao

6. `Dist_Engenharia`
   - GroupCategory: Distribution;
   - GroupScope: Global;
   - Description: `grupo de distribuicao de e-mails`;
   - membro direto de `Dist_engenharia_suporte` e `Dist_Tecnico_SSA`.

7. `Dist_Todos`
   - GroupCategory: Distribution;
   - GroupScope: Global;
   - Description: `grupo de distribuicao de e-mails`;
   - memberOf retornou `Domain Users`.

8. `Dist_Todos_SSA`
   - GroupCategory: Distribution;
   - GroupScope: Global;
   - Description: `grupo de distribuicao de e-mails`;
   - sem parent group direto retornado.

## Conclusao sobre os grupos

Nenhum dos 8 grupos retornou `adminCount=1` ou `isCriticalSystemObject=True` na consulta realizada.

Porem, a verificacao revelou associacoes indiretas que precisam ser entendidas antes de qualquer escrita:

- `_Engenharia` esta aninhado diretamente em `_JumpServer`;
- `_Tecnica SSA` esta aninhado diretamente em `_GAP2` e `_Tecnica`.

Portanto, nao e suficiente validar somente os atributos do grupo diretamente selecionado. Antes de pre-selecionar automaticamente grupos de seguranca, o sistema/procedimento deve considerar os grupos ancestrais/transitivos e evitar conceder acesso indireto nao compreendido.

Essa observacao nao significa que os grupos estejam errados. Eles aparecem em 5/5 usuarios equivalentes e podem representar a configuracao correta do perfil atual. A pendencia e apenas confirmar o impacto da cadeia de grupos antes do teste real.

Os grupos de distribuicao devem permanecer classificados separadamente dos grupos de seguranca. Eles atendem finalidade de e-mail/distribuicao e nao devem ser tratados como equivalentes a grupos de autorizacao.

## Resultado - identidade ficticia

Identidade proposta:

- CN/Nome: `Teste Provisionamento Automind`;
- alias/sAMAccountName: `teste.provisionamento`;
- UPN: `teste.provisionamento@automind.com.br`;
- SMTP primario candidato: `teste.provisionamento@automind.co`;
- SMTP secundario candidato: `teste.provisionamento@automind.com.br`;
- OU candidata: `OU=Engenharia,OU=03.UDN,OU=Automind,DC=automind,DC=com,DC=br`.

A consulta de colisao nao retornou nenhum objeto para:

- sAMAccountName;
- UPN;
- mail nos dois dominios verificados;
- proxyAddresses SMTP nos dois enderecos;
- CN `Teste Provisionamento Automind` diretamente na OU Engenharia.

Conclusao: **a identidade ficticia esta livre nas verificacoes executadas**.

Nenhum objeto foi criado.

## Estado do primeiro teste ponta a ponta

Ja validado:

- OU de destino;
- perfil Title + Department;
- manager de referencia;
- alias ficticio e ausencia de colisao;
- UPN e enderecos SMTP candidatos;
- 8 grupos comuns do perfil;
- categoria/escopo dos 8 grupos.

Pendencia imediata antes de montar o objeto final:

1. mapear a cadeia transitiva dos grupos de seguranca comuns, principalmente `_Engenharia` e `_Tecnica SSA`;
2. confirmar que nenhum ancestral representa grupo privilegiado/protegido ou acesso que nao deva ser concedido automaticamente;
3. depois montar a pre-visualizacao completa do chamado TOPdesk e do objeto AD, sem escrita;
4. solicitar autorizacao explicita antes de criar o chamado TOPdesk ficticio;
5. solicitar nova autorizacao explicita antes de criar o usuario no AD.

## Regra de local dos testes

Toda solicitacao futura deve indicar claramente um dos tres locais:

- **AD**;
- **servidor 10.1.2.21**;
- **maquina do usuario**.
