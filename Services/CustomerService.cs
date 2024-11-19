using ConsoleApp1.Helpers;
using ConsoleApp1.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ConsoleApp1.Services
{
    public class CustomerService
    {
        private readonly IMongoCollection<Customer> _customersCollection;
        private readonly OperationService           _operationsService;
        private readonly TransactionService         _transactionsService;
        private readonly SequenceService            _sequenceService;


        public CustomerService(IOptions<MongoDBSettings> mongoSettings, OperationService operationService, TransactionService transactionService, SequenceService sequenceService)
        {
            var mongoClient       = new MongoClient(mongoSettings.Value.ConnectionString);
            var mongoDatabase     = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _customersCollection  = mongoDatabase.GetCollection<Customer>("Customers");
            _operationsService    = operationService;
            _transactionsService  = transactionService;
            _sequenceService      = sequenceService;

        }
        public async Task addCustomer(Customer customer)
        {
            customer.customerID = await _sequenceService.GetNextSequenceValueAsync("Customers");
            await _customersCollection.InsertOneAsync(customer);
        }
        public async Task<List<Customer>> getOrdinaryCustomers()
        {
            List<Customer> ordinaryCustomers = await _customersCollection.Find(x => x.customerType == "O").ToListAsync();
            return ordinaryCustomers; 
        }
        public async Task<List<Customer>> getVIPCustomers()
        {
            List<Customer> VIPCustomers = await _customersCollection.Find(x => x.customerType == "V").ToListAsync();
            return VIPCustomers;
        }
    
        public async Task<Customer> retrieveCustomer(String username)
        {
            Customer? customer = await _customersCollection.Find(u => u.userName == username).FirstOrDefaultAsync();
            return customer;
        }
        public async Task<Customer> retrieveCustomer(int userID)
        {
            Customer? customer = await _customersCollection.Find(u => u.customerID == userID).FirstOrDefaultAsync();
            return customer;
        }
        public async Task<Customer> checkCustomer(string username, string password)
        {

            Customer? customer = await _customersCollection.Find(u => u.userName == username).FirstOrDefaultAsync();
            if (customer == null)
            {
                return null;
            }
            else
            {
                bool checkPassword = EncryptionServices.DecryptPassword(customer.password) == password ? true : false;
                return (checkPassword ? customer : null);
            }
            return customer;
        }
        public async Task<bool> checkCustomer(string username)
        {
            bool checkCustomerExistence = await _customersCollection.Find(u => u.userName == username && u.activityStatus == true).AnyAsync();
            return !checkCustomerExistence;
        }
        public async Task registerVIPUser(string userName, string password, string email, string birthdate)
        {
            string encryptedPassword = EncryptionServices.EncryptPassword(password);
            DateTime birthDate       = DateTime.Parse(birthdate);
            Customer VIPCustomer     = new Customer
            {
                userName             = userName,
                password             = encryptedPassword,
                email                = email,
                birthDate            = birthDate,
                accountDate          = DateTime.Now,
                operationCounter     = 0,
                accountTimer         = DateTime.Now.AddDays(1),
                customerType         = "V",
                activityStatus       = true,
            };
            await addCustomer(VIPCustomer);
            Customer newCustomer      = await _customersCollection.Find(c => c.userName == userName).FirstOrDefaultAsync();
            Operation signUpOperation = new Operation
            {
                operationDate         = DateTime.Now,
                customerID            = newCustomer.customerID,
                operationType         = "SignUp",
                successStatus         = true
            };
            await _operationsService.addOperation(signUpOperation);
            Console.WriteLine("Registration Completed Successfully");
        }
        public async Task registerOrdinaryUser(string userName, string password, string email, string birthdate)
        {
            string encryptedPassword  = EncryptionServices.EncryptPassword(password);
            DateTime birthDate        = DateTime.Parse(birthdate);
            Customer ordinaryCustomer = new Customer
            {
                userName         = userName,
                password         = encryptedPassword,
                email            = email,
                birthDate        = birthDate,
                accountDate      = DateTime.Now,
                operationCounter = 0,
                accountTimer     = DateTime.Now.AddDays(1),
                customerType     = "O",
                activityStatus   = true
            };
            await addCustomer(ordinaryCustomer);
            Customer newCustomer      = await _customersCollection.Find(c => c.userName == userName).FirstOrDefaultAsync();
            Operation signUpOperation = new Operation
            {
                operationDate         = DateTime.Now,
                customerID            = newCustomer.customerID,
                operationType         = "SignUp",
                successStatus         = true
            };
            await _operationsService.addOperation(signUpOperation);           
            Console.WriteLine("Registration Completed Successfully");
        }
        public async Task signIn(Customer customer)
        {
            Operation signInOperation = new Operation
            {
                operationDate         = DateTime.Now,
                customerID            = customer.customerID,
                operationType         = "SignIn",
                successStatus         = true
            };
            await _operationsService.addOperation(signInOperation);
            Console.WriteLine("Welcome On Board!");
        }
        public async Task signOut(Customer customer)
        {
            Operation signOutOperation = new Operation
            {
                operationDate          = DateTime.Now,
                customerID             = customer.customerID,
                operationType          = "SignOut",
                successStatus          = true
            };
            await _operationsService.addOperation(signOutOperation);
        }
        public async Task<double> getDepositInfo(Customer customer)
        {
            Operation DepositOperation = new Operation
            {
                operationDate          = DateTime.Now,
                customerID             = customer.customerID,
                operationType          = "DepositInquiry",
                successStatus          = true
            };
            await _operationsService.addOperation(DepositOperation);
            return customer.bankDeposit;
        }
        public async Task depositMoney(Customer customer, double amount)
        {
            if (!await canPerformOperation(customer))
            {
                Console.WriteLine($"You have reached your operation limit. You can perform financial operations again after {customer.accountTimer:dddd, MMMM d, yyyy h:mm tt}");
                return;
            }
            var filter                 = Builders<Customer>.Filter.Eq(c => c.customerID, customer.customerID);
            var update                 = Builders<Customer>.Update.Inc(c => c.bankDeposit, amount).Inc(c=>c.operationCounter,1);
            Operation DepositOperation = new Operation
            {
                operationDate          = DateTime.Now,
                customerID             = customer.customerID,
                moneyAmount            = amount,
                operationType          = "Deposit",
                successStatus          = true
            };

            await _operationsService.addOperation(DepositOperation);
            var result                = await _customersCollection.UpdateOneAsync(filter, update);
            customer.bankDeposit      = customer.bankDeposit + amount;
            customer.operationCounter = customer.operationCounter + 1;
            Console.WriteLine("Operation Completed Successfully!");
        }
        public async Task withdrawMoney(Customer customer, double amount)
        {
            if (!await canPerformOperation(customer))
            {
                Console.WriteLine($"You have reached your operation limit. You can perform financial operations again after {customer.accountTimer:dddd, MMMM d, yyyy h:mm tt}");
                return;
            }
            var filter                  = Builders<Customer>.Filter.Eq(c => c.customerID, customer.customerID);
            var update                  = Builders<Customer>.Update.Inc(c => c.bankDeposit, -1 * amount).Inc(c => c.operationCounter, 1);
            Operation WithdrawOperation = new Operation
            {
                operationDate           = DateTime.Now,
                customerID              = customer.customerID,
                moneyAmount             = amount,
                operationType           = "Withdraw",
                successStatus           = true
            };
            await _operationsService.addOperation(WithdrawOperation);
            var result                = await _customersCollection.UpdateOneAsync(filter, update);
            customer.bankDeposit      = customer.bankDeposit - amount;
            customer.operationCounter = customer.operationCounter + 1;
            Console.WriteLine("Operation Completed Successfully!");
        }
        public async Task<bool> verifyIdentity(string userName, string password)
        {
            bool checkCustomer = await _customersCollection.Find(u => u.userName == userName && EncryptionServices.DecryptPassword(u.password) == password && u.activityStatus == true).AnyAsync();
            return checkCustomer;
        }
        public async Task makeTransaction(Customer customer ,Customer recipient, double amount)
        {
            if (!await canPerformOperation(customer))
            {
                Console.WriteLine($"You have reached your operation limit. You can perform financial operations again after {customer.accountTimer:dddd, MMMM d, yyyy h:mm tt}");
                return;
            }
            var filter = Builders<Customer>.Filter.Eq(c => c.customerID, customer.customerID);
            var update = Builders<Customer>.Update.Inc(c => c.bankDeposit, -1 * amount).Inc(c => c.operationCounter, 1);
            Transaction transaction = new Transaction
            {
            senderCustomerID             = customer.customerID,
            recipientCustomerID          = recipient.customerID,
            transferredMoney             = amount,
            senderPretransferDeposit     = customer.bankDeposit,
            recipientPretransferDeposit  = recipient.bankDeposit,
            transactionState             = "Pending",
            transactionTime              = DateTime.Now,
            successStatus                = true,
            IsCompleteTransfer           = false,
            };
            await _transactionsService.addTransaction(transaction);
            var result                = await _customersCollection.UpdateOneAsync(filter, update);
            customer.bankDeposit      = customer.bankDeposit - amount;
            customer.operationCounter = customer.operationCounter + 1;
            Console.WriteLine("Transaction Completed Successfully!");
        }
        public async Task acceptTransaction(Customer customer, Transaction transaction)
        {
            if (!await canPerformOperation(customer))
            {
                Console.WriteLine($"You have reached your operation limit. You can perform financial operations again after {customer.accountTimer:dddd, MMMM d, yyyy h:mm tt}");
                return;
            }
            if (transaction.recipientCustomerID == customer.customerID)
            {
                Customer senderCustomer = await retrieveCustomer(transaction.senderCustomerID);
                var filterCustomer      = Builders<Customer>.Filter.Eq(c => c.customerID, customer.customerID);
                var updateCustomer      = Builders<Customer>.Update.Inc(c => c.bankDeposit, transaction.transferredMoney).Inc(c => c.operationCounter, 1);
                var filterTransaction   = Builders<Transaction>.Filter.Eq(c => c.transactionID, transaction.transactionID);
                var updateTransaction   = Builders<Transaction>.Update.Set(c => c.transactionState, "Accepted")
                                                                           .Set(c => c.IsCompleteTransfer, true)
                                                                           .Set(c => c.senderPosttransferDeposit, senderCustomer.bankDeposit)
                                                                           .Set(c => c.recipientPosttransferDeposit, customer.bankDeposit + transaction.transferredMoney)
                                                                           .Set(c => c.conclusionTime, DateTime.Now);
                await _transactionsService.UpdateTransaction(filterTransaction, updateTransaction);
                var result_cu             = await _customersCollection.UpdateOneAsync(filterCustomer, updateCustomer);
                customer.bankDeposit      = customer.bankDeposit + transaction.transferredMoney;
                customer.operationCounter = customer.operationCounter + 1;
            }
            else
            {
                Console.WriteLine("Please Check Your Input!");
            }
        }
        public async Task rejectTransaction(Customer customer, Transaction transaction)
        {
            if (!await canPerformOperation(customer))
            {
                Console.WriteLine($"You have reached your operation limit. You can perform financial operations again after {customer.accountTimer:dddd, MMMM d, yyyy h:mm tt}");
                return;
            }
            if (transaction.recipientCustomerID == customer.customerID)
            {
                Customer senderCustomer                  = await retrieveCustomer (transaction.senderCustomerID);
                var filterSender                         = Builders<Customer>.Filter.Eq(c => c.customerID, senderCustomer.customerID);
                var updateSender                         = Builders<Customer>.Update.Inc(c => c.bankDeposit, transaction.transferredMoney);
                var filterRecipient                      = Builders<Customer>.Filter.Eq(c => c.customerID, customer.customerID);
                var updateRecipient                      = Builders<Customer>.Update.Inc(c => c.operationCounter, 1);
                var filterTransaction                    = Builders<Transaction>.Filter.Eq(c => c.transactionID, transaction.transactionID);
                var updateTransaction                    = Builders<Transaction>.Update.Set(c => c.transactionState, "Rejected")
                                                                           .Set(c => c.IsCompleteTransfer, true)
                                                                           .Set(c => c.senderPosttransferDeposit, senderCustomer.bankDeposit + transaction.transferredMoney)
                                                                           .Set(c => c.recipientPosttransferDeposit, customer.bankDeposit)
                                                                           .Set(c => c.conclusionTime, DateTime.Now);
                await _transactionsService.UpdateTransaction(filterTransaction, updateTransaction);
                var result_se             = await _customersCollection.UpdateOneAsync(filterSender, updateSender);
                var result_re             = await _customersCollection.UpdateOneAsync(filterRecipient, updateRecipient);
                customer.operationCounter = customer.operationCounter + 1;

            }
            else
            {
                Console.WriteLine("Please Check Your Input!");
            }
        }
        public async Task ViewRecentTransactions(Customer customer)
        {
            List<Transaction> pendingTransactions = await _transactionsService.GetPendingTransactions(customer);
            if (pendingTransactions.Count == 0)
            {
                Console.WriteLine("No Pending Transactions Found!");
            }
            else
            {
                foreach (Transaction transaction in pendingTransactions)
                {
                    Console.WriteLine(transaction.getPendingTransactionString());
                }
            }    
            Operation ListOperation = new Operation
            {
                operationDate       = DateTime.Now,
                customerID          = customer.customerID,
                operationType       = "ListTransactions",
                successStatus       = true
            };
            await _operationsService.addOperation(ListOperation);
        }


        public async Task<bool> handleVIPUserDeletion(Customer customer)
        {
            List<Transaction> pendingTransactions = await _transactionsService.GetPendingTransactions(customer);
            int pendingTransactionCount           = pendingTransactions.Count;
            int waitSeconds                       = 30;
            string? userResponse                  = null;
            if (pendingTransactionCount > 0)
            {
                Console.WriteLine($"You have {pendingTransactionCount} pending transaction(s). Do you want to proceed? y/n");
                Console.WriteLine($"Please answer within {waitSeconds} seconds!");
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                userResponse = await ATM.ReadInputAsync(cts.Token);
                if (userResponse == null)
                {
                    Console.WriteLine($"Time expired! You didn't enter anything within {waitSeconds} seconds.");
                    return false;
                }
                else if (userResponse == "y")
                {
                }
                else if (userResponse == "n")
                {
                    return false;
                }
                else
                {
                    Console.WriteLine("Please type either y or n to declare your decision");
                    return false;
                }
            }
            await resolveTransactions(customer);
            var filterCu = Builders<Customer>.Filter.Eq(c => c.customerID, customer.customerID);
            var updateCu = Builders<Customer>.Update.Set(c => c.activityStatus, false);
            await _customersCollection.UpdateOneAsync(filterCu, updateCu);

            Console.WriteLine("Account Deleted Successfully");
            return true;
        }

        public async Task listAllOperations(Customer customer)
        {
            List<Operation> fO = await _operationsService.retrieveFinancialOperation(customer);
            List<Operation> mO = await _operationsService.retrieveManagerialOperation(customer);
            Console.WriteLine("Financial Operations:");
            if (fO.Count == 0)
            {
                Console.WriteLine("No Financial Operations Were Found!");
            }
            else
            {
                foreach (Operation op in fO)
                {
                    Console.WriteLine(op.getFinancialOperationString());
                }
            }
            Console.WriteLine("Managerial Operations:");
            if (mO.Count == 0)
            {
                Console.WriteLine("No Managerial Operations Were Found!");
            }
            else
            {
                foreach (Operation op in mO)
                {
                Console.WriteLine(op.getManagerialOperationString());
                }
            }
            Console.WriteLine("Transaction-related Operations:");
            List<Transaction> refundedTransactions = await _transactionsService.getRefundedTransactions(customer);
            List<Transaction> settledTransactions  = await _transactionsService.getSettledTransactions(customer);
            List<Transaction> madeTransactions     = await _transactionsService.getMadeTransactions(customer);
            refundedTransactions.AddRange(settledTransactions);
            refundedTransactions.AddRange(madeTransactions);
            if (refundedTransactions.Count == 0)
            {
                Console.WriteLine("No Transaction-related Operations Were Found!");
            }
            else
            {
                foreach (Transaction transaction in refundedTransactions)
                {
                    Console.WriteLine(transaction.getTransactionString());
                }
            }         
            Operation ListOperation = new Operation
            {
                operationDate       = DateTime.Now,
                customerID          = customer.customerID,
                operationType       = "ListOperations",
                successStatus       = true
            };
            await _operationsService.addOperation(ListOperation);
        }
        public async Task<bool> canPerformOperation(Customer customer)
        {
            int operationLimit = 10;
            if (DateTime.Now > customer.accountTimer)
            {
                DateTime newTime = customer.accountTimer;
                while (true)
                {
                    newTime = newTime.AddDays(1);
                    if (newTime > customer.accountTimer)
                    {
                        break;
                    }
                }
                var filter                = Builders<Customer>.Filter.Eq(c => c.customerID, customer.customerID);
                var update                = Builders<Customer>.Update.Set(c => c.accountTimer, newTime)
                                                                     .Set(c => c.operationCounter, 0);
                var result                = await _customersCollection.UpdateOneAsync(filter, update);
                customer.accountTimer     = newTime;
                customer.operationCounter = 0;
                return true;
            }
            else
            {
                if (customer.operationCounter < operationLimit)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public async Task resolveTransactions(Customer deletedCustomer)
        {
            List<Transaction> transactionsToBeResolved = await _transactionsService.GetPendingTransactions(deletedCustomer);
            foreach (Transaction transaction in transactionsToBeResolved)
            {
                Customer senderCustomer = await retrieveCustomer(transaction.senderCustomerID);
                var filterCu            = Builders<Customer>.Filter.Eq(c => c.customerID, transaction.senderCustomerID);
                var updateCu            = Builders<Customer>.Update.Inc(c => c.bankDeposit, transaction.transferredMoney);
                var filterDe            = Builders<Customer>.Filter.Eq(c => c.customerID, deletedCustomer.customerID);
                var updateDe            = Builders<Customer>.Update.Set(c => c.activityStatus, false);
                var resultCu            = await _customersCollection.UpdateOneAsync(filterCu, updateCu);
                var resultDe            = await _customersCollection.UpdateOneAsync(filterDe, updateDe);
                var filterTr            = Builders<Transaction>.Filter.Eq(c => c.transactionID, transaction.transactionID);
                var updateTr            = Builders<Transaction>.Update.Set(c => c.recipientPosttransferDeposit, deletedCustomer.bankDeposit)
                                                           .Set(c => c.senderPosttransferDeposit, senderCustomer.bankDeposit)
                                                           .Set(c => c.conclusionTime, DateTime.Now)
                                                           .Set(c => c.transactionState, "Refunded")
                                                           .Set(c => c.IsCompleteTransfer, true);
                await _transactionsService.UpdateTransaction(filterTr, updateTr);
            }
        }

    }
}
