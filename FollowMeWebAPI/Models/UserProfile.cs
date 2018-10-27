using DAL.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace QualiAPI.Models
{
    public class UserProfile
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Image { get; set; }
        public bool Status { get; set; }//If the traveler-user active (for example when the battery ends the traveler is not active.)   
        public List<MessageUser> UserMessageNeedGet { get; set; }//List of error message that the traveler should receive
        public List<Histories> MessagesThatGet { get; set; }
        public Marker Marker { get; set; }//The location of the traveler-user on the map
    }
}
