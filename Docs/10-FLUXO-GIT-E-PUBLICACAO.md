# Fluxo Git e publicação

Decisão aprovada em 22/09/2026.

- GitHub: backup/espelho do código do projeto.
- Azure DevOps Repos: origem utilizada no processo de publicação existente.
- Azure Pipeline + Release: já criados e testados pelo responsável do projeto.
- Branch de trabalho/publicação atual: `release`.
- Não haverá versionamento numérico nesta etapa.
- Quando a branch `master` for criada/adotada, iniciar a política de versionamento do sistema.
- Alterações devem ser commitadas antes dos testes no servidor para evitar perda de trabalho.
- Não alterar remotes, pipelines ou releases sem aprovação prévia.
