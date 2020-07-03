using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFiveSite.Models;
using System;

namespace OFiveSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        public IndexModel IndexModel = new IndexModel();

        public string UserIp { get; set; }

        #region defaut

        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public ActionResult Index()
        {
            UserIp = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            IndexModel.UserIp = UserIp;
            if (!Global.IsOnline)
            {
                if (UserIp == Global.OwnerIp)
                {
                    Global.Sessions[UserIp] = Session.New(OFiveSite.User.Owner);
                    return Redirect("~/Main/Main");
                }
            }
            IndexModel.UserState = UserState.FirstPage;
            IndexModel.JoinState = JoinState.NotSend;
            return View(IndexModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult ReturnToMainPage(string UserIp, JoinState JoinState)
        {
            IndexModel.UserIp = UserIp;
            IndexModel.JoinState = JoinState;
            IndexModel.UserState = UserState.FirstPage;
            return View("Index", IndexModel);
        }

        #endregion

        #region joinreg
        private bool StringIsValid(string str)
        {
            return !string.IsNullOrEmpty(str) && !Regex.IsMatch(str, @"(^[^a-zA-Z_]|[^a-zA-Z\d_])");
        }

        private bool StringContainsNumbers(string str)
        {
            foreach (string number in Global.numbers)
            {
                if (str.Contains(number))
                    return true;
            }
            return false;
        }

        [HttpGet]
        public IActionResult Join(string login, string password, string UserIp, UserState UserState, JoinState JoinState)
        {
            IndexModel.UserIp = UserIp;
            IndexModel.JoinState = JoinState;
            IndexModel.UserState = UserState;
            IndexModel.Login = login;
            if (JoinState == JoinState.NotSend)
            {
                if (!Global.LoadUsers())
                {
                    IndexModel.Message = "Неверный логин/пароль (ошибка файла)";
                    Global.DebugSystemError("File error with: login: " + IndexModel.Login + " ip: " + IndexModel.UserIp);
                }
                else
                {
                    if (Global.users.Where(x => x.Login == login).FirstOrDefault() == default)
                    {
                        IndexModel.Message = "Неверный логин";
                    }
                    else
                    {
                        IndexModel.JoinState = JoinState.Send;
                        if (!Global.WaitForJoinUsers.ContainsKey(login))
                        {
                            string key = Global.GenerateKey();
                            Global.WaitForJoinUsers.Add(login, new Key(key, Global.TimeToRefreshUsersJoin));
                            Global.Debug("User is trying to log in:\nLogin: " + login + "\nPassword: " + key + "\nAccess Level: " + Global.users.Where(x => x.Login == login).FirstOrDefault().AccessLevel);
                        }
                    }
                }
            }
            if (JoinState == JoinState.Send)
            {
                if (!Global.LoadUsers())
                {
                    IndexModel.Message = "Неверный логин/пароль (ошибка файла 2)";
                    Global.DebugSystemError("File error 2 with: login: " + IndexModel.Login + " ip: " + IndexModel.UserIp);
                }
                else
                {
                    if (Global.WaitForJoinUsers.ContainsKey(login))
                    {
                        if (Global.WaitForJoinUsers[login].Password == password)
                        {
                            if (Global.users.Where(x => x.Login == login).FirstOrDefault() != default)
                            {
                                if (Global.UserIsAdmin(Global.users.Where(x => x.Login == login).FirstOrDefault().DiscordId) != null)
                                {
                                    Global.users.Where(x => x.Login == login).First().SetAccessLevel(Global.GetAccessLevel(Global.UserIsAdmin(Global.users.Where(x => x.Login == login).FirstOrDefault().DiscordId)));
                                    Global.users.Where(x => x.Login == login).First().SetName(Global.UserIsAdmin(Global.users.Where(x => x.Login == login).FirstOrDefault().DiscordId).Username);

                                    Global.Sessions[UserIp] = Session.New(Global.users.Where(x => x.Login == login).FirstOrDefault());
                                    
                                    IndexModel.JoinState = JoinState.Send;
                                    IndexModel.UserState = UserState.Join;
                                    IndexModel.Password = password;
                                    Global.Debug("User logged in: \nDiscord user: " + Program._client.GetUser(Global.users.Where(x => x.Login == login).First().DiscordId).Mention + " \nBan Count: " + Global.users.Where(x => x.Login == login).First().BanCount + " \nWarn Count: " + Global.users.Where(x => x.Login == login).First().WarnCount + "\nAccess Level: " + Global.users.Where(x => x.Login == login).First().AccessLevel);
                                    return Redirect("~/Main/Main");
                                }
                            }
                            IndexModel.JoinState = JoinState.Send;
                            IndexModel.UserState = UserState.Join;
                            IndexModel.Password = password;
                            IndexModel.Message = "У вас нет доступа для входа в аккаунт";
                            Global.DebugError("Wrong user try to get access to " + Global.SiteName + ":\nLogin: " + IndexModel.Login + "\nDiscordId: " + Global.users.Where(x => x.Login == login).First().DiscordId);
                        }
                        else
                        {
                            IndexModel.Message = "Неверный логин/пароль";
                        }
                    }
                    else
                    {
                        IndexModel.Message = "Неверный логин/пароль или Ваш пароль истек";
                    }
                }
            }
            return View("Index", IndexModel);
        }

        [HttpGet]
        public ActionResult Register(string discordid, string login, string UserIp, JoinState JoinState)
        {
            IndexModel.UserIp = UserIp;
            IndexModel.JoinState = JoinState;
            IndexModel.UserState = UserState.TryRegister;
            IndexModel.Login = login;
            IndexModel.DiscordId = discordid;
            if (!ulong.TryParse(discordid, out ulong id))
            {
                IndexModel.Message = "Неверный discord ID";
            }
            else if (login.Count() < 6 || login.Count() > 20)
            {
                IndexModel.Message = "Логин вашего аккаунта должен быть от 6 до 20 символов";
            }
            else
            {
                if (StringIsValid(login) && !StringContainsNumbers(login))
                {
                    if (Global.GetUserById(id) != default)
                    {
                        IndexModel.Message = "Аккаунт с таким discordID уже зарегистрирован";
                    }
                    else if (Global.WaitForVerifyUsers.Keys.Contains(id))
                    {
                        IndexModel.Message = "Аккаунт с таким discordID уже подал заявку на регистрацию";
                    }
                    else
                    {
                        if (Global.GetUserByLogin(login) != default)
                        {
                            IndexModel.Message = "Аккаунт с таким логином уже зарегистрирован";
                        }
                        else if (Global.WaitForVerifyUsers.Values.Contains(login))
                        {
                            IndexModel.Message = "Аккаунт с таким логином уже подал заявку на регистрацию";
                        }
                        else
                        {
                            Global.WaitForVerifyUsers.Add(id, login);
                            IndexModel.Message = "Для подтверждения дискорд аккаунта, отправьте боту свой логин:" + login + "\nЗаявка на регистрацию удалится автоматически через " + Global.CurrentTimeToRefreshUsersJoin + " секунд";
                            Global.Debug("User is trying to register:\nid: " + id + "\nlogin: " + login);
                        }
                    }
                }
                else
                {
                    IndexModel.Message = "Логин имеет недопустимые символы";
                }

            }
            return View("Index", IndexModel);
        }

        #endregion

        #region firstpage

        [HttpGet]
        public IActionResult TryRegister(string UserIp, JoinState JoinState)
        {
            IndexModel.UserIp = UserIp;
            IndexModel.JoinState = JoinState;
            IndexModel.UserState = UserState.TryRegister;
            return View("Index", IndexModel);
        }

        [HttpGet]
        public IActionResult TryJoin(string UserIp, JoinState JoinState)
        {
            IndexModel.UserIp = UserIp;
            IndexModel.JoinState = JoinState;
            IndexModel.UserState = UserState.TryJoin;
            return View("Index", IndexModel);
        }

        [HttpGet]
        public IActionResult JoinAsGuest(string UserIp, JoinState JoinState)
        {
            if (!Global.Sessions.ContainsKey(UserIp))
            {
                Global.Sessions[UserIp] = Session.New(OFiveSite.User.Guest);
            }
            IndexModel.UserIp = UserIp;
            IndexModel.JoinState = JoinState;
            IndexModel.UserState = UserState.Guest;
            return View("Index", IndexModel);
        }

        #endregion

    }
}
