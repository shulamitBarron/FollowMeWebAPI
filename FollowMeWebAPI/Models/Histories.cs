using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using QualiAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
  public  class Histories
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public DateTime DateError { get; set; }//Date send the error message
        public Group Group { get; set; }
        public string User { get; set; }
        public MessageGroup Message { get; set; }
        public Marker UserMarker { get; set; }
    }
}
