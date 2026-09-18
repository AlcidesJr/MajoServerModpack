# AGENTS.md

Este arquivo é o mapa operacional do MajoServerModpack. O projeto segue o padrão de desenvolvimento de `AlcidesJr/Fix_Development`.

## Idioma

- Interações, estados, relatórios, findings e resumos: português do Brasil.
- Preserve nomes de código, APIs, paths e termos técnicos quando necessário.

## Fonte de verdade

- GitHub e arquivos versionados deste repositório são a fonte de verdade.
- Antes de agir, reconsulte branch base, branch da tarefa, PR, CI, review threads, `work/BOARD.md`, `TASK.md`, `STATUS.md` e dependências.
- Não presuma estado de conversas anteriores quando puder consultar o repositório.

## Tarefa atômica

- Uma tarefa `MAJO-xxx` trabalha somente no próprio escopo.
- Trabalho adjacente deve virar nova tarefa.
- Dependência que bloqueia diretamente: `BLOCKED_BY=<ref>`.
- Gate herdado/externo não causado pela tarefa: `DEFERRED_GATE=<ref>`.
- Regressão causada pela tarefa nunca é `DEFERRED_GATE`.

## Ciclo obrigatório

`PLANNED → INTAKE → PLANNING → READY → IMPLEMENTING → VERIFYING → IN_REVIEW → SECURITY_REVIEW → READY_TO_MERGE → MERGED → CLOSEOUT → DONE`

Estados excepcionais: `BLOCKED`, `CANCELLED`.

## Planejamento antes de código

Antes de mudança não trivial:

1. ler `TASK.md` e contratos;
2. recuperar estado fresco;
3. atualizar `PLAN.md`;
4. identificar superfícies, conflitos, testes, riscos e rollback;
5. somente então implementar.

## Dependências do produto

- Dependências externas de plataforma aprovadas inicialmente: **BepInEx** e **Jötunn**.
- Mods funcionais de terceiros nunca são dependências do Majo.
- Nova dependência de runtime exige decisão arquitetural explícita.
- APIs de framework devem ser isoladas por `Majo.Platform` quando o isolamento trouxer benefício real de manutenção.

## Projetos de referência

- Repositórios externos servem como referência de comportamento, problemas resolvidos, edge cases e técnicas.
- Não portar código cegamente.
- Antes de adotar uma solução, revisar: arquitetura, compatibilidade com Valheim atual, segurança, allocations, hot paths, networking, persistência, patches e conflitos.
- Quando código for efetivamente derivado/adaptado, registrar proveniência e atribuição aplicável.

## Conflitos obrigatórios

Toda feature deve ser avaliada quanto a:

1. conflito funcional;
2. conflito de Harmony patch;
3. conflito de regras/configurações;
4. conflito de persistência/dados;
5. conflito de performance/duplicação de scans;
6. conflito de rede/ownership/RPC;
7. conflito de input/UI/foco/hotkeys.

Métodos críticos do Valheim devem ter ownership explícito no `PatchCoordinator`. Sempre que razoável, um hot path deve possuir um único ponto de patch Majo.

## Segurança

- Cliente nunca é fonte de verdade para ações privilegiadas.
- Ações administrativas passam pelo `SecureRpcGateway`.
- Identidade do peer deve ser vinculada à conexão real antes de autorização.
- Permissões, payload, ranges e rate limits são validados no servidor.
- Jötunn pode fornecer transporte/sincronização, mas não é a fronteira final de confiança do Majo.

## Performance

- Não otimizar por intuição quando a mudança tocar hot path.
- Preferir baseline → mudança → benchmark/profiling → comparação.
- Evitar scanners duplicados; serviços compartilhados como `WorldIndex`, caches e schedulers devem ser preferidos.
- UI e diagnóstico devem ser event-driven ou budgeted quando polling contínuo for desnecessário.

## Versionamento

Formato exclusivo do produto: `MAJOR.MINOR.PATCH`, três inteiros, por exemplo:

`0.0.0 → 0.0.1 → 0.0.2 → ... → 0.1.0 → ... → 1.0.0 → 1.0.1`

Não adicionar sufixos `alpha`, `beta` ou `rc` sem decisão explícita.

## Arquivos por tarefa

Cada tarefa mantém:

- `TASK.md`
- `PLAN.md`
- `STATUS.md`
- `EVIDENCE.md`
- `REVIEW.md`

## Fechamento

Uma tarefa só chega a `DONE` quando implementação/documentação aplicável foi integrada, gates e reviews foram tratados, merge SHA foi registrado e `STATUS.md`, `EVIDENCE.md` e `BOARD.md` estão consistentes.
