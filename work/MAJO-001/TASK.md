# TASK — MAJO-001

## Título

Core runtime

## Objetivo

Criar o primeiro runtime real e carregável do MajoServerModpack sobre BepInEx + Jötunn, estabelecendo a infraestrutura mínima para as tarefas futuras sem antecipar funcionalidades de gameplay, configuração, UI ou networking seguro.

## Contexto

A MAJO-000 encerrou a fundação arquitetural e de governança. A MAJO-001 transforma esses contratos em código executável, mantendo BepInEx e Jötunn como únicas dependências externas de plataforma aprovadas.

## Escopo

- bootstrap BepInEx do MajoServerModpack;
- integração controlada com Jötunn;
- camada `Majo.Platform`;
- lifecycle central do Majo;
- detecção e exposição do contexto de execução;
- `ModuleRegistry` com metadata, dependências e isolamento básico de falhas;
- logging base consistente;
- diagnostics básicos;
- `InputRegistry` com bindings e detecção de colisões;
- `PatchCoordinator` com ownership e consumidores, sem patches funcionais;
- metadata de `MajoVersion`, `ProtocolVersion`, `ConfigSchema` e `DataSchema`;
- testes de lógica pura sem fingir runtime Valheim;
- build reproduzível;
- CI para governance, testes, build e validação do artefato.

## Fora de escopo

- MAJO-002, MAJO-003, MAJO-004 ou tarefas posteriores;
- `SecureRpcGateway`, autorização, permissões, handshake ou protocolo de rede funcional;
- config sync/configuração funcional;
- Majo Control Panel;
- features de gameplay;
- port/adaptação de patches ou funcionalidades de mods de referência;
- auto-download/auto-update de frameworks em runtime;
- release/tag.

## Critérios de aceite

- [x] Plugin BepInEx mínimo compila e possui dependência explícita de Jötunn.
- [x] `Majo.Platform` contém somente adapters úteis, sem wrappers vazios.
- [x] Runtime central possui bootstrap/start/stop observáveis e falhas seguras.
- [x] Contexto de execução é exposto sem ser usado como autorização.
- [x] `ModuleRegistry` cobre registro, dependências, lifecycle, duplicidade e isolamento básico de falhas.
- [x] `InputRegistry` cobre keyboard/gamepad metadata, contexto, binding atual/default e conflitos.
- [x] `PatchCoordinator` impede ownership ambíguo e registra consumidores.
- [x] Diagnostics expõem versões, contexto e resumos de módulos/inputs/patches.
- [x] `MajoVersion`, `ProtocolVersion`, `ConfigSchema` e `DataSchema` são independentes.
- [x] Build não versiona DLLs proprietárias do Valheim.
- [x] Testes focados passam.
- [x] Build reproduzível do plugin passa no ambiente definido.
- [ ] CI aplicável passa no HEAD revisado da remediação.
- [ ] Review independente e security review concluídos no HEAD da remediação.
- [ ] PR de remediação integrado, merge SHA registrado e novo closeout concluído.

## Dependências

- DEPENDS_ON: MAJO-000 — DONE
- Plataforma: BepInEx + Jötunn

## Baseline promovida de plataforma

Promovida após os gates de build/runtime da tarefa:

- Valheim: `1.0.15`;
- BepInExPack_Valheim: `5.4.2350`;
- BepInEx upstream: `5.4.23.5`;
- Jötunn: `2.30.1`;
- target framework do plugin: `net462`, alinhado ao Jötunn 2.30.1.

A combinação acima é a baseline suportada pela MAJO-001. Nenhuma atualização é automática.

## Riscos conhecidos

- Valheim 1.0.15 é posterior ao update explicitamente citado pelo Jötunn 2.30.0 para Valheim 1.0.7; build não substitui smoke real.
- distinguir single-player de listen server sem heurística frágil pode não ser possível em todos os estados; o runtime deve expor incerteza em vez de adivinhar.
- build do plugin depende de assemblies do jogo/framework disponíveis externamente, sem versioná-los no Git.
- abstrações prematuras podem criar custo maior do que o desacoplamento obtido.

## Referências

- Issue #4
- `AGENTS.md`
- `docs/ARCHITECTURE.md`
- `docs/DEPENDENCIES.md`
- `docs/COMPATIBILITY.md`
- `docs/INPUT.md`
- `docs/PATCH-OWNERSHIP.md`
- `docs/SECURITY-BASELINE.md`
- `docs/VERSIONING.md`
- `docs/WORKFLOW.md`
- `work/MAJO-000/`
