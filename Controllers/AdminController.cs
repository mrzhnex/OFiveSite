using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFivePlugin;
using OFiveSite.Models;

namespace OFiveSite.Controllers
{
    public class AdminController : Controller
    {
        public AdminModel AdminModel = new AdminModel();
        private readonly IHttpContextAccessor httpContextAccessor;
        public string UserIp { get; set; }

        #region default
        private void SessionHelper()
        {
            UserIp = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            AdminModel.UserIp = UserIp;
        }

        public ActionResult Admin()
        {
            SessionHelper();
            Start();
            return View(AdminModel);
        }
        public AdminController(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public void Start()
        {
            AdminModel.Server = Global.servers.First();
            AdminModel.Player = AdminModel.Server.players.First();
            AdminModel.Reason = Global.reasons.First();
            AdminModel.Time = AdminModel.Reason.Time;
            AdminModel.Comment = "Без комментария";
            AdminModel.CustomReason = "Введите причину";
            Global.Sessions[UserIp].Reason = AdminModel.Reason;
        }

        #endregion

        #region admins
        [HttpGet]
        public IActionResult Refresh(string UserIp, string Server, string Player, string Reason, int Time, string Comment, string CustomReason, string Message = "")
        {
            int time = Time;
            AdminModel.UserIp = UserIp;
            if (Global.servers.Where(x => x.ServerName == Server).FirstOrDefault() == default)
            {
                Global.DebugSystemError("Catch wrong server name: '" + Server + "'");
                AdminModel.Server = Global.servers.First();
            }
            else
            {
                AdminModel.Server = Global.servers.Where(x => x.ServerName == Server).First();
            }
            if (AdminModel.Server.players.Where(x => x.Nickname == Player).FirstOrDefault() == default)
            {
                Global.DebugSystemError("Catch wrong player nickname: '" + Player + "'");
                AdminModel.Player = AdminModel.Server.players.First();
            }
            else
            {
                AdminModel.Player = AdminModel.Server.players.Where(x => x.Nickname == Player).FirstOrDefault();
            }
            AdminModel.Reason = Global.reasons.Where(x => x.Id == Reason).First();
            if (Global.Sessions.ContainsKey(UserIp))
            {
                if (Global.Sessions[UserIp].Reason.Id != AdminModel.Reason.Id)
                {
                    Time = AdminModel.Reason.Time;
                }
            }
            else
            {
                Time = AdminModel.Reason.Time;
            }
            AdminModel.Time = Time;
            Global.Sessions[UserIp].Reason = AdminModel.Reason;
            AdminModel.CustomReason = CustomReason;
            AdminModel.Comment = Comment;
            AdminModel.Message = Message;
            if (time != Time)
            {
                return RedirectToAction("Refresh", new { UserIp, Server, Player, Reason, Time, Comment, CustomReason, Message });
            }
            return View("Admin", AdminModel);
        }

        [HttpGet]
        public IActionResult Ban(string UserIp, string Server, string Player, string Reason, int Time, string Comment, string CustomReason)
        {

            AdminModel.UserIp = UserIp;
            AdminModel.Server = Global.servers.Where(x => x.ServerName == Server).First();
            if (AdminModel.Server.players.Where(x => x.Nickname == Player).FirstOrDefault() == default)
            {
                Global.DebugSystemError("Catch wrong player nickname: '" + Player + "'");
                return RedirectToAction("Refresh", new { UserIp, Server, AdminModel.Server.players.First().Nickname, Reason, Time, Comment, CustomReason, AdminModel.Message });
            }
            else
            {
                AdminModel.Player = AdminModel.Server.players.Where(x => x.Nickname == Player).FirstOrDefault();
            }
            AdminModel.Reason = Global.reasons.Where(x => x.Id == Reason).First();
            if (Global.Sessions.ContainsKey(UserIp))
            {
                if (Global.Sessions[UserIp].Reason.Id != AdminModel.Reason.Id)
                {
                    Time = AdminModel.Reason.Time;
                }
            }
            else
            {
                Time = AdminModel.Reason.Time;
            }
            AdminModel.Time = Time;
            Global.Sessions[UserIp].Reason = AdminModel.Reason;
            AdminModel.CustomReason = CustomReason;
            AdminModel.Comment = Comment;


            if (AdminModel.Player.Playerid == 0)
            {
                AdminModel.Message = "Вы не можете забанить этого игрока";
                Global.Debug("User " + Program._client.GetUser(Global.Sessions[AdminModel.UserIp].User.DiscordId).Mention + " try to ban Server player on Server " + AdminModel.Server.ServerName);
            }
            else
            {
                string reasontranscript = AdminModel.Reason.Transcript;
                if (AdminModel.Reason.Id == "3" || AdminModel.Reason.Id == "4")
                {
                    reasontranscript = CustomReason;
                }
                string reasonid = AdminModel.Reason.Id;
                if (AdminModel.Reason.Id == "4")
                {
                    reasonid = "Своя причина";
                }
                ReadyCommand c = new ReadyCommand();
                if (reasonid != "0")
                {
                    int bancount = (Global.InfoSite.BanCount = Global.InfoSite.BanCount + 1);
                    if (!Global.SaveSiteInfo())
                    {
                        Global.DebugSystemError("Failed to save site info");
                    }
                    c = new ReadyCommand()
                    {
                        SenderDiscordId = Global.Sessions[UserIp].User.DiscordId,
                        IsSuccess = true,
                        FailedDebugMessage = "Бан не прошел. Бан номер " + bancount + ". Администратор " + Program._client.GetUser(Global.Sessions[AdminModel.UserIp].User.DiscordId).Mention + " попытался забанить игрока " + AdminModel.Player.Nickname + " (steamid: " + AdminModel.Player.SteamId + ") на сервере '" + AdminModel.Server.ServerName + "' по причине " + reasonid + "(" + reasontranscript + ") на " + AdminModel.Time + " минут. Комментарий администратора: '" + AdminModel.Comment + "'",
                        SuccessDebugMessage = "Бан номер " + bancount + ". Администратор " + Program._client.GetUser(Global.Sessions[AdminModel.UserIp].User.DiscordId).Mention + " забанил игрока " + AdminModel.Player.Nickname + " (steamid: " + AdminModel.Player.SteamId + ") на сервере '" + AdminModel.Server.ServerName + "' по причине " + reasonid + "(" + reasontranscript + ") на " + AdminModel.Time + " минут. Комментарий администратора: '" + AdminModel.Comment + "'",
                        CommandName = "ban",
                        Args = new List<string>()
                        {
                            AdminModel.Player.Playerid.ToString(),
                            AdminModel.Time.ToString(),
                            reasonid + "(" + reasontranscript + ") на " + AdminModel.Time + " минут администратором " + Program._client.GetUser(Global.Sessions[AdminModel.UserIp].User.DiscordId).Username + ". Комментарий: " + AdminModel.Comment
                        }

                    };
                }
                else
                {
                    c = new ReadyCommand()
                    {
                        SenderDiscordId = Global.Sessions[UserIp].User.DiscordId,
                        IsSuccess = true,
                        FailedDebugMessage = string.Empty,
                        SuccessDebugMessage = string.Empty,
                        CommandName = "ban",
                        Args = new List<string>()
                        {
                            AdminModel.Player.Playerid.ToString(),
                            AdminModel.Time.ToString(),
                            reasonid + "(" + reasontranscript + ") на " + AdminModel.Time + " минут администратором " + Program._client.GetUser(Global.Sessions[AdminModel.UserIp].User.DiscordId).Username + ". Комментарий: " + AdminModel.Comment
                        }

                    };
                }
                if (!Global.SaveUsers())
                {
                    Global.DebugSystemError("Failed to save USER: \nName: " + Global.users.Where(x => x.DiscordId == Global.Sessions[UserIp].User.DiscordId).First().Name + " \nDiscordId: " + Global.users.Where(x => x.DiscordId == Global.Sessions[UserIp].User.DiscordId).First().DiscordId);
                }

                AdminModel.Message = Global.SendCommandToReady(c, AdminModel.Server.ServerName);
            }

            return RedirectToAction("Refresh", new { UserIp, Server, AdminModel.Server.players.First().Nickname, Reason, Time, Comment, CustomReason, AdminModel.Message });
        }


        #endregion
    }
}