# Core runtime

## Lifecycle

```text
BepInEx Plugin.Awake
  -> platform metadata/context adapters
  -> MajoRuntime
  -> ModuleRegistry.InitializeAll
  -> ModuleRegistry.StartAll
  -> diagnostics

Plugin.OnDestroy
  -> ModuleRegistry.StopAll (ordem reversa)
```

Falha de um módulo é registrada como `Failed`. Dependentes não iniciam; módulos independentes continuam quando a degradação é segura. Erros estruturais do grafo (dependência ausente/ciclo) são falhas de bootstrap e não são mascarados.

## Contexto de execução

Estados expostos:

- `MenuOrPreWorld`;
- `DedicatedServer`;
- `ListenServer`;
- `ConnectedClient`;
- `LocalWorld`;
- `Unknown`.

A detecção é sob demanda, sem polling por frame.

Dedicated/headless usa o sinal do Jötunn. Com ZNet disponível, o adapter usa a classificação local/client/server do Jötunn. Para separar `ListenServer` de `LocalWorld`, a integração lê o sinal interno `m_openServer` por reflection cacheada uma única vez na inicialização do tipo. Se esse detalhe mudar em uma versão futura do Valheim, o adapter retorna `LocalWorld` e emite warning uma vez, em vez de inferir por número de peers.

Contexto não concede permissão nem autoridade. Autorização pertence à MAJO-002.

## ModuleRegistry

O registry é explícito; não existe scan de assemblies.

Metadata inicial:

- ModuleId;
- Name;
- Version;
- ExecutionSide;
- Dependencies;
- Capabilities;
- RiskLevel;
- LifecycleState.

Estados:

`Registered -> Initialized -> Started -> Stopped`

Qualquer etapa pode terminar em `Failed`.

## InputRegistry

O Core modela bindings como `Device + Control`, sem depender de Unity. Keyboard e gamepad são first-class.

Conflitos:

- mesmo binding + mesmo contexto: conflito;
- contexto `Global` colide com qualquer contexto;
- contextos diferentes podem reutilizar binding;
- compartilhamento só é aceito quando os dois registros declaram `AllowSharedBinding=true`;
- rebinding conflitante é recusado e o binding atual não é alterado.

Nenhuma ação funcional de gameplay é registrada na MAJO-001.

## PatchCoordinator

O coordinator registra apenas metadata:

- surface;
- owner único;
- consumidores.

Registrar owner diferente para uma surface já possuída falha explicitamente. A MAJO-001 não aplica Harmony patches funcionais.

## Version metadata

- MajoVersion: `0.0.1`;
- ProtocolVersion: `0`;
- ConfigSchema: `0`;
- DataSchema: `0`.

Zero nos schemas/protocolo significa que a MAJO-001 somente reserva a dimensão de versionamento; não implementa esses contratos.


## Relação com o launcher

O core runtime não depende do launcher.

Caminho preferencial:

```text
MajoLauncher -> Steam/Valheim -> BepInEx -> Jötunn -> MajoServerModpack.dll
```

Caminho manual também suportado:

```text
Steam/Valheim -> BepInEx -> Jötunn -> MajoServerModpack.dll
```

Ambos produzem o mesmo runtime. Qualquer marker futuro de “iniciado pelo launcher” é apenas diagnóstico/UX e nunca concede permissões nem altera a trust boundary da MAJO-002.
