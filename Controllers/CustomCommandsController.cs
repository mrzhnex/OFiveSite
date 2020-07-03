using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFiveSite.Models;

namespace OFiveSite.Controllers
{
    public class CustomCommandsController : Controller
    {
        public readonly CustomCommandsModel CustomCommandsModel = new CustomCommandsModel();

        private readonly IHttpContextAccessor httpContextAccessor;
        public string UserIp { get; set; }

        public IActionResult CustomCommands()
        {
            UserIp = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            CustomCommandsModel.UserIp = UserIp;
            return View(CustomCommandsModel);
        }

        public CustomCommandsController(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

    }
}