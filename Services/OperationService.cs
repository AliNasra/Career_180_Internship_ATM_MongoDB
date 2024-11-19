using ConsoleApp1.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Services
{
    public class OperationService
    {
        private readonly IMongoCollection<Operation> _operationsCollection;
        private readonly SequenceService             _sequenceService;


        public OperationService(IOptions<MongoDBSettings> mongoSettings, SequenceService sequenceService)
        {
            var mongoClient       = new MongoClient(mongoSettings.Value.ConnectionString);
            var mongoDatabase     = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _operationsCollection = mongoDatabase.GetCollection<Operation>("Operations");
            _sequenceService      = sequenceService;
        }
        public async Task<List<Operation>> retrieveFinancialOperation(Customer customer)
        {
            string[] opts                       = {"Deposit", "Withdraw"};
            List<Operation> financialOperations = await _operationsCollection.Find(x => x.customerID == customer.customerID && opts.Contains(x.operationType)).ToListAsync();
            return financialOperations;
        }
        public async Task<List<Operation>> retrieveManagerialOperation(Customer customer)
        {
            string[] opts                       = { "SignIn", "SignUp", "SignOut", "ListOperations", "ListTransactions", "DepositInquiry" };
            List<Operation> mangerialOperations = await _operationsCollection.Find(x => x.customerID == customer.customerID && opts.Contains(x.operationType)).ToListAsync();
            return mangerialOperations;
        }
        public async Task addOperation(Operation operation)
        {
            operation.operationID  = await _sequenceService.GetNextSequenceValueAsync("Operations");
            //Console.WriteLine($"ID is : {operation.operationID}");
            await _operationsCollection.InsertOneAsync(operation);
        }


    }
}
