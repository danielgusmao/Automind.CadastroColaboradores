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

## Regra de pacote completo durante preparacao de release - 24/09/2026

Enquanto o projeto estiver sendo preparado para um pacote completo de release/teste:

- o checkpoint deve permanecer dentro do proprio projeto em `Docs/`;
- nao gerar ZIP de checkpoint separado a cada validacao intermediaria;
- o pacote completo deve carregar o historico/checkpoint atualizado;
- checkpoint separado pode voltar a ser utilizado em uma nova fase de testes ou por solicitacao explicita;
- deploy inicial da linha piloto deve permanecer `Automind:Mode=ReadOnly`;
- ativacao `PilotWrite` nao faz parte implicitamente do deploy e exige etapa separada.

## Fluxo operacional atual no Visual Studio - 24/09/2026

Durante a fase inicial de ajustes e testes, o fluxo oficial do responsavel pelo projeto e:

1. O pacote completo preparado pelo assistente e usado para atualizar o projeto local aberto no Visual Studio.
2. O desenvolvimento local permanece na branch `release`.
3. Antes de qualquer envio, executar build local e conferir `git status`/`git status --short`.
4. As alteracoes sao registradas somente por commit nesta fase. Nao criar tag, release numerada, bump de versao ou branch adicional por causa de cada ajuste.
5. O Git e a trilha de backup/historico do codigo durante esta fase de ajustes iniciais.
6. Existem dois remotes no repositorio local:
   - `origin`: GitHub, usado como backup/espelho do codigo;
   - `azure`: Azure DevOps Repos, usado pelo fluxo de CI/CD existente.
7. Depois do commit, o fluxo normal e enviar a branch `release` para os dois remotes:

```powershell
git push origin release
git push azure release
```

8. O Azure DevOps recebe a atualizacao e o processo de Pipeline/Release existente publica o projeto no servidor para validacao visual e funcional.
9. O objetivo dessa publicacao frequente e permitir conferir rapidamente como os ajustes ficam no ambiente real enquanto o projeto ainda esta em construcao.
10. Nao realizar versionamento numerico nesta etapa. A politica de versao sera iniciada quando a linha principal/master for adotada conforme decisao ja registrada.

### Sequencia local de referencia

Os comandos abaixo representam o fluxo normal no Visual Studio/Developer PowerShell. A mensagem de commit deve descrever a alteracao da rodada e nao deve ser reutilizada mecanicamente.

```powershell
git branch --show-current
dotnet build -c Release
git status --short
git add .
git commit -m "<descricao objetiva da alteracao>"
git remote -v
git push origin release
git push azure release
```

### Pacote completo x artefato de publish

- O pacote completo enviado pelo assistente contem codigo-fonte, `Docs/` e checkpoint e serve para atualizar o projeto local no Visual Studio.
- O publish executado pelo CI/CD/Azure DevOps e o mecanismo normal de entrega ao servidor nesta fase.
- Nao copiar o ZIP completo de codigo-fonte diretamente sobre `C:\Automind.CadastroColaboradores` como se fosse um publish do IIS.
- O checkpoint deve acompanhar o pacote completo do projeto durante esta fase; nao gerar ZIP de checkpoint separado a cada ajuste.

### Cuidado com arquivos gerados

Antes de `git add .`, revisar sempre `git status --short`. A pasta local `artifacts/` pode conter publishes/ZIPs gerados para teste e nao deve ser incluida inadvertidamente em um commit sem decisao explicita. O `.gitignore` atual ainda deve ser revisado especificamente para essa pasta antes de automatizar essa exclusao.
