using DAL;
using DAL.Models;
using MongoDB.Bson;
using QualiAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public static class User
    {

        public async static Task<UserProfile> LoginUser(string phone)
        {
            var response = await conectDB.getUser(phone);
            return response;
        }


        public async static Task<bool> RegisterUser(UserProfile user)
        {
            var insertUser = await conectDB.addNewUser(user);
            return insertUser;
        }

        public async static Task<bool> updateMarker(string phone, double lat, double lng)
        {
            return await conectDB.UpdateMarker(phone, lat, lng);
        }

        public async static Task<List<UserProfile>> getAllUsersNotInGroup(string pass)
        {
            var group = await conectDB.getUserOfGroup(pass);
            var all = await conectDB.getAllUsers();
            var managment = await conectDB.getManagmentOfGroup(pass);
            List<UserProfile> notInGroup = new List<UserProfile>();
            foreach (var item in all)
            {
                UserProfile user = await conectDB.getUser(item.Phone);
                if (group.Where(p => p.UserPhoneGroup == user.Phone).FirstOrDefault() == null && managment.Where(p => p.PhoneManagment == user.Phone).FirstOrDefault() == null)
                    notInGroup.Add(item);
            }
            return notInGroup;
        }

        public async static Task<List<Group>> checkOpenGroupAndConfirm(string phone)
        {
            var d = 0;
            List<Group> groups = await GroupS.getAllGroupUser(phone);//כל הקבוצות שהמשתמש רשום עליהם
            List<Group> send = new List<Group>();
            foreach (var item in groups)
            {
                if (item.Status == true && item.DateBeginTrip <= DateTime.Now && item.DateEndTrip >= DateTime.Now)
                {
                    foreach (var item1 in item.OkUsers)
                    {
                        UserProfile user = await conectDB.getUser(item1.UserPhoneGroup);
                        if (user.Phone == phone)
                        {
                            d = 1;
                        }
                    }

                    if (d == 0)
                        send.Add(item);

                }
            }
            return send;
        }

        //public async static Task<List<Group>> getAllUsersInGroup(string pass)
        //{
        //    var userInGroup = await conectDB.getUserOfGroup(pass);
        //    return ;
        //}

        public async static Task<bool> AgreeToAddGroup(string code, string phone)
        {

            try
            {
                UserProfile user = await conectDB.getUser(phone);
              List<Group> all = await conectDB.getAllGroup();
              Group gr= all.FirstOrDefault(p => p.Code == code);
                if (gr != null&&gr.ListManagment.Where(p=>p.PhoneManagment==phone).ToList().Count==0&&gr.Users.Where(p=>p.UserPhoneGroup==phone).ToList().Count==0)
                {
                    UserInGroup users = new UserInGroup();
                    users.UserPhoneGroup = user.Phone;
                    gr.Users.Add(users);
                    gr.OkUsers.Add(users);
                    await conectDB.UpdateGroup(gr);
                    foreach (var item in gr.ListManagment)
                    {
                        var manager = await conectDB.getUser(item.PhoneManagment);
                        manager.UserMessageNeedGet.Add(new MessageUser() {UserName=manager.FirstName+" "+manager.LastName, Group = gr, Message = gr.ErrorMessage.Find(p => p.CodeError == 9) });
                        await conectDB.UpdateUserMeesage(user);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async static Task<List<MessageUser>> getAllMessageUser(string phone)
        {
            try
            {
                UserProfile user = await conectDB.getUser(phone);
               var allMessage = new List<MessageUser>();
                foreach (var item in user.UserMessageNeedGet)
                {
                    allMessage.Add(item);
                    user.MessagesThatGet.Add(new Histories() { Group = item.Group, DateError = DateTime.Now, Message =  item.Message, User = user.FirstName + " " + user.LastName,UserMarker=new Marker() {Lat=user.Marker.Lat,Lng=user.Marker.Lng } });
                }
                await conectDB.UpdateUserMeesage(user);
                 bool b= await conectDB.RemoveMessage(user);
                if(b)
                return allMessage;
                return null;
            }
            catch (Exception ex)
            {

                throw new Exception("תקלה בקבלת האזהרות למטייל " + phone);
            }
        }


        public async static Task<bool> UpdateUserStatus(string phone)
        {
            var user = await conectDB.getUser(phone);
            user.Status = !user.Status;
            return await conectDB.UpdateUser(user);
        }

        public async static Task<bool> UpdateUser(UserProfile user)
        { 
            return await conectDB.UpdateUser(user);
        }

       async public static Task<List<Histories>> getAllHistories(string phone)
        {
            var user =await conectDB.getUser(phone);
            return await conectDB.getHistoriesUser(user);
        }

       async public static Task<int[]> getHistoriesByMonthUser(string phone)
        {
            int[] arr = new int[12];
            var user = await conectDB.getUser(phone);
            var histories =await conectDB.getHistoriesUser(user);
            var yearnow = histories.Where(p => p.DateError.Year == DateTime.Now.Year&&p.Group.Users.Find(pp=>pp.UserPhoneGroup==phone)!=null).ToList();
            var lastYear = histories.Where(p => p.DateError.Year == DateTime.Now.Year && p.Group.Users.Find(pp => pp.UserPhoneGroup == phone) != null).ToList();
            int i;
            for (i = 0; i <= DateTime.Now.Month; i++)
            {
              arr[i]=yearnow.Count(p => p.DateError.Month == i+1);
            }
            for (; i < 12; i++)
            {
                arr[i] = lastYear.Count(p => p.DateError.Month == i + 1);
            }
            return arr;
        }

        async public static Task<int[]> getHistoriesByMonthManager(string phone)
        {
            int[] arr = new int[12];
            var user = await conectDB.getUser(phone);
            var histories = await conectDB.getHistoriesUser(user);
            var yearnow = histories.Where(p => p.DateError.Year == DateTime.Now.Year && p.Group.ListManagment.Find(pp => pp.PhoneManagment == phone) != null).ToList();
            var lastYear = histories.Where(p => p.DateError.Year == DateTime.Now.Year && p.Group.ListManagment.Find(pp => pp.PhoneManagment == phone) != null).ToList();
            int i;
            for (i = 0; i <= DateTime.Now.Month; i++)
            {
                arr[i] = yearnow.Count(p => p.DateError.Month == i + 1);
            }
            for (; i < 12; i++)
            {
                arr[i] = lastYear.Count(p => p.DateError.Month == i + 1);
            }
            return arr;
        }

        public class barChartData
        {
            public int data { get; set; }
            public string label { get; set; }
        }


        async public static Task<List< barChartData>> getBarChar(string phone,int type)
        {
            List<barChartData> data = new List<barChartData>();
            var user = await conectDB.getUser(phone);
            var allgroup = await conectDB.getAllGroup();
            var groups = allgroup.Where(p => p.Users.Find(pp => pp.UserPhoneGroup == phone) != null || p.ListManagment.Find(pp => pp.PhoneManagment == phone) != null);
            foreach (var item in groups)
            {
                var histories = await getAllHistories(phone);
                barChartData b = new barChartData();
                if(type==1)
                b.data = histories.Count(p => p.Group.Password == item.Password&&p.DateError.Month==DateTime.Now.Month);
                else b.data = histories.Count(p => p.Group.Password == item.Password && p.DateError.Year == DateTime.Now.Year);
                b.label = item.Name;
                data.Add(b);
            }
            return data;

        }
    }
}
