using ConsoleApp1.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ConsoleApp1.Services
{
    public class SequenceService
    {
        private readonly IMongoCollection<Sequence> _sequenceCollection;

        public SequenceService(IOptions<MongoDBSettings> mongoSettings)
        {
            var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _sequenceCollection = mongoDatabase.GetCollection<Sequence>("Counters"); 
        }

        public async Task<int> GetNextSequenceValueAsync(string collectionName)
        {
            var sequence = await _sequenceCollection.Find(x => x.CollectionName == collectionName).FirstOrDefaultAsync();

            if (sequence == null)
            {
                int startIndex = 1;
                sequence       = new Sequence { CollectionName = collectionName, LastValue = startIndex };
                await _sequenceCollection.InsertOneAsync(sequence);
                return startIndex;
            }
            else
            {
                int newIndex = sequence.LastValue + 1;
                var update   = Builders<Sequence>.Update.Set(s => s.LastValue, newIndex);
                await _sequenceCollection.UpdateOneAsync(s => s.CollectionName == collectionName, update);
                return newIndex;
            }
        }
    }

}
