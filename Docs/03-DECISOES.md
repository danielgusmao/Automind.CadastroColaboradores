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
- O sistema apenas informa esse pré-requisito; o cadastro manual permanece disponível sem a extensão.
- Pasta padrão do operador: `%USERPROFILE%\Automind\Extensoes\TopdeskBridge\`.
- A extensão usa a sessão SAML já autenticada do operador e faz somente GET.
- Em HTTP 401, deve solicitar autenticação TOPdesk/SAML.
- O JSON do incidente é enviado ao backend do Cadastro apenas para parse e preenchimento do formulário.
- Nenhuma credencial TOPdesk é persistida pelo Cadastro.
