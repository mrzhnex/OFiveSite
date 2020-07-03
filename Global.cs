using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using OFivePlugin;
using Discord;
using OFiveSite.Models;

namespace OFiveSite
{
    public static class Global
    {
        /// <summary>
        //0.1) переделать все МОГ (захвата, сдерживания) в МОГ альфа-1, Апполон-3 и т.д.
        //1) утвердить альфу13
        //1.2)утвердить добавление спонсоров, эпсилонов, гмов (+ распределение под уровни допуска)
        //2)личный кабинет, доступность и т.д.
        //3)скрытые варны
        //4)кастомные команды
        //5)bbc - надо ли "сохранять текст" при перезагрузке (именно перезагрузке) страницы
        //6)выход smod
        //7)новая директория на хостинге (etc/ofivesite) для хранения randomnames.txt и staticnames.txt (и всего остального, что касается плагинов)
        //8)разобраться с font style в bbc
        //9)добавление разных фич на сайт (визуальных + полезных)
        //10)кто когда и как именно будет делать "сайт для всех" и как он будет связан с админ-сайтом
        //11)сайт с логами
        //11.2)если не планируется сайт с логами в ближайшее время - скинуть жалобы с логами (из дисмиссала) на меня
        /// </summary>
        #region main
        public static bool IsOnline = true;
        public static readonly bool IsHost = true;

        public static readonly string VersionPlugin = OFivePlugin.Global.Version;
        public static readonly string VersionSite = "0.F.1.1"; //worked (without font "style:")

        //2)
        //Сделать всплывающее окно при реквесте (on DebugRequest(message))
        //синхронизировать поток данных реквеста (дабы не было двойных принятий реквеста разными людьми)::::
        //::::при повторной проверке waitforreqeust.contains(request) != выводить, что реквест уже принят кем то другим
        //протестировать работоспособность

        public static readonly string OwnerIp = "0.0.0.1";
        public static string GetDataPath()
        {
            if (IsHost)
                return OFivePlugin.Global.hostglobalpath;
            else
                return OFivePlugin.Global.debugglobalpath;
        }
        #endregion

        #region servers
        public static List<ServerInfo> servers = new List<ServerInfo>();

        private static readonly string ServerInfoFileName = "ServerInfo.xml";
        private static readonly string WrongServerName = "Обновление данных...";

        private static readonly Dictionary<string, string> ServerInfoFiles = new Dictionary<string, string>()
        {
            { "My Server", Path.Combine("D://Scp Sl server//SCP Secret Laboratory") },
            { "Full RP", Path.Combine("//home//scpsl8//scpsl_server//") },
            { "Light RP 1", Path.Combine("//home//scpsl1//scpsl_server//") },
            { "Light RP 2", Path.Combine("//home//scpsl2//scpsl_server//") },
            { "Light RP 3", Path.Combine("//home//scpsl3//scpsl_server//") },
            { "Light RP 4", Path.Combine("//home//scpsl4//scpsl_server//") },
            { "Light RP 5", Path.Combine("//home//scpsl5//scpsl_server//") },
            { "Light RP 6", Path.Combine("//home//scpsl6//scpsl_server//") },
            { "Light RP Event", Path.Combine("//home//scpsl7//scpsl_server//") },
            { "Light RP Lightsout", Path.Combine("//home//scpsl9//scpsl_server//") },
            { "Non RP 1", Path.Combine("//home//scpsl10//scpsl_server//") },
            { "Non RP 2", Path.Combine("//home//scpsl11//scpsl_server//") }
        };

        private static readonly XmlSerializer Formatter_Commands = new XmlSerializer(typeof(List<ReadyCommand>));

