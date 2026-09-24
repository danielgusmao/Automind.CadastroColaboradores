# Decisões aprovadas

1. Nenhuma modificação no AD sem apresentação prévia e aprovação explícita.
2. Fase inicial do AD é somente leitura.
3. Login da aplicação será por usuário e senha do AD.
4. Acesso será restrito inicialmente a membros do grupo `_informatica`.
5. Nome de guerra deve conter exatamente dois nomes.
6. Nome de guerra gera login/e-mail no formato `nome.sobrenome@automind.com.br`, sem acentos.
7. Nome completo recebido em caixa alta será normalizado e permanecerá editável.
8. Empresa no AD será sempre `Automind`.
9. Telefone só será gravado futuramente no AD se `Deseja divulgar contato na intranet = Sim`.
10. Cargo deve ficar em inglês no AD. RH pode informar em português; o sistema sugere tradução e traduções aprovadas devem ser reutilizadas.
11. OUs serão listadas diretamente do AD para seleção manual.
12. Sugestão de grupos deve comparar usuários ativos com mesmo cargo/departamento e sugerir os grupos comuns a todos.
13. Usuário de referência é opcional e serve para inspeção/cópia assistida, não como regra principal.
14. Grupos privilegiados/protegidos nunca serão copiados automaticamente.
15. Senha inicial futura terá 14 caracteres e nunca será persistida em banco/log/histórico.
16. Histórico registrará chamado, data, colaborador, operador, OU, grupos, resultados e demais dados de auditoria; nunca senha.
17. Teams terá destinatários configuráveis e também o superior imediato; envio será confirmado pelo operador.
18. Antes de atribuir licença M365, o sistema deverá consultar SKUs existentes, usados e disponíveis.
19. Área administrativa fará parte do sistema.
20. Identidade visual deve usar cores e logo Automind.
21. Telefone celular deve ser sempre apresentado no formato `(DD) 9 XXXX-XXXX`, por exemplo `(71) 9 8169-6721`. Se a origem trouxer 10 dígitos (DDD + 8 dígitos), por se tratar de campo exclusivamente celular, o sistema acrescentará o nono dígito após o DDD. Se vier com código do país `55`, ele será removido para apresentação interna.


## Extensão TOPdesk Bridge - instalação e importação
- Importação de chamados TOPdesk depende da extensão Automind TOPdesk Bridge.
- O sistema apenas informa esse pré-requisito; o preenchimento manual permanece disponível sem a extensão, mas continua exigindo um número de chamado TOPdesk.
- Pasta padrão do operador: `%USERPROFILE%\Automind\Extensoes\TopdeskBridge\`.
- A extensão usa a sessão SAML já autenticada do operador e faz somente GET.
- Em HTTP 401, deve solicitar autenticação TOPdesk/SAML.
- O JSON do incidente é enviado ao backend do Cadastro apenas para parse e preenchimento do formulário.
- Nenhuma credencial TOPdesk é persistida pelo Cadastro.

22. Todo cadastro de colaborador deve estar vinculado a um chamado TOPdesk. O modo manual não significa "sem chamado": ele será usado quando existir um chamado simples que não utiliza o formulário estruturado de criação de usuário.
23. No modo manual, o número do chamado permanece obrigatório; apenas os dados do colaborador serão preenchidos manualmente.

## Decisões de implementação em 23/09/2026 - leitura real do AD

24. Após os testes de leitura no AD, foi autorizado substituir os mocks da tela por consultas reais no sistema.
25. Microsoft 365/Entra permanece pausado; licença não deve bloquear a fase atual.
26. A lista de OUs deve ser consultada diretamente do AD e filtrada por uma allowlist de DistinguishedNames configurável.
27. O botão `Validar no AD` deve executar somente consultas reais e nunca escrever no diretório.
28. A sugestão de grupos deve usar `Title + Department`, grupos diretos `memberOf`, incidência na coorte e proteção de grupos privilegiados.
29. Efeitos indiretos de grupos de segurança devem ser exibidos quando existirem.
30. Esta entrega completa continua sem `New-ADUser`, senha ou inclusão em grupos; a implementação/ativação de escrita exige autorização específica posterior.
31. Branch continua `release`, sem tag/versionamento numérico nesta fase; publicação por commit e push nos remotes existentes.

## Decisões de identidade técnica e delegação - 23/09/2026

32. Autorização humana e permissão técnica do AD ficam separadas.
33. `_informatica` continua sendo o grupo de autorização para uso administrativo do CadastroColaboradores; não receberá, por causa do sistema, ACLs de criação de usuários ou alteração de grupos no AD.
34. Um usuário de baixo privilégio administrativo poderá criar colaboradores pelo sistema desde que esteja autorizado na aplicação por `_informatica`.
35. As leituras/escritas de provisionamento não devem usar as credenciais do operador humano.
36. A identidade técnica alvo é uma gMSA exclusiva do CadastroColaboradores. Nome operacional sugerido: `gMSA_CadColab$`.
37. A gMSA deve ser autorizada para uso somente pelo servidor `SV052022-6121` (`10.1.2.21`) ou por grupo técnico de hosts estritamente equivalente.
38. Criar um grupo técnico dedicado para receber a delegação, sugerido `SG_CadastroColaboradores_AD_Writer`; somente a identidade técnica da aplicação deve participar desse mecanismo.
39. Não conceder Domain Admin, Enterprise Admin, Account Operators ou controle total do domínio à identidade técnica.
40. Delegar somente nas OUs aprovadas/allowlist as permissões necessárias à criação e manutenção dos atributos autorizados do usuário.
41. Delegação de grupos será separada da delegação de OUs; somente grupos explicitamente aprovados poderão receber alteração de `member` pela identidade técnica.
42. Grupos administrativos/protegidos, `_informatica` e equivalentes ficam fora da delegação de escrita do CadastroColaboradores.
43. O AD registrará a gMSA como identidade executora; a aplicação deve registrar também o usuário humano que iniciou a ação e o chamado TOPdesk correspondente.
44. Antes de habilitar escrita, testar em sequência: gMSA/host, leitura, criação fictícia, senha, manager, grupos e limpeza do objeto de teste.

## Politica obrigatoria de mudanca e rollback - 23/09/2026

45. Toda mudanca em AD, IIS ou permissoes deve seguir uma sequencia linear: leitura -> baseline -> comando de mudanca -> rollback preparado -> autorizacao -> uma mudanca -> validacao -> documentacao -> proxima etapa.
46. Ao primeiro resultado diferente do esperado, interromper o procedimento; nao executar a proxima mudanca.
47. Nao reiniciar controladores de dominio, AD DS/NTDS, KDC/Netlogon, servidor `10.1.2.21` ou IIS como parte normal deste projeto.
48. `iisreset` fica proibido no procedimento normal. Se a identidade do App Pool precisar ser ativada futuramente, tratar eventual recycle somente do pool como impacto localizado, em etapa separada e autorizada.
49. Nao executar `New-KdsRootKey`; o ambiente ja possui KDS root key confirmada.
50. Nao aplicar delegacoes em lote. O piloto comecara por uma unica OU e um grupo de cada vez, com ACL anterior registrada e rollback exato preparado.
51. Rollback deve remover somente o que o projeto adicionou; evitar restaurar ACL completa de forma cega para nao apagar mudancas concorrentes de outros administradores.
52. Nenhuma escrita pode ocorrer antes do checkpoint registrar estado anterior, objeto alvo, criterio de sucesso, criterio de parada e rollback.
53. A exclusao de objetos de teste tambem e uma operacao de escrita e exige autorizacao propria.
54. Toda solicitacao de comando deve indicar explicitamente o local de execucao: AD, servidor `10.1.2.21` ou maquina do operador.

## Contrato de escrita de usuarios no AD - 23/09/2026

55. A gMSA `gMSA_CadColab$` foi criada exclusivamente para o CadastroColaboradores e somente `SV052022-6121$` esta autorizado a recuperar sua senha gerenciada.
56. Toda escrita futura de usuario deve respeitar `Docs/16-CONTRATO-ESCRITA-AD.md`.
57. O sistema somente podera escrever atributos explicitamente autorizados; qualquer atributo novo exige aprovacao e documentacao antes da implementacao.
58. Criacao segura: usuario inicialmente desabilitado, atributos/senha/manager/grupos aplicados e validados, habilitacao somente como ultima etapa.
59. `manager` deve receber o DistinguishedName do superior previamente localizado no AD.
60. Grupos terao allowlist de escrita propria; grupos protegidos, administrativos ou fora da allowlist ficam bloqueados.
61. O sistema nao apagara automaticamente usuario real ja habilitado em caso de falha parcial; deve parar, manter/desabilitar a conta e exigir revisao do rollback.
62. Auditoria deve separar operador humano, chamado TOPdesk e identidade tecnica executora.

## 24/09/2026 - decisoes para o primeiro piloto de escrita

- implementar o caminho de escrita antes de ativa-lo;
- entregar/publicar inicialmente com `Automind:Mode=ReadOnly`;
- usar `PilotWrite` como unico valor que habilita o servico de escrita;
- manter allowlist de escrita separada e inicialmente restrita a `07.Outros`;
- revalidar `_informatica` no momento da escrita, sem confiar apenas no cookie de login;
- exigir que o worker esteja realmente executando como `AUTOMIND\gMSA_CadColab$`;
- bloquear memberships de grupos mesmo que a pre-validacao consiga sugeri-los;
- habilitar o usuario somente depois da releitura do objeto;
- nao excluir automaticamente um objeto criado parcialmente;
- nao persistir a senha temporaria em log/auditoria;
- validar a capacidade de escrita do arquivo de auditoria antes de ativar o primeiro piloto;
- build .NET 10 e publicacao ReadOnly sao gates obrigatorios antes da futura mudanca para `PilotWrite`.

## 24/09/2026 - fluxo de desenvolvimento, backup e deploy durante ajustes iniciais

- o responsavel trabalha o projeto diretamente no Visual Studio;
- branch operacional atual: `release`;
- `origin` aponta para GitHub e funciona como backup/espelho do codigo;
- `azure` aponta para Azure DevOps Repos e alimenta o fluxo de publicacao existente;
- cada pacote completo entregue pelo assistente e usado para atualizar o projeto local antes do build/commit;
- durante esta fase, registrar alteracoes somente com commits, sem tag e sem incremento de versao do projeto;
- depois do build e revisao de `git status`, fazer commit e push de `release` para `origin` e `azure`;
- o Azure DevOps Pipeline/Release publica a atualizacao no servidor para testes visuais e funcionais de cada rodada;
- Git e o mecanismo de historico/backup do codigo, mas nao substitui os baselines e rollbacks especificos de IIS/AD ja documentados;
- checkpoint deve permanecer dentro do pacote completo enquanto estivermos preparando e testando esta release;
- antes de `git add .`, conferir arquivos gerados, especialmente `artifacts/`, para evitar commit acidental de publish/ZIP local.

## 24/09/2026 - nomes curtos e versionamento dos pacotes

- iniciar versionamento numerico dos pacotes completos a partir de `0.1.0`;
- formato oficial do ZIP: `CadColab-vX.Y.Z.zip`;
- durante a linha piloto, ajustes normais incrementam o patch;
- registrar a versao em `VERSION.txt` e no checkpoint;
- a versao de pacote nao implica criacao de tag Git; tags continuam dependentes de solicitacao explicita;
- reduzir nomes/caminhos internos de documentacao; checkpoints historicos foram consolidados em `Docs/CP-HIST.md`;
- manter nomes tecnicos do projeto/solution/assemblies sem alteracao.

## 24/09/2026 - comportamento das sugestoes de grupos no piloto v0.1.1

- `GroupWritesEnabled=false` bloqueia **escrita de memberships**, mas nao deve apagar a utilidade visual da sugestao por cargo;
- grupos comuns ao cargo continuam marcados automaticamente;
- excecoes permanecem desmarcadas;
- grupos protegidos permanecem bloqueados/desabilitados;
- durante o piloto sem escrita de grupos, as marcacoes sao informativas: podem aparecer na pre-validacao e na previa, mas o comando de criacao envia `GroupDns=[]` e nao altera memberships;
- a criacao do usuario piloto nao deve exigir que o operador desmarque os grupos comuns;
- nao adicionar um indicador temporario de "OU autorizada para escrita piloto" na grade de pre-validacao; a tela continua exibindo `OU valida`;
- a seguranca de escrita da OU continua obrigatoria no backend por `WriteAllowedOuDns`, atualmente restrita a `07.Outros`;
- selecionar outra OU por engano pode passar a validacao de leitura, mas a criacao real deve continuar bloqueada pelo escopo de escrita do backend.



## 24/09/2026 - ativacao do PilotWrite na v0.1.3

- autorizacao explicita recebida para gerar a `v0.1.3` com `Automind:Mode=PilotWrite`;
- a ativacao permite escrita real apenas na OU `07.Outros`;
- `GroupWritesEnabled=false` permanece; sugestoes de grupos podem ficar marcadas visualmente, mas `GroupDns=[]` e nenhuma membership e gravada;
- Microsoft 365, Teams, TOPdesk, `proxyAddresses` e `pwdLastSet` continuam sem escrita;
- o servico de auditoria deve conseguir iniciar o arquivo local antes da primeira escrita AD; se isso falhar, a criacao deve parar antes de criar o usuario;
- rollback primario da aplicacao: voltar `Automind:Mode=ReadOnly` e publicar;
- a versao `0.1.3` passa a ser registrada no projeto/pacote, sem criar tag Git automaticamente.


## 24/09/2026 - seleção de grupos antes da escrita

- referências de cargo/departamento podem pesquisar todo o escopo de leitura do AD, independentemente da OU de destino do novo colaborador;
- `07.Outros` permanece o único escopo de escrita real do piloto de criação de usuário;
- não criar grupo fictício apenas para testar sugestão; grupos reais de outras OUs podem ser usados como referência somente leitura;
- o colaborador em cadastro deve ser excluído da própria coorte pelo `sAMAccountName`;
- seleção de acesso terá três origens: comuns ao cargo, exceções observadas e outros grupos pesquisados manualmente no AD;
- qualquer grupo manual deve ser resolvido/revalidado no backend e grupos protegidos continuam proibidos;
- `GroupWritesEnabled=false` permanece até fase posterior de delegação/allowlist de escrita de grupos.

## 24/09/2026 - politica primordial de documentacao cumulativa

- documentacao e checkpoint passam a ser requisito de integridade do pacote;
- nenhum documento/checkpoint historico pode ser removido em versoes futuras sem autorizacao explicita;
- `Docs/CP-HIST.md` e consolidacao adicional, nao substituto para `Docs/Checkpoints/`;
- toda interacao relevante deve acrescentar informacao ao checkpoint/historico;
- cada pacote deve ser compreensivel por outro humano/LLM sem acesso ao chat;
- antes da entrega, comparar o inventario de documentos com a versao anterior e confirmar que nada foi perdido;
- nomes curtos continuam desejaveis para evitar problemas de caminho, mas nunca a custa de apagar conteudo historico.
