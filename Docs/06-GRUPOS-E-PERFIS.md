## v0.1.9 - primeiro teste real de membership

O teste real autorizado usa exclusivamente `_CriaMovePastas` em `07.Outros` e o usuario `teste.cadcolab`. A aplicacao altera o atributo `member` do objeto grupo; `memberOf` no usuario e reflexo calculado pelo AD. O teste nao muda a regra de sugestao por cargo e nao habilita ainda a escrita generica dos grupos comuns/excecoes.

# Grupos e perfis

## Implementação atual

A sugestão deixou de ser mock e consulta o AD real.

Dado `Cargo em inglês (Title) + Departamento (Department)`:

1. pesquisar usuários ativos equivalentes abaixo de `Automind:Ad:PeopleSearchBase`;
2. excluir usuários localizados nas OUs configuradas em `Automind:Ad:SuggestionExcludedOuDns` (atualmente `07.Outros`), evitando que contas piloto/teste distorçam a coorte;
3. excluir também o próprio colaborador por `sAMAccountName` e, quando disponível, por `CN + OU`;
4. ler os grupos diretos de cada usuário via `memberOf`;
5. contar a incidência de cada grupo;
6. consultar metadados do grupo no AD;
7. calcular ancestrais de grupos de segurança usando a regra LDAP `1.2.840.113556.1.4.1941`;
8. classificar grupos protegidos/privilegiados;
9. pré-selecionar somente grupos presentes em todos os usuários equivalentes e que não estejam protegidos.

O grupo primário não aparece em `memberOf` e, portanto, não faz parte desta comparação automática.

## Classificação

- `EncontradoEm == TotalComparados` e não protegido -> **Comum ao cargo**, pré-selecionado.
- Presente somente em parte da coorte -> **Exceção**, exibido sem seleção automática.
- Grupo com `adminCount=1`, `isCriticalSystemObject`, nome protegido configurado ou ancestral protegido -> **Protegido**, nunca pré-selecionado.

A interface também mostra:

- categoria `Security` ou `Distribution`;
- escopo `Global`, `Universal` ou `DomainLocal`;
- efeitos indiretos de grupos de segurança quando houver grupos ancestrais.

## Seleção manual - Outros grupos

Além das sugestões pela coorte, a interface possui o campo **Outros grupos**:

- pesquisa grupos reais no AD por nome/sAMAccountName;
- exige no mínimo 2 caracteres;
- mostra categoria, escopo e efeitos indiretos;
- grupos protegidos aparecem bloqueados;
- grupo já presente nas sugestões não é duplicado;
- grupo selecionado manualmente participa da pré-validação e da prévia;
- o backend resolve novamente cada DN selecionado e rejeita grupo inexistente ou protegido, sem confiar apenas no navegador.

Enquanto `GroupWritesEnabled=false`, tanto sugestões quanto grupos manuais permanecem apenas na prévia e o usuário não é adicionado a nenhum grupo.

## Perfil real validado - Automation Systems Analyst / ENGENHARIA

Nos testes de 23/09/2026, 5 usuários ativos equivalentes apresentaram 8 grupos comuns 5/5:

1. `_Técnica SSA`;
2. `Dist_Engenharia`;
3. `_Todos SSA`;
4. `_Todos`;
5. `_Engenharia`;
6. `_GA_E-CLIC`;
7. `Dist_Todos`;
8. `Dist_Todos_SSA`.

Também foram confirmados efeitos indiretos:

- `_Engenharia` -> `_JumpServer`;
- `_Técnica SSA` -> `_GAP2` e `_Tecnica`.

Esses dados não estão hardcoded no serviço: são resultado esperado do AD real para o perfil testado.

## Usuário de referência

Permanece fora desta entrega. O botão da interface está desabilitado para não simular uma funcionalidade que ainda não foi implementada.

## Regra de escopo confirmada - 24/09/2026

A OU de destino do novo usuario e o escopo usado para pesquisa de referencia sao conceitos diferentes:

- escrita piloto do usuario permanece restrita a `07.Outros`;
- pesquisa de usuarios equivalentes e grupos pode consultar todas as OUs permitidas pelo escopo de leitura;
- grupos reais de outras OUs servem como referencia de cargo sem receber alteracao durante essa fase;
- nao criar grupo ficticio apenas para testar a sugestao;
- `_CriaMovePastas` existente em `07.Outros` pertence ao legado e nao participa do CadColab.

## Exclusao da OU piloto da coorte - 24/09/2026

`07.Outros` continua sendo a OU piloto de escrita, mas nao deve servir como fonte estatistica de perfis. A v0.1.7 introduz `Automind:Ad:SuggestionExcludedOuDns` para excluir essa OU da coorte de referencia sem limitar a leitura das OUs reais de negocio.
