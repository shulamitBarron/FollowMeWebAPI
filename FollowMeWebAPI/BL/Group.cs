using DAL;
using QualiAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using System.Net.Mail;
using System.Configuration;
using System.Net;
//using System.IO;
using System.Data;
using System.Xml;
using System.Device.Location;
using DAL.Models;


namespace BL
{
    public static class GroupS
    {
        /// <summary>
        /// פונקציה שמחזירה את כל הקבוצות שהמשתמש התרחק ממנהלי הקבוצה הנוכחים בטיול
        /// </summary>
        /// <param name="phone">מספר הפלאפון של המטייל</param>
        /// <returns>קבוצות שהתרחק ממנהלי הקבוצות</returns>
        public async static Task<List<Group>> getGroupDangerous(string phone)
        {
            //קבלת המטייל מדטה בייס לפי מספר הפלאפון
            var user = await conectDB.getUser(phone);
            //קבלת כל הקבוצות שהמשתמש רשום עליהם 
            List<Group> groupUser = await getAllGroupUser(phone);
            List<Group> farGroup = new List<Group>();
            foreach (var group in groupUser)
            {
                //בדיקה האם הקבוצה פעילה לפי סטטוס ולפי תאריך
                if (group.Status == true && group.DateBeginTrip <= DateTime.Now &&
                                                 group.DateEndTrip >= DateTime.Now)
                {
                    double latA = user.Marker.Lat;//נקודת רוחב של המטייל
                    double longA = user.Marker.Lng;//נקודת אורך של המטייל
                    int d = 0;
                    foreach (var managment in group.ListManagment)//מעבר על כל מנהלי הקבוצה
                    {

                        if (managment.ComeToTrip == true)//מנהל נוכח בטיול
                        {
                            //קבלה מדטה בייס את פרטי המנהל על ידי מספר הפלאפון שלו
                            var managmentMarker = await conectDB.getUser(managment.PhoneManagment);
                            double latB = managmentMarker.Marker.Lat;//נקודת רוחב של מיקום המנהל
                            double longB = managmentMarker.Marker.Lng;//נקודת אורך של מיקום המנהל

                            double distance = GetDistanceBetweenToPoint(latA, longA, latB, longB); ;
                            //כאשר המרחק גדול ממה שהוגדר בהגדרות קבוצה נדלק הדגל
                            if (distance > group.DefinitionGroup.Distance)
                                d++;
                        }

                    }
                    // בדיקה האם התרחק מכל מנהלי הקבוצה
                    if (d == group.ListManagment.Count(p => p.ComeToTrip == true))
                        farGroup.Add(group);//הוספה למערך של כל הקבוצות שהמטייל התרחק מהם
                }

                //הפעלת פונקציה של שליחת התראה על כל אחת ואחת מהקבוצות
                foreach (var item in farGroup)
                {
                    await sendMessageFarGroup(item, user, 5);
                }
            }
            return farGroup;
        }



        public async static Task<double> getLessDistance(string pass, double lat, double lng)
        {

            Group group = await getGroupByPass(pass);
            double lessFar = 100000000000;
            foreach (var managment in group.ListManagment)
            {

                double latA = lat;
                double longA = lng;
                int d = 0;
                var managmentMarker = await conectDB.getUser(managment.PhoneManagment);
                double latB = managmentMarker.Marker.Lat;//נקודת רוחב של מיקום המנהל
                double longB = managmentMarker.Marker.Lng;//נקודת אורך של מיקום המנהל

                double distance = GetDistanceBetweenToPoint(latA, longA, latB, longB); ;

                if (distance < lessFar)
                    lessFar = distance;

            }
            return lessFar;
        }

        async public static Task<List<Group>> groupOfUserStatusFalse(string phone)
        {
            var allGroup = await conectDB.getAllGroup();
            return allGroup.Where(p => (p.DateBeginTrip > DateTime.Now || p.Status == false) && p.Users.FirstOrDefault(p2 => p2.UserPhoneGroup == phone) != null).ToList();
        }

        public async static Task<List<Group>> getManagmentGroup(string phone)
        {
            var allGroup = await conectDB.getAllGroup();
            List<Group> groupManagment = new List<Group>();
            foreach (var item in allGroup)
            {
                if (item.ListManagment.FirstOrDefault(p => p.PhoneManagment.Equals(phone)) != null && item.DateBeginTrip <= DateTime.Now && item.DateEndTrip >= DateTime.Now && item.Status == true ||
                  item.ListManagment.FirstOrDefault(p => p.PhoneManagment.Equals(phone)) != null && item.DateBeginTrip > DateTime.Now && item.Status == true)
                    groupManagment.Add(item);
            }
            return groupManagment;
        }

