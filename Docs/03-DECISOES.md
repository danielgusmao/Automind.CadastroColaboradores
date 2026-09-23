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
