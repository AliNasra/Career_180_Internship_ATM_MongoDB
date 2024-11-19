using Microsoft.Extensions.DependencyInjection;
using ConsoleApp1.Helpers;
using ConsoleApp1.Models;
using ConsoleApp1.Services;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;


namespace ConsoleApp1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration      = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

            var mongoSettings      = new MongoDBSettings();
            configuration.GetSection("MongoDB").Bind(mongoSettings);
            var mongoOptions       = Options.Create(mongoSettings);
            var sequenceService    = new SequenceService(mongoOptions);
            var transactionService = new TransactionService(mongoOptions, sequenceService);
            var operationService   = new OperationService(mongoOptions, sequenceService);
            var customerService    = new CustomerService(mongoOptions, operationService, transactionService, sequenceService);
            var mongoDBService     = new MongoDBService(mongoSettings);
            bool isConnected       = mongoDBService.CheckConnection();

            if (isConnected)
            {
                Console.WriteLine("Connection Established");
                ATM atm = new ATM(customerService, transactionService);
                await atm.handleUser();
            }
            else
            {
                Console.WriteLine("Connection Failed");
                Console.ReadLine();
            }
        }

    }
}
