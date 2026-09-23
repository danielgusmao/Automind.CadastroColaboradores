# Grupos e perfis

## Implementação atual

A sugestão deixou de ser mock e consulta o AD real.

Dado `Cargo em inglês (Title) + Departamento (Department)`:

1. pesquisar usuários ativos equivalentes abaixo de `Automind:Ad:PeopleSearchBase`;
2. ler os grupos diretos de cada usuário via `memberOf`;
3. contar a incidência de cada grupo;
4. consultar metadados do grupo no AD;
5. calcular ancestrais de grupos de segurança usando a regra LDAP `1.2.840.113556.1.4.1941`;
6. classificar grupos protegidos/privilegiados;
7. pré-selecionar somente grupos presentes em todos os usuários equivalentes e que não estejam protegidos.

O grupo primário não aparece em `memberOf` e, portanto, não faz parte desta comparação automática.

## Classificação

- `EncontradoEm == TotalComparados` e não protegido -> **Comum ao cargo**, pré-selecionado.
- Presente somente em parte da coorte -> **Exceção**, exibido sem seleção automática.
- Grupo com `adminCount=1`, `isCriticalSystemObject`, nome protegido configurado ou ancestral protegido -> **Protegido**, nunca pré-selecionado.

A interface também mostra:

- categoria `Security` ou `Distribution`;
- escopo `Global`, `Universal` ou `DomainLocal`;
- efeitos indiretos de grupos de segurança quando houver grupos ancestrais.

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
