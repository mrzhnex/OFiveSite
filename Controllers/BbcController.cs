using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFivePlugin;
using OFiveSite.Models;

namespace OFiveSite.Controllers
{
    public class BbcController : Controller
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly BbcModel BbcModel = new BbcModel();
        private readonly string font1 = "<font ";
        private readonly string font2 = "</font>";
        public string UserIp { get; set; }
        public IActionResult Bbc()
        {
            UserIp = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            BbcModel.Broadcast = Global.broadcasts.First();
            BbcModel.Message = "";
            BbcModel.Player = Player.Server;
            BbcModel.Server = Global.servers.First();
            BbcModel.Team = Global.teams.First();
            BbcModel.Text = "Текст оповещения";
            BbcModel.colors = "000000";
            BbcModel.Time = 5;
            BbcModel.UserIp = UserIp;
            return View(BbcModel);
        }
        public BbcController(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Refresh(string UserIp, string Server, string BroadcastName, string Player, int Team, int Time, string Text, string colors, string Message = "")
        {
            BbcModel.UserIp = UserIp;
            BbcModel.Broadcast = Global.broadcasts.Where(x => x.Name == BroadcastName).First();
            if (Global.servers.Where(x => x.ServerName == Server).FirstOrDefault() == default)
            {
                Global.DebugSystemError("Catch wrong server name: '" + Server + "'");
                BbcModel.Server = Global.servers.First();
            }
            else
            {
                BbcModel.Server = Global.servers.Where(x => x.ServerName == Server).First();
            }
            if (BbcModel.Server.players.Where(x => x.Nickname == Player).FirstOrDefault() == default)
            {
                Global.DebugSystemError("Catch wrong player nickname: '" + Player + "'");
                BbcModel.Player = BbcModel.Server.players.First();
            }
            else
            {
                BbcModel.Player = BbcModel.Server.players.Where(x => x.Nickname == Player).FirstOrDefault();
            }
            BbcModel.Team = Global.teams.Where(x => x.Id == Team).First();
            BbcModel.Time = Time;
            BbcModel.colors = colors;
            if (BbcModel.Time < 1)
            {
                BbcModel.Time = 1;
            }
            if (BbcModel.Time > 999)
            {
                BbcModel.Time = 999;
            }
            BbcModel.Text = Text;
            BbcModel.Message = Message;
            return View("Bbc", BbcModel);
        }

        [HttpGet]
        public IActionResult BcAction(string UserIp, string Server, string BroadcastName, string Player, int Team, int Time, string Text2, string colors)
        {
            #region main
            BbcModel.UserIp = UserIp;
            string Text = Text2;
            BbcModel.Broadcast = Global.broadcasts.Where(x => x.Name == BroadcastName).First();
            if (Global.servers.Where(x => x.ServerName == Server).FirstOrDefault() == default)
            {
                Global.DebugSystemError("Catch wrong server name: '" + Server + "'");
                return RedirectToAction("Refresh", new { UserIp, Server, BroadcastName, Player, Team, Time, Text, colors, BbcModel.Message });
            }
            else
            {
                BbcModel.Server = Global.servers.Where(x => x.ServerName == Server).First();
            }
            if (BbcModel.Server.players.Where(x => x.Nickname == Player).FirstOrDefault() == default)
            {
                Global.DebugSystemError("Catch wrong player nickname: '" + Player + "'");
                BbcModel.Player = BbcModel.Server.players.First();
            }
            else
            {
                BbcModel.Player = BbcModel.Server.players.Where(x => x.Nickname == Player).FirstOrDefault();
            }
            BbcModel.Team = Global.teams.Where(x => x.Id == Team).First();
            BbcModel.Time = Time;
            BbcModel.colors = colors;
            if (BbcModel.Time < 1)
            {
                BbcModel.Time = 1;
            }
            else if (BbcModel.Time > 999)
            {
                BbcModel.Time = 999;
            }
            BbcModel.Text = Text2;
            BbcModel.colors = colors;
            #endregion

            string sendtext = GetBody(Text);
            Global.DebugSystem("text1: " + sendtext);
            while (sendtext.Contains(font1))
            {
                Global.DebugSystem("index: " + sendtext.IndexOf(font1));
                if (!sendtext.Contains(font2))
                {
                    Global.DebugShutdownError(sendtext);
                }
                int numberoffontbetween = sendtext.Substring(sendtext.IndexOf(font1), sendtext.IndexOf(font2) - sendtext.IndexOf(font1)).Count(x => x.ToString() == font1);                
                int indexFontStart = sendtext.IndexOf(font1);
                string font;
                int indexof = sendtext.IndexOf("\">");
                string sendtext2 = sendtext;
                int tempadder = 0;
                int count = 0;
                while (indexof < sendtext.IndexOf(font1))
                {
                    Global.Debug(count + "text: '" + sendtext2 + "'");
                    int lenght = sendtext2.IndexOf("\">") + "\">".Length;
                    sendtext2 = sendtext2.Remove(0, lenght);
                    tempadder += lenght;
                    indexof = sendtext2.IndexOf("\">") + tempadder;
                    Global.DebugSystem("new sendtext2: '" + sendtext2 + "'");
                    Global.DebugSystem("new indexof: " + indexof);
                    count++;
                }

                font = sendtext.Substring(sendtext.IndexOf(font1), indexof - sendtext.IndexOf(font1) + "\">".Length);
                Global.DebugSystem("mfont: '" + font + "'");
                Global.DebugSystem("text0: '" + sendtext + "'" + sendtext.Substring(sendtext.IndexOf(font1)).IndexOf("\">") + " " + sendtext.IndexOf(font1) + " " + "\">".Length + "'");
                Global.DebugSystem("Success");

                sendtext = sendtext.Remove(indexFontStart, font.Length).Insert(indexFontStart, CreateNewFontStart(font, GetFontType(font)));
                int indexFontClose = sendtext.IndexOf(font2, numberoffontbetween + 1);
                sendtext = sendtext.Remove(indexFontClose, "</font>".Length).Insert(indexFontClose, CreateNewFontClose(GetFontType(font)));
            }
            ReadyCommand c = new ReadyCommand();
            switch (BbcModel.Broadcast.BroadcastType)
            {
                case BroadcastType.Bc:
                    c = new ReadyCommand()
                    {
                        SenderDiscordId = Global.Sessions[UserIp].User.DiscordId,
                        IsSuccess = true,
                        FailedDebugMessage = "",
                        SuccessDebugMessage = "Администратор " + Program._client.GetUser(Global.Sessions[BbcModel.UserIp].User.DiscordId).Mention + " run bc command with args: server: " + BbcModel.Server.ServerName + " time: " + BbcModel.Time + " text: '" + Text + "'",
                        CommandName = Global.BroadcastCommands[BbcModel.Broadcast.BroadcastType],
                        Args = new List<string>()
                        {
                            BbcModel.Time.ToString(),
                            sendtext
                        }
                    };
                    BbcModel.Message = Global.SendCommandToReady(c, BbcModel.Server.ServerName);
                    break;
                case BroadcastType.Pbc:
                    c = new ReadyCommand()
                    {
                        SenderDiscordId = Global.Sessions[UserIp].User.DiscordId,
                        IsSuccess = true,
                        FailedDebugMessage = "",
                        SuccessDebugMessage = "Администратор " + Program._client.GetUser(Global.Sessions[BbcModel.UserIp].User.DiscordId).Mention + " run pbc command with args: server: " + BbcModel.Server.ServerName + " target: " + BbcModel.Player.Nickname + " time: " + BbcModel.Time + " text: '" + Text + "'",
                        CommandName = Global.BroadcastCommands[BbcModel.Broadcast.BroadcastType],
                        Args = new List<string>()
                        {
                            BbcModel.Player.Playerid.ToString(),
                            BbcModel.Time.ToString(),
                            sendtext
                        }
                    };
                    BbcModel.Message = Global.SendCommandToReady(c, BbcModel.Server.ServerName);
                    break;
                case BroadcastType.Tbc:
                    c = new ReadyCommand()
                    {
                        SenderDiscordId = Global.Sessions[UserIp].User.DiscordId,
                        IsSuccess = true,
                        FailedDebugMessage = "",
                        SuccessDebugMessage = "Администратор " + Program._client.GetUser(Global.Sessions[BbcModel.UserIp].User.DiscordId).Mention + " run tbc command with args: server: " + BbcModel.Server.ServerName + " team: " + BbcModel.Team.Name + " time: " + BbcModel.Time + " text: '" + Text + "'",
                        CommandName = Global.BroadcastCommands[BbcModel.Broadcast.BroadcastType],
                        Args = new List<string>()
                        {
                            BbcModel.Team.SmodTeam.ToString(),
                            BbcModel.Time.ToString(),
                            sendtext
                        }
                    };
                    BbcModel.Message = Global.SendCommandToReady(c, BbcModel.Server.ServerName);
                    break;

            }

            return RedirectToAction("Refresh", new { UserIp, Server, BroadcastName, Player, Team, Time, Text, colors, BbcModel.Message });
        }

        #region other
        enum FontType
        {
            Size,
            Color,
            SizeAndColor
        }

        private string CreateNewFontStart(string font, FontType fontType)
        {
            Global.DebugSystem("font: '" + font + "'");
            switch (fontType)
            {
                case FontType.Color:
                    return "<color=\"" + GetColorByFont(font) + "\">";
                case FontType.Size:
                    return "<size=\"" + GetSizeByFont(font) + "\">";
                case FontType.SizeAndColor:
                    return "<color=\"" + GetColorByFont(font) + "\"><size=\"" + GetSizeByFont(font) + "\">";
                default:
                    break;
            }
            return font;
        }

        private string GetColorByFont(string font)
        {
            return GetBetweenWithoutRemovedEmpty(font, "color=\"", "\"");
        }

        private string GetSizeByFont(string font)
        {
            return GetBetweenWithoutRemovedEmpty(font, "size=\"", "\"");
        }
        

        private string CreateNewFontClose(FontType fontType)
        {
            switch (fontType)
            {
                case FontType.Color:
                    return "</color>";
                case FontType.Size:
                    return "</size>";
                case FontType.SizeAndColor:
                    return "</size></color>";
                default:
                    break;
            }
            return string.Empty;
        }

        private FontType GetFontType(string font)
        {
            if (font.Contains("color=") && font.Contains("size="))
                return FontType.SizeAndColor;
            else if (font.Contains("size="))
                return FontType.Size;
            else if (font.Contains("color="))
                return FontType.Color;
            else
                return default;
        }
        #endregion

        #region Split Into Styles
        private string GetBody(string text)
        {
            return text.Replace("<head></head>", string.Empty).Replace("<body>", string.Empty).Replace("</body>", string.Empty).Replace("<div>", " ").Replace("</div>", string.Empty);
        }

        public static string GetBetweenWithoutRemovedEmpty(string text, string first, string last)
        {
            Global.DebugSystem("text: '" + text + "' first: '" + first + "' last: '" + last + "'");
            text = text.Substring(text.IndexOf(first) + first.Length);
            try
            {
                text = text.Replace(text.Substring(text.IndexOf(last)), string.Empty);
            }
            catch (Exception)
            {
                throw new Exception("text: '" + text + "' first: '" + first + "' last: '" + last + "'");
            }
            
            return text;
        }
        #endregion
    }
}