using System;
using System.Collections;
using System.Text;
using DreamSeeker.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace DreamSeeker.Managers
{
    public class AuthClient
    {
        private const string BaseUrl = "https://localhost:5210";

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
    }
}