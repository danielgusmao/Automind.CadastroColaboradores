# CHECKPOINT V061 - CadColab v0.1.4 - selecao de grupos ampliada sem escrita

Data: 24/09/2026.

Mudança de prioridade autorizada durante os testes: antes de habilitar escrita de grupos, completar a experiência de seleção e validação.

- pesquisa de referência continua usando `Title + Department` em todas as OUs abaixo de `PeopleSearchBase`;
- o login do colaborador em edição é excluído da coorte, evitando o caso observado em que `teste.cadcolab` transformou uma referência de 5/5 em 5/6 depois de ser criado;
- interface separa visualmente **Grupos comuns ao cargo**, **Exceções encontradas** e **Grupos protegidos**;
- incluído campo **Outros grupos** com pesquisa por nome/sAMAccountName diretamente no AD;
- grupos manuais podem ser selecionados quando não aparecem na coorte;
- o backend revalida todos os DNs selecionados e rejeita inexistentes ou protegidos;
- grupos protegidos continuam bloqueados também na busca manual;
- a prévia passa a incluir grupos selecionados manualmente;
- `Automind:Mode=PilotWrite` permanece;
- `WriteAllowedOuDns` continua somente `07.Outros`;
- `GroupWritesEnabled=false` permanece e o writer continua recebendo `GroupDns=[]`;
- nenhuma associação de usuário a grupo é habilitada nesta versão;
- nenhuma alteração de ACL/AD faz parte deste pacote;
- `VERSION.txt` e `.csproj` atualizados para `0.1.4`;
- a antiga previsão de usar `v0.1.4` para habilitar escrita de grupos fica superada por esta decisão; a escrita de grupos será versionada somente depois de ACL/allowlist e teste controlado aprovados.

Validação executável neste ambiente: `site.js` passou em `node --check`. O ambiente de geração não possui .NET SDK; build .NET 10 continua obrigatório na máquina do Visual Studio antes de commit/push.
