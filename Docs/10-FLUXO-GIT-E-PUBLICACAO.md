# Atualizacao corrente - v0.1.14

Validacao local:

```powershell
git branch --show-current;dotnet build -c Release;git status --short
```

Commit/push apos build aprovado:

```powershell
git add .;git commit -m "feat: adiciona historico e recuperacao M365 v0.1.14";git push origin release;git push azure release
```

---

# Atualizacao corrente - v0.1.13

Validacao antes do commit:

```powershell
git branch --show-current;dotnet build -c Release;git status --short
```

Commit/push apos build aprovado:

```powershell
git add .;git commit -m "feat: habilita atribuicao M365 controlada v0.1.13";git push origin release;git push azure release
```

Nao criar tag automaticamente.

---

# Fluxo Git e publicação

Decisão aprovada em 22/09/2026.

- GitHub: backup/espelho do código do projeto.
- Azure DevOps Repos: origem utilizada no processo de publicação existente.
- Azure Pipeline + Release: já criados e testados pelo responsável do projeto.
- Branch de trabalho/publicação atual: `release`.
- Pacotes completos de atualização são versionados numericamente a partir de `0.1.0`.
- O versionamento do pacote não cria tag Git automaticamente; a branch operacional continua `release`.
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

## Nomes curtos e versao de pacote - 24/09/2026

A partir desta rodada, a regra anterior de "sem versionamento numerico" fica substituida **para os pacotes de atualizacao**:

- todo pacote completo deve usar nome curto: `CadColab-vX.Y.Z.zip`;
- versao atual da linha piloto: `0.1.11`;
- ajustes incrementais desta linha usam patch: `0.1.1`, `0.1.2`, `0.1.3`, `0.1.4` etc.;
- mudanca funcional maior ainda dentro do piloto pode incrementar minor (`0.2.0`);
- `VERSION.txt` na raiz registra a versao do pacote;
- o projeto, solution, namespaces e nomes tecnicos existentes **nao devem ser renomeados** por causa desta regra;
- nao criar Git tag automaticamente. O fluxo continua por commit na branch `release`, com push para `origin` e `azure`, salvo solicitacao explicita;
- `Docs/CHECKPOINT.md` e o unico checkpoint cumulativo, com o mais novo no topo;
- documentos tematicos evoluem no proprio arquivo, sem copias por versao.

Motivo: evitar falhas de extracao/copia por comprimento excessivo de caminho e manter cada atualizacao claramente identificada.

## Integridade documental obrigatoria - regra vigente

Antes de gerar/entregar qualquer novo `CadColab-vX.Y.Z.zip`:

1. partir do pacote mais novo;
2. atualizar `Docs/CHECKPOINT.md` e `Docs/CONTEXTO-ATUAL.md`;
3. evoluir os documentos tematicos afetados;
4. incorporar qualquer conteudo relevante antes de remover arquivo redundante;
5. regenerar `Docs/DOC-MANIFEST-SHA256.txt`;
6. garantir que outra LLM/humano consiga continuar sem o chat.

O requisito e preservar informacao, nao quantidade de arquivos.

## Atualizacao v0.1.8 - plataforma Windows explicita

O projeto passa de `net10.0` para `net10.0-windows`. Como o sistema e exclusivo de IIS/Windows e usa `System.DirectoryServices`, os antigos 15 avisos CA1416 deixam de ser esperados. Nao usar `NoWarn` ou `#pragma` para mascara-los. O build local esperado passa a ser 0 erros / 0 CA1416.
