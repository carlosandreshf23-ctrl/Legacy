# backend

Servicio de mundo persistente de LEGADO: PERÚ.

**Estado actual**: no inicializado. Se introduce a partir de la **Fase 8** (`WorldSimulationSystem`, mundo vivo sin jugadores presentes) y se completa en **Fase 9** (multijugador). Ver `docs/MultiplayerArchitecture.md` §2.

Hasta entonces, las Fases 1-7 usan una implementación local-mock del mismo contrato de repositorio (`IPersistable` / `IWorldStateRepository`, ver `docs/SaveSystem.md` §1), para no bloquear progreso jugable temprano en infraestructura de servidor.

**Responsabilidad cuando se implemente**: única fuente de verdad para `Family`, `Character`, `Relationship`, `DecisionEvent`, `FamilyPatrimony`, `Asset`, `MapKnowledgeState`, `AuthorityRecord`, `PlayerAccount` (ver `docs/DataModel.md`, sección "Persistencia vs. derivado"); ejecuta `WorldSimulationSystem` como proceso continuo.

**Candidato tecnológico por defecto** (a revisar con datos reales en Fase 8-9, ver `docs/MultiplayerArchitecture.md` §2): Unity Gaming Services (Cloud Save + Cloud Code) como punto de partida, con opción de migrar a backend propio si la simulación continua lo exige.
