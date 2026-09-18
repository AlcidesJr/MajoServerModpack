# Fluxo de trabalho

Adaptado de `AlcidesJr/Fix_Development`.

## Estados

`PLANNED → INTAKE → PLANNING → READY → IMPLEMENTING → VERIFYING → IN_REVIEW → SECURITY_REVIEW → READY_TO_MERGE → MERGED → CLOSEOUT → DONE`

Excepcionais: `BLOCKED`, `CANCELLED`.

## Regras

- estado descreve o que é verdade agora;
- plano precede implementação;
- testes partem do mais focado para integração/global;
- finding efetivo retorna o fluxo para correção;
- segurança é proporcional à superfície;
- HEAD revisado deve corresponder ao HEAD que passou pelos gates;
- merge não equivale a DONE até closeout documental.

## Segurança mínima para Majo

Avaliar quando aplicável:

- identidade/autorização de peer;
- payload RPC;
- rate limiting/abuso;
- filesystem/WebMap;
- serialização;
- persistência/migrations;
- networking/ownership;
- configuração server-authoritative;
- supply chain de frameworks;
- operações administrativas/destrutivas.

P0/P1 aberto bloqueia merge.

## Evidência

Não declarar PASS sem comando/run/commit verificável. Gates herdados podem ser `DEFERRED_GATE` somente quando comprovadamente externos ao diff.


## HEAD auditável sem autorreferência

Um commit Git não pode conter o próprio SHA dentro de um arquivo versionado, porque alterar o arquivo altera o SHA do commit.

Para evitar uma cadeia infinita de commits de sincronização:

- `CONTENT_HEAD`: último commit substantivo que altera produto/arquitetura/documentação de conteúdo;
- commits posteriores que alterem somente `STATUS.md`, `EVIDENCE.md`, `REVIEW.md` ou `BOARD.md` são `metadata-only`;
- o HEAD exato submetido ao review é registrado na timeline do PR/review, que não altera o Git tree;
- antes de merge, confirmar que qualquer commit posterior ao `CONTENT_HEAD` é realmente metadata-only; caso contrário, definir novo `CONTENT_HEAD` e repetir gates afetados.

Não declarar que um SHA é “HEAD atual” quando ele é apenas o último conteúdo revisado; nomear o campo de acordo com sua semântica.
