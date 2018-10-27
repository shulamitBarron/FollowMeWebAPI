using MongoDB.Driver;
using MongoDB.Driver.Linq;
using QualiAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Threading.Tasks;
using DAL.Models;
using MongoDB.Bson;
using BL;
using DAL;

namespace QualiAPI.Controllers
{
    public class MarkerUser
    {
        public Marker marker;
        public string image;
        public double distanceLessManagment;
        public bool statusDistance;
    }
    public class GroupController : ApiController
    {


        /// <summary>
        ///   קבלת כל הקבוצות שהמשתמש רשום עליהם והם פעילות
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        ///
        [HttpGet]
        [Route("api/groupOfUser/{phone}")]
        public async Task<IHttpActionResult> groupOfUser([FromUri] string phone)
        {
            try
            {
                List<Group> gro = await GroupS.getGroupOpenToUser(phone);
                return Ok(gro);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// קבלת כל הקבוצות שהמשתמש רשום עליהם והם כרגע לא פעילות
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>


        [HttpGet]
        [Route("api/groupOfUserStatusFalse/{phone}")]
        public async Task<IHttpActionResult> groupOfUserStatusFalse([FromUri] string phone)
        {
            try
            {
                List<Group> gro = await BL.GroupS.groupOfUserStatusFalse(phone);
                return Ok(gro);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// בדיקה האם קיים קוד קבוצה כזאת
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/checkCodeGroup/{Code}/{phone}")]
        public async Task<IHttpActionResult> checkCodeGroup([FromUri] string Code, string phone)
        {
            try
            {
                var gro = await BL.GroupS.checkCodeGroup(Code);
                if (gro != null)
                {
                    await BL.GroupS.AddToGroup(Code, phone);
                    return Ok(gro);
                }
                return Content(HttpStatusCode.BadRequest, "לא היתה אפשרות להצטרף לקבוצה");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        /// <summary>
        /// קבלת כל הקבוצות הרלוונטיות
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/getAllGroups")]
        public IHttpActionResult getAllGroups()
        {
            try
            {
                var gro = BL.GroupS.getAllGroups();
                return Ok(gro);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// קבלת כל הקבוצות הרלוונטיות
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/getAllGroupsForTest")]
        public async Task<IHttpActionResult> getAllGroupsForTest()
        {
            try
            {
                var gro = await BL.GroupS.getAllGroupsForTets();
                return Ok(gro);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        /// <summary>
        /// קבלת כל הקבוצות שלא פעילות כרגע
        /// </summary>
        /// <returns></returns>

        //TODO:מה הפונקציה עושה
        [HttpGet]
        [Route("api/getAllGroupsDisable")]
        public IHttpActionResult getAllGroupsDisable()
        {
            try
            {
                var gro = GroupS.getAllGroupsDisable();
                return Ok(gro);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// הוספת קבוצה חדשה
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        ///
        [HttpPost]
        [Route("api/addGroup")]
        public async Task<IHttpActionResult> addGroup([FromBody]Group group)
        {

            try
            {
                Group newGroup = await BL.GroupS.addGroup(group);
                if (newGroup == null)
                    return Content(HttpStatusCode.BadRequest, "הוספת הקבוצה נכשלה");
                return Ok(newGroup);
            }
            catch (Exception ex)
            {

                return Content(HttpStatusCode.BadRequest, ex.Message);
            }

        }


        /// <summary>
        /// מחיקת קבוצה
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        [Route("api/deleteGroup/{password}")]
        [HttpDelete]
        public async Task<IHttpActionResult> Delete([FromUri]string password)
        {
            try
            {
                bool b = await GroupS.deleteGroup1(password);
                if (b == true)
                    return Ok(b);
                return NotFound();
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.NotFound, true);
            }

        }

        /// <summary>
        /// עדכון פרטי קבוצה 
        /// </summary>
        /// <param name="group2"></param>
        /// <returns></returns>
        [Route("api/updateGroup")]
        [HttpPost]
        public async Task<IHttpActionResult> updateGroup([FromBody]Group group2)
        {

            Group group = await BL.GroupS.UpdateGroup1(group2);
            if (group == null)
                return NotFound();
            return Ok(group);

        }

        /// <summary>
        /// משתמשים של קבוצה מסוימת
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/UsersOfGroup/{pass}")]
        public async Task<IHttpActionResult> UsersOfGroup([FromUri] string pass)
        {
            try
            {
                var gro = await BL.GroupS.GetUserOfGroup1(pass);
                return Ok(gro);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// נקודות של מנהלי קבוצה מסוימת
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/getManagmentsMarker")]
        public async Task<IHttpActionResult> getManagmentsMarker([FromBody] Group pass)
        {
            try
            {
                var g = await BL.GroupS.getGroupByPass(pass.Password);
                List<string> users = g.ListManagment.Select(p => p.PhoneManagment).ToList();
                List<MarkerUser> markers = new List<MarkerUser>();
                foreach (var item in users)
                {
                    UserProfile user = await conectDB.getUser(item);
                    if (user != null)
                    {
                        MarkerUser marker = new MarkerUser();
                        marker.marker = user.Marker;
                        marker.image = user.Image;
                        markers.Add(marker);
                        var man = g.ListManagment.Find(p => p.PhoneManagment == item);
                        marker.statusDistance = man.ComeToTrip;
                    }
                       
                }


                return Ok(markers);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        /// <summary>
        /// קבלת מקומי המטיילים לקבוצה מסוימת 
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/getUsersMarker")]
        public async Task<IHttpActionResult> getUsersMarker([FromBody] Group pass)
        {
            try
            {
                var g = await BL.GroupS.getGroupByPass(pass.Password);
                List<string> users = g.Users.Select(p => p.UserPhoneGroup).ToList();
                List<MarkerUser> markers = new List<MarkerUser>();
                foreach (var item in users)
                {
                    UserProfile user = await conectDB.getUser(item);
                    if (user != null)
                    {
                        var markerUser = new MarkerUser();
                        markerUser.image = user.Image;
                        markerUser.statusDistance = user.Status;
                        markerUser.distanceLessManagment = await GroupS.getLessDistance(g.Password,user.Marker.Lat,user.Marker.Lng);
                        markerUser.marker = user.Marker;
                       markers.Add(markerUser);
                    }
                        
                }

                return Ok(markers);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// הוספת משתמש לקבוצה
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/updateUsersGroup")]
        public async Task<IHttpActionResult> updateUsersGroup([FromBody]Group group)
        {
            try
            {
                bool b = await BL.GroupS.updateUsersGroup(group);
                if (b == true)
                    return Ok(b);
                return Content(HttpStatusCode.BadRequest, "תקלה בשמירת הנתונים");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpPost]
        [Route("api/updateManagersGroup")]
        public async Task<IHttpActionResult> updateManagersGroup([FromBody]Group group)
        {
            try
            {
                
                bool b = await conectDB.UpdateManagersGroup(group);
                if (b == true)
                    return Ok(b);
                return Content(HttpStatusCode.BadRequest, "תקלה בשמירת הנתונים");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        /// <summary>
        /// קבלת קבוצה
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/getGroupByPass")]
        public async Task<IHttpActionResult> getGroupByPass([FromBody]string pass)
        {
            try
            {
                Group group = await BL.GroupS.getGroupByPass(pass);
                return Ok(group);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("api/getManagersOfGroup/{password}")]
        public async Task<IHttpActionResult> getManagersOfGroup(string password)
        {
            List<UserProfile> users = new List<UserProfile>();
            try
            {
                Group group = await conectDB.getGroup(password);
                foreach (var item in group.ListManagment)
                {
                    users.Add(await conectDB.getUser(item.PhoneManagment));
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("api/getUsersNotManagerOfGroup/{password}")]
        public async Task<IHttpActionResult> getUsersNotManagerOfGroup(string password)
        {
            try
            {
                var all =await conectDB.getAllUsers();
                Group group = await conectDB.getGroup(password);
                List<UserProfile> users = new List<UserProfile>();
                foreach (var item in all)
                {
                    if (group.ListManagment.Find(p => p.PhoneManagment == item.Phone) == null)
                        users.Add(item);
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }




        [HttpPost]
        [Route("api/sendMessage/{password}/{message}")]
        public async Task<IHttpActionResult> sendMessage(string password,string message, [FromBody] List<UserProfile> users)
        {
          Group group=  await conectDB.getGroup(password);
            try
            {
                foreach (var item in users)
                {
                    item.UserMessageNeedGet.Add(new MessageUser() { Group = group, UserName = item.FirstName + " " + item.LastName, Message = new MessageGroup() { CodeError = 10, MessageError = message } });
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }


    }

}

