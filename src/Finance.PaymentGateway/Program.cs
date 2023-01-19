using NServiceBus;
using System;
using System.Threading.Tasks;

namespace Finance.PaymentGateway
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceName = typeof(Program).Namespace;
            Console.Title = serviceName;

            const string connectionString = @"Host=localhost;Port=7432;Username=db_user;Password=P@ssw0rd;Database=finance_service_database";
            var config = new EndpointConfiguration(serviceName);
            config.ApplyCommonConfigurationWithPersistence(connectionString);
            
            var endpointInstance = await Endpoint.Start(config);

            Console.WriteLine($"{serviceName} started. Press any key to stop.");
            Console.ReadLine();

            await endpointInstance.Stop();
        }
    }
}
