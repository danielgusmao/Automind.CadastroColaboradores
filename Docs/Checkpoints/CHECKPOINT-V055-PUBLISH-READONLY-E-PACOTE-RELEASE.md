# CHECKPOINT V055 - Publish ReadOnly validado e regra de pacote de release

Data: 24/09/2026

## Resultado do publish local

O publish foi executado na maquina de desenvolvimento com .NET 10 e concluido com sucesso.

Resultado informado:

- projeto `Automind.CadastroColaboradores` em `net10.0`;
- build/publish concluido com sucesso;
- 15 avisos `CA1416`, todos relacionados a APIs `System.DirectoryServices` suportadas apenas em Windows;
- nenhum erro de compilacao informado;
- pasta gerada: `artifacts\\CadastroColaboradores-ReadOnly-20260924-091040`.

## Validacoes de seguranca do artefato publicado

A validacao do conteudo publicado confirmou:

- `Automind:Mode=ReadOnly`;
- allowlist de leitura com 23 OUs;
- `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br` presente na allowlist de leitura;
- allowlist de escrita com exatamente 1 OU;
- unico alvo de escrita configurado: `OU=07.Outros,OU=Automind,DC=automind,DC=com,DC=br`;
- `GroupWritesEnabled=False`;
- `Microsoft365.Enabled=False`;
- `Automind.CadastroColaboradores.dll` presente;
- `web.config` presente;
- hashes SHA-256 foram gerados para `appsettings.json`, DLL e ZIP do publish.

Os hashes completos nao foram registrados neste checkpoint porque a saida copiada na conversa foi apresentada truncada. Nao inferir ou completar valores de hash.

## Estado de seguranca

O pacote permanece bloqueado para escrita real porque `Automind:Mode=ReadOnly`.

Portanto, esta etapa nao habilitou:

- criacao de usuarios no AD;
- escrita de grupos;
- `proxyAddresses`;
- `pwdLastSet`;
- Microsoft 365;
- Teams;
- escrita no TOPdesk.

## Nova regra operacional de checkpoint/pacote

Durante a preparacao de um pacote completo para release/teste:

- nao gerar um ZIP de checkpoint separado a cada etapa;
- manter `Docs/CHECKPOINT.md`, `Docs/CP-HIST.md` e demais documentos atualizados dentro do proprio pacote completo do projeto;
- entregar o checkpoint junto com o pacote completo;
- voltar a gerar checkpoint separado apenas ao entrar em uma nova fase de testes, quando isso ajudar a continuidade, ou mediante pedido explicito;
- a regra anterior de consolidar varios testes somente leitura/simulacao em uma unica linha de PowerShell continua valida;
- alteracoes reais continuam uma por vez, com efeito esperado, rollback e validacao preparados previamente.

## Proximo marco

O proximo pacote pode seguir para commit/publicacao em `ReadOnly` para validacao do deploy. A ativacao `PilotWrite` continua sendo uma etapa separada e nao deve ocorrer implicitamente durante o deploy.

---
