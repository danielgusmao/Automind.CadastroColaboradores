# CHECKPOINT V017 - Politica de mudanca AD/IIS, impacto zero e rollback linear

Data: 23/09/2026
Projeto: Automind.CadastroColaboradores
Status: REGRA OPERACIONAL OBRIGATORIA - nenhuma escrita autorizada neste checkpoint

## 1. Motivo

Antes de qualquer criacao de gMSA, grupo tecnico, delegacao de OU, permissao de `member`, alteracao de identidade do App Pool ou escrita de usuario no Active Directory, o projeto passa a adotar uma politica extremamente conservadora, linear, auditavel e reversivel.

O objetivo e impedir que a implantacao do provisionamento provoque indisponibilidade no Active Directory, controladores de dominio, IIS ou demais servicos corporativos.

## 2. Regra principal

Nenhuma alteracao sera executada apenas porque o comando foi preparado.

Para cada mudanca futura devem existir, ANTES da escrita:

1. teste somente leitura;
2. baseline do estado atual;
3. identificacao exata do objeto afetado;
4. comando de mudanca revisado;
5. comando/procedimento de rollback revisado;
6. criterio objetivo de sucesso;
7. criterio objetivo de parada;
8. autorizacao explicita do operador responsavel;
9. uma unica alteracao por vez;
10. validacao imediatamente apos a alteracao;
11. registro documental do resultado antes de avancar.

Ao primeiro resultado diferente do esperado, o procedimento para e nao executa a proxima etapa.

## 3. Operacoes proibidas durante esta implantacao

Sem uma necessidade tecnica excepcional, nova analise e autorizacao especifica, NAO executar:

- reinicializacao de controlador de dominio;
- `Restart-Computer` em DC;
- reinicializacao dos servicos AD DS/NTDS;
- reinicializacao de KDC/Netlogon como tentativa de correcao;
- `iisreset`;
- reinicializacao completa do servidor `10.1.2.21`;
- alteracao de Schema do AD;
- alteracao de GPO;
- alteracao de DNS do dominio;
- alteracao de configuracao KDS existente;
- `New-KdsRootKey` (o ambiente ja possui KDS root key validada);
- delegacao no dominio inteiro quando uma OU/grupo especifico e suficiente;
- inclusao da identidade tecnica em Domain Admins, Enterprise Admins, Account Operators ou grupos equivalentes;
- delegacao de escrita diretamente ao grupo `_informatica`;
- mudancas em lote de varias OUs/grupos sem validacao intermediaria.

## 4. Separacao de identidades

### Usuario humano

- autentica no CadastroColaboradores;
- deve estar autorizado pela regra da aplicacao, atualmente `_informatica`;
- pode possuir baixo privilegio administrativo no AD;
- nao fornece suas credenciais para a rotina de provisionamento;
- deve ser gravado na auditoria como operador humano.

### Identidade tecnica

Estado alvo planejado: gMSA exclusiva `gMSA_CadColab$`.

- executa as operacoes LDAP autorizadas;
- nao possui senha estatica em arquivo de configuracao;
- recebe apenas delegacao minima;
- nao recebe privilegio administrativo geral;
- AD registrara essa identidade como executor tecnico.

## 5. Politica linear por fase

### Fase 0 - diagnostico e baseline - SOMENTE LEITURA

Nenhuma alteracao.

Registrar:

- existencia/ausencia dos nomes planejados da gMSA e grupo tecnico;
- objeto do servidor `SV052022-6121$`;
- configuracao atual do App Pool `CadastroColaboradores`;
- ACL atual da pasta da aplicacao;
- ACL atual de cada OU que futuramente receber delegacao;
- ACL atual de cada grupo que futuramente receber `Write Members`;
- membros atuais do futuro grupo tecnico, caso ele ja exista;
- estado atual de replicacao/saude somente se uma verificacao adicional for necessaria.

Resultado esperado: inventario completo, nenhuma mudanca.

### Fase 1 - criar somente a gMSA

Esta fase NAO esta autorizada ainda.

Pre-condicoes:

- Fase 0 concluida;
- nome confirmado livre;
- PrincipalsAllowedToRetrieveManagedPassword revisado;
- comando de criacao apresentado;
- rollback apresentado;
- autorizacao explicita.

Mudanca unica: criar a gMSA.

Validacao imediata: objeto existe com somente os parametros aprovados.

Rollback previsto: remover somente a gMSA criada, desde que nenhuma etapa posterior dependa dela.

Nao reiniciar DC.

### Fase 2 - criar somente o grupo tecnico

Esta fase NAO esta autorizada ainda.

Mudanca unica: criar `SG_CadastroColaboradores_AD_Writer` no escopo/OU que forem aprovados.

Validar imediatamente categoria, escopo, DN e membros.

Rollback: remover somente o grupo se ainda nao houver ACLs/dependencias associadas; se houver, remover dependencias primeiro na ordem inversa.

### Fase 3 - associar identidade tecnica ao mecanismo de delegacao

Esta fase NAO esta autorizada ainda.

Uma unica associacao por vez.

Validar membro antes/depois.

Rollback: remover exatamente a associacao adicionada.

### Fase 4 - instalar/testar gMSA somente no `10.1.2.21`

Esta fase NAO esta autorizada ainda.

Antes:

- confirmar RSAT/comandos disponiveis;
- baseline local;
- nenhum reboot planejado.

Mudanca futura: instalar a service account localmente somente no servidor autorizado.

Validacao: `Test-ADServiceAccount` e leitura do estado local.

Rollback planejado: desinstalar localmente a service account, sem alterar DCs.

Se qualquer ferramenta solicitar reboot, PARAR e revisar; nao reiniciar automaticamente.

### Fase 5 - delegacao de UMA OU piloto

Esta fase NAO esta autorizada ainda.

Nao delegar todas as OUs de uma vez.

Antes de cada OU:

- capturar ACL atual;
- identificar somente ACEs que serao adicionadas;
- preparar a remocao dessas mesmas ACEs;
- obter autorizacao.

Comecar por uma unica OU piloto aprovada, preferencialmente a usada no teste controlado.

Depois da mudanca:

- conferir ACL;
- testar leitura;
- testar apenas a capacidade estritamente planejada;
- documentar;
- somente depois avaliar a proxima OU.

Rollback: remover somente as ACEs adicionadas pelo projeto. Evitar restaurar cegamente uma ACL completa, pois isso poderia apagar alteracoes concorrentes feitas por outros administradores.

### Fase 6 - delegacao de UM grupo piloto

Esta fase NAO esta autorizada ainda.

Tratar separadamente da OU.

Antes:

- capturar ACL do grupo;
- registrar membros atuais;
- preparar ACE exata de `member` e rollback exato.

Um grupo por vez.

Grupos privilegiados/protegidos permanecem fora do escopo.

### Fase 7 - preparar IIS sem impacto global

Esta fase NAO esta autorizada ainda.

Nunca usar `iisreset` como procedimento normal deste projeto.

Antes de qualquer troca de identidade:

- exportar/registrar configuracao atual do App Pool;
- registrar ACLs locais;
- garantir acesso local necessario para a identidade tecnica;
- definir rollback para `ApplicationPoolIdentity`;
- escolher janela controlada.

A ativacao de uma nova identidade no App Pool pode exigir reciclagem somente desse App Pool/processo. Isso e um impacto localizado na aplicacao e deve ser tratado como mudanca separada, anunciada e autorizada; nao deve reiniciar IIS inteiro nem o servidor.

Rollback: restaurar a configuracao anterior do App Pool e reciclar somente o pool afetado, se necessario.

### Fase 8 - leitura real usando a identidade tecnica

Antes de habilitar qualquer escrita no codigo:

- OUs;
- superior;
- grupos;
- disponibilidade de identidade;
- cadeia transitiva.

Tudo deve funcionar pela identidade tecnica.

Se leitura falhar, nenhuma escrita sera habilitada.

### Fase 9 - habilitar escrita por feature flag/configuracao controlada

A rotina de escrita deve permanecer desabilitada por padrao ate toda a infraestrutura anterior ser validada.

Antes de habilitar:

- build aprovado;
- auditoria pronta;
- OU piloto definida;
- grupos piloto definidos;
- rollback da aplicacao definido;
- autorizacao explicita.

### Fase 10 - criar UM usuario ficticio

Somente um objeto.

Sequencia:

1. pre-validacao integral;
2. exibir a previa;
3. confirmar login/UPN/CN livres;
4. confirmar OU;
5. confirmar manager;
6. confirmar atributos;
7. confirmar grupos selecionados;
8. autorizacao humana;
9. criar usuario;
10. validar objeto;
11. parar e documentar antes de senha/grupos adicionais se esses passos forem separados.

Nenhum lote de usuarios durante o piloto.

### Fase 11 - senha, manager e grupos

Cada capacidade e tratada como subetapa independente e validada imediatamente.

Ordem recomendada:

1. senha/estado da conta;
2. manager;
3. grupos, um conjunto aprovado por vez.

### Fase 12 - limpeza do usuario ficticio

Somente mediante decisao explicita.

Antes de excluir:

- confirmar que e o objeto ficticio correto;
- registrar DN/SID/grupos;
- confirmar que nao ha dependencia real.

A exclusao e uma nova escrita e tambem exige autorizacao.

## 6. Politica de rollback

Rollback segue a ordem inversa das mudancas e nunca e executado preventivamente sem necessidade.

Exemplo de dependencia:

1. escrita da aplicacao;
2. identidade do App Pool;
3. delegacoes de grupos;
4. delegacoes de OU;
5. associacao ao grupo tecnico;
6. grupo tecnico;
7. instalacao local da gMSA;
8. objeto gMSA.

Para desfazer, percorrer de cima para baixo na ordem inversa, sempre validando cada passo.

Nao remover gMSA/grupo tecnico enquanto ainda existirem ACLs, App Pools ou aplicacoes dependentes deles.

## 7. Evidencia obrigatoria por mudanca

Cada etapa futura deve registrar no checkpoint:

- data/hora;
- local da execucao: AD / `10.1.2.21` / maquina do operador;
- usuario humano que executou/autorizou;
- estado antes;
- comando executado;
- objeto alvo;
- resultado;
- estado depois;
- comando de rollback preparado;
- rollback executado ou nao;
- motivo para avancar ou parar.

## 8. Regra de local dos comandos

Todo comando passado ao operador deve vir explicitamente rotulado como um dos tres locais:

- **EXECUTAR NO AD**;
- **EXECUTAR NO SERVIDOR 10.1.2.21**;
- **EXECUTAR NA SUA MAQUINA**.

## 9. Estado ao final deste checkpoint

- nenhuma gMSA criada;
- nenhum grupo tecnico criado;
- nenhuma ACL alterada;
- nenhum usuario criado/removido;
- nenhum grupo teve membros alterados;
- App Pool continua como estava;
- nenhum DC reiniciado;
- IIS nao foi reiniciado;
- `10.1.2.21` nao foi reiniciado;
- fase atual: baseline somente leitura.
