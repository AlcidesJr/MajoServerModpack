# Input e hotkeys

## Objetivo

Centralizar bindings do produto e impedir que módulos Majo entrem em conflito entre si.

## Registro

Cada ação deve registrar metadata equivalente a:

```text
ActionId
Module
Context
DefaultBinding
CurrentBinding
Device
AllowSharedBinding
Priority
Description
```

## Contextos

Exemplos:

- Gameplay
- Building
- Inventory
- Map
- RadialMenu
- AdminPanel
- WebMapControl
- Debug

Uma mesma tecla pode ser válida em contextos mutuamente exclusivos. O registry decide pelo contexto ativo, não por ordem acidental de Harmony patches.

## Regras

- nunca substituir binding do usuário silenciosamente;
- distinguir modificadores Left/Right quando a API permitir;
- gamepad é first-class;
- modal/painel deve declarar captura de cursor e teclado;
- fechar painel deve restaurar estado anterior de input/cursor;
- conflito é exibido no Majo Control Panel;
- bindings locais permanecem `ClientPreference`.

## Integração

Módulos consultam `InputRegistry` em vez de observar `Input.GetKey*` de forma independente quando a ação for configurável.

Hot paths de input devem evitar alocações por frame.
