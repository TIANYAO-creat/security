using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;

namespace ApiToGuardSystem.Controllers
{
    public class RegisterController : ApiController
    {
        private GuardSystemEntities1 guardDb = new GuardSystemEntities1();

        //[Route("api/v1/login/GetToken")]
        [HttpPost] 
        public JObject GetToken(string username, string password)
        {
            string msg = string.Empty;
            string token = string.Empty;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                msg = "传入账号或密码为空，请重新输入";
                return GetResponseString("200", new { data = msg });
            }

            var guard_user = guardDb.Guard_user.Where(s => s.ACCOUNT == username.Trim() && s.PASSWORD == password.Trim() && s.LOCATION == "GZ").FirstOrDefault();
            if (guard_user == null)
            {
                msg = "账号密码错误或不存在，请输入正确的账号密码";
                return GetResponseString("200", new { data = msg });
            }
            else
            {
                token = getTokenCode(guard_user.RID.ToString(), username, password);
                msg = token;
            }

            return GetResponseString("200", new { data = msg });
        }

        private string getTokenCode(string rid, string username, string password)
        {
            string tokenCode = string.Format("{0}##^^##{1}##^^##{2}##^^##{3}", rid, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Ticks);

            return Convert.ToBase64String(Encoding.Default.GetBytes(tokenCode));
        }

        public bool IsValidToken(string key)
        {
            try
            {
                var _key = Convert.FromBase64String(key);
                string token = Encoding.Default.GetString(_key);

                string[] words = Split2(token.ToString(), "##^^##");

                DateTime b = new DateTime(long.Parse(words[3].ToString()));
                double diff = DateTime.Now.Subtract(b).TotalMinutes;

                if (b.Day == DateTime.Now.Day)
                {
                    if (diff <= 60)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private string[] Split2(string s, string separator)
        {
            return s.Split(new string[] { separator }, StringSplitOptions.None);
        }

        protected JObject GetResponseString(string status, object data)
        {
            JArray array = new JArray();
            JObject response = new JObject();

            response.Add("meta", JToken.Parse(JsonConvert.SerializeObject(new { status = status })));
            response.Add("data", JToken.Parse(JsonConvert.SerializeObject(data)));

            return response;
        }
    }
}