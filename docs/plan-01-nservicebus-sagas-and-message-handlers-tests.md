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
1. Keep using `xUnit` + `NServiceBus.Testing` (`TestableMessageHandlerContext` and saga testing APIs).
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
