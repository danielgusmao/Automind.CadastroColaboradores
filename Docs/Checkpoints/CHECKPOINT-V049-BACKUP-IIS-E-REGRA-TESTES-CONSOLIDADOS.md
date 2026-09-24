# CHECKPOINT V049 - Backup IIS e regra de testes consolidados

Data: 24/09/2026

## Backup IIS confirmado

No servidor `10.1.2.21`, foi criado o backup:

`CadColab-Pre-gMSA-20260924-082229`

Arquivos e hashes:

- `C:\Windows\System32\inetsrv\backup\CadColab-Pre-gMSA-20260924-082229\applicationHost.config`
  - SHA-256: `B46EE9FEA44440598CB5B3014A631E3FD89E78D4358FCF22DA90385853511840`
- `C:\Temp\CadastroColaboradores-Rollback\IIS-CadastroColaboradores-before-20260924-082229.txt`
  - SHA-256: `CD51C31F6CE9B87B1E09B00C5B5D48BAE2756CFB07946777FC3DE0755F8EE45B`

Nenhuma troca de identidade, recycle, restart, `iisreset`, alteracao de AD ou alteracao NTFS ocorreu durante o backup.

## Regra operacional nova

Para economizar tempo, varios testes podem ser consolidados em uma unica linha de PowerShell quando:

1. forem somente leitura ou simulacao;
2. forem executados no mesmo local;
3. nao introduzirem escrita real;
4. cada resultado puder ser distinguido na saida;
5. a linha impedir que uma falha intermediaria gere conclusao enganadora.

Pode haver mais de 3 validacoes na mesma linha.

A regra de seguranca para escrita continua inalterada: `ALTERACAO REAL` deve ocorrer uma mudanca por vez, com comando, efeito esperado, rollback e validacao preparados antes da execucao.

## Proximo passo

Preparar a troca controlada do App Pool `CadastroColaboradores` para `AUTOMIND\gMSA_CadColab$`, com:

- comando de alteracao real isolado;
- rollback restrito ao App Pool;
- validacoes consolidadas em uma unica linha de leitura apos a mudanca;
- sem `iisreset`;
- sem alterar outros pools, sites, AD ou NTFS.
