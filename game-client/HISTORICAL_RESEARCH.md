# HISTORICAL_RESEARCH.md — LEGADO: PERÚ

Registro de estado de investigación histórica por elemento (prompt Fase 2 §63; principio general Regla 3 del proyecto). Ningún elemento marcado `FICTIONAL` o `NEEDS_RESEARCH` debe presentarse en el juego como dato histórico verificado. Este documento se amplía en cada milestone que introduzca contenido con pretensión histórica.

Estados usados: `VERIFIED` (evento/hecho histórico real) · `NEEDS_RESEARCH` (se necesita investigación documental antes de darlo por bueno) · `APPROXIMATED` (basado en referencias generales de época/región, no en una fuente específica de Pisco/Paracas) · `FICTIONAL` / `PERIOD-INSPIRED` (invención narrativa deliberada, inspirada en el periodo pero sin pretensión de exactitud).

---

## Geografía y trazado urbano

| Elemento | Estado | Notas |
|---|---|---|
| Pisco/Paracas — trazado urbano 1820 | `NEEDS_RESEARCH` | El HUB de Milestone 2.3 usará **compresión geográfica** (prompt §23) deliberada; no debe presentarse como plano histórico exacto hasta contrastar con fuentes (planos coloniales tardíos/republicanos tempranos, si existen y son accesibles). |
| Costa/playa junto al asentamiento (posición, orientación) | `APPROXIMATED` | Orientación general costa peruana (mar al oeste); no verificado contra la geografía real de Pisco/Paracas específicamente. |

## Arquitectura

| Elemento | Estado | Notas |
|---|---|---|
| Casa Salazar — distribución y materiales | `FICTIONAL / PERIOD-INSPIRED` | Adobe + techo de quincha/caña es un método constructivo costero histórico plausible para la época, pero la vivienda concreta y su distribución son ficticias (familia inventada). Ver `ART_BIBLE_v0.1.md` §7. |
| Técnica de techado representada (bandas alternas, "thatch") | `NEEDS_HISTORICAL_VALIDATION` | Marcado explícitamente en `ART_BIBLE_v0.1.md` §7 (prompt §14 pide este marcado para elementos de vestimenta/arquitectura no verificados aún). |

## Vestimenta

| Elemento | Estado | Notas |
|---|---|---|
| Vestimenta de Mateo Salazar (17 años, familia de comercio/transporte, 1820) | `NEEDS_HISTORICAL_VALIDATION` | Diseño actual: camisa clara sencilla, pantalón oscuro, sin sombrero — inspirado en juventud trabajadora de costa, sin fuente documental específica contrastada todavía (prompt §14). |
| Vestimenta del NPC pescador (sombrero de ala ancha, delantal) | `NEEDS_HISTORICAL_VALIDATION` | Mismo estado; diferenciación pensada por legibilidad de gameplay antes que por precisión documental. |

## Eventos históricos

| Elemento | Estado | Notas |
|---|---|---|
| Desembarco de la expedición libertadora (San Martín, Paracas, septiembre de 1820) | `VERIFIED EVENT / IMPLEMENTATION PENDING` | El evento en sí es un hecho histórico verificado y ya está referenciado como gancho narrativo del vertical slice (Milestone 2.7). Su implementación en juego (rumores, fecha exacta, forma de presentarlo) no se ha construido todavía — ver `05_TIME_SYSTEM.md` para el mecanismo de eventos `HARD` que lo hará cumplir su resultado macro sin que el jugador pueda alterarlo. |

## Economía

| Elemento | Estado | Notas |
|---|---|---|
| Moneda/denominación de 1820 | `NEEDS_RESEARCH` | Milestone 2.5 usará `CurrencyPrototype` internamente hasta verificar la denominación histórica correcta (prompt §27); no mostrar un nombre de moneda real como definitivo antes de esa validación. |
| Productos de mercado (alimentos/herramientas/suministros prototipo) | `NEEDS_RESEARCH` | Lista de 5-8 ítems de Milestone 2.5 será prototipo hasta verificación. |

---

## Proceso

```
Research → Source → Verification → Game Data → Narrative
```

Ningún elemento pasa de `NEEDS_RESEARCH`/`FICTIONAL` a `VERIFIED` sin que este documento se actualice explícitamente citando la referencia usada (`SourceReferences`, ver `11_HISTORICAL_DATA_PIPELINE.md`). Este archivo es la única fuente de verdad sobre qué está validado; el código (`HistoricalStatus` en `03_DATA_MODEL.md`) debe mantenerse en sincronía con lo aquí registrado.
