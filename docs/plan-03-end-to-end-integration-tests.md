# Plan 3: End-to-end integration tests for the full flow

## Goal
Test the complete reservation-to-payment-to-shipping workflow across services with realistic infrastructure.

## Scope
- Full business flow:
  1. reserve ticket
  2. checkout reservation
  3. payment authorization/charge
  4. shipping initialization and completion signals
- Services involved:
  - `Website`
  - `Reservations.Service`
  - `Finance.Service`
  - `Finance.PaymentGateway`
  - `Shipping.Service`

## Test environment
1. Run integration tests on Linux only.
2. Use Docker Compose to start required infrastructure (PostgreSQL and transport dependencies already defined for local/devcontainer setup).
3. Start service processes in test setup (or via testcontainers/docker-compose orchestration).

## Test design
1. Add an integration test project (for example `Flow.IntegrationTests`) that:
   - sends HTTP requests through `Website` endpoints
   - waits/polls for eventual consistency outcomes in read models/databases
   - verifies expected final state (checked out + payment succeeded + shipping initiated/completed event presence)
2. Cover at least:
   - happy path
   - payment failure path (authorization/charge failure)
   - duplicate message resilience for one critical step
3. Ensure no messages end up in the `error` queue by spinning up a queue listener for the error queue. A message in the error queue results in a failing test for the happy path, or might be the expected result for a failure path

## CI strategy (Linux only)
1. Add a Linux-only workflow job for integration tests:
   - `runs-on: ubuntu-latest`
   - start Docker dependencies
   - run `dotnet test` for integration test project
2. Keep current fast unit tests in existing CI path; run integration suite in separate job/stage.
3. Publish logs/artifacts (service logs + test output) when integration tests fail.

## Done criteria
- Integration tests run automatically on Linux CI.
- At least one full happy-path scenario validates the whole distributed flow.
- Failures provide enough logs to diagnose orchestration or infrastructure issues quickly.
