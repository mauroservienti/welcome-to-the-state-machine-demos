# Plan 2: ServiceComposer ViewModel composition tests

## Goal
Validate that ViewModel composition handlers/subscribers enrich responses correctly and remain stable when composition inputs vary.

## Scope
- Composition endpoints in:
  - `Reservations.ViewModelComposition`
  - `Finance.ViewModelComposition`
  - `Shipping.ViewModelComposition`
  - `Ticketing.ViewModelComposition`
- Focus on:
  - `*GetHandler`
  - `*PostHandler`
  - `*LoadedSubscriber`

## Test design
1. Create composition tests that execute handlers/subscribers with controlled input models and verify final composed view model shape.
2. Use in-memory doubles for dependencies (DB context/repositories/message session) to avoid external services.
3. Add tests for:
   - successful enrichment of composed models
   - missing optional sections (composition still succeeds)
   - conflicting/duplicate data (deterministic merge behavior)
   - post handlers producing expected commands and redirects/results

## Incremental rollout
1. Start with `Reservations.ViewModelComposition` and `Finance.ViewModelComposition` checkout/review flows.
2. Add `Shipping` and `Ticketing` composition tests.
3. Add regression tests for known tricky enrichments (reserved tickets, available tickets, review/checkedout pages).

## Done criteria
- Every composition project has at least one test covering each handler/subscriber category used in runtime flows.
- Tests validate both data enrichment and command dispatch behavior.
- No dependency on Docker or running endpoints for these tests.
