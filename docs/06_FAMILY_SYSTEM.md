# 06_FAMILY_SYSTEM.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

## 1. Qué almacena una familia

`Family` (`03_DATA_MODEL.md` §2 y `01_ARCHITECTURE.md` capa Families) almacena: miembros afiliados actuales (`MemberIds`, índice de conveniencia, no fuente de genealogía — ver §2), patrimonio (`FamilyPatrimony`), reputación regional (`FamilyReputation`), historia (agregado consultable de `DecisionRecord` donde `FamilyId` coincide), alianzas/rivalidades (`Relationship` con `SourceId`/`TargetId` = `FamilyId`), conocimiento geográfico (`MapKnowledgeState`) y secretos (subconjunto de `Relationship`/`DecisionRecord` con `Visibility = PRIVATE`, ver `07_CONSEQUENCE_SYSTEM.md`).

## 2. `CharacterDefinition` vs `CharacterInstance`

Ya definidos en `03_DATA_MODEL.md` §1. Resumen de la distinción funcional:

- `CharacterDefinition` = plantilla de diseño (usada para figuras históricas y como base contextual de personajes de casa). Vive en `Content`, no cambia entre partidas.
- `CharacterInstance` = el personaje real de esa partida: edad, salud, experiencia, inventario, relaciones actuales, decisiones tomadas, estado, profesión actual, ubicación, rasgos adquiridos. Es lo que persiste en el guardado.

## 3. Genealogía relacional (mecanismo central de este sistema)

Ver el razonamiento completo en `03_DATA_MODEL.md` §2. Resumen operativo para este documento:

- **`GenealogyGraph`**: servicio dentro de `FamilySystem` que opera sobre el grafo `ParentIds`/`SpouseId` de todos los `CharacterInstance`, sin importar `FamilyId`.
- **Nunca se copian personajes entre árboles.** Un matrimonio entre un Salazar y una Arrieta no duplica a ninguno de los dos en el árbol del otro: crea una `Relationship` (`KinshipType = SPOUSE`) entre ambos `CharacterId` existentes, y cualquier hijo posterior es un `CharacterInstance` con `ParentIds` apuntando a ambos progenitores originales, cada uno conservando su `FamilyId` de origen.
- Consultas soportadas sin ambigüedad: padres, hijos, hermanos (comparten al menos un `ParentId`), parejas (`SpouseId`/`Relationship.KinshipType`), familias conectadas (unión de los `FamilyId` presentes entre los ancestros/descendientes de un `CharacterId`), ancestros/descendientes a N generaciones (recorrido del grafo, con memoización para evitar recomputar sobre 100 años de historia en cada consulta — ver §5).
- **Adopción narrativa**: no se modela en el juego base (fuera de alcance ahora), pero el esquema ya lo admite sin cambio estructural: sería otra `Relationship.KinshipType = ADOPTED` combinada opcionalmente con un `ParentIds` no biológico — se decide en la fase que lo requiera, no ahora.
- **Herencia**: al morir un `CharacterInstance`, `FamilySystem` determina el nuevo `ActiveProtagonistId` de su `Family` (regla narrativa: heredero directo vivo más cercano por el grafo, dentro del mismo `FamilyId`) y transfiere `FamilyPatrimony` según las reglas de `03_DATA_MODEL.md` §6. La genealogía (grafo `ParentIds`) no se altera por la herencia — herencia es una operación de `PatrimonySystem`/`FamilySystem` sobre quién controla qué, no una operación sobre el grafo genealógico en sí, que es un registro histórico inmutable.

## 4. Cambio de protagonista sin respawn

Cuando `CharacterInstance.State` pasa a `DECEASED`:

1. `FamilySystem` emite `OnCharacterDied`.
2. Se determina el heredero (§3) y se marca `IsProtagonist = true` en el nuevo `CharacterInstance`, `false` en el fallecido.
3. `Family.ActiveProtagonistId` se actualiza.
4. El mundo, el patrimonio, las relaciones y el conocimiento geográfico de la familia **no se reinician**: el jugador continúa la misma partida a través de otro miembro vivo del árbol, exactamente como especifica el requisito de "sin respawn" del diseño. El `CharacterInstance` fallecido permanece en el grafo genealógico para siempre (consultable, ej. por `ConsequenceEngine` para efectos generacionales que referencian a un ancestro específico).

## 5. Simulación interna de varias décadas (validación del diseño, no implementación aún)

El diseño debe soportar, sin degradar por escala, un escenario como: simular internamente 30-100 años de una familia (nacimientos, matrimonios entre casas, muertes, herencias) para validar que el grafo genealógico y el cambio de protagonista funcionan sin corromper estado. Esto es exactamente lo que exige la prueba de Fase 3 (`Personaje A envejece → muere → herencia → Personaje B controlable, sin destruir el estado del mundo`) y también el requisito de testing de saltos temporales acelerados (`12_DEVELOPMENT_ROADMAP.md` §Testing, ejecutar 1820→1920 en segundos). El diseño lo soporta porque:

- El grafo genealógico es apend-only en su forma de nodos/aristas (nunca se borra un `CharacterInstance` fallecido), por lo que simular más años solo añade nodos, no reescribe los existentes — coste lineal, no cuadrático, en número de personajes.
- Las consultas de ancestros/descendientes están acotadas por profundidad de generación solicitada, no recorren el grafo completo por defecto.

## 6. Fuera de alcance de esta fase

No se construye el árbol familiar de cien años, ni las cuatro familias completas (eso es Fase 3 y Fase 10 respectivamente). Aquí solo se fija el mecanismo del grafo relacional y la regla de "nunca copiar personajes entre árboles".
