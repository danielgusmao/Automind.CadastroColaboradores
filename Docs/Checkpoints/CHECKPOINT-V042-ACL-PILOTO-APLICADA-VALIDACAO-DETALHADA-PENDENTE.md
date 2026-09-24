# CHECKPOINT V042 - ACL piloto aplicada; validacao detalhada pendente

Data: 23/09/2026

## Escopo

OU piloto:

`OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`

Principal tecnico:

`AUTOMIND\SG_CadastroColaboradores_AD_Writer`

## Resultado da escrita real

O comando protegido de `Set-Acl` foi executado e retornou:

`ACL PILOTO APLICADA`

Validacao estrutural:

- ACE total: `56`;
- ACEs do grupo tecnico: `16`;
- owner: `AUTOMIND\Domain Admins`;
- heranca bloqueada: `False`;
- todas as ACEs listadas do projeto: `IsInherited=False`.

## Evidencias pos-escrita

Arquivos locais:

- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-ACL-after-pilot.xml`
- `C:\Temp\CadastroColaboradores-Rollback\OU-07-Outros-SDDL-after-pilot.txt`

SHA-256:

- XML: `5B2832851DA742D9AA4B3B768214282F694B41626BFC965ABE8F4B16E02C4584`
- SDDL: `31E8B76A144EFCB7CA83C8897651FB55FC3B1FDF362CCAA09CC3CC900BF95AC3`

## Anomalia observada na listagem

O conjunto esperado possui uma ACE `WriteProperty` para `givenName` e uma para `company`.

GUIDs esperados:

- `givenName`: `f0f8ff8e-1191-11d0-a060-00aa006c33ed`
- `company`: `f0f8ff88-1191-11d0-a060-00aa006c33ed`

Na saida recebida, `f0f8ff88-1191-11d0-a060-00aa006c33ed` apareceu duas vezes e `f0f8ff8e-1191-11d0-a060-00aa006c33ed` nao apareceu.

Ainda nao e possivel concluir se existe ACE incorreta no AD ou se ocorreu problema de exibicao/copia. A proxima acao deve ser exclusivamente de leitura, contando as ACEs por cada GUID esperado.

## Regra de seguranca

- nenhuma nova escrita enquanto a divergencia nao for esclarecida;
- nao executar rollback automaticamente;
- nao usar restauracao integral do SDDL;
- se a divergencia for confirmada, preparar correcao granular com efeito esperado, rollback e validacao antes de executar.
