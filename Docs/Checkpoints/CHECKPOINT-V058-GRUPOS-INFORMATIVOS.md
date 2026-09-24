# CHECKPOINT V058 - CadColab v0.1.1 - sugestoes de grupos informativas no piloto

Data: 24/09/2026.


- teste ReadOnly com `teste.cadcolab` concluiu a pre-validacao AD sem pendencias;
- a OU `05.Terceiros-Ext` observada no teste foi selecao acidental e nao altera o escopo do piloto;
- grupos comuns continuam marcados automaticamente, excecoes desmarcadas e protegidos bloqueados;
- com `GroupWritesEnabled=false`, as marcacoes passam a ser explicitamente informativas e nao bloqueiam a criacao piloto;
- o backend de criacao continua enviando `GroupDns=[]`, logo nenhuma membership e gravada;
- removida a exigencia de desmarcar grupos antes da criacao;
- nao foi criado indicador temporario de OU de escrita na pre-validacao;
- `WriteAllowedOuDns` continua sendo a trava real e permanece restrita a `07.Outros`;
- `Mode=ReadOnly`, M365, proxyAddresses e pwdLastSet permanecem bloqueados;
- pacote completo versionado como `CadColab-v0.1.1.zip`.


