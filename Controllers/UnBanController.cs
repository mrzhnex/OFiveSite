using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFivePlugin;
using OFiveSite.Models;
using System.Collections.Generic;
using System.Linq;

namespace OFiveSite.Controllers
{
    public class UnBanController : Controller
    {
        UnBanModel UnBanModel = new UnBanModel();
        private readonly IHttpContextAccessor httpContextAccessor;
        public string UserIp { get; set; }
        public IActionResult UnBan()
        {
            UserIp = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            UnBanModel.UserIp = UserIp;
            UnBanModel.SteamId = "0";
            UnBanModel.Reason = "Причина разбана";
            UnBanModel.Comment = "Без комментария";
            UnBanModel.Message = "";
            return View(UnBanModel);
        }
        public UnBanController(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Refresh(string UserIp, string SteamId, string Reason, string Comment, string Message = "")
        {
            UnBanModel.UserIp = UserIp;
            UnBanModel.Reason = Reason;
            UnBanModel.SteamId = SteamId;
            UnBanModel.Comment = Comment;
            UnBanModel.Message = Message;
            return View("UnBan", UnBanModel);
        }

        [HttpGet]
        public IActionResult UnBanAction(string UserIp, string SteamId, string Reason, string Comment)
        {
            UnBanModel.UserIp = UserIp;
            UnBanModel.Reason = Reason;
            UnBanModel.SteamId = SteamId;
            UnBanModel.Comment = Comment;

            if (UnBanModel.SteamId == "0")
            {
                UnBanModel.Message = "Вы не можете разбанить этого игрока";
                Global.Debug("User " + Program._client.GetUser(Global.Sessions[UnBanModel.UserIp].User.DiscordId).Mention + " try to unban Server player");
            }
            else
            {
                ReadyCommand c = new ReadyCommand()
                {
                    SenderDiscordId = Global.Sessions[UserIp].User.DiscordId,
                    IsSuccess = true,
                    FailedDebugMessage = "Администратор " + Program._client.GetUser(Global.Sessions[UnBanModel.UserIp].User.DiscordId).Mention + " попытался разбанить игрока со SteamId64: " + UnBanModel.SteamId + " по причине '" + UnBanModel.Reason + "'. Комментарий администратора: '" + UnBanModel.Comment + "'",
                    SuccessDebugMessage = "Игрок (Nickname) (SteamId64: " + UnBanModel.SteamId + "), забаненный по причине '(Reason)', был разбанен администратором " + Program._client.GetUser(Global.Sessions[UnBanModel.UserIp].User.DiscordId).Mention + " по причине '" + UnBanModel.Reason + "'. Комментарий администратора: '" + UnBanModel.Comment + "'",
                    CommandName = "unban",
                    Args = new List<string>()
                    {
                        UnBanModel.SteamId
                    }
                };
                if (!Global.SaveUsers())
                {
                    Global.DebugSystemError("Failed to save USER: \nName: " + Global.users.Where(x => x.DiscordId == Global.Sessions[UserIp].User.DiscordId).First().Name + " \nDiscordId: " + Global.users.Where(x => x.DiscordId == Global.Sessions[UserIp].User.DiscordId).First().DiscordId);
                }

                UnBanModel.Message = Global.SendCommandToReady(c, Global.servers.FirstOrDefault().ServerName);
            }

            return RedirectToAction("Refresh", new { UserIp, SteamId, Reason, Comment, UnBanModel.Message });
        }
    }
}