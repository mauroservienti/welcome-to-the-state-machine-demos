#region app-host

using Particular.Aspire.Hosting.ServicePlatform.Transport;

var builder = DistributedApplication.CreateBuilder(args);

var transportUserName = builder.AddParameter("transportUserName", "guest", secret: true);
var transportPassword = builder.AddParameter("transportPassword", "guest", secret: true);

var transport = builder.AddRabbitMQ("transport", transportUserName, transportPassword)
    .WithManagementPlugin(15672)
    .WithUrlForEndpoint("management", url => url.DisplayText = "RabbitMQ Management");

transportUserName.WithParentRelationship(transport);
transportPassword.WithParentRelationship(transport);

var pgUser = builder.AddParameter("pgUser", "postgres");
var pgPassword = builder.AddParameter("pgPassword", "P@ssw0rd", secret: true);

var database = builder.AddPostgres("database", pgUser, pgPassword)
    .WithEndpoint(targetPort: 5432, port: 5499, name: "tcp"); // Forces the host machine to use port 5432

database.WithPgAdmin(resource => 
{
    resource.WithParentRelationship(database);
    resource.WithUrlForEndpoint("http", url => url.DisplayText = "pgAdmin");
});


pgUser.WithParentRelationship(database);
pgPassword.WithParentRelationship(database);

var shippingDB      = database.AddDatabase("shipping-db");
var reservationDB   = database.AddDatabase("reservation-db");
var ticketingDB     = database.AddDatabase("ticketing-db");
var financeDB       = database.AddDatabase("finance-db"); 
var websiteDB       = database.AddDatabase("website-db"); 


var dbInitializer = builder.AddProject<Projects.DbInitializer>("DbInitializer")
    .WithReference(reservationDB)
    .WithReference(ticketingDB)
    .WithReference(financeDB)
    .WaitFor(database); // Wait until Postgres container is live before running scripts


var platform = builder
    .AddParticularPlatform("particular")
    .WithTransportRabbitMQ(RabbitMqRouting.QuorumConventionalRouting, transport)
    .AddDefaultComponents();

builder.AddProject<Projects.Reservations_Service>("ReservationsService")
    .WithReference(reservationDB) 
    .WaitFor(dbInitializer) 
    .WithParticularPlatform(platform)
    .WaitFor(platform);

builder.AddProject<Projects.Finance_Service>("FinanceService")
    .WithReference(financeDB) 
    .WaitFor(dbInitializer) 
    .WithParticularPlatform(platform)
    .WaitFor(platform);

builder.AddProject<Projects.Finance_PaymentGateway>("FinancePaymentGateway")
    .WithReference(financeDB) 
    .WaitFor(dbInitializer) 
    .WithParticularPlatform(platform)
    .WaitFor(platform);

builder.AddProject<Projects.Shipping_Service>("ShippingService")
    .WithReference(shippingDB)          
    .WaitFor(dbInitializer)             
    .WithParticularPlatform(platform)   
    .WaitFor(platform);                

// 1. Inform the AppHost metadata layer about the website project
var website = builder.AddProject<Projects.Website>("Website")
    .WithReference(websiteDB)
    .WithReference(financeDB)
    .WithReference(ticketingDB)
    .WithReference(reservationDB)    
    .WithParticularPlatform(platform)  
    .WaitForCompletion(dbInitializer)
    .WaitFor(platform)
    .WithUrlForEndpoint("http", url => url.DisplayText = "Welcome to the (state) Machine");


builder.Build().Run();
#endregion