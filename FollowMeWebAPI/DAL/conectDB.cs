using DAL.Models;
using QualiAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;
using System.Timers;

namespace DAL
{

    public class conectDB
    {

        static Timer TimerToExcuteFunctionEveryDay = new Timer(24 * 60 * 60 * 1000);//every date 
        static Timer TimerToExcuteFunctionEveryMinute = new Timer(60000);

        public static List<MessageGroup> messagesToGroup = new List<MessageGroup>() { new MessageGroup() { MessageError="זהירות מטייל התרחק מקבוצתך",CodeError=1},
        new MessageGroup() { MessageError="זהירות מטייל לקראת התרחקות מקבוצתך",CodeError=2},
        new MessageGroup() { MessageError="נפתחה הקבוצה",CodeError=3},
        new MessageGroup() { MessageError="אישור הצטרפות לטיול",CodeError=4},
        new MessageGroup() { MessageError="סכנה התרחקת מקבוצתך",CodeError=5},
        new MessageGroup() { MessageError="זהירות אתה מתקרב לקראת התרחקות מהקבוצה",CodeError=6},
        new MessageGroup() { MessageError="נפתחה קבוצתך שאליה אתה רשום",CodeError=7},
        new MessageGroup() { MessageError="מטייל מבקש עזרה",CodeError=8},
        new MessageGroup() { MessageError="מטייל הצטרף לקבוצת",CodeError=9},
        new MessageGroup() { MessageError="הודעה מותאמת אישית למטייל",CodeError=10}};

        /// <summary>
        /// function do- every date delete group that finish
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async static void CheckForTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            var groups = await getAllGroup();
            groups.ForEach(async group =>
            {
                if (group.DateEndTrip < DateTime.Now)
                    try
                    {
                        await deleteGroup(group.Password);
                    }
                    catch (Exception)
                    {

                    }
            });
        }