        public static string SendCommandToReady(ReadyCommand command, string server)
        {
            if (server == WrongServerName)
            {
                return "Сервер обновляет данные. Команда не отправлена";
            }
            List<ReadyCommand> commands = new List<ReadyCommand>();
            if (File.Exists(Path.Combine(ServerInfoFiles[server], OFivePlugin.Global.ReadyCommandsFileName)))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(Path.Combine(ServerInfoFiles[server], OFivePlugin.Global.ReadyCommandsFileName)))
                    {
                        commands = (List<ReadyCommand>)Formatter_Commands.Deserialize(sr);
                    }
                }
                catch (InvalidOperationException)
                {
                    return "Произошла техническая ошибка. Пожалуйста, сообщите об этом руководству.";
                }
            }
            commands.Add(command);
            try
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(ServerInfoFiles[server], OFivePlugin.Global.ReadyCommandsFileName)))
                {
                    Formatter_Commands.Serialize(sw, commands);
                }
                return "Команда успешно послана на сервер";
            }
            catch (InvalidOperationException)
            {
                return "Произошла техническая ошибка (2). Пожалуйста, сообщите об этом руководству.";
            }
        }

        public static void ReceptionReadyCommand()
        {
            foreach (KeyValuePair<string, string> kvp in ServerInfoFiles)
            {
                if (!File.Exists(Path.Combine(kvp.Value, OFivePlugin.Global.ResultReadyCommandsFileName)))
                {
                    continue;
                }
                List<ReadyCommand> readyCommands = new List<ReadyCommand>();
                try
                {
                    using (FileStream fs = new FileStream(Path.Combine(kvp.Value, OFivePlugin.Global.ResultReadyCommandsFileName), FileMode.Open))
                    {
                        readyCommands = (List<ReadyCommand>)Formatter_Commands.Deserialize(fs);
                    }
                    foreach (ReadyCommand readyCommand in readyCommands)
                    {
                        if (readyCommand.SuccessDebugMessage != string.Empty)
                        {
                            switch (readyCommand.CommandName.ToLower())
                            {
                                case "ban":
                                    if (readyCommand.IsSuccess)
                                    {
                                        users.Where(x => x.DiscordId == readyCommand.SenderDiscordId).First().AddBanCount();
                                        BanOutput(readyCommand.SuccessDebugMessage);
                                    }
                                    else
                                    {
                                        Debug(readyCommand.FailedDebugMessage);
                                    }
                                    break;
                                case "oban":
                                    users.Where(x => x.DiscordId == readyCommand.SenderDiscordId).First().AddBanCount();
                                    if (readyCommand.IsSuccess)
                                    {
                                        BanOutput(readyCommand.SuccessDebugMessage);
                                    }
                                    else
                                    {
                                        BanOutput(readyCommand.FailedDebugMessage);
                                    }
                                    break;
                                case "unban":
                                    if (readyCommand.IsSuccess)
                                    {
                                        users.Where(x => x.DiscordId == readyCommand.SenderDiscordId).First().RemoveBanCount();
                                        UnBanOutput(readyCommand.SuccessDebugMessage);
                                    }
                                    else
                                    {
                                        Debug(readyCommand.FailedDebugMessage);
                                    }
                                    break;
                                case "bc":
                                    if (readyCommand.IsSuccess)
                                    {
                                        DebugBroadcast(readyCommand.SuccessDebugMessage);
                                    }
                                    else
                                    {
                                        DebugError(readyCommand.FailedDebugMessage);
                                    }
                                    break;
                                case "pbc":
                                    if (readyCommand.IsSuccess)
                                    {
                                        DebugBroadcast(readyCommand.SuccessDebugMessage);
                                    }
                                    else
                                    {
                                        DebugError(readyCommand.FailedDebugMessage);
                                    }
                                    break;
                                case "tbc":
                                    if (readyCommand.IsSuccess)
                                    {
                                        DebugBroadcast(readyCommand.SuccessDebugMessage);
                                    }
                                    else
                                    {
                                        DebugError(readyCommand.FailedDebugMessage);
                                    }
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (File.Exists(Path.Combine(kvp.Value, OFivePlugin.Global.ResultReadyCommandsFileName)))
                        File.Delete(Path.Combine(kvp.Value, OFivePlugin.Global.ResultReadyCommandsFileName));
                    DebugSystemError("Failed output command result by: " + ex.Message);
                }
            }
        }

        private static readonly XmlSerializer Formatter_ServerInfo = new XmlSerializer(typeof(ServerInfo));

        public static void RefreshServers()
        {
            List<ServerInfo> temp = new List<ServerInfo>();
            foreach (KeyValuePair<string, string> kvp in ServerInfoFiles)
            {
                ServerInfo serverInfo = new ServerInfo();
                try
                {
                    using (FileStream fs = new FileStream(Path.Combine(kvp.Value, ServerInfoFileName), FileMode.Open))
                    {
                        serverInfo = (ServerInfo)Formatter_ServerInfo.Deserialize(fs);
                        serverInfo.ServerName = kvp.Key;
                        serverInfo.Output = serverInfo.ServerName + " (admins: " + serverInfo.CurrentAdmins + " players: " + serverInfo.CurrentPlayers + " max: " + serverInfo.MaxPlayers + ")";
                    }
                }
                catch (Exception)
                {
                    serverInfo = new ServerInfo()
                    {
                        ServerName = WrongServerName,
                        players = new List<Player>() { new Player("Server", 0, "0", "Server", 0, 0) },
                        CurrentAdmins = 1,
                        CurrentPlayers = 1,
                        MaxPlayers = 1,
                        Output = WrongServerName
                    };
                    if (File.Exists(Path.Combine(kvp.Value, ServerInfoFileName)))
                        File.Delete(Path.Combine(kvp.Value, ServerInfoFileName));
                }
                temp.Add(serverInfo);
            }
            servers.Clear();
            servers.AddRange(temp);
        }

        #endregion

        #region generatekey
        public static Random rand = new Random();
        public static int key_lenght = 8;
        public static List<string> symbols = new List<string>()
        {
            "0","1","2","3","4","5","6","7","8","9",
            "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","r","s","t","u","y","v","w","x","z",
        };

        public static List<string> numbers = new List<string>()
        {
            "0","1","2","3","4","5","6","7","8","9"
        };

        public static string GenerateKey()
        {
            string key = string.Empty;
            for (int i = 0; i < key_lenght; i++)
            {
                key += symbols[rand.Next(0, symbols.Count)];
            }
            return key;
        }
        #endregion

        #region discordbot
        public static readonly string botToken = "Bot token was here";
        public static readonly ulong serverId = 351766372264574976;
        
        public static readonly string VerifyMessage = "Ваш аккаунт на сайте " + SiteName + " успешно подтвержден. Логин для входа на сайт:";
        public static readonly string FailedVerifyMessage = "У Вас нет доступа к этому действию.";

        public static bool active = true;
        private static readonly XmlSerializer Formatter_InfoSite = new XmlSerializer(typeof(SiteInfo));
        private static readonly string InfoSite_Filename = Path.Combine(GetDataPath(), "InfoSite.xml");
        public static SiteInfo InfoSite { get; set; }
        public static bool LoadSiteInfo()
        {
            if (!File.Exists(InfoSite_Filename))
            {
                return false;
            }
            try
            {
                using (FileStream fs = new FileStream(InfoSite_Filename, FileMode.Open))
                {
                    InfoSite = (SiteInfo)Formatter_InfoSite.Deserialize(fs);
                }
                return true;
            }
            catch (InvalidOperationException)
            {
                if (!File.Exists(InfoSite_Filename))
                {
                    File.Delete(InfoSite_Filename);
                }
                return false;
            }
        }
        public static bool SaveSiteInfo()
        {
            try
            {
                using (FileStream fs = new FileStream(InfoSite_Filename, FileMode.OpenOrCreate))
                {
                    Formatter_InfoSite.Serialize(fs, InfoSite);
                }
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
        #endregion

        #region sessions
        public static Dictionary<string, Session> Sessions = new Dictionary<string, Session>();
        public static Dictionary<string, Session> OldSessions = new Dictionary<string, Session>();

        public static User GetUserById(ulong id)
        {
            return users.Where(x => x.DiscordId == id).FirstOrDefault();
        }
        public static User GetUserByLogin(string login)
        {
            return users.Where(x => x.Login == login).FirstOrDefault();
        }

        public static Dictionary<string, Key> WaitForJoinUsers = new Dictionary<string, Key>();
        public static int CurrentTimeToRefreshUsersJoin = 180;
        public static readonly int TimeToRefreshUsersJoin = 180;
        
        public static Discord.WebSocket.SocketGuildUser UserIsAdmin(ulong id)
        {
            if (IsOnline)
            {
                if (Program._client.GetGuild(serverId).GetUser(id) != null)
                {
                    foreach (KeyValuePair<int, List<ulong>> kvp in AccessLevelDiscordRoles)
                    {
                        if (kvp.Key < AdminAccessLevel)
                        {
                            continue;
                        }
                        if (Program._client.GetGuild(serverId).GetUser(id).Roles.Where(x => kvp.Value.Contains(x.Id)).FirstOrDefault() != default)
                        {
                            return Program._client.GetGuild(serverId).GetUser(id);
                        }
                    }
                }
            }
            return null;
        }
        public static int GetAccessLevel(Discord.WebSocket.SocketGuildUser user)
        {
            foreach (KeyValuePair<int, List<ulong>> kvp in AccessLevelDiscordRoles)
            {
                if (user.Roles.Where(x => kvp.Value.Contains(x.Id)).FirstOrDefault() != default)
                {
                    return kvp.Key;
                }
            }
            return 0;
        }
        #endregion

        #region debug
        public static readonly string SiteName = "Сайт проекта SCP:SL Roleplay";

        public static readonly ulong bot_logs = 639060639070814208;
        public static readonly ulong bot_system_logs = 644212616675328000;
        public static readonly ulong bot_broadcast_logs = 644753565029498890;
        
        public static readonly ulong bot_errors = 639060592178757656;
        public static readonly ulong bot_system_errors = 644212576376324096;
        public static readonly ulong bot_shutdown_errors = 644212648896102430;


        public static readonly ulong bot_ban_warn_kick = 636600998864879646;
        public static readonly ulong bot_unban_unwarn = 642366398412095523;

        public static readonly ulong bot_command_channel = 648546943374458881;
        public static readonly ulong light_rp_admin_request = 648546315499864064;
        public static readonly ulong bot_request_logs = 648557322561519616;

        #region debugmethods
        public static void Debug(string message)
        {
            if (IsOnline)
                (Program._client.GetGuild(serverId).GetChannel(bot_logs) as IMessageChannel).SendMessageAsync("DateTime: " + DateTime.Now + "; Message: " + message + "\n");
            else
                Console.WriteLine(message);
        }
        public static void DebugSystem(string message)
        {
            if (IsOnline)
                (Program._client.GetGuild(serverId).GetChannel(bot_system_logs) as IMessageChannel).SendMessageAsync("DateTime: " + DateTime.Now + "; Message: " + message + "\n");
            else
                Console.WriteLine(message);
        }
        public static void DebugBroadcast(string message)
        {
            if (IsOnline)
                (Program._client.GetGuild(serverId).GetChannel(bot_broadcast_logs) as IMessageChannel).SendMessageAsync("DateTime: " + DateTime.Now + "; Message: " + message + "\n");
            else
                Console.WriteLine(message);
        }
        public static void DebugRequest(string message)
        {
            if (IsOnline)
                (Program._client.GetGuild(serverId).GetChannel(bot_request_logs) as IMessageChannel).SendMessageAsync("DateTime: " + DateTime.Now + "; Message: " + message + "\n");
            else
                Console.WriteLine(message);
        }

        public static void DebugError(string message)
        {
            if (IsOnline)
                (Program._client.GetGuild(serverId).GetChannel(bot_errors) as IMessageChannel).SendMessageAsync("DateTime: " + DateTime.Now + "; Message: " + message + "\n");
            else
                Console.WriteLine(message);
        }
        public static void DebugSystemError(string message)
        {
            if (IsOnline)
                (Program._client.GetGuild(serverId).GetChannel(bot_system_errors) as IMessageChannel).SendMessageAsync("DateTime: " + DateTime.Now + "; Message: " + message + "\n");
            else
                Console.WriteLine(message);
        }
        public static void DebugShutdownError(string message)
        {
            if (IsOnline)
                (Program._client.GetGuild(serverId).GetChannel(bot_shutdown_errors) as IMessageChannel).SendMessageAsync("DateTime: " + DateTime.Now + "; Message: " + message + "\n**Site shutdown...**\n");
            else
                Console.WriteLine(message +"\n**Site shutdown...**\n");
            Environment.Exit(77);
        }

        public static void BanOutput(string message)
        {
            if (IsOnline)
                (Program._client.GetGuild(serverId).GetChannel(bot_ban_warn_kick) as IMessageChannel).SendMessageAsync(message);
            else
                Console.WriteLine(message);
        }
        public static void UnBanOutput(string message)
        {
            if (IsOnline)
                (Program._client.GetGuild(serverId).GetChannel(bot_unban_unwarn) as IMessageChannel).SendMessageAsync(message);
            else
                Console.WriteLine(message);
        }
        public static IUserMessage RequestOutput(string message)
        {
            if (IsOnline)
                return (Program._client.GetGuild(serverId).GetChannel(light_rp_admin_request) as IMessageChannel).SendMessageAsync(message).Result;
            else
                Console.WriteLine(message);
            return null;
        }
        
        public static void DebugBaldeurik(string message)
        {
            if (IsOnline)
                (Program._client.GetGuild(serverId).GetChannel(651481328482517003) as IMessageChannel).SendMessageAsync(message);
            else
                Console.WriteLine(message);
        }
        #endregion

        #endregion

        #region broadcast
        public static readonly List<Team> teams = new List<Team>()
        {
            new Team("Наблюдатели", 1, Smod2.API.Team.SPECTATOR),
            new Team("Класс Д", 2, Smod2.API.Team.CLASSD),
            new Team("Ученые", 3, Smod2.API.Team.SCIENTIST),
            new Team("МОГ и охрана", 4, Smod2.API.Team.NINETAILFOX),
            new Team("Повстанцы хаоса", 5, Smod2.API.Team.CHAOS_INSURGENCY),
            new Team("СЦП объекты", 6, Smod2.API.Team.SCP),
            new Team("Обучение", 7, Smod2.API.Team.TUTORIAL)
        };

        public static readonly List<Broadcast> broadcasts = new List<Broadcast>()
        {
            new Broadcast("Обычный", BroadcastType.Bc),
            new Broadcast("Персональный", BroadcastType.Pbc),
            new Broadcast("Командный", BroadcastType.Tbc)
            ///добавить остальные
        };

        public static readonly Dictionary<BroadcastType, string> BroadcastCommands = new Dictionary<BroadcastType, string>()
        {
            {BroadcastType.Bc, "bc" },
            {BroadcastType.Pbc, "pbc" },
            {BroadcastType.Tbc, "tbc" },
            {BroadcastType.Dbc, "dbc" },
            {BroadcastType.Sbc, "sbc" },
            {BroadcastType.Gbc, "gbc" }
        };
        #endregion

        #region commands
        public const string CommandPrefix = "!";
        public static readonly string CommandToGetPassword = CommandPrefix + "getpassword";
        public static readonly string accept_command = CommandPrefix + "accept";
        
        //admin command
        public const string CommandToGetSessions = CommandPrefix + "sessions";
        public const string CommandToGetOldSessions = CommandPrefix + "oldsessions";
        public const string CommandToWarnUser = CommandPrefix + "warn";
        public const string CommandToUnWarnUser = CommandPrefix + "unwarn";
        public static readonly int DiscordMessageMaxLenght = 2000;

        public static void CommandAnswer(string message)
        {
            if (IsOnline)
                (Program._client.GetGuild(serverId).GetChannel(bot_command_channel) as IMessageChannel).SendMessageAsync(message);
            else
                Console.WriteLine(message);
        }
        public static string GetTimeBySeconds(ulong seconds)
        {
            ulong minutes = 0;
            ulong hours = 0;
            ulong days = 0;

            while (seconds > 59)
            {
                seconds -= 60;
                minutes++;
                if (minutes - 60 >= 0)
                {
                    minutes -= 60;
                    hours++;
                }
                if (hours - 24 >= 0)
                {
                    hours -= 24;
                    days++;
                }
            }
            return "'" + days + " дней " + hours + " часов " + minutes + " минут " + seconds + " секунд'";
        }
        public static List<string> OldSessionsInfo()
        {
            List<string> messages = new List<string>();
            string message = "**Old Session Info (Unic sessions - " + OldSessions.Count + "):**";
            foreach (KeyValuePair<string, Session> session in OldSessions)
            {
                try
                {
                    if ((message + "\nIp: " + session.Key + " SessionTime: " + GetTimeBySeconds(session.Value.Time) + " Login: " + session.Value.User.Login + " AccessLevel: " + session.Value.User.AccessLevel + " DiscordUser: " + UserIsAdmin(session.Value.User.DiscordId).Mention + " BanCount: " + session.Value.User.BanCount + " WarnCount: " + session.Value.User.WarnCount).Length > DiscordMessageMaxLenght)
                    {
                        messages.Add(message);
                        message = string.Empty;
                    }
                    message = message + "*\nIp: " + session.Key + " SessionTime: " + GetTimeBySeconds(session.Value.Time) + " Login: " + session.Value.User.Login + " AccessLevel: " + session.Value.User.AccessLevel + " DiscordUser: " + UserIsAdmin(session.Value.User.DiscordId).Mention + " BanCount: " + session.Value.User.BanCount + " WarnCount: " + session.Value.User.WarnCount;
                }
                catch (NullReferenceException)
                {
                    DebugSystemError("Catch exception: discord user not in an server or not an admin: " + session.Value.User.DiscordId);
                    if ((message + "\nIp: " + session.Key + " SessionTime: " + GetTimeBySeconds(session.Value.Time) + " Login: " + session.Value.User.Login + " AccessLevel: " + session.Value.User.AccessLevel + " DiscordUser: *Отсутствует на сервере* BanCount: " + session.Value.User.BanCount + " WarnCount: " + session.Value.User.WarnCount).Length > DiscordMessageMaxLenght)
                    {
                        messages.Add(message);
                        message = string.Empty;
                    }
                    message = message + "*\nIp: " + session.Key + " SessionTime: " + GetTimeBySeconds(session.Value.Time) + " Login: " + session.Value.User.Login + " AccessLevel: " + session.Value.User.AccessLevel + " DiscordUser: *Отсутствует на сервере* BanCount: " + session.Value.User.BanCount + " WarnCount: " + session.Value.User.WarnCount;
                }
            }
            messages.Add(message);
            return messages;
        }
        public static List<string> SessionsInfo()
        {
            List<string> messages = new List<string>();
            string message = "**Session Info (Unic sessions - " + Sessions.Count + "):**";
            foreach (KeyValuePair<string, Session> session in Sessions)
            {
                try
                {
                    if ((message + "\nIp: " + session.Key + " SessionTime: " + GetTimeBySeconds(session.Value.Time) + " Login: " + session.Value.User.Login + " AccessLevel: " + session.Value.User.AccessLevel + " DiscordUser: " + UserIsAdmin(session.Value.User.DiscordId).Mention + " BanCount: " + session.Value.User.BanCount + " WarnCount: " + session.Value.User.WarnCount).Length > DiscordMessageMaxLenght)
                    {
                        messages.Add(message);
                        message = string.Empty;
                    }
                    message = message + "*\nIp: " + session.Key + " SessionTime: " + GetTimeBySeconds(session.Value.Time) + " Login: " + session.Value.User.Login + " AccessLevel: " + session.Value.User.AccessLevel + " DiscordUser: " + UserIsAdmin(session.Value.User.DiscordId).Mention + " BanCount: " + session.Value.User.BanCount + " WarnCount: " + session.Value.User.WarnCount;
                }
                catch (NullReferenceException)
                {
                    DebugSystemError("Catch exception: discord user not in an server or not an admin: " + session.Value.User.DiscordId);
                    if ((message + "\nIp: " + session.Key + " SessionTime: " + GetTimeBySeconds(session.Value.Time) + " Login: " + session.Value.User.Login + " AccessLevel: " + session.Value.User.AccessLevel + " DiscordUser: *Отсутствует на сервере* BanCount: " + session.Value.User.BanCount + " WarnCount: " + session.Value.User.WarnCount).Length > DiscordMessageMaxLenght)
                    {
                        messages.Add(message);
                        message = string.Empty;
                    }
                    message = message + "*\nIp: " + session.Key + " SessionTime: " + GetTimeBySeconds(session.Value.Time) + " Login: " + session.Value.User.Login + " AccessLevel: " + session.Value.User.AccessLevel + " DiscordUser: *Отсутствует на сервере* BanCount: " + session.Value.User.BanCount + " WarnCount: " + session.Value.User.WarnCount;
                }
            }
            messages.Add(message);
            return messages;
        }
        #endregion

        #region request
        public static XmlSerializer Formatter_Requests = new XmlSerializer(typeof(List<Request>));
        public static List<Request> WaitingRequests = new List<Request>();
        public static bool LoadRequests()
        {
            foreach (KeyValuePair<string, string> kvp in ServerInfoFiles)
            {
                if (!File.Exists(Path.Combine(kvp.Value, OFivePlugin.Global.requests_file_name)))
                {
                    continue;
                }
                List<Request> requests = new List<Request>();
                try
                {
                    using (FileStream fs = new FileStream(Path.Combine(kvp.Value, OFivePlugin.Global.requests_file_name), FileMode.Open))
                    {
                        requests = (List<Request>)Formatter_Requests.Deserialize(fs);
                    }
                }
                catch (Exception ex)
                {
                    DebugSystemError("Failed send request by: " + ex.Message);
                }
                foreach (Request request in requests)
                {
                    request.ServerName = kvp.Key;
                    IUserMessage temp = RequestOutput(CreateRequestMessage(request.Id, request.Reason, request.SenderNickname, kvp.Key) + GetAllAdministatorsDiscordMention());
                    DebugRequest(CreateRequestMessage(request.Id, request.Reason, request.SenderNickname, kvp.Key));
                    request.DiscordMessage = CreateRequestMessage(request.Id, request.Reason, request.SenderNickname, kvp.Key);
                    request.MessageId = temp.Id;
                }
                WaitingRequests.AddRange(requests);
            }
            return false;
        }
        private static string CreateRequestMessage(int id, string reason, string player, string server)
        {
            return "**Запрос номер " + id + "**\n1) " + player + "\n2) " + server + "\n3) " + reason;
        }
        public static Request TryAcceptRequest(int id)
        {
            if (WaitingRequests.Where(x => x.Id == id).FirstOrDefault() == default)
            {
                return null;
            }
            Request request = WaitingRequests.Where(x => x.Id == id).FirstOrDefault();
            Program._client.GetGuild(serverId).GetTextChannel(light_rp_admin_request).GetMessageAsync(WaitingRequests.Where(x => x.Id == id).FirstOrDefault().MessageId).Result.DeleteAsync();
            WaitingRequests.Remove(WaitingRequests.Where(x => x.Id == id).FirstOrDefault());
            return request;
        }   
        public static bool OnlineTryAcceptRequest(int id, ulong discordId)
        {
            if (TryAcceptRequest(id) == null)
            {
                return false;
            }
            else
            {
                if (UserIsAdmin(discordId) == null)
                {
                    return false;
                }
                RequestOutput(UserIsAdmin(discordId).Mention + " принял через сайт запрос с номером " + id);
                users.Where(x => x.DiscordId == discordId).FirstOrDefault().AddRecieveRequestCount();
                if (!SaveUsers())
                {
                    DebugSystemError("Failed to save USER:\nName: " + users.Where(x => x.DiscordId == discordId).FirstOrDefault().Name + "\nDiscordId: " + users.Where(x => x.DiscordId == discordId).FirstOrDefault().DiscordId);
                }
                return true;
            }
        }
        private static string GetAllAdministatorsDiscordMention()
        {
            string mention = "\n";
            foreach (ulong id in AdminRolesId)
            {
                if (Program._client.GetGuild(serverId).GetRole(id) == null)
                {
                    DebugSystemError("Request admin id " + id + " not found");
                }
                mention = mention + Program._client.GetGuild(serverId).GetRole(id).Mention + " ";
            }
            return mention;
        }
        private static readonly List<ulong> AdminRolesId = new List<ulong>()
        {
            500750905000591371, //мог захвата
            500757073848434689, //мог отслеживания
            498563720935637002, //мог сдерживания
            500755445502181376, //мог конспирации
        };
        #endregion

        #region loaded_data

        #region reasons
        public static List<Reason> reasons = new List<Reason>();
        public static string reasons_file_name = Path.Combine(GetDataPath(), "reasons.xml");
        public static XmlSerializer Formatter_Reasons = new XmlSerializer(typeof(List<Reason>));
        public static bool LoadReasons()
        {
            if (!File.Exists(reasons_file_name))
            {
                return false;
            }
            try
            {
                using (FileStream fs = new FileStream(reasons_file_name, FileMode.Open))
                {
                    reasons = (List<Reason>)Formatter_Reasons.Deserialize(fs);
                }
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
        #endregion

        #region users
        public static List<User> users = new List<User>();
        public static Dictionary<ulong, string> WaitForVerifyUsers = new Dictionary<ulong, string>();
        public static string users_file_name = Path.Combine(GetDataPath(), "users.xml");
        public static XmlSerializer Formatter_Users = new XmlSerializer(typeof(List<User>));
        public static bool LoadUsers()
        {
            if (!File.Exists(users_file_name))
            {
                return false;
            }
            List<User> tempUsers = new List<User>();
            try
            {
                using (FileStream fs = new FileStream(users_file_name, FileMode.OpenOrCreate))
                {
                    foreach (User user in (List<User>)Formatter_Users.Deserialize(fs))
                    {
                        if (!tempUsers.Contains(user))
                        {
                            tempUsers.Add(user);
                        }
                    }
                }
                users.Clear();
                users.AddRange(tempUsers);
                return true;
            }
            catch (InvalidOperationException)
            {
                if (File.Exists(users_file_name))
                {
                    File.Delete(users_file_name);
                }
                return false;
            }
        }
        public static bool SaveUsers()
        {
            if (File.Exists(users_file_name))
            {
                File.Delete(users_file_name);
            }
            try
            {
                using (FileStream fs = new FileStream(users_file_name, FileMode.OpenOrCreate))
                {
                    Formatter_Users.Serialize(fs, users);
                }
                return true;
            }
            catch (InvalidOperationException)
            {
                if (File.Exists(users_file_name))
                {
                    File.Delete(users_file_name);
                }
                return false;
            }
        }
        #endregion

        #endregion

        #region custom commands
        public static readonly Dictionary<Command, CommandInfo> CommandInfo = new Dictionary<Command, CommandInfo>()
        {
            {new TranqillMaster(), new CommandInfo("Транквилизатор", "Транквилизатор используется для усыпления жертвы", 3) }
        };
        public static readonly int AdminAccessLevel = 2;
        public static readonly Dictionary<int, List<ulong>> AccessLevelDiscordRoles = new Dictionary<int, List<ulong>>()
        {
            {5, new List<ulong>(){ 434261615467036673, 603337712488939541 } }, //класс А
            {4, new List<ulong>(){ 500749870953660426, 447805014686302209, 498563720935637002 } }, //командиры/гмы/инженеры        
            {3, new List<ulong>(){ 434261263283912714, 500750905000591371 } }, //спонсоры/эпсилоны/директоры        
            {2, new List<ulong>(){ 500757073848434689, 435747850328997889 } }, //администрация   
            {1, new List<ulong>(){ 492054077216129034, 464313428261994496 } }, //персонал комплекса/сотрудники на пенсии       
            {0, new List<ulong>(){ 434262941681057796 } }, //класс д
        };
        #endregion
    }
}