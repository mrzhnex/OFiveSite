using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;

namespace OFiveSite
{
    public class Program
    {
        public static DiscordSocketClient _client;
        private CommandService _command;
        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _command = new CommandService();
            _client.Log += Log;
            await RegisterCommandAsync();
            await _client.LoginAsync(TokenType.Bot, Global.botToken);
            await _client.StartAsync();
            await Task.Delay(-1);
        }
        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
        public async Task RegisterCommandAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _command.AddModulesAsync(Assembly.GetEntryAssembly());
        }
        public async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg.Content is null || arg.Author.IsBot || (arg.Channel.ToString()[0] != '@' && arg.Channel.Id != Global.bot_command_channel && arg.Channel.Id != Global.light_rp_admin_request))
            {
                return;
            }
            if (arg.Channel.Id == Global.light_rp_admin_request)
            {
                if (!arg.Content.ToString().ToLower().Contains(Global.accept_command))
                {
                    await arg.DeleteAsync();
                }
                else
                {
                    if (arg.Content.ToString().Split(' ').Length != 2)
                    {
                        await arg.DeleteAsync();
                    }
                    else
                    {
                        if (!int.TryParse(arg.Content.ToString().Split(' ')[1], out int id))
                        {
                            await arg.DeleteAsync();
                        }
                        else
                        {
                            await arg.DeleteAsync();
                            OFivePlugin.Request request = Global.TryAcceptRequest(id);
                            if (request != null)
                            {
                                if (Global.users.Where(x => x.DiscordId == arg.Author.Id).FirstOrDefault() != default)
                                {
                                    Global.users.Where(x => x.DiscordId == arg.Author.Id).FirstOrDefault().AddRecieveRequestCount();
                                    if (!Global.SaveUsers())
                                    {
                                        Global.DebugSystemError("Failed to save USER:\nName: " + arg.Author.Username + "\nDiscordId: " + arg.Author.Id);
                                    }
                                }
                                await arg.Channel.SendMessageAsync("Администратор " + arg.Author.Mention + " принял **Запрос № " + request.Id + "**" + " на сервер **" + request.ServerName + "** от игрока **" + request.SenderNickname + "**\n```" + request.DiscordMessage + "```");
                            }
                        }
                    }
                }
            }
            if (arg.Channel.Id == Global.bot_command_channel)
            {
                switch (arg.Content.ToString())
                {
                    case Global.CommandToWarnUser:
                        break;
                    case Global.CommandToUnWarnUser:
                        break;
                    case Global.CommandToGetSessions:
                        await arg.DeleteAsync();
                        foreach (string sessionInfo in Global.SessionsInfo())
                        {
                            Global.CommandAnswer(sessionInfo);
                        }
                        break;
                    case Global.CommandToGetOldSessions:
                        foreach (string oldSessionInfo in Global.OldSessionsInfo())
                        {
                            Global.CommandAnswer(oldSessionInfo);
                        }
                        break;
                    default:
                        await arg.DeleteAsync();
                        var temp = await arg.Channel.SendMessageAsync("Wrong command: '" + arg.Content.ToString() + "'");
                        await Task.Delay(3000);
                        await temp.DeleteAsync();
                        break;
                }
            }
            if (arg.Channel.ToString()[0] == '@')
            {
                if (arg.Content.ToString() == Global.CommandToGetPassword)
                {
                    if (Global.UserIsAdmin(arg.Author.Id) != null)
                    {
                        if (Global.GetUserById(arg.Author.Id) != default)
                        {
                            if (Global.WaitForJoinUsers.ContainsKey(Global.GetUserById(arg.Author.Id).Login))
                            {
                                await _client.GetUser(arg.Author.Id).SendMessageAsync("Временный пароль для входа на сайт: ||" + Global.WaitForJoinUsers[Global.GetUserById(arg.Author.Id).Login].Password + "||\nИстекает через " + Global.WaitForJoinUsers[Global.GetUserById(arg.Author.Id).Login].AliveTime + " секунд");
                            }
                        }
                    }
                }
                if (Global.WaitForVerifyUsers.Count > 0)
                {
                    if (Global.WaitForVerifyUsers.ContainsKey(arg.Author.Id))
                    {
                        if (Global.WaitForVerifyUsers.ContainsValue(arg.Content.ToString()))
                        {
                            if (Global.UserIsAdmin(arg.Author.Id) != null)
                            {
                                User user = new User()
                                {
                                    AccessLevel = Global.GetAccessLevel(Global.UserIsAdmin(arg.Author.Id)),
                                    Login = arg.Content.ToString(),
                                    DiscordId = arg.Author.Id,
                                    Name = Global.UserIsAdmin(arg.Author.Id).Nickname,
                                    DiscordMention = Global.UserIsAdmin(arg.Author.Id).Mention
                                };
                                Global.users.Add(user);
                                await _client.GetUser(arg.Author.Id).SendMessageAsync(Global.VerifyMessage + arg.Content.ToString());
                                if (Global.SaveUsers())
                                {
                                    Global.DebugSystem("Create and save USER:\nLogin: " + user.Login + "\nDiscord User: " + user.DiscordMention + "\nAccess Level:" + user.AccessLevel);
                                }
                                else
                                {
                                    Global.DebugSystemError("Failed to create and save USER:\nName: " + arg.Author.Username + "\nDiscord Id: " + arg.Author.Id);
                                }
                            }
                            else
                            {
                                await _client.GetUser(arg.Author.Id).SendMessageAsync(Global.FailedVerifyMessage);
                                Global.DebugError("Wrong user is trying to get access to " + Global.SiteName + "\nDiscord Name: " + arg.Author.Username + "\nDiscord Id: " + arg.Author.Id);
                            }
                            Global.WaitForVerifyUsers.Remove(arg.Author.Id);
                        }
                    }
                }
            }         
        }
        public static void Main(string[] args)
        {
            Thread thread = new Thread(ReloadWaitForVerifyAndJoinUsers);
            Thread thread1 = new Thread(CreateHostBuilder(args).Build().Run);
            Thread thread2 = new Thread(ReloadServers);
            Thread thread3 = new Thread(new Program().RunBotAsync().GetAwaiter().GetResult);
            Thread thread4 = new Thread(ReloadSessions);

            #region systemstart
            if (Global.IsOnline)
            {
                thread3.Start();
                thread4.Start();
                while (_client.GetGuild(Global.serverId) == null)
                {
                    Thread.Sleep(2000);
                }
                while (_client.GetGuild(Global.serverId).GetChannel(Global.bot_system_logs) as IMessageChannel == null)
                {
                    Thread.Sleep(2000);
                }
            }
            Global.DebugSystem("\n**------------------------------------**\n**Start new server...**\nSite version: " + Global.VersionSite + " Plugin version: " + Global.VersionPlugin);
            try
            {
                if (!System.IO.Directory.Exists(Global.GetDataPath()))
                {
                    System.IO.Directory.CreateDirectory(Global.GetDataPath());
                    Global.DebugSystemError("Create directory: " + Global.GetDataPath());
                }
            }
            catch (Exception)
            {
                Global.DebugShutdownError("Failed create directory: " + Global.GetDataPath());

            }
            if (Global.LoadUsers())
            {
                Global.DebugSystem("Users loaded: " + Global.users.Count);
                if (Global.SaveUsers())
                {
                    Global.DebugSystem("Success saved all users");
                }
                else
                {
                    Global.DebugSystemError("Failed to save all users");
                }
            }
            else
            {
                Global.DebugSystem("Users not loaded");
            }
            if (Global.LoadReasons())
            {
                Global.DebugSystem("Reasons loaded: " + Global.reasons.Count);
            }
            else
            {
                Global.DebugShutdownError("Reasons not loaded.");
            }
            if (Global.LoadSiteInfo())
            {
                Global.DebugSystem("SiteInfo loaded: GlobalBanCount: " + Global.InfoSite.BanCount + " GlobalRequestCount: " + Global.InfoSite.RequestCount);
            }
            else
            {
                Global.InfoSite = new OFivePlugin.SiteInfo()
                {
                    BanCount = 0,
                    RequestCount = 0
                };
                Global.DebugSystem("SiteInfo not loaded. Creating new...");
                if (!Global.SaveSiteInfo())
                {
                    Global.DebugSystemError("Failed to save site info");
                }
            }
            #endregion

            thread.Start();
            thread1.Start();
            thread2.Start();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        private static void ReloadWaitForVerifyAndJoinUsers()
        {
            while (Global.active)
            {
                while (Global.CurrentTimeToRefreshUsersJoin > 0)
                {
                    Thread.Sleep(1000);
                    Global.CurrentTimeToRefreshUsersJoin--;

                    for (int i = 0; i < Global.WaitForJoinUsers.Count; i++)
                    {
                        Global.WaitForJoinUsers.Values.ToArray()[i].AliveTime--;
                        if (Global.WaitForJoinUsers.Values.ToArray()[i].AliveTime <= 0)
                        {
                            Global.WaitForJoinUsers.Remove(Global.WaitForJoinUsers.Keys.ToArray()[i]);
                        }
                    }
                }
                Global.WaitForVerifyUsers.Clear();
                Global.CurrentTimeToRefreshUsersJoin = Global.TimeToRefreshUsersJoin;
            }
        }
        private static void ReloadServers()
        {
            while (Global.active)
            {
                Global.LoadRequests();
                Global.ReceptionReadyCommand();
                Global.RefreshServers();
                Thread.Sleep((int)(OFivePlugin.TimeComponent.timeIsUp * 1000));
            }
        }
        private static void ReloadSessions()
        {
            Dictionary<string, Session> SetToOld = new Dictionary<string, Session>();
            while (Global.active)
            {
                for (int i = 0; i < Global.Sessions.Count; i++)
                {
                    if (Global.UserIsAdmin(Global.Sessions.Values.ToArray()[i].User.DiscordId) != null)
                    {
                        Global.Sessions.Values.ToArray()[i].TickTime();
                        Global.Sessions.Values.ToArray()[i].User.SetAccessLevel(Global.GetAccessLevel(Global.UserIsAdmin(Global.Sessions.Values.ToArray()[i].User.DiscordId)));
                        Global.Sessions.Values.ToArray()[i].User.SetName(Global.UserIsAdmin(Global.Sessions.Values.ToArray()[i].User.DiscordId).Nickname);
                    }
                    else
                    {
                        SetToOld.Add(Global.Sessions.Keys.ToArray()[i], Global.Sessions.Values.ToArray()[i]);
                        Global.Sessions.Remove(Global.Sessions.Keys.ToArray()[i]);
                    }
                }

                foreach (KeyValuePair<string, Session> old in SetToOld)
                {
                    Global.OldSessions[old.Key] = old.Value;
                }
                SetToOld.Clear();
                Thread.Sleep(1000);
            }
        }
        private static void Baldeurik()
        {
            string message = "baldeurik start\n";
            foreach (SocketRole role in _client.GetGuild(Global.serverId).Roles)
            {
                if (role.Members.Count() == 0)
                {
                    message = message + role.Mention + " is empty\n";
                }
            }
            Global.DebugBaldeurik(message + "\nbaldeurik end");
        }
    }
}