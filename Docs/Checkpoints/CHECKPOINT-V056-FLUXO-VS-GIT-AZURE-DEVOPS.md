# CHECKPOINT V056 - Fluxo Visual Studio, Git e Azure DevOps

Data: 24/09/2026.

## Objetivo

Registrar o fluxo real usado pelo responsavel enquanto o CadastroColaboradores esta em fase de ajustes iniciais e testes frequentes no servidor.

## Fluxo confirmado

1. O projeto e aberto e ajustado no Visual Studio.
2. O pacote completo entregue pelo assistente e usado para atualizar a copia local do projeto.
3. Branch operacional atual: `release`.
4. Executar build local antes do envio.
5. Revisar `git status`/`git status --short`.
6. Registrar a rodada com commit descritivo.
7. Nao criar tag, versao numerica ou bump de versao nesta fase.
8. `origin` aponta para GitHub e atua como backup/espelho do codigo.
9. `azure` aponta para Azure DevOps Repos e alimenta o fluxo de CI/CD existente.
10. Enviar `release` aos dois remotes.
11. O Azure DevOps Pipeline/Release publica a nova rodada no servidor para teste visual e funcional.
12. O responsavel valida o comportamento no ambiente real e a proxima rodada segue o mesmo ciclo.

## Sequencia de referencia

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

## Regras

- Git e o historico/backup de codigo desta fase.
- Rollbacks de AD e IIS continuam independentes e seguem seus documentos especificos.
- O pacote completo contem `Docs/` e checkpoint.
- Nao gerar checkpoint ZIP separado enquanto esta fase estiver ativa.
- Nao copiar o ZIP de codigo-fonte diretamente para a pasta publicada do IIS como substituto do publish.
- Antes de `git add .`, revisar arquivos gerados. A pasta `artifacts/` observada no projeto pode conter publish/ZIP local e nao deve entrar em commit por acidente.
- O `.gitignore` atual ainda nao exclui `artifacts/`; qualquer alteracao nele deve ser deliberada e revisada antes de aplicar.