        public async static Task<List<Group>> getManagmentGroupThatFalse(string phone)
        {
            var allGroup = await conectDB.getAllGroup();

            return allGroup.Where(p => p.ListManagment.FirstOrDefault(p2 => p2.PhoneManagment.Equals(phone)) != null && p.DateBeginTrip > DateTime.Now || p.ListManagment.FirstOrDefault(p2 => p2.PhoneManagment.Equals(phone)) != null && p.DateBeginTrip <= DateTime.Now && p.DateEndTrip >= DateTime.Now && p.Status == false).ToList();

        }

        public async static Task<Group> checkCodeGroup(string Code)
        {

            List<Group> all = await conectDB.getAllGroup();
            foreach (var item in all)
            {
                if (item.Code == Code && item.DateEndTrip >= DateTime.Now)
                    return item;
            }
            return new Group();
        }

        public async static Task<Group> AddToGroup(string Code, string phone)
        {
            //הוספת משתמש לקבוצה כאשר הוא עצמו מצרף את עצמו
            List<Group> all = await conectDB.getAllGroup();
            foreach (var item in all)
            {
                if (item.Code == Code)
                {
                    item.Users.Add(new UserInGroup() { UserPhoneGroup = phone, Definition = new DefinitionUser() { IsSeeMeAll = true } });
                    item.OkUsers.Add(new UserInGroup() { UserPhoneGroup = phone, Definition = new DefinitionUser() { IsSeeMeAll = true } });
                    await conectDB.UpdateUsersGroup(item);
                    await conectDB.UpdateOkUser(item.Password, phone);
                    return item;
                }
            }
            return new Group();
        }

        public async static Task<List<Group>> getAllGroups()
        {
            var all = await conectDB.getAllGroup();
            List<Group> groupsOk = new List<Group>();
            foreach (var item in all)
            {
                if (item.DateBeginTrip <= DateTime.Now && item.DateEndTrip > DateTime.Now)
                    groupsOk.Add(item);
            }
            return groupsOk;

        }
        public async static Task<List<Group>> getAllGroupsForTets()
        {
            var all = await conectDB.getAllGroup();
            return all;

        }

        public async static Task<List<Group>> getAllGroupsDisable()
        {
            var all = await conectDB.getAllGroup();
            List<Group> groupsOk = new List<Group>();
            foreach (var item in all)
            {
                if (item.DateBeginTrip > DateTime.Now && item.DateEndTrip > DateTime.Now || item.Status == false)
                    groupsOk.Add(item);
            }
            return groupsOk;

        }





        public async static Task<List<Group>> getAllGroupUser(string phone)
        {
            var groups = await conectDB.getAllGroup();
            List<Group> g = new List<Group>();
            foreach (var item in groups)
            {
                if (item.DateBeginTrip <= DateTime.Now && item.DateEndTrip > DateTime.Now || item.DateBeginTrip >= DateTime.Now)
                {
                    var d = item.Users;
                    if (item.Users != null)
                        foreach (var item1 in d)
                        {
                            if (item1.UserPhoneGroup != null)
                            {
                                var user = await conectDB.getUser(item1.UserPhoneGroup);
                                if (user != null)
                                {
                                    if (user.Phone == phone)
                                        g.Add(item);
                                }
                            }
                        }
                }
            }
            return g;
        }

        public async static Task<List<Group>> getGroupOpenToUser(string phone)
        {
            var groups = await conectDB.getAllGroup();
            List<Group> g = new List<Group>();
            foreach (var item in groups)
            {
                if (item.DateBeginTrip <= DateTime.Now && item.DateEndTrip > DateTime.Now && item.Status == true)
                {
                    var d = item.Users;
                    if (item.Users != null)
                        foreach (var item1 in d)
                        {
                            if (item1.UserPhoneGroup != null)
                            {
                                var user = await conectDB.getUser(item1.UserPhoneGroup);
                                if (user != null)
                                {
                                    if (user.Phone == phone)
                                        g.Add(item);
                                }
                            }
                        }
                }
            }
            return g;
        }

        public async static Task<Group> getGroupByPass(string pass)
        {
            return await conectDB.getGroup(pass);
        }