        async static void CheckForTimeEveryMinute_Elapsed(object sender, ElapsedEventArgs e)
        {
            var groups = await getAllGroup();
            groups.ForEach(async group =>
            {
                if (group.DateBeginTrip <= DateTime.Now && group.DefinitionGroup.eWhenStatusOpen == WhenStatusOpen.ONDATEBEGIN)
                    try
                    {
                        group.Status = true;
                        await UpdateGroup(group);
                        //TODO:שליחת הודעה לכל חברי הקבוצה
                        foreach (var item in group.OkUsers)
                        {
                            // await conectDB.UpdateGroup(group);
                            var user = await conectDB.getUser(item.UserPhoneGroup);
                            user.UserMessageNeedGet.Add(new MessageUser() { Group = group, UserName = user.FirstName + " " + user.LastName, Message = group.ErrorMessage.Find(err => err.CodeError == 3) });
                            await conectDB.UpdateUserMeesage(user);
                        }
                        foreach (var item in group.Users)
                        {
                            // await conectDB.UpdateGroup(group);
                            var user = await conectDB.getUser(item.UserPhoneGroup);
                            if(group.OkUsers.Find(p=>p.UserPhoneGroup==user.Phone)==null)
                            { 
                            user.UserMessageNeedGet.Add(new MessageUser() { Group = group, UserName = user.FirstName + " " + user.LastName, Message = group.ErrorMessage.Find(err => err.CodeError == 4) });
                            await conectDB.UpdateUserMeesage(user);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
            });
        }

        static conectDB()
        {
            TimerToExcuteFunctionEveryDay.Elapsed += new ElapsedEventHandler(CheckForTime_Elapsed);
            TimerToExcuteFunctionEveryDay.Enabled = true;
            TimerToExcuteFunctionEveryMinute.Elapsed += new ElapsedEventHandler(CheckForTimeEveryMinute_Elapsed);
            TimerToExcuteFunctionEveryMinute.Enabled = true;
        }

        /// <summary>
        /// קבלת כל המשתמשים
        /// </summary>
        /// <returns></returns>
        async static public Task<List<UserProfile>> getAllUsers()
        {
            var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
            var database = client.GetDatabase("followMe");
            var userCollection = database.GetCollection<UserProfile>("users");
            var users = await userCollection.Find(a => true).ToListAsync();
            return users;
        }


        /// <summary>
        /// קבלת משתמש
        /// </summary>
        /// <param name="phone">פלאפוlloן</param>
        /// <returns></returns>
        public async static Task<UserProfile> getUser(string phone)
        {
            var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");

            var database = client.GetDatabase("followMe");
            var userCollection = database.GetCollection<UserProfile>("users");
            var u = await userCollection.Find(a => a.Phone == phone).ToListAsync();
            if (u.Count == 0)
                return null;
            else return u.First();
        }

       async public static Task<List<Histories>> getHistoriesUser(UserProfile user)
        {
            try
            {
                var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
                var database = client.GetDatabase("followMe");
                var historiesCollection = database.GetCollection<Histories>("histories");
                var allHistories = await historiesCollection.Find(a => a.User.Equals(user.FirstName+" "+user.LastName)).ToListAsync();
                return allHistories;
            }
            catch (Exception ex)
            {

                throw new Exception("לא היה אפשרות לשליפת הקבוצות" + ex.Message);
            }
        }

        /// <summary>
        /// קבלת כל הקבוצות 
        /// </summary>
        ///
        /// <returns></returns>
        public async static Task<List<Group>> getAllGroup()
        {
            try
            {
                var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
                var database = client.GetDatabase("followMe");
                var groupCollection = database.GetCollection<Group>("groups");
                var allGroups = await groupCollection.Find(a => true).ToListAsync();
                return allGroups;
            }
            catch (Exception ex)
            {

                throw new Exception("לא היה אפשרות לשליפת הקבוצות" + ex.Message);
            }
        }
        /// <summary>
        /// החזרת קבוצה
        /// </summary>
        /// <param name="password">סיסמת קבוצה</param>
        /// <returns></returns>
        public async static Task<Group> getGroup(string password)
        {
            try
            {
                var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
                var database = client.GetDatabase("followMe");
                var groupCollection = database.GetCollection<Group>("groups");
                var rightGroup = await groupCollection.Find(a => a.Password == password).ToListAsync();
                if (rightGroup.Count > 0)
                {
                    return rightGroup.First();
                }
                return null;
            }
            catch (Exception ex)
            {

                throw new Exception("לא היה אפשרות לשליפת הקבוצה" + ex.Message);
            }
        }
        /// <summary>
        /// מחקת הודעה שנשלחה למטייל
        /// </summary>
        /// <param name="user">מטייל</param>
        /// <returns></returns>
        async public static Task<bool> RemoveMessage(UserProfile user)
        {
            var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
            var database = client.GetDatabase("followMe");
            var allUsers = database.GetCollection<UserProfile>("users");
            var historyCollection = database.GetCollection<Histories>("histories");
            try
            {
                foreach (var item in user.UserMessageNeedGet)
                {
                    var history = new Histories() { DateError = DateTime.Now, Group = item.Group, Message = item.Message, User = item.UserName,UserMarker=new Marker() {Lat= user.Marker.Lat,Lng= user.Marker.Lng } };
                    await historyCollection.InsertOneAsync(history);
                }
                user.UserMessageNeedGet.RemoveAll(p=>true);
                var filter = Builders<UserProfile>.Filter.Eq("Phone", user.Phone);
                //TODO:לשנות לא מוחק את ההתראות
                var update = Builders<UserProfile>.Update.Set(userMessage=> userMessage.UserMessageNeedGet, new List<MessageUser>());
                var result = await allUsers.UpdateOneAsync(filter, update);

           
                return true;
            }
            catch (Exception ex)
            {
                return false;

            }


        }


        /// <summary>
        /// עדכון קבוצה
        /// </summary>
        /// <param name="group">קבוצה</param>
        /// <returns></returns>
        public async static Task<Group> UpdateGroup(Group group)
        {
            try
            {
                var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
                var database = client.GetDatabase("followMe");
                var groupCollection = database.GetCollection<Group>("groups");
                List<Group> rightGroup = await groupCollection.Find(a => a.Password.Equals(group.Password)).ToListAsync();
                if (rightGroup.Count == 0)
                    return null;
                if (rightGroup[0].Status != group.Status && group.Status == false)
                {
                  bool b= await sendMessageAllUserInGroup(group, rightGroup[0].ErrorMessage.First(err => err.CodeError == 3));
                }
                var filter = Builders<Group>.Filter.Eq("Password", group.Password);
                var update = Builders<Group>.Update.Set("Status", group.Status).Set("DateBeginTrip", group.DateBeginTrip).Set("DateEndTrip", group.DateEndTrip).Set("Users", group.Users).Set("OkUsers", group.OkUsers)
                            .Set("DefinitionGroup.Distance", group.DefinitionGroup.Distance).Set("DefinitionGroup.eWhenStatusOpen", group.DefinitionGroup.eWhenStatusOpen).Set("DefinitionGroup.GoogleStatus", group.DefinitionGroup.GoogleStatus);

                await groupCollection.UpdateOneAsync(filter, update);
                return group;
            }
            catch (Exception ex)
            {

                throw new Exception("שגיאה בעדכון קבוצה " + ex.Message);
            }
        }

        async private static Task<bool> sendMessageAllUserInGroup(Group group, MessageGroup messageGroup)
        {
            try
            {
                foreach (var item in group.Users)
                {
                    var user = await conectDB.getUser(item.UserPhoneGroup);
                    user.UserMessageNeedGet.Add(new MessageUser() { Group = group, Message = messageGroup, UserName = user.FirstName + " " + user.LastName });
                    await conectDB.UpdateUser(user);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }


        }



        /// <summary>
        /// עדכון משתמש האם פעיל או לא
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async static Task<bool> UpdateUser(UserProfile user)
        {
            var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
            var database = client.GetDatabase("followMe");
            var allUsers = database.GetCollection<UserProfile>("users");
            if (allUsers.Find(p => p.Phone == user.Phone) == null)
                return false;
            var filter = Builders<UserProfile>.Filter.Eq("Phone", user.Phone);
           
            var update = Builders<UserProfile>.Update.Set("Status", user.Status).Set("Email",user.Email).Set("FirstName", user.FirstName).
                Set("Image", user.Image).Set("LastName", user.LastName);
            await allUsers.UpdateOneAsync(filter, update);
            return true;
        }

        /// <summary>
        /// עדכון הודעות שהמשתמש צריך לקבל
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async static Task<bool> UpdateUserMeesage(UserProfile user)
        {
            var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
            var database = client.GetDatabase("followMe");
            var allUsers = database.GetCollection<UserProfile>("users");
            if (allUsers.Find(p => p.Phone == user.Phone) == null)
                return false;
            var filter = Builders<UserProfile>.Filter.Eq("Phone", user.Phone);
            var update = Builders<UserProfile>.Update.Set("UserMessageNeedGet", user.UserMessageNeedGet).Set("MessagesThatGet", user.MessagesThatGet);
            await allUsers.UpdateOneAsync(filter, update);
            return true;
        }
        /// <summary>
        /// כניסה ראשונית למערכת
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async static Task<bool> addNewUser(UserProfile user)
        {
            try
            {
                var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");

                var database = client.GetDatabase("followMe");
                var userCollection = database.GetCollection<UserProfile>("users");
                //await userCollection.InsertOneAsync(new UserProfile() { email = "ggggggg", firstName = "bh fcg", id = new ObjectId() });
                var u = await userCollection.Find(sb => sb.Phone.Equals(user.Phone)).ToListAsync();
                if (u.Count == 0)
                {
                    user.Status = true;
                    user.Marker = new Marker();
                    user.Marker.NameAndPhone = user.FirstName + " " + user.LastName;
                    user.Id = ObjectId.GenerateNewId();
                    user.UserMessageNeedGet = new List<MessageUser>();
                    user.MessagesThatGet = new List<Histories>();
                    await userCollection.InsertOneAsync(user);
                    return true;
                }
                else return false;

            }
            catch (Exception e)
            {

                throw new Exception("קרתה שגיאה במהלך הוספת המשתמש");
            }


        }
        /// <summary>
        /// הוספת משתמש לקבוצה
        /// </summary>
        /// <param name="group">קבוצה</param>
        /// <returns></returns>
        public async static Task<bool> UpdateUsersGroup(Group group)
        {
            try
            {//לבדוק האם הפןנקציה הגיונית בכלל
                var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
                var database = client.GetDatabase("followMe");
                var allGroups = database.GetCollection<Group>("groups");
                var gr = await getGroup(group.Password);
                if (gr == null)
                    return false;
                // gr.users = new List<UserInGroup>();
                // gr.users = group.users;
                var filter = Builders<Group>.Filter.Eq("Password", group.Password);
                if (gr.Users == null)
                {
                    gr.Users = new List<UserInGroup>();
                }
                // gr.users.Add(new UserInGroup() { UserPhoneGroup = phone, Definition = new DefinitionUser() { SeeMeAll = true } });
                var update = Builders<Group>.Update.Set("Users", group.Users)
               .Set("OkUsers", group.OkUsers);
                await allGroups.UpdateOneAsync(filter, update);

                foreach (var item in group.Users)
                {
                    if(group.OkUsers.Find(p=>p.UserPhoneGroup==item.UserPhoneGroup)==null)
                    {
                        var user = await getUser(item.UserPhoneGroup);
                        user.UserMessageNeedGet.Add(new MessageUser() {UserName=user.FirstName+" "+user.LastName, Group = gr, Message = gr.ErrorMessage.Find(p => p.CodeError == 4) });
                        await UpdateUserMeesage(user);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {

                throw new Exception(" לא היתה אפשרות לשמירת השנוים " + ex.Message);
            }
        }

        public async static Task<bool> UpdateManagersGroup(Group group)
        {
            try
            {//לבדוק האם הפןנקציה הגיונית בכלל
                var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
                var database = client.GetDatabase("followMe");
                var allGroups = database.GetCollection<Group>("groups");
                var gr = await getGroup(group.Password);
                if (gr == null)
                    return false;
             
                var filter = Builders<Group>.Filter.Eq("Password", group.Password);
                var update = Builders<Group>.Update.Set("ListManagment", group.ListManagment)
               .Set("OkUsers", group.OkUsers);
                await allGroups.UpdateOneAsync(filter, update);
                return true;
            }
            catch (Exception ex)
            {

                throw new Exception(" לא היתה אפשרות לשמירת השנוים " + ex.Message);
            }
        }


        /// <summary>
        /// הוספת משתמש למשתמשים המאשרים את הצטרפותם לקבוצה
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="phone"></param>
        /// <returns></returns>
        public async static Task<bool> UpdateOkUser(string pass, string phone)
        {
            try
            {//לבדוק האם הפןנקציה הגיונית בכלל
                var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
                var database = client.GetDatabase("followMe");
                var allGroups = database.GetCollection<Group>("groups");
                var gr = await getGroup(pass);
                var user = gr.Users.Where(p => p.UserPhoneGroup == phone).FirstOrDefault();

                if (gr == null || user == null)
                    return false;
                var filter = Builders<Group>.Filter.Eq("Password", pass);
                if (gr.OkUsers == null)
                    gr.OkUsers = new List<UserInGroup>();
                gr.OkUsers.Add(user);
                var update = Builders<Group>.Update.Set("OkUsers", gr.OkUsers);
                await allGroups.UpdateOneAsync(filter, update);
                return true;
            }
            catch (Exception ex)
            {

                throw new Exception(" לא היתה אפשרות לשמירת השנוים " + ex.Message);
            }
        }


        /// <summary>
        /// הוספת קבוצה חדשה
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public async static Task<Group> addNewGroup(Group group)
        {
            try
            {
                var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
                var database = client.GetDatabase("followMe");
                var allGroups = database.GetCollection<Group>("groups");
                var findGroup = await getGroup(group.Password);
                if (findGroup == null)
                {

                    group.Users = new List<UserInGroup>();
                    group.OkUsers = new List<UserInGroup>();
                    group.Status = false;
                    group.DefinitionGroup = new DefinitionGroup();
                    group.DefinitionGroup.GoogleStatus = GoogleStatus.WALKING;
                    group.DefinitionGroup.Distance = 500;
                    group.DefinitionGroup.eWhenStatusOpen = WhenStatusOpen.ONOPEN;
                    group.ErrorMessage = messagesToGroup;
                    await allGroups.InsertOneAsync(group);
                    return group;
                }
                return null;
            }
            catch (Exception)
            {

                throw new Exception("לא היה ניתן להוסיף קבוצה");
            }
        }
        /// <summary>
        /// קבלת משתמשים הרשומים לקבוצה מסוימת
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        public async static Task<List<UserInGroup>> getUserOfGroup(string pass)
        {
            var gg = await getGroup(pass);
            if (gg == null)
                throw new Exception("שגיאה בפרטי הקבוצה ");
            return gg.Users.ToList();
        }
        /// <summary>
        /// מנהלי קבוצה
        /// </summary>
        /// <param name="pass">סיסמת קבוצה</param>
        /// <returns></returns>
        public async static Task<List<ManagmentInGroup>> getManagmentOfGroup(string pass)
        {
            var gg = await getGroup(pass);
            if (gg == null)
                throw new Exception("שגיאה בפרטי הקבוצה ");
            return gg.ListManagment.ToList();
        }
        /// <summary>
        /// מחיקת קבוצה
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        public async static Task<bool> deleteGroup(string pass)
        {
            try
            {

                var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
                var database = client.GetDatabase("followMe");
                var allGroup = database.GetCollection<Group>("groups");

                var Deleteone = await allGroup.DeleteOneAsync(
                                    Builders<Group>.Filter.Eq("Password", pass));
                return true;
            }
            catch (Exception ex)
            {

                throw new Exception("קרתה טעות במחיקת הקבוצה", ex);
            }

        }

        /// <summary>
        /// עדכון נקודות מיקום במשתמש
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        public async static Task<bool> UpdateMarker(string phone, double lat, double lng)
        {
            try
            {
                var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
                var database = client.GetDatabase("followMe");
                var allUsers = database.GetCollection<UserProfile>("users");
                var uu = await getUser(phone);
                if (uu != null)
                {
                    Marker m = new Marker() { Lat = lat, Lng = lng, NameAndPhone = uu.LastName + " " + uu.FirstName + " " + uu.Phone };
                    var filter = Builders<UserProfile>.Filter.Eq("Phone", phone);
                    var update = Builders<UserProfile>.Update.Set("Marker", m);
                    var result = await allUsers.UpdateOneAsync(filter, update);
                    return true;

                }
                return false;

            }
            catch (Exception ex)
            {

                throw new Exception("קרתה טעות בעדכון המיקום", ex);
            }

        }
        /// <summary>
        /// הסרת מנהל מקבוצה
        /// </summary>
        /// <param name="group"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async static Task<bool> RemoveMangment(Group group, UserProfile user)
        {
            try
            {
                var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
                var database = client.GetDatabase("followMe");
                var allGroups = database.GetCollection<Group>("groups");
                var groupCheck = await getGroup(group.Password);
                if (groupCheck != null)
                {

                    if (groupCheck.ListManagment.FirstOrDefault(p => p.PhoneManagment == user.Phone) != null)
                    {
                        groupCheck.ListManagment.Remove(groupCheck.ListManagment.First(p => p.PhoneManagment == user.Phone));
                        var filter = Builders<Group>.Filter.Eq(s => s.Password, group.Password);
                        var update = Builders<Group>.Update.Set("ListManagment", groupCheck.ListManagment);
                        var result = await allGroups.UpdateOneAsync(filter, update);
                        return true;
                    }
                    else return false;
                }
                return false;
            }
            catch (Exception)
            {

                throw new Exception("לא היה ניתן למחוק את המנהל ");
            }
        }
        /// <summary>
        /// הוספת הזהרה להסטוריה
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <param name="group"></param>
        async public static void setErrorInHistory(UserProfile user, string message, Group group)
        {
            //TODO:לגמור את הפנקציה כי שניתי את 
            var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
            var database = client.GetDatabase("followMe");
            var allHistoriesUser = database.GetCollection<Histories>("historyUser");
            //   var userHistory = await allHistoriesUser.Find(a => a.PhoneUser == user.phone).FirstAsync();
            //  MessageUser messageHistory = new MessageUser() { Group = group, Message = message };
            // if(userHistory!=null)
            // {
            //userHistory.UserMessage.Add(group.password, messageHistory);
            // }
            // var allHistoriesGroups = database.GetCollection<Histories>("histories");
            // var messageHistoryGroup = await allHistoriesGroups.Find(a => a.Group == group).FirstAsync();
            // MessageGroup messageGroupHistory = new MessageGroup() { MessageError = message, ManagmentsNeedGet = group.listManagment };
            //if (messageHistoryGroup != null)
            //{
            //    messageHistoryGroup.MessagesGroup.Add(user.phone, messageGroupHistory);
            //}
            //var filter = Builders<Histories>.Filter.Eq("Group", group.password);
            //var update = Builders<Histories>.Update.Set("MessagesGroup", messageHistoryGroup.MessagesGroup);
            //var result = await allHistoriesGroups.UpdateOneAsync(filter, update);

            //var filter1 = Builders<Histories>.Filter.Eq("PhoneUser", user.phone);
            //var update1 = Builders<Histories>.Update.Set("UserMessage", userHistory.UserMessage);
            //var result1 = await allHistoriesUser.UpdateOneAsync(filter1, update1);

        }
        /// <summary>
        /// הוספת התראה למטייל
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <param name="group"></param>
        public async static Task<bool> setNewErrorToUser(UserProfile user, MessageGroup message, Group group)
        {
            try
            {
                var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
                var database = client.GetDatabase("followMe");
                var allUsers = database.GetCollection<UserProfile>("users");
                user.UserMessageNeedGet.Add(new MessageUser() { Message = message, Group = group,UserName=user.FirstName+" "+user.LastName });
                var filter = Builders<UserProfile>.Filter.Eq("Phone", user.Phone);
                var update = Builders<UserProfile>.Update.Set("UserMessageNeedGet", user.UserMessageNeedGet);
                var result = await allUsers.UpdateOneAsync(filter, update);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// מנהל חדש לקבוצה
        /// </summary>
        /// <param name="group"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async static Task<bool> AddManagment(string group, string user)
        {//לבדוק האם בא לטיול או לא ולפי זה לעדכן
            try
            {
                var client = new MongoClient("mongodb://207322868:a5!e9HT4U8N29bi@cluster0-shard-00-00-pvem6.mongodb.net:27017,cluster0-shard-00-01-pvem6.mongodb.net:27017,cluster0-shard-00-02-pvem6.mongodb.net:27017/test?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&retryWrites=true");
                var database = client.GetDatabase("followMe");
                var allGroups = database.GetCollection<Group>("groups");
                var u = await getUser(user);
                //שליפת כל הקבוצות
                var g = await getGroup(group);
                if (g != null)
                {
                    ManagmentInGroup managment = new ManagmentInGroup();//add new managment
                    managment.PhoneManagment = user;
                    managment.ComeToTrip = true;
                    g.ListManagment.Add(managment);
                    var filter = Builders<Group>.Filter.Eq("Password", group);
                    var update = Builders<Group>.Update.AddToSet("ListManagment", g.ListManagment);
                    var result = await allGroups.UpdateOneAsync(filter, update);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {

                throw new Exception("לא היה ניתן להוסיף את המשתמש בתור מנהל" + ex.Message);
            }
        }



    }
}

