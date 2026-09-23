# CHECKPOINT V015 - Correcao de build dos servicos Development

Data: 23/09/2026

## Contexto

Ao sobrepor o pacote completo em um workspace Git existente, os arquivos antigos `Services/DevelopmentAdReadOnlyService.cs` e `Services/DevelopmentAccessSuggestionService.cs` permaneceram fisicamente no clone local. Como projetos SDK-style compilam automaticamente os arquivos `*.cs` presentes no diretorio, o `DevelopmentAdReadOnlyService` antigo continuou sendo compilado mesmo sem estar registrado no `Program.cs`.

Depois da expansao de `IAdReadOnlyService`, esse arquivo antigo deixou de cumprir o contrato da interface e gerou os erros CS0738/CS0535.

## Correcao

- `DevelopmentAdReadOnlyService.cs` volta a fazer parte do pacote completo e implementa integralmente o contrato atual de `IAdReadOnlyService`.
- o fallback Development falha de forma segura: nao retorna OU ficticia, nao afirma disponibilidade de identidade e nao valida grupos/OU como reais.
- `DevelopmentAccessSuggestionService.cs` tambem volta a fazer parte do pacote e retorna lista vazia, impedindo a reintroducao dos grupos ficticios antigos (`GG_Colaboradores`, `VPN_Usuarios`, etc.).
- `Program.cs` continua registrando exclusivamente `WindowsAdReadOnlyService` e `WindowsAccessSuggestionService`.
- nenhuma escrita no Active Directory foi adicionada.

## Motivo de compatibilidade

ZIPs sobrepostos em uma pasta existente nao removem arquivos que desapareceram da nova versao. Manter estes dois arquivos em estado compativel evita que clones antigos falhem no build por arquivos residuais.

## Validacao disponivel neste ambiente

- interface e implementacoes conferidas estaticamente;
- `git diff --check` deve permanecer limpo;
- este ambiente nao possui o SDK .NET, portanto o `dotnet build` final continua sendo executado na maquina do usuario antes do commit.
