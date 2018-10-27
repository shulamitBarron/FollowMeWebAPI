using DAL.Models;
using QualiAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace QualiAPI.Controllers
{
    public class ManagmentController : ApiController
    {

        /// <summary>
        /// הוספת מטייל-משתמש לקבוצה
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/AddManagment/{group}/{phone}")]
        public async Task<IHttpActionResult> AddManagment([FromUri]  string phone, [FromUri]string group)
        {
            try
            {
                if (await BL.Managment.AddManagment1(group, phone) == true)
                    return Ok(true);
                return Content(HttpStatusCode.BadRequest, false);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
        }

        /// <summary>
        /// שליחת התראה למטייל 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/sendMessageComplex")]
        public async Task<IHttpActionResult> sendMessageComplex([FromBody] MessageUser message)
        {
            try
            {
                if (await BL.Managment.sendMessageComplex(message) == true)
                    return Ok(true);
                return Content(HttpStatusCode.BadRequest, false);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
        }



        /// <summary>
        /// קבלת קבוצות בהם המטייל- המשתמש מנהל בהם והם פעילות
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/getManagmentGroup/{phone}")]
        public async Task<IHttpActionResult> getManagmentGroup([FromUri] string phone)
        {
            try
            {
                var gro = await BL.GroupS.getManagmentGroup(phone);
                return Ok(gro);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// קבלת קבוצות בהם המטייל- המשתמש מנהל בהם והם לא פעילות
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/getManagmentGroupFalse/{phone}")]
        public async Task<IHttpActionResult> getManagmentGroupFalse([FromUri] string phone)
        {
            try
            {
                var groups = await BL.GroupS.getManagmentGroupThatFalse(phone);
                return Ok(groups);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("api/sendHelpMessage/{phone}")]
        public async Task<IHttpActionResult> sendHelpMessage([FromUri]string phone)
        {
            try
            {
                if (await BL.Managment.sendHelpMessage(phone) == true)
                    return Ok(true);
                return Content(HttpStatusCode.BadRequest, false);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
        }

    }
}
