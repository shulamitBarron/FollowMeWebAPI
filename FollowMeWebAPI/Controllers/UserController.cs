using DAL;
using DAL.Models;
using QualiAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace QualiAPI.Controllers
{
    public class UserController : ApiController
    {
        public class help
        {
            public string phone;
            public double lat;
            public double lng;
        }
        /// <summary>
        /// עדכון נקידות מיקום למשתמש
        /// </summary>
        /// <param name="mar">טלפון+מקום בנקודות</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/updateMarker")]
        public async Task<IHttpActionResult> UpdateMarker([FromBody] help mar)
        {
            try
            {
                bool b = await BL.User.updateMarker(mar.phone, mar.lat, mar.lng);
                return Ok(b);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// קבלת כל המשתמשים של קבוצה מסוימת
        /// </summary>
        /// <param name="pass">סיסמת קבוצה</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/getAllUsersNotInGroup/{pass}")]
        public async Task<IHttpActionResult> getAllUsersNotInGroup([FromUri]string pass)
        {
            try
            {
                var users = await BL.User.getAllUsersNotInGroup(pass);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("api/getUserInf/{phone}")]
        public async Task<IHttpActionResult> getUserInf([FromUri]string phone)
        {
            try
            {
                var user = await conectDB.getUser(phone);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        //[HttpGet]
        //[EnableCors("*", "*", "*")]
        //[Route("api/getAllUsersInGroup/{pass}")]
        //public async Task<IHttpActionResult> getAllUsersInGroup([FromUri]string pass)
        //{
        //    try
        //    {
        //        var users = await BL.User.getAllUsersInGroup(pass);
        //        return Ok(users);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content(HttpStatusCode.BadRequest, ex.Message);
        //    }
        //}
        /// <summary>
        /// קבלת כל הקבוצות שהמטייל-משתמש מאשר את השתתפותו בטיול 
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/checkOpenGroupAndConfirm/{phone}")]
        public IHttpActionResult checkOpenGroupAndConfirm([FromUri]string phone)
        {
            try
            {
                var groupConfirm = BL.User.checkOpenGroupAndConfirm(phone);
                return Ok(groupConfirm);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// בודק האם המטייל רחוק מכל מנהלי הקבוצה
        /// </summary>
        /// <param name="phone">פלאפון של המטייל</param>
        /// <returns>מחזיר רשימה של כל הקבוצות שהמטייל התרחק מהמנהלים של קבוצה זו</returns>
        [HttpGet]
        [Route("api/CheckDistance/{phone}")]
        [EnableCors("*","*","*")]
        public async Task<IHttpActionResult> CheckDistance([FromUri] string phone)
        {
            try
            {

               var far = await BL.GroupS.getGroupDangerous(phone);
                return Ok(far);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// האם יש למטייל- משתמש התראות שהוא צריך לקבל
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/CheckIfHaveMessage/{phone}")]
        public async Task<IHttpActionResult> CheckIfHaveMessage([FromUri] string phone)
        {
            try
            {

                List<MessageUser> messages = await BL.User.getAllMessageUser(phone);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// המטייל מאשר את השתתפותו בטיול
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/AgreeToAddGroup/{pass}/{phone}")]
        public async Task<IHttpActionResult> AgreeToAddGroup([FromUri] string pass, [FromUri] string phone)
        {
            try
            {

                bool b = await BL.User.AgreeToAddGroup(pass, phone);
                if (b == false)
                    throw new Exception("כשלון בעדכון האישור");
                return Ok(b);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// עדכון סטטוס משתמש- מטייל
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/updateStatusUser/{phone}")]
        public async Task<IHttpActionResult> updateStatusUser([FromUri] string phone)
        {
            try
            {
                bool b = await BL.User.UpdateUserStatus(phone);
                return Ok(b);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        [HttpPost]
        [Route("api/updateUser")]
        public async Task<IHttpActionResult> updateUser([FromBody]UserProfile user)
        {
            try
            {
                bool b = await BL.User.UpdateUser(user);
                return Ok(b);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("api/getHistoriesUser/{phone}")]
        public async Task<IHttpActionResult> getHistories([FromUri]string phone)
        {
            try
            {
              List<Histories> histories=await BL.User.getAllHistories(phone);
                return Ok(histories);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        [HttpGet]
        [Route("api/getArrayByMonthUser/{phone}")]
        public async Task<IHttpActionResult> getArrayByMonthUser([FromUri]string phone)
        {
            try
            {
                int[] arr = new int[12];
               arr= await BL.User.getHistoriesByMonthUser(phone);
               
                return Ok(arr);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        [HttpGet]
        [Route("api/getArrayByMonthManager/{phone}")]
        public async Task<IHttpActionResult> getArrayByMonthManager([FromUri]string phone)
        {
            try
            {
                int[] arr = new int[12];
                arr = await BL.User.getHistoriesByMonthManager(phone);

                return Ok(arr);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

      
        [HttpGet]
        [Route("api/getBarChar/{phone}/{type}")]
        public async Task<IHttpActionResult> getBarChar([FromUri]string phone,[FromUri]int type)
        {
            try
            {
              var arr = await BL.User.getBarChar(phone,type); return Ok(arr); 
                
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }


    }
}
