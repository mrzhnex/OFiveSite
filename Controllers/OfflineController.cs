using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFivePlugin;
using OFiveSite.Models;

namespace OFiveSite.Controllers
{
    public class OfflineController : Controller
    {
        public OfflineModel OfflineModel = new OfflineModel();
        private readonly IHttpContextAccessor httpContextAccessor;
        public string UserIp { get; set; }

        #region default

        public ActionResult Offline()
        {
            UserIp = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            OfflineModel.UserIp = UserIp;
            OfflineModel.SteamId = "0";
            OfflineModel.Reason = Global.reasons.First();
            OfflineModel.Time = OfflineModel.Reason.Time;
            OfflineModel.Comment = "Без комментария";
            OfflineModel.CustomReason = "Введите причину";
            Global.Sessions[UserIp].Reason = OfflineModel.Reason;
            return View(OfflineModel);
        }
        public OfflineController(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region admins
        [HttpGet]
        public IActionResult Refresh(string UserIp, string SteamId, string Reason, int Time, string Comment, string CustomReason, string Message = "")
        {
            int time = Time;
            OfflineModel.UserIp = UserIp;
            OfflineModel.SteamId = SteamId;
            OfflineModel.Reason = Global.reasons.Where(x => x.Id == Reason).First();
            if (Global.Sessions.ContainsKey(UserIp))
            {
                if (Global.Sessions[UserIp].Reason.Id != OfflineModel.Reason.Id)
                {
                    Time = OfflineModel.Reason.Time;
                }
            }
            else
            {
                Time = OfflineModel.Reason.Time;
            }
            OfflineModel.Time = Time;
            Global.Sessions[UserIp].Reason = OfflineModel.Reason;
            OfflineModel.CustomReason = CustomReason;
            OfflineModel.Comment = Comment;
            OfflineModel.Message = Message;
            if (time != Time)
            {
                return RedirectToAction("Refresh", new { UserIp, SteamId, Reason, Time, Comment, CustomReason, Message });
            }
            return View("Offline", OfflineModel);
        }

        [HttpGet]
        public IActionResult Ban(string UserIp, string Nickname, string SteamId, string Reason, int Time, string Comment, string CustomReason)
        {
            OfflineModel.UserIp = UserIp;
            OfflineModel.SteamId = SteamId;
            OfflineModel.Reason = Global.reasons.Where(x => x.Id == Reason).First();
            if (Global.Sessions.ContainsKey(UserIp))
            {
                if (Global.Sessions[UserIp].Reason.Id != OfflineModel.Reason.Id)
                {
                    Time = OfflineModel.Reason.Time;
                }
            }
            else
            {
                Time = OfflineModel.Reason.Time;
            }
            OfflineModel.Time = Time;
            Global.Sessions[UserIp].Reason = OfflineModel.Reason;
            OfflineModel.CustomReason = CustomReason;
            OfflineModel.Comment = Comment;


            if (OfflineModel.SteamId == "0")
            {
                OfflineModel.Message = "Вы не можете забанить этого игрока";
                Global.Debug("User " + Program._client.GetUser(Global.Sessions[OfflineModel.UserIp].User.DiscordId).Mention + " try to ban Server player (offline mode)");
            }
            else
            {
                string reasontranscript = OfflineModel.Reason.Transcript;
                if (OfflineModel.Reason.Id == "3" || OfflineModel.Reason.Id == "4")
                {
                    reasontranscript = CustomReason;
                }
                string reasonid = OfflineModel.Reason.Id;
                if (OfflineModel.Reason.Id == "4")
                {
                    reasonid = "Своя причина";
                }
                int bancount = (Global.InfoSite.BanCount = Global.InfoSite.BanCount + 1);
                if (!Global.SaveSiteInfo())
                {
                    Global.DebugSystemError("Failed to save site info");
                }
                ReadyCommand c = new ReadyCommand()
                {
                    SenderDiscordId = Global.Sessions[UserIp].User.DiscordId,
                    IsSuccess = true,
                    FailedDebugMessage = "Бан номер " + bancount + " (Оффлайн-мод). Администратор " + Program._client.GetUser(Global.Sessions[OfflineModel.UserIp].User.DiscordId).Mention + " забанил игрока со SteamId64: " + OfflineModel.SteamId + " по причине " + reasonid + "(" + reasontranscript + ") на " + OfflineModel.Time + " минут. Комментарий администратора: '" + OfflineModel.Comment + "'",
                    SuccessDebugMessage = "Бан номер " + bancount + ". Администратор " + Program._client.GetUser(Global.Sessions[OfflineModel.UserIp].User.DiscordId).Mention + " забанил игрока со SteamId64: " + OfflineModel.SteamId + " по причине " + reasonid + "(" + reasontranscript + ") на " + OfflineModel.Time + " минут. Комментарий администратора: '" + OfflineModel.Comment + "'",
                    CommandName = "oban",
                    Args = new List<string>()
                    {
                        "",
                        OfflineModel.SteamId,
                        OfflineModel.Time.ToString(),
                        reasonid + "(" + reasontranscript + ") на " + OfflineModel.Time + " минут администратором " + Program._client.GetUser(Global.Sessions[OfflineModel.UserIp].User.DiscordId).Username + ". Комментарий: " + OfflineModel.Comment
                    }

                };
                if (!Global.SaveUsers())
                {
                    Global.DebugSystemError("Failed to save USER: \nName: " + Global.users.Where(x => x.DiscordId == Global.Sessions[UserIp].User.DiscordId).First().Name + " \nDiscordId: " + Global.users.Where(x => x.DiscordId == Global.Sessions[UserIp].User.DiscordId).First().DiscordId);
                }

                OfflineModel.Message = Global.SendCommandToReady(c, Global.servers.First().ServerName);
            }

            return RedirectToAction("Refresh", new { UserIp, Nickname, SteamId, Reason, Time, Comment, CustomReason, OfflineModel.Message });
        }


        #endregion
    }
}