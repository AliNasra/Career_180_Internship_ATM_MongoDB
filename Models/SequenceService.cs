using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Models
{
    public class Sequence
    {
        [BsonId]
        public string CollectionName { get; set; }  // Name of the collection
        public int LastValue { get; set; }  // Last incremented value
    }

}
