using System;

namespace DreamSeeker.Data
{
    /// <summary>
    /// 登录请求数据，对应服务端 LoginRequest。
    /// </summary>
    [Serializable]
    public class LoginRequest
    {
        public string Username;//登录用户名
        public string Password;//登录密码

        public LoginRequest() { }

        public LoginRequest(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    /// <summary>
    /// 登录响应数据，对应服务端 LoginResponse。
    /// </summary>
    [Serializable]
    public class LoginResponse
    {
        public int UserId;//服务端用户唯一 ID
        public string Username;//登录用户名
        public string AccessToken;//访问令牌
        public int ExpiresInSeconds;//访问令牌有效时间，单位为秒

        public LoginResponse() { }
    }
}
