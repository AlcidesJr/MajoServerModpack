# PLAN — MAJO-001

## Resumo da solução

Implementar uma única DLL funcional `MajoServerModpack.dll` em `net462`, com um plugin BepInEx mínimo que compõe adapters de plataforma e um core testável sem Unity. O core terá registries explícitos e ownership próprio; Jötunn será usado somente onde reduz acoplamento real com Valheim. Nenhuma descoberta de módulos por reflection ou polling por frame será introduzida.

## Estado atual

- `main` fresco: `3a5533085258dd86d700df84caa0e411cff20921`.
- MAJO-000: DONE.
- Issue MAJO-001: #4.
- Branch: `task/MAJO-001-core-runtime`.
- Não existe runtime/gameplay no repositório.
- BLOCKED_BY: none.
- DEFERRED_GATE: none.

## Baseline de plataforma candidata

Revalidação em fontes oficiais/upstream em 2026-09-18:

- Valheim `1.0.15`;
- BepInExPack_Valheim `5.4.2350`, contendo BepInEx `5.4.23.5`;
- Jötunn `2.30.1`;
- Jötunn 2.30.1 compila para `net462`.

A promoção para matriz suportada depende dos gates desta tarefa. O fato de ser a versão mais recente não é critério suficiente.

## Estrutura concreta

```text
src/
└── MajoServerModpack/
    ├── MajoServerModpack.csproj
    ├── Plugin.cs
    ├── Platform/
    │   ├── Framework/
    │   └── Game/
    └── Core/
        ├── Diagnostics/
        ├── Input/
        ├── Logging/
        ├── Modules/
        ├── Patching/
        └── Runtime/

tests/
└── MajoServerModpack.Core.Tests/
    ├── MajoServerModpack.Core.Tests.csproj
    └── Program.cs
```

A distribuição continua sendo uma DLL principal. O projeto de testes reutilizará por link os arquivos de lógica pura do Core; não será criado um segundo assembly de runtime somente para facilitar testes.

## Superfícies afetadas

- `src/MajoServerModpack/`;
- `tests/MajoServerModpack.Core.Tests/`;
- `docs/DEPENDENCIES.md`;
- `docs/COMPATIBILITY.md`;
- documentação de build/runtime da MAJO-001;
- `.github/workflows/`;
- `work/MAJO-001/`;
- `work/BOARD.md`.

## Contratos e compatibilidade

- BepInEx é o loader/lifecycle/logging base.
- Jötunn é hard dependency de plataforma do plugin, sem delegar futura autoridade.
- `Majo.Platform` encapsula apenas integração sensível a framework/jogo.
- `Majo.Core` não depende diretamente de Unity/BepInEx/Jötunn em sua lógica testável.
- `MajoVersion` inicial do runtime será `0.0.1`; `0.0.0` permanece a fundação documental.
- `ProtocolVersion`, `ConfigSchema` e `DataSchema` começam em `0` e não implementam protocolo/schema funcional.
- Contexto de execução é informação operacional, nunca autorização.
- Não será criado patch Harmony funcional nesta tarefa.

## Desenho do lifecycle

1. BepInEx instancia `Plugin`.
2. O plugin cria adapters de logging/platform e `MajoRuntime`.
3. Registries são construídos explicitamente.
4. `ModuleRegistry` valida IDs/dependências e executa lifecycle determinístico.
5. Falha de um módulo marca o módulo como `Failed`; dependentes não iniciam e o runtime continua quando a degradação é segura.
6. Diagnostics de bootstrap são emitidos uma vez.
7. Shutdown solicita stop em ordem reversa.

Não haverá scan de assemblies ou reflection recorrente.

## ModuleRegistry

Metadata inicial:

- `ModuleId`;
- `Name`;
- `Version`;
- `ExecutionSide`;
- `Dependencies`;
- `Capabilities`;
- `RiskLevel`;
- `LifecycleState`.

Estados: `Registered`, `Initialized`, `Started`, `Stopped`, `Failed`.

Validações:

- ID vazio/inválido;
- ID duplicado;
- self-dependency;
- dependência ausente;
- ciclo;
- dependência falhada antes de iniciar consumidor.

## InputRegistry

Representação de binding será independente de Unity para manter o core testável:

- device: keyboard/gamepad;
- control token;
- action id;
- module id;
- contexto;
- default/current binding;
- allow-shared;
- prioridade;
- descrição.

Na MAJO-001, conflito significa o mesmo binding no mesmo contexto, ou binding global que colide com outro contexto, salvo compartilhamento explícito. O registry não sobrescreve binding silenciosamente e não registra ações reais de gameplay.

## PatchCoordinator

Registry somente de metadata:

- surface id;
- owner único;
- consumidores;
- diagnostics.

Segundo owner diferente para a mesma surface é rejeitado. Nenhum `Harmony.Patch*` funcional será adicionado.

## Contexto de execução

- dedicated/headless pode ser detectado cedo pelo Jötunn;
- após `ZNet`, usar estado do jogo/Jötunn para dedicated/local/client;
- menu/pre-world permanece explícito;
- single-player vs listen host só será separado quando houver sinal estável e verificável; caso contrário, usar estado local não ambíguo quanto à autoridade e registrar a limitação.

Nenhuma heurística por número de peers será usada para decidir host vs single-player.

## Logging e diagnostics

O Core recebe uma interface mínima de log; o adapter BepInEx apenas prefixa categoria/contexto. Diagnostics de bootstrap devem incluir:

- MajoVersion;
- ProtocolVersion;
- ConfigSchema;
- DataSchema;
- Valheim;
- BepInEx;
- Jötunn;
- contexto de execução;
- módulos registrados/ativos;
- patches registrados;
- inputs registrados;
- duração de bootstrap quando razoável.

Sem log em `Update`.

## Estratégia de build

- plugin: SDK-style C# targeting `net462`;
- Jötunn: package/ref exata `2.30.1`;
- assemblies Valheim/BepInEx: obtidos do ambiente de desenvolvimento/build e referenciados pela estratégia oficial do Jötunn;
- nenhuma DLL proprietária entra no Git;
- build local usa paths explícitos (`VALHEIM_INSTALL`, `BEPINEX_PATH`, `VALHEIM_MANAGED`) quando necessários;
- CI deve provisionar o ambiente temporário, compilar e publicar somente artefatos Majo.

## Estratégia de testes

1. harness de lógica pura, sem Valheim:
   - ModuleRegistry;
   - transitions;
   - IDs duplicados;
   - dependências ausentes/cíclicas;
   - failure isolation;
   - InputRegistry;
   - binding conflicts;
   - PatchCoordinator;
   - metadata/versionamento.
2. build `Release` da DLL do plugin.
3. validação de artefato: DLL Majo existe; nenhum assembly proprietário é publicado como artefato Majo.
4. governance e `git diff --check`.
5. smoke real BepInEx/Jötunn/Valheim é integration/manual harness quando não for seguro/estável automatizar o processo inteiro.

Testes de lógica pura não simularão falsamente Unity/Valheim.

## CI

Pipeline progressivo:

```text
governance
→ core-tests
→ runtime-build
→ artifact-validation
```

Falha causada pela MAJO-001 será corrigida nesta tarefa.

## Segurança

Superfície relevante: sim.

Revisar:

- ausência de ação privilegiada/API RPC;
- contexto de execução não usado como autorização;
- nenhuma confiança em flag client-side;
- nenhum download/update em runtime;
- nenhum filesystem arbitrário;
- nenhuma credencial/secret;
- supply chain fixada;
- diagnósticos sem dados sensíveis.

## Dados e migrations

N/A. Nenhuma persistência própria será implementada.

## Paralelismo

Pode paralelizar apenas review/análise de contratos. Implementação do core e adapters compartilha contratos e será integrada por uma única branch para evitar divergência de ownership.

## Rollback

Reverter o PR da MAJO-001 remove a DLL/runtime e alterações de CI/documentação. Não há migration, save ou formato persistido a restaurar.

## Riscos

- APIs Valheim/Jötunn mudaram em 1.0.x → limitar integração a adapters pequenos e validar build/smoke.
- build externo depender de Steam/Thunderstore/GitHub → fixar versões, não baixar nada em runtime e registrar falha externa separadamente somente se realmente herdada.
- distinguir listen host de single-player por heurística → não adivinhar; expor estado conservador quando necessário.
- abstração excessiva → interfaces apenas nas fronteiras com benefício de teste/manutenção.
- lifecycle parcialmente iniciado → stop reverso somente do que alcançou estado iniciado.
- conflito de input futuro → registro central já nasce como fonte única de metadata Majo.

## Critério para iniciar implementação

- [x] Escopo compreendido.
- [x] Dependência MAJO-000 satisfeita.
- [x] Contratos relevantes identificados.
- [x] Baseline candidata de plataforma revalidada.
- [x] Estratégia de teste definida.
- [x] Riscos materiais tratados no plano.

Estado autorizado após integração deste planejamento: `READY`, então `IMPLEMENTING`.

## Remediação de review tardio — 2026-09-19

O review automático publicou um finding P2 após os merges dos PRs #5 e #6: `m_openServer` é campo de instância de `ZNet`, mas o adapter o buscava com `BindingFlags.Static` e lia com alvo nulo. A MAJO-001 foi reaberta explicitamente.

Plano restrito:

1. buscar `m_openServer` com `BindingFlags.Instance`;
2. ler o valor na instância `ZNet` já validada por `Detect()`;
3. repetir testes puros, build real, validação de artefato e CI;
4. executar rereview e SECURITY_REVIEW no HEAD corrigido;
5. integrar a remediação e produzir novo closeout auditável.

Rollback: reverter o PR de remediação restaura o comportamento anterior. Não há migration, persistência ou mudança de protocolo.
