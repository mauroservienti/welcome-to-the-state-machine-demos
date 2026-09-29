using Particular.Aspire.Hosting.ServicePlatform.Transport;

var builder = DistributedApplication.CreateBuilder(args);

var rabbitUser = builder.AddParameter("rabbit-user", "guest");
var rabbitPassword = builder.AddParameter("rabbit-password", "guest", secret: true);
var transport = builder.AddRabbitMQ("transport", rabbitUser, rabbitPassword)
    .WithManagementPlugin();

/*
 * One PostgreSQL server hosting one database per logical store,
 * mirroring the database containers defined in the dev container.
 * Resource names are what endpoints look up via ConnectionStrings:<name>.
 */
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var ticketingDatabase = postgres.AddDatabase("ticketing-database", "ticketing_database");
var financeDatabase = postgres.AddDatabase("finance-database", "finance_database");
var financeServiceDatabase = postgres.AddDatabase("finance-service-database", "finance_service_database");
var reservationsDatabase = postgres.AddDatabase("reservations-database", "reservations_database");
var reservationsServiceDatabase = postgres.AddDatabase("reservations-service-database", "reservations_service_database");
var shippingServiceDatabase = postgres.AddDatabase("shipping-service-database", "shipping_service_database");
var websiteDatabase = postgres.AddDatabase("website-database", "website_database");

var platform = builder.AddParticularPlatform("particular")
    .WithTransportRabbitMQ(RabbitMqRouting.ClassicConventionalRouting, transport)
    .AddDefaultComponents();

var createRequiredDatabases = builder.AddProject<Projects.CreateRequiredDatabases>("create-required-databases")
    .WithReference(ticketingDatabase).WaitFor(ticketingDatabase)
    .WithReference(financeDatabase).WaitFor(financeDatabase)
    .WithReference(reservationsDatabase).WaitFor(reservationsDatabase);

builder.AddProject<Projects.Finance_PaymentGateway>("finance-paymentgateway")
    .WithReference(transport).WaitFor(transport)
    .WithReference(financeServiceDatabase).WaitFor(financeServiceDatabase)
    .WithParticularPlatform(platform);

builder.AddProject<Projects.Finance_Service>("finance-service")
    .WithReference(transport).WaitFor(transport)
    .WithReference(financeServiceDatabase).WaitFor(financeServiceDatabase)
    .WithReference(financeDatabase)
    .WaitForCompletion(createRequiredDatabases)
    .WithParticularPlatform(platform);

builder.AddProject<Projects.Reservations_Service>("reservations-service")
    .WithReference(transport).WaitFor(transport)
    .WithReference(reservationsServiceDatabase).WaitFor(reservationsServiceDatabase)
    .WithReference(reservationsDatabase)
    .WaitForCompletion(createRequiredDatabases)
    .WithParticularPlatform(platform);

builder.AddProject<Projects.Shipping_Service>("shipping-service")
    .WithReference(transport).WaitFor(transport)
    .WithReference(shippingServiceDatabase).WaitFor(shippingServiceDatabase)
    .WithParticularPlatform(platform);

builder.AddProject<Projects.Website>("website")
    .WithReference(transport).WaitFor(transport)
    .WithReference(websiteDatabase).WaitFor(websiteDatabase)
    .WithReference(ticketingDatabase)
    .WithReference(financeDatabase)
    .WithReference(reservationsDatabase)
    .WaitForCompletion(createRequiredDatabases)
    .WithParticularPlatform(platform)
    .WithExternalHttpEndpoints();

builder.Build().Run();
