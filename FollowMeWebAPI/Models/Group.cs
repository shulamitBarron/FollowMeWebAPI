
using DAL.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace QualiAPI.Models
{
    public class Group
    {
        private DateTime _dateBeginTrip;
        private DateTime _dateEndTrip;

        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Password { get; set; }
        public string Code { get; set; }//Group code to allow the user to add himself to a trip-group 
        public bool Status { get; set; }//Group status is active or turned off
        public DefinitionGroup DefinitionGroup { get; set; }//Definition Group
        public List<ManagmentInGroup> ListManagment { get; set; }//List of group tutorials
        public List<UserInGroup> Users { get; set; }//List of travellers
        public List<UserInGroup> OkUsers { get; set; }//List of travellers confirm their participation in the trip
        public DateTime DateBeginTrip
          { get => _dateBeginTrip; set => _dateBeginTrip = (value >= DateTime.Now) ? value : _dateBeginTrip = DateTime.Now; }//Date begin of the trip- group (check if date valid).
        public DateTime DateEndTrip
          { get => _dateEndTrip; set => _dateEndTrip = (value>DateTime.Now)?value:_dateEndTrip=DateTime.Now.AddDays(1); }//Date end of the trip- group (check if date valid).
        public List<MessageGroup> ErrorMessage { get; set; }//List of custom error messages for each group
    }
}
