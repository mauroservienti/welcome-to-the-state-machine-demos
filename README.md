# Welcome to the (state) machine - Demos

A ticket booking system - based on SOA principles.

This demo aims to demonstrate how to use sagas to overcome the architectural limitations of Process Managers in distributed systems. This is the support demo of my [Welcome to the (state) machine](https://milestone.topics.it/talks/welcome-to-the-state-machine.html) talk.

The demo assumes some knowledge of `service boundaries` and the role of `ViewModel Composition` in SOA-based systems. I recommend watching the following 2 talks to learn more about figuring out service boundaries and dealing with UI/ViewModel aspects in a microservices/SOA system without reintroducing coupling by making requests/responses between the services.

- [Finding your service boundaries - a practical guide](https://www.youtube.com/watch?v=tVnIUZbsxWI) by [@adamralph](https://twitter.com/adamralph)
- [All Our Aggregates Are Wrong](https://www.youtube.com/watch?v=KkzvQSuYd5I) by [@mauroservienti](https://twitter.com/mauroservienti) (myself)

An exhaustive dissertation about `ViewModel Composition` is available on my blog in the [ViewModel Composition series](https://milestone.topics.it/categories/view-model-composition).

## Current status

- The solution currently targets **.NET 10**.
- The recommended way to run the demos is via the provided **VS Code Dev Container**.
- Build and startup automation are provided via `.vscode/tasks.json` and `.vscode/launch.json`.
- The Website project restores client-side libraries via **LibMan/cdnjs**, so internet access is required while building.

## Requirements

The following requirements must be met to run the demos successfully:

- [Visual Studio Code](https://code.visualstudio.com/) and the [Dev containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers).
- [Docker](https://www.docker.com/get-started) must be pre-installed on the machine, including Docker Compose support.

## What this solution contains

The solution (`src/welcome-to-the-state-machine-demos.sln`) is organized around services, message contracts, data access, UI composition, and supporting infrastructure.

| Project | Role |
| --- | --- |
| `CreateRequiredDatabases` | Utility app that creates the PostgreSQL databases required by the demos. |
| `Finance.Data` | Finance data access and persistence model. |
| `Finance.Messages.Commands` | Finance command message contracts. |
| `Finance.Messages.Events` | Finance event message contracts. |
| `Finance.PaymentGateway.Messages` | Message contracts exchanged with the payment gateway endpoint. |
| `Finance.PaymentGateway` | Simulated external payment gateway endpoint. |
| `Finance.Service` | Finance business service and saga orchestration participants. |
| `Finance.ViewModelComposition` | Finance contribution to composed website view models. |
| `NServiceBus.Shared` | Shared NServiceBus setup/supporting infrastructure code. |
| `Policies.Tests` | Tests for saga policies and regular NServiceBus message handlers. |
| `Reservations.Data` | Reservations data access and persistence model. |
| `Reservations.Messages.Commands` | Reservations command message contracts. |
| `Reservations.Messages.Events` | Reservations event message contracts. |
| `Reservations.Service` | Reservations business service and saga orchestration participants. |
| `Reservations.ViewModelComposition.Events` | Events used by view-model composition components. |
| `Reservations.ViewModelComposition` | Reservations contribution to composed website view models. |
| `Shipping.Data` | Shipping data access and persistence model. |
| `Shipping.Messages.Commands` | Shipping command message contracts. |
| `Shipping.Messages.Events` | Shipping event message contracts. |
| `Shipping.Service` | Shipping business service and saga orchestration participants. |
| `Shipping.ViewModelComposition` | Shipping contribution to composed website view models. |
| `Ticketing.Data` | Ticketing read model data access for the composed UI. |
| `Ticketing.ViewModelComposition.Events` | Ticketing-focused events for UI composition workflows. |
| `Ticketing.ViewModelComposition` | Ticketing-level composition logic that aggregates service contributions. |
| `Website` | ASP.NET Core front-end hosting the ticketing user experience. |

## How to configure Visual Studio Code to run the demos

- Clone the repository
  - On Windows, make sure to clone on a short path, e.g., `c:\dev`, to avoid any "path too long" error
- Open the repository root folder in Visual Studio Code
- Make sure Docker is running
  - If you're using Docker for Windows with Hyper-V, make sure that the cloned folder, or a parent folder, is mapped in Docker
- Open the Visual Studio Code command palette (`F1` on all supported operating systems, for more information on VS Code keyboard shortcuts, refer to [this page](https://www.arungudelli.com/microsoft/visual-studio-code-keyboard-shortcut-cheat-sheet-windows-mac-linux/))
- Type `Reopen in Container`, the command palette supports auto-completion; the command should be available by typing `reop`

Wait for Visual Studio Code Dev containers extension to:

- download the required container images
- configure the docker environment
- configure the remote Visual Studio Code instance with the required extensions

> Note: no changes will be made to your Visual Studio Code installation; all changes will be applied to the VS Code instance running in the remote container

The repository `devcontainer` configuration will:

- One or more container instances:
  - One RabbitMQ instance with management plugin support
  - One .NET-enabled container where the repository source code will be mapped
  - A few PostgreSQL instances
- Configure the VS Code remote instance with:
  - The C# extension (`ms-dotnettools.csharp`)
  - The PostgreSQL Explorer extension (`ckolkman.vscode-postgres`)

Once the configuration is completed, VS Code will show a new `Ports` tab in the bottom-docked terminal area. The `Ports` tab will list all the ports the remote containers expose.

## Containers connection information

The default RabbitMQ credentials are:

- Username: `guest`
- Password: `guest`

The default PostgreSQL credentials are:

- User: `db_user`
- Password: `P@ssw0rd`

## How to run the demos

To execute the demo, open the root folder in VS Code, press `F1`, and search for `Reopen in container`. Wait for the Dev Container to complete the setup process.

Once the demo content has been reopened in the dev container:

1. Press `F1`, search for `Run task`, and execute `Build & create databases` (or run `Build solution` first and `Create databases` after).
2. Go to the `Run and Debug` VS Code section and start one of the available demo compounds (for example, `Demo - (build & deploy data)`).

### Disclaimer

This demo is built using [NServiceBus Sagas](https://docs.particular.net/nservicebus/sagas/); I work for [Particular Software](https://particular.net/), the makers of NServiceBus.
