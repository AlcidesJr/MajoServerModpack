# Decisões arquiteturais

## ADR-001 — Produto independente

**Decisão:** mods funcionais externos são referências, não dependências.

**Motivo:** bugs, evolução e compatibilidade do produto devem ser controlados pelo projeto Majo.

## ADR-002 — Dependências de plataforma

**Decisão:** BepInEx e Jötunn são frameworks de plataforma aprovados inicialmente.

**Motivo:** BepInEx fornece o bootstrap/patching; Jötunn abstrai plumbing específico de Valheim que sofre alterações com versões do jogo.

## ADR-003 — Anti-corruption layer

**Decisão:** integrações externas relevantes são isoladas por `Majo.Platform`.

**Motivo:** reduzir blast radius de mudanças de framework sem criar wrappers vazios.

## ADR-004 — Patch ownership

**Decisão:** métodos críticos/hot paths possuem ownership no `PatchCoordinator`; sobreposições são compostas em uma implementação Majo.

## ADR-005 — Autoridade

**Decisão:** regras de gameplay/world/server são server-authoritative. Preferências visuais/input podem ser locais. Ações privilegiadas são validadas no servidor.

## ADR-006 — Segurança de RPC

**Decisão:** Jötunn pode fornecer transporte, mas `SecureRpcGateway` é responsável pela fronteira de confiança do produto.

## ADR-007 — Versionamento

**Decisão:** versão pública sempre `MAJOR.MINOR.PATCH`, iniciando em `0.0.0`, sem prerelease suffix por padrão.

## ADR-008 — Uma unidade funcional de distribuição

**Decisão:** objetivo de distribuição é uma DLL funcional principal do Majo, com BepInEx/Jötunn como frameworks externos.

## ADR-009 — Compatibilidade por lado/capability

**Decisão:** cada módulo declara lado de execução e requisitos de peer. A compatibilidade de rede será baseada em protocolo/capabilities; versão pública sozinha não é contrato suficiente.

## ADR-010 — Input ownership central

**Decisão:** hotkeys e gamepad bindings do Majo passam por `InputRegistry`, com contexto e detecção de conflito.

**Motivo:** um modpack unificado não pode reproduzir conflitos de teclas comuns em coleções de mods independentes.

## ADR-011 — Supply chain controlada

**Decisão:** versões de BepInEx/Jötunn são promovidas manualmente e registradas com origem/versão/hash do artefato testado quando aplicável. O Majo não faz auto-update de frameworks.

## ADR-012 — Serviços externos seguros por padrão

**Decisão:** qualquer serviço HTTP/WebSocket futuro, incluindo WebMap, nasce desabilitado e com exposição mínima por padrão. Binding público, autenticação e proxy externo são decisões explícitas do operador.

**Motivo:** não transformar uma feature opcional em superfície de rede pública acidental.


## ADR-013 — Launcher como aplicação separada

**Decisão:** o Majo terá um launcher desktop próprio, separado do processo do Valheim e do `MajoServerModpack.dll`.

**Motivo:** configuração, preflight, perfis e inicialização são responsabilidades de desktop; misturá-las ao runtime aumentaria acoplamento e risco.

## ADR-014 — Launcher não é dependência de runtime

**Decisão:** o plugin Majo deve continuar inicializando e operando quando o Valheim for iniciado manualmente sem launcher.

## ADR-015 — Schema de configuração único

**Decisão:** MAJO-003 deverá expor schema/manifest machine-readable versionado consumido pelo launcher e pela futura UI in-game.

**Motivo:** defaults, ranges, authority, conflitos e requisitos de restart não podem ser duplicados.

## ADR-016 — Launcher não concede autoridade

**Decisão:** dados produzidos pelo launcher são configuração/metadata local, nunca prova de peer identity, admin ou permissão perante servidor remoto.

## ADR-017 — Release train inicial compartilhado

**Decisão:** inicialmente launcher e runtime pertencem ao mesmo release público `MajoVersion`; compatibilidade do launcher com configuração é validada separadamente pelo `ConfigSchema`/manifest.
