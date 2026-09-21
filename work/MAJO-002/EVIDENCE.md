# EVIDENCE — MAJO-002

## Intake

- main HEAD: `b9eb209a633721f3acdb028aed7377f2f7c34d58`;
- main workflow: governance run 35471672684 — PASS;
- MAJO-001: DONE;
- issue MAJO-001 #4: closed/completed;
- implementation merge MAJO-001: `31275f2a24679030fb2110f0eeea756eee51d159`;
- remediation merge: `cc7b273264c393473918b61b14f95aacde3725ff`;
- closeout merge: `36ab695506dd746e21cf1976f5ef56590782bbd2`;
- baseline-doc merge: `9160dee10c4b9b081bdbda303ef792987c275e4c`;
- PRs abertos no intake: 0;
- issue MAJO-002: #10;
- branch: `task/MAJO-002-authority-secure-networking`;
- BLOCKED_BY: none;
- DEFERRED_GATE: none.

## APIs/plataforma

Jötunn 2.30.1 revalidado:

- `ModCompatibility.ZNet_OnNewConnection` registra RPC diretamente em `peer.m_rpc`;
- callback direto recebe o `ZRpc` real;
- `ZNetExtension.IsAdmin` usa peer/socket e `m_adminList` no servidor;
- `CustomRPC` recebe sender routed id, logo não é a trust boundary primária;
- `NetworkCompatibility(EveryoneMustHaveMod, None)` permite exigir presença sem usar `MajoVersion` como wire contract.

## Planejamento

- `TASK.md` e `PLAN.md` criados;
- trust boundary: conexão real → TrustedPeerContext → SecureRpcGateway;
- threat model registrado em `PLAN.md`.

## Implementação

pending

## Verificação

pending

## Review

pending

## Security review

pending

## Integração

pending