        public static Random rand;
        public async static Task<Group> addGroup(Group g)
        {
            //TODO:לבדוק מדוע התאריכים לא עוברים בצורה תקינה
            rand = new Random();
            var d = true;
            var num = 0;
            while (d)
            {
                d = false;
                num = rand.Next(1000, 10000);
                var allGroup = await conectDB.getAllGroup();
                foreach (var item in allGroup)
                {
                    if (num.ToString().Equals(item.Code))
                        d = true;
                    break;
                }
                if (d == false)
                {
                    g.Code = num.ToString();
                }
            }
            if (g.DateBeginTrip < DateTime.Now)
                g.DateBeginTrip = DateTime.Now;
            if (g.DateEndTrip < DateTime.Now)
                g.DateEndTrip = DateTime.Now.AddDays(1);
            Group newGroup = await conectDB.addNewGroup(g);
            if (newGroup != null)
                return newGroup;
            return null;
        }

        public async static Task<Group> checkGroup(string pass, string name)
        {
            //לא גמור לבדוק זאת שוב
            var g = await conectDB.getAllGroup();//.Where(p => p.password == pass && p.name == name).FirstOrDefault();
            return g.Where(p => p.Password == pass && p.Name == name).FirstOrDefault();
        }


        public async static Task<bool> deleteGroup1(string pass)
        {
            bool b = await conectDB.deleteGroup(pass);
            return b;
        }

        public async static Task<Group> UpdateGroup1(Group gr)
        {
            var thisGroup = await conectDB.getGroup(gr.Password);
            var thisStatus = thisGroup.Password;
            var group = await conectDB.UpdateGroup(gr);
            if (group != null)
            {
                return gr;
            }
            return null;
        }


        public async static Task<bool> sendEmail(UserProfile toSend,UserProfile fromSend, Group gr, string message)
        {
            SmtpClient client = new SmtpClient();
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("c0556777462@gmail.com", "207322868");
            client.UseDefaultCredentials = false;
            client.Credentials = credentials;
            //can be obtained from your model
            MailMessage msg = new MailMessage();
       
            msg.From = new MailAddress(fromSend.Email);
            
            msg.To.Add(new MailAddress(toSend.Email.ToString()));
            msg.Subject = " " + gr.Name;
            msg.IsBodyHtml = true;
            msg.Body = string.Format("<html><head>הודעה שנשלחה מהאפלקציה  followme</head><body><p>" + message + "</br>- " + fromSend.FirstName + " " + fromSend.LastName + " " + fromSend.Phone + "</p></body>");
            try
            {
                client.Send(msg);
                return true;
            }
            catch (Exception ex)
            {
                // throw new Exception("נכשלה שליחת המייל ל" + user.firstName + " " + user.lastName, ex);
                return false;

            }
        }

        public async static Task<bool> updateUsersGroup(Group group)
        {
            try
            {
              Group gr=  await conectDB.getGroup(group.Password);
                foreach (var item in group.Users)
                {
                   if(gr.Users.FirstOrDefault(p=>p.UserPhoneGroup==item.UserPhoneGroup)==null)
                    {
                      UserProfile user=  await conectDB.getUser(item.UserPhoneGroup);
                        user.UserMessageNeedGet.Add(new MessageUser() { Group = gr, Message = gr.ErrorMessage.Find(p => p.CodeError == 4), UserName = user.FirstName + " " + user.LastName });
                       await conectDB.UpdateUserMeesage(user);
                    }
                }
               
                await conectDB.UpdateUsersGroup(group);
                return true;

            }
            catch (Exception ex)
            {

                throw new Exception("נכשלה הוספת משתמשים לקבוצה" + ex);
            }

        }

        //public async static Task<bool> SendWhatsapp(UserProfile user, Group group,string messsage)
        //{
        //    bool b = true;
        //    //TODO:צריך לשנות שיעבוד על באמת

        //    WhatsApp wa = new WhatsApp("0556777462", group.password, group.name, true);
        //    wa.OnConnectSuccess += () =>
        //    {

        //        wa.OnLoginSuccess += (phonenumber, data) =>
        //        {
        //            wa.SendMessage(user.phone, messsage);

        //        };
        //        wa.OnLoginFailed += (data) =>
        //        {

        //        };
        //        try
        //        {
        //            wa.Login();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    };


        //    wa.OnConnectFailed += (ex) =>
        //    {

        //    };
        //    wa.Connect();
        //    return true;
        //}



        public async static Task<bool> SendSMS(UserProfile user, Group gr, string message)
        {
            //SmtpClient smpt = new SmtpClient();
            //MailMessage massage = new MailMessage();
            //smpt.Credentials = new NetworkCredential("chaya", "207322868");
            //smpt.Host = "ipipi.com";
            //massage.From = new MailAddress("followme@ipipi.com");
            //var userMail = await conectDB.getUser(user.phone);
            //massage.To.Add(new MailAddress(userMail.email.ToString()));
            //massage.Subject = " הודעה מקבוצת"+gr.name;
            //massage.Body = message+" " + gr.name + " " + gr.description;
            //smpt.Send(massage);
            return true;
        }

