using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFiveSite.Models;

namespace OFiveSite.Controllers
{
    public class PersonalAccountController : Controller
    {
        public readonly PersonalAccountModel CustomCommandsModel = new PersonalAccountModel();

        private readonly IHttpContextAccessor httpContextAccessor;
        public string UserIp { get; set; }

        public IActionResult PersonalAccount()
        {
            UserIp = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            CustomCommandsModel.UserIp = UserIp;
            return View(CustomCommandsModel);
        }

        public PersonalAccountController(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

    }
}