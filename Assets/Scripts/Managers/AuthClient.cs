using System;
using System.Collections;
using System.Text;
using DreamSeeker.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace DreamSeeker.Managers
{
    public class AuthClient : BaseManager<AuthClient>
    {
        private const string BaseUrl = "http://localhost:5210";

        /// <summary>
        /// 提供给 BaseManager 反射创建认证客户端实例的私有构造函数。
        /// </summary>
        private AuthClient() { }

        public IEnumerator Login(string username, string password,
            Action<LoginResponse> onSuccess, Action<string> onFailure)
        {
            var data = new LoginRequest
            {
                Username = username,
                Password = password,
            };
            string json = JsonUtility.ToJson(data);
            byte[] body = Encoding.UTF8.GetBytes(json);

            // 创建一个HTTP请求
            using var request = new UnityWebRequest(
                $"{BaseUrl}/api/auth/login", UnityWebRequest.kHttpVerbPOST);
            
            // 将body设置为请求的上传内容
            request.uploadHandler = new UploadHandlerRaw(body);
            // 为请求设置“响应数据接收器”。
            // 服务器返回的数据会被暂存在内存缓冲区中，之后可以通过：
            // request.downloadHandler.text
            // 读取响应文本。
            request.downloadHandler = new DownloadHandlerBuffer();
            // 设置 HTTP 请求头，告诉服务器：
            // 我发送的请求体内容是 JSON 格式。
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                onFailure?.Invoke($"HTTP {request.responseCode}: {request.error}");
            }
        }

        /// <summary>
        /// 向服务端提交注册请求。
        /// </summary>
        public IEnumerator Register(string username, string password,
            Action onSuccess, Action<string> onFailure)
        {
            var data = new LoginRequest
            {
                Username = username,
                Password = password,
            };
            string json = JsonUtility.ToJson(data);
            byte[] body = Encoding.UTF8.GetBytes(json);

            using var request = new UnityWebRequest(
                $"{BaseUrl}/api/auth/register", UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFailure?.Invoke($"HTTP {request.responseCode}: {request.error}");
            }
        }
    }
}
