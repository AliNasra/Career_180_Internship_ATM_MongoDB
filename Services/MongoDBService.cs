using ConsoleApp1.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Services
{
    public class MongoDBService
    {
        private readonly IMongoDatabase _database;

        public MongoDBService(MongoDBSettings mongoSettings)
        {
            var client = new MongoClient(mongoSettings.ConnectionString);
            _database = client.GetDatabase(mongoSettings.DatabaseName);
        }

        public bool CheckConnection()
        {
            try
            {
                var command = new BsonDocument("ping", 1);
                _database.RunCommand<BsonDocument>(command);
                Console.WriteLine("MongoDB connection is healthy.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to MongoDB: {ex.Message}");
                return false;
            }
        }
    }
}
