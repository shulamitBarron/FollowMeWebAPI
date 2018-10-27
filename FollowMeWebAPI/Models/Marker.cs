using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QualiAPI.Models
{
    public class Marker
    {
        public Double Lng { get; set; }//longitude
        public Double Lat { get; set; }//latitude
        public string NameAndPhone { get; set; }//full name user+ phone user
    }
}