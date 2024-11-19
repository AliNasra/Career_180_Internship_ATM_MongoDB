using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace ConsoleApp1.Models
{
    public class Transaction : IComparable<Transaction>
    {
        [BsonId]
        [BsonElement("transactionID")]
        public int      transactionID                { get; set; }
        public int      senderCustomerID             { get; set; }
        public int      recipientCustomerID          { get; set; }
        public double   transferredMoney             { get; set; }
        public double   senderPretransferDeposit     { get; set; }
        public double?  senderPosttransferDeposit    { get; set; }
        public double   recipientPretransferDeposit  { get; set; }
        public double?  recipientPosttransferDeposit { get; set; }
        public string   transactionState             { get; set; }
        public DateTime transactionTime              { get; set; }
        public DateTime conclusionTime               { get; set; }
        public bool     successStatus                { get; set; }
        public bool     IsCompleteTransfer           { get; set; }

        public int CompareTo(Transaction other)
        {
            return this.transactionTime.CompareTo(other.transactionTime);
        }
        public string getTransactionString()
        {
            return $"{this.transactionTime:dddd, MMMM d, yyyy h:mm tt} - Transaction of ID {this.transactionID} from user of ID: {this.senderCustomerID} resolved as: {this.transactionState}";
        }
        public string getPendingTransactionString()
        {
            return $"{this.transactionTime:dddd, MMMM d, yyyy h:mm tt} - Transaction of ID {this.transactionID} from user of ID: {this.senderCustomerID} with amount {this.transferredMoney}";
        }
    }
}
