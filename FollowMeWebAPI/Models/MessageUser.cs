using QualiAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class MessageUser
    {
        public MessageGroup Message { get; set; }
        public Group Group { get; set; }
        public string UserName { get; set; }
    }
}
