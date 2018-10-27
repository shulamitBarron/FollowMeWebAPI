using DAL;
using DAL.Models;
using QualiAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
  static public class  Managment
  {
    public async static Task<bool> AddManagment1(string group,string user )
    {
      return await conectDB.AddManagment(group, user);
    }

        public async static Task<bool> sendMessageComplex(MessageUser messageUser )
        {
          var allUsers=  await conectDB.getAllUsers();
          var correctUser = allUsers.FirstOrDefault(p => p.Marker.NameAndPhone.Equals(messageUser.UserName));
          return await conectDB.setNewErrorToUser(correctUser, messageUser.Message, messageUser.Group);
        }


        public async static Task<bool> sendHelpMessage(string phone)
        {
            var allGroup = await conectDB.getAllGroup();
            var fromUser =await conectDB.getUser(phone);
            var group = allGroup.Where(p => p.Users.Find(pp => pp.UserPhoneGroup == phone) != null).ToList();
            foreach (var item in group)
            {
                foreach (var item1 in item.ListManagment)
                {
                    var user = await conectDB.getUser(item1.PhoneManagment);
                    await conectDB.setNewErrorToUser(user,item.ErrorMessage.Find(ppp=>ppp.CodeError==8),item);
                    await GroupS.sendEmail(fromUser, user, item, item.ErrorMessage.Find(ppp => ppp.CodeError == 8).MessageError);
                }
            }
            return true;
        }
    }
}
