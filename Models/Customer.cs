using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConsoleApp1.Models
{
    public class Customer
    {
        [BsonId]
        [BsonElement("customerID")]
        public int        customerID;
        public string     userName;
        public string     password;
        public double     bankDeposit;
        public string     email;
        public DateTime   birthDate;
        public DateTime   accountDate;
        public DateTime   accountTimer;
        public int        operationCounter;
        public string     customerType;
        public bool       activityStatus;
    }
}
