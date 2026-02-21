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

## Implementation notes

### Key infrastructure changes
- Added `DbContextOptions` constructor to `Ticketing.Data.TicketingContext` (required for in-memory EF support).
- Refactored all handlers/subscribers that hard-coded `new Data.SomeContext()` to accept an internal `Func<Context>` constructor, matching the pattern established in Plan 1.
- Added `[assembly: InternalsVisibleTo("ViewModelComposition.Tests")]` to all four composition projects so the test assembly can access their internal handler classes.
- `ServiceComposer.AspNetCore` stores the composed model at key `"composed-response-model"` and the context at `"composition-context"` in `HttpContext.Items` — verified via reflection against v4.1.3.

### Test helpers (`TestHelpers.cs` in `ViewModelComposition.Tests`)
- `FakeCompositionContext` — implements `ICompositionContext`; captures `RaiseEvent<T>` calls for assertion.
- `FakeTransactionalSession` — implements `ITransactionalSession`; captures `Send` calls; only the object-form `Send` overload is implemented since handlers only use that path.
- `FakeCompositionEventsPublisher<TEvent>` — captures the `CompositionEventHandler<TEvent>` delegate registered by a subscriber so tests can invoke it directly.
- `TestRequestBuilder.Build(...)` — creates a `DefaultHttpContext` with the composition infrastructure wired into `Items`, supports injecting cookies, route values, and form data.

### Test project: `src/ViewModelComposition.Tests`
xUnit + `Microsoft.EntityFrameworkCore.InMemory` + `ServiceComposer.AspNetCore` + `NServiceBus.TransactionalSession`.

#### Covered tests
| File | Handler/Subscriber | Tests |
|------|-------------------|-------|
| `TicketingViewModelCompositionTests.cs` | `AvailableTicketsGetHandler` | loads tickets → sets VM + raises event; empty DB → empty VM |
| | `ReservedTicketsLoadedSubscriber` | enriches with description; unknown ID does not throw |
| `ReservationsViewModelCompositionTests.cs` | `TicketsReservationGetHandler` | no cookie → null; cookie with no DB record → null; with record → VM + event |
| | `ReservationsReservePostHandler` | sends `ReserveTicket` with correct IDs |
| | `ReservationsCheckoutPostHandler` | sends `CheckoutReservation` with correct reservation ID |
| | `ReservationsCheckedoutGetHandler` | expires `reservation-id` cookie |
| | `AvailableTicketsLoadedSubscriber` | no reservation → TicketsLeft = Total; with reservation → TicketsLeft reduced |
| `FinanceViewModelCompositionTests.cs` | `ReservationsReservePostHandler` | sends `StoreReservedTicket` |
| | `ReservationsCheckoutPostHandler` | sends `InitializeReservationPaymentPolicy` |
| | `ReservationsCheckedoutGetHandler` | expires payment-method cookie |
| | `ReservationsFinalizePostHandler` | stores payment method to cookie |
| | `AvailableTicketsLoadedSubscriber` | enriches with ticket price |
| | `ReservedTicketsLoadedSubscriber` | enriches prices, totals, payment methods |
| | `ReviewReservedTicketsLoadedSubscriber` | enriches prices, totals, selected payment method |
| `ShippingViewModelCompositionTests.cs` | `ReservationsCheckoutPostHandler` | sends `InitializeReservationShippingPolicy` for ShipAtHome and CollectAtTheVenue |
| | `ReservationsCheckedoutGetHandler` | expires delivery-option cookie |
| | `ReservationsFinalizePostHandler` | stores delivery option to cookie |
| | `ReservedTicketsLoadedSubscriber` | adds DeliveryOptions list to VM |
| | `ReviewReservedTicketsLoadedSubscriber` | sets selected delivery option for each option |

Total: **26 tests**, all passing without external infrastructure.

