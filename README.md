# Welcome to the (state) machine - Demos

A tickets booking system - based on SOA principles.

This demo aims to demonstrate how to use sagas to overcome the architectural limitations of Process Managers in distributed systems. This is the support demo of my [Welcome to the (state) machine](https://milestone.topics.it/talks/welcome-to-the-state-machine.html) talk.

The demo assumes some knowledge of what `service boundaries` are and what's the role `ViewModel Composition` in SOA based systems. I recommend you watch the following 2 talks, by [@adamralph](https://twitter.com/adamralph) and [@mauroservienti](https://twitter.com/mauroservienti) (myself), to find out more about how to figure out your service boundaries, and how to deal with UI/ViewModel aspects in a microservices/SOA system without reintroducing coupling by doing request/response between the services.

- [Finding your service boundaries - a practical guide](https://www.youtube.com/watch?v=tVnIUZbsxWI)
- [All Our Aggregates Are Wrong](https://www.youtube.com/watch?v=KkzvQSuYd5I)

An exhaustive dissertation about `ViewModel Composition` is available on my blog in the [ViewModel Composition series](https://milestone.topics.it/categories/view-model-composition).

## How to get the sample working locally

### Get a copy of this repository

Clone or download this repo locally on your machine. If you're downloading a zip copy of the repo please be sure the zip file is unblocked before decompressing it. In order to unblock the zip file:

- Right-click on the downloaded copy
- Choose Property
- On the Property page tick the unblock checkbox
- Press OK

### Check your machine is correctly configured

In order to run the sample the following machine configuration is required:

- PowerShell execution policy to allow script execution, from an elevated PowerShell run the following:

```powershell
Set-ExecutionPolicy Unrestricted
```

- Visual Studio 2019 with [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0) support (Community Edition is supported), available for download at [https://www.visualstudio.com/downloads/](https://www.visualstudio.com/downloads/)

- A SQL Server edition or the `LocalDb` instance installed by Visual Studio, in case of a clean machine with `LocalDb`only please install:
  - Microsoft ODBC Driver 11 for SQL Server, available for download at [https://www.microsoft.com/en-us/download/details.aspx?id=36434](https://www.microsoft.com/en-us/download/details.aspx?id=36434)
  - Microsoft ODBC Command Line Utilities 11 for SQL Server, available for download at [https://www.microsoft.com/en-us/download/details.aspx?id=36433](https://www.microsoft.com/en-us/download/details.aspx?id=36433)

NOTE: On a clean machine do not install latest version, as of this writing 13.1, of Microsoft ODBC Driver and Microsoft ODBC Command Line Utilities as the latter is affected by a bug that prevents the `LocalDb` instance to be accessible at configuration time.

### Databases setup

To simplify `LocalDB` instance setup 2 PowerShell scripts, in the [scripts](scripts) folder, are provided for your convenience. Both need to be run from an elevated PowerShell console.

- Run `Setup-LocalDB-Instance.ps1`, with elevation, to create the `LocalDB` instance and all the required databases
- Run `Teardown-LocalDB-Instance.ps1`, with elevation, to drop all the databases and delete the `LocalDB` instance

The created `LocalDB` instance is named `(localdb)\welcome-to-the-state-machine`. To only recreate databases run the `Recreate-Databases.ps1`, with elevation, against an already created `LocalDB` instance.

NOTE: If you receive errors regarding "Microsoft ODBC Driver", you can work around these by connecting to the `(localdb)\welcome-to-the-state-machine` database using, for example, Visual Studio or SQL Managerment Studio, and running the SQL contained in the `Setup-Databases.sql` to manually create databases.

NOTE: In case the database setup script fails with a "sqllocaldb command not found" error it is possible to install `LocalDb` as a standalone package by downloading it separately at [https://www.microsoft.com/en-us/download/details.aspx?id=29062](https://www.microsoft.com/en-us/download/details.aspx?id=29062)

## Startup projects

Solutions is configured to use the [SwitchStartupProject](https://marketplace.visualstudio.com/items?itemName=vs-publisher-141975.SwitchStartupProject) Visual Studio Extension to manage startup projects. The extension is not a requirement, it's handy.

Ensure the following projects are set as startup projects:

- `Website`
- `Reservations.Service`
- `Finance.Service`
- `Finance.PaymentGateway`
- `Shipping.Service`

## NServiceBus configuration

This sample has no [NServiceBus](https://particular.net/nservicebus) related pre-requisites as it's configured to use [Learning Transport](https://docs.particular.net/nservicebus/learning-transport/) and [Learning Persistence](https://docs.particular.net/nservicebus/learning-persistence/), both explicitly designed for short term learning and experimentation purposes.

They should also not be used for longer-term development, i.e. the same transport and persistence used in production should be used in development and debug scenarios. Select a production [transport](https://docs.particular.net/transports/) and [persistence](https://docs.particular.net/persistence/) before developing features. 

> NOTE: Do not use the learning transport or learning persistence to perform any kind of performance analysis.

### Disclaimer

This demo is built using [NServiceBus Sagas](https://docs.particular.net/nservicebus/sagas/), I work for [Particular Software](https://particular.net/), the makers of NServiceBus.
