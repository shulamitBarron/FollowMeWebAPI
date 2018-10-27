using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QualiAPI.Models
{
    public class DefinitionGroup
    {
        public WhenStatusOpen eWhenStatusOpen { get; set; }//when group-trip start (enum WhenStatusOpen { onOpen, onDateBegin })Automatic or manual 
        public double Distance { get; set; }//Variable that indicates a safe distance for group members
        public GoogleStatus GoogleStatus { get; set; }//google status (walking,driving ,atc)
    }
}
