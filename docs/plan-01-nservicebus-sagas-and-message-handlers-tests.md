# Plan 1: NServiceBus saga and message handler tests

## Goal
Increase confidence in saga orchestration and handler behavior by extending the existing `Policies.Tests` approach.

## Scope
- Sagas:
  - `ReservationPolicy`
  - `PaymentPolicy`
  - `ShippingPolicy`
- Message handlers not covered yet:
  - `Reservations.Service` handlers
  - `Finance.Service` handlers
  - `Shipping.Service` handlers

## Test design
1. Keep using `xUnit` + `NServiceBus.Testing` (`TestableMessageHandlerContext`, saga testing APIs, and if needed the `TestableSaga<TSaga>` [approach](https://docs.particular.net/nservicebus/testing/saga-scenario-testing)).
2. Add one test class per handler/saga in a test project close to `Policies.Tests` conventions.
3. For each saga:
   - verify correlation mapping
   - verify outgoing commands/events for each incoming message
   - verify completion behavior (`MarkAsComplete`) when flow ends
4. For each handler:
   - verify state changes in data model (or mocked repository/context interaction)
   - verify emitted commands/events/replies
   - verify idempotent behavior where duplicate message handling is possible

## Incremental rollout
1. Start from happy-path tests for all sagas.
2. Add negative/edge cases (missing data, duplicate messages, already-completed flows).
3. Add handler tests for services with highest business impact first (`Reservations.Service`, then `Finance.Service`, then `Shipping.Service`).

## Done criteria
- Every saga has happy-path and completion tests.
- Each message handler has at least one behavior test.
- Tests run in CI with `dotnet test` without external infrastructure dependencies.

## Status check for PR #453
PR: https://github.com/mauroservienti/welcome-to-the-state-machine-demos/pull/453

### Covered by PR #453
- Added infrastructure-free handler behavior tests in `Policies.Tests` using `xUnit` + `NServiceBus.Testing`.
- Added tests for handlers in all three scoped service areas:
  - `Reservations.Service`
  - `Finance.Service`
  - `Shipping.Service`
- Added in-memory EF support/hooks to enable deterministic handler tests.

### Completed after PR #453
- Added saga-focused tests for:
  - `ReservationPolicy`
  - `PaymentPolicy`
  - `ShippingPolicy`
- Added edge/idempotency coverage for duplicate-message and missing-data handler scenarios.
- Plan 1 done criteria are now satisfied in `Policies.Tests`.

### Summary
PR #453 handled the Plan 1 kickoff and a meaningful subset of handler coverage; subsequent work completed the remaining saga and edge/idempotency coverage to satisfy Plan 1.
