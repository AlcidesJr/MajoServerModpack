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
