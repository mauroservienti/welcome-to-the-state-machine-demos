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

            var config = new EndpointConfiguration(serviceName);
            config.ApplyCommonConfigurationWithPersistence(@"Data Source=(localdb)\welcome-to-the-state-machine;Initial Catalog=Finance.Service;Integrated Security=True");
            
            var endpointInstance = await Endpoint.Start(config);

            Console.WriteLine($"{serviceName} started. Press any key to stop.");
            Console.ReadLine();

            await endpointInstance.Stop();
        }
    }
}
