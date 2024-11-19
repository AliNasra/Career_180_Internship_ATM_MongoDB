using ConsoleApp1.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using MongoDB.Driver.Core.Servers;

namespace ConsoleApp1.Services
{
    public class TransactionService
    {
        private readonly IMongoCollection<Transaction> _transactionsCollection;
        private readonly SequenceService               _sequenceService;


        public TransactionService(IOptions<MongoDBSettings> mongoSettings, SequenceService sequenceService)
        {
            var mongoClient         = new MongoClient(mongoSettings.Value.ConnectionString);
            var mongoDatabase       = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _transactionsCollection = mongoDatabase.GetCollection<Transaction>("Transactions");
            _sequenceService        = sequenceService;
        }
        public async Task<List<Transaction>> GetPendingTransactions(Customer customer)
        {
            List<Transaction> transactions = await _transactionsCollection.Find(x => x.recipientCustomerID == customer.customerID && x.IsCompleteTransfer == false).ToListAsync();
            return transactions;
        }
        public async Task<Transaction> GetTransaction(Customer senderCustomer, Customer recipientCustomer)
        {
            Transaction transaction = await _transactionsCollection.Find(x => x.recipientCustomerID == recipientCustomer.customerID && x.senderCustomerID == senderCustomer.customerID).FirstOrDefaultAsync();
            return transaction;
        }

        public async Task<Transaction> GetTransaction(int transactionId)
        {
            Transaction transaction = await _transactionsCollection.Find(x => x.transactionID == transactionId).FirstOrDefaultAsync();
            return transaction;
        }

        public async Task<Transaction> GetTransaction(Customer recipientCustomer, int transactionId)
        {
            Transaction transaction = await _transactionsCollection.Find(x => x.transactionID == transactionId && x.recipientCustomerID == recipientCustomer.customerID).FirstOrDefaultAsync();
            return transaction;
        }
        public async Task addTransaction(Transaction transaction)
        {
            transaction.transactionID = await _sequenceService.GetNextSequenceValueAsync("Transactions");
            await _transactionsCollection.InsertOneAsync(transaction);
        }

        public async Task UpdateTransaction(FilterDefinition<Transaction> filter, UpdateDefinition<Transaction> update)
        {
            var result_tr = await _transactionsCollection.UpdateOneAsync(filter, update);
        }

        public async Task<List<Transaction>> getRefundedTransactions(Customer customer)
        {
            List<Transaction> transaction = await _transactionsCollection.Find(x => x.senderCustomerID == customer.customerID && x.transactionState == "Refunded").ToListAsync();
            return transaction;
        }
        public async Task<List<Transaction>> getSettledTransactions(Customer customer)
        {
            List<Transaction> transaction = await _transactionsCollection.Find(x => x.recipientCustomerID == customer.customerID && x.IsCompleteTransfer == true).ToListAsync();
            return transaction;
        }
        public async Task<List<Transaction>> getMadeTransactions(Customer customer)
        {
            List<Transaction> transaction = await _transactionsCollection.Find(x => x.senderCustomerID == customer.customerID).ToListAsync();
            return transaction;
        }
    }
}