        public async static Task<bool> sendMessageOpenGroup(Group gr)
        {
           
            var group = await conectDB.getGroup(gr.Password);
            foreach (var item in group.Users)
            {
                var user = await conectDB.getUser(item.UserPhoneGroup);
                user.UserMessageNeedGet.Add(new MessageUser() {UserName=user.FirstName+" "+user.LastName, Group = gr, Message = gr.ErrorMessage.Find(p => p.CodeError == 4) });
                await conectDB.UpdateUserMeesage(user);
            }

            foreach (var item in group.OkUsers)
            {
                var user = await conectDB.getUser(item.UserPhoneGroup);
                if(group.OkUsers.Find(p=>p.UserPhoneGroup==item.UserPhoneGroup)==null)
                { 
                user.UserMessageNeedGet.Add(new MessageUser() {UserName=user.FirstName+" "+user.LastName, Group = gr, Message = gr.ErrorMessage.Find(p => p.CodeError == 3) });
                await conectDB.UpdateUserMeesage(user);
                }
            }
            return true;
        }

        /// <summary>
        /// //שליחת התראה לכל מנהלי הקבוצה ולמטייל
        /// </summary>
        /// <param name="gr">קבוצה</param>
        /// <param name="user">משתמש שהתרחק</param>
        /// <param name="CodeMess">קוד התראה</param>
        /// <returns></returns>
        public async static Task<bool> sendMessageFarGroup(Group gr, UserProfile user, int CodeMess)
        {
            foreach (var item in user.MessagesThatGet)
            {
                if (item.DateError.AddMinutes(3) >= DateTime.Now && item.UserMarker.Lat == user.Marker.Lat && item.UserMarker.Lng == user.Marker.Lng &&
                    item.Group.Password == gr.Password && item.Message.CodeError == CodeMess)
                    return false;
            }
            //בדיקה האם לשלח הודעה בדחיפות כל כך גבוהה
            if (user.MessagesThatGet.FirstOrDefault(mes => mes.Group.Password == gr.Password && mes.Message.CodeError == CodeMess && mes.DateError.AddMinutes(3) >= DateTime.Now && mes.UserMarker.Lat == user.Marker.Lat && mes.UserMarker.Lng == user.Marker.Lng) != null)
                return false;
            //הוספת התראה חדשה למטייל
            user.UserMessageNeedGet.Add(new MessageUser()
            {
                Group = gr,
                UserName = user.FirstName + " " + user.LastName,
                Message = gr.ErrorMessage.Where(p => p.CodeError == CodeMess).First()
            });


            //user.MessagesThatGet.Add(new Histories() { Group = gr, DateError = DateTime.Now, Message = gr.ErrorMessage.FirstOrDefault(p => p.CodeError == CodeMess), User = user.LastName + " " + user.FirstName });
            //עדכון בדטה בייס
            await conectDB.UpdateUserMeesage(user);
            //מעבר על כל מנהלי הקבוצה
            foreach (var item in gr.ListManagment)//מעבר על כל מנהלי הקבוצה
            {
                //קבלת פרטי המנהל מהדטה בייס
                UserProfile userManagment = await conectDB.getUser(item.PhoneManagment);
                //הוספת התראה חדשה למנהל קבוצה
                userManagment.UserMessageNeedGet.Add(new MessageUser()
                {
                    Group = gr,
                    UserName = user.FirstName + " " + user.LastName,
                    Message = gr.ErrorMessage.Where(p => p.CodeError == CodeMess - 4).First()
                });
                //עדכון בדטה בייס
                await conectDB.UpdateUserMeesage(userManagment);
            }
            return true;
        }


        public async static Task<List<UserProfile>> GetUserOfGroup1(string pass)
        {
            var allUsers = await conectDB.getUserOfGroup(pass);
            List<UserProfile> users = new List<UserProfile>();
            foreach (var item in allUsers)
            {
                var user = await conectDB.getUser(item.UserPhoneGroup);
                users.Add(user);
            }
            return users;
        }

        public static async Task<bool> DeleteGroupsOver()
        {
            foreach (var item in await conectDB.getAllGroup())
            {
                if (item.DateBeginTrip < DateTime.Now)
                {
                    await conectDB.deleteGroup(item.Password);
                }
            }
            return true;
        }

        public static double GetDistanceBetweenToPoint(double lat1, double long1, double lat2, double long2)
        {
            var locA = new GeoCoordinate(lat1, long1);
            var locB = new GeoCoordinate(lat2, long2);
            //פונקציה שמחזירה את המרחק מ2 הנקודות במטרים
            double distance = locA.GetDistanceTo(locB);
            return distance;
        }

    }
}
