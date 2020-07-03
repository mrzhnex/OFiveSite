using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFiveSite.Models;

namespace OFiveSite.Controllers
{
    public class MainController : Controller
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        public readonly MainModel MainModel = new MainModel();
        public string UserIp { get; set; }

        public IActionResult Main()
        {
            UserIp = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            MainModel.UserIp = UserIp;
            return View(MainModel);
        }

        public MainController(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public IActionResult ToAdmin()
        {
            return Redirect("~/Admin/Admin");
        }
        public IActionResult ToOffline()
        {
            return Redirect("~/Offline/Offline");
        }
        public IActionResult ToUnBan()
        {
            return Redirect("~/UnBan/UnBan");
        }
        public IActionResult ToBc()
        {
            return Redirect("~/Bc/Bc");
        }
        public IActionResult ToBbc()
        {
            return Redirect("~/Bbc/Bbc");
        }
        public IActionResult ToCustom()
        {
            return Redirect("~/CustomCommands/CustomCommands");
        }
        public IActionResult ToMain()
        {
            return Redirect("~/Main/Main");
        }
    }
}