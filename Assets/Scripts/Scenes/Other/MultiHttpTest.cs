using Cysharp.Threading.Tasks;
using MajdataPlay.Net.Curl;
using MajdataPlay.Net.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MajdataPlay.Scenes.Other
{
    public class MultiHttpTest : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private string testUrl = "https://httpbin.org/get";
        [SerializeField, Min(1)] private int concurrentRequests = 20;
        [SerializeField] string method = "GET";
        [SerializeField] string payload = "";

        [Header("UI")]
        [SerializeField] private Text logText;
        [SerializeField] private Button curlButton;
        [SerializeField] private Button httpClientButton;
        [SerializeField] private Button unityWebRequestButton;

        // Curl client and HttpClient instances
        private HttpClient _curl;
        private HttpClient _httpClient;

        // Logging and cancellation
        private StringBuilder _log = new();
        private CancellationTokenSource _cts;

        void Start()
        {
            // 初始化客户端（根据需要调整参数）
            //_curl = new CurlHttpClient(true) { PreferredVersion = HttpVersion.PreferH3, VerifySSL = true };

            CurlHttpMessageHandler chm = new CurlHttpMessageHandler()
            {
                MaxConnectionsPerServer = 64
            };
            UnityHttpMessageHandler _unityHttpHandler = new UnityHttpMessageHandler();
            _httpClient = new HttpClient();
            _curl = new(chm);

            if (curlButton != null)
                curlButton.onClick.AddListener(() => RunCurlBatch().Forget());
            if (httpClientButton != null)
                httpClientButton.onClick.AddListener(() => RunHttpClientBatch().Forget());
            if (unityWebRequestButton != null)
                unityWebRequestButton.onClick.AddListener(() => RunUnityWebRequestBatch().Forget());
        }

        void OnDestroy()
        {
            _httpClient?.Dispose();
            _cts?.Cancel();
            _cts?.Dispose();
        }

        void ResetLog()
        {
            _log.Clear();
            if (logText != null) logText.text = string.Empty;
        }

        void Log(string msg)
        {
            Debug.Log(msg);
            _log.AppendLine(msg);
            if (logText != null) logText.text = _log.ToString();
        }

        void CancelRunning()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            ResetLog();
        }

        // ---------- Curl ----------
        async UniTask RunCurlBatch()
        {
            CancelRunning();
            var ct = _cts.Token;
            var start = DateTime.UtcNow;

            var tasks = new UniTask[concurrentRequests];
            for (int i = 0; i < concurrentRequests; i++)
            {
                int idx = i;
                tasks[i] = PerformCurlAsync(testUrl, idx, method, payload, ct);
            }

            try
            {
                await UniTask.WhenAll(tasks);
                var totalMs = (DateTime.UtcNow - start).TotalMilliseconds;
                Log($"[Curl] Total: {totalMs:F1} ms, Avg: {totalMs / concurrentRequests:F1} ms");
            }
            catch (OperationCanceledException)
            {
                Log("[Curl] Canceled");
            }
            catch (Exception ex)
            {
                Log($"[Curl] Exception: {ex.GetType().Name}: {ex.Message}");
            }
        }

        async UniTask PerformCurlAsync(string url, int index, string method, string payload, CancellationToken ct)
        {
            try
            {
                var httpMethod = new HttpMethod(method);
                var httpPayload = default(HttpContent?);
                if (!string.IsNullOrEmpty(payload))
                {
                    httpPayload = new StringContent(payload);
                }
                var httpMessage = new HttpRequestMessage(httpMethod, url)
                {
                    Content = httpPayload
                };
                using var resp = await _curl.SendAsync(httpMessage, ct);
                bool success = resp.StatusCode is >= HttpStatusCode.OK and < HttpStatusCode.MultipleChoices;
                var body = await resp.Content.ReadAsByteArrayAsync() ?? Array.Empty<byte>();
                int length = body.Length;
                string sha256 = ComputeSha256Hex(body);
                Log($"[Curl] #{index} {(success ? "SUCCESS" : "FAIL")} ({resp.StatusCode}) BodyLen={length} SHA256={sha256}");
            }
            catch (OperationCanceledException)
            {
                // propagate to caller
                throw;
            }
            catch (Exception ex)
            {
                Log($"[Curl] #{index} FAIL ({ex.GetType().Name})");
            }
        }

        // ---------- HttpClient ----------
        async UniTask RunHttpClientBatch()
        {
            CancelRunning();
            var ct = _cts.Token;
            var start = DateTime.UtcNow;

            var tasks = new UniTask[concurrentRequests];
            for (int i = 0; i < concurrentRequests; i++)
            {
                int idx = i;
                tasks[i] = PerformHttpClientAsync(testUrl, idx, method, payload, ct);
            }

            try
            {
                await UniTask.WhenAll(tasks);
                var totalMs = (DateTime.UtcNow - start).TotalMilliseconds;
                Log($"[HttpClient] Total: {totalMs:F1} ms, Avg: {totalMs / concurrentRequests:F1} ms");
            }
            catch (OperationCanceledException)
            {
                Log("[HttpClient] Canceled");
            }
            catch (Exception ex)
            {
                Log($"[HttpClient] Exception: {ex.GetType().Name}: {ex.Message}");
            }
        }

        async UniTask PerformHttpClientAsync(string url, int index, string method, string payload, CancellationToken ct)
        {
            try
            {
                var httpMethod = new HttpMethod(method);
                var httpPayload = default(HttpContent?);
                if (!string.IsNullOrEmpty(payload))
                {
                    httpPayload = new StringContent(payload);
                }
                var httpMessage = new HttpRequestMessage(httpMethod, url)
                {
                    Content = httpPayload
                };
                using var resp = await _httpClient.SendAsync(httpMessage, ct);
                bool success = resp.IsSuccessStatusCode;

                // 读取 body bytes
                byte[] bodyBytes = Array.Empty<byte>();
                try
                {
                    // ReadAsByteArrayAsync 没有 cancellation token 在某些框架版本中，
                    // 这里先读取完整内容（通常足够用于测试）。若需严格支持 ct，可改用 ReadAsStreamAsync 并手动读取。
                    bodyBytes = await resp.Content.ReadAsByteArrayAsync();
                }
                catch
                {
                    // 忽略读取错误，bodyBytes 保持为空
                }

                int length = bodyBytes?.Length ?? 0;
                string sha256 = ComputeSha256Hex(bodyBytes);
                Log($"[HttpClient] #{index} {(success ? "SUCCESS" : "FAIL")} ({(int)resp.StatusCode}) BodyLen={length} SHA256={sha256}");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log($"[HttpClient] #{index} FAIL ({ex.GetType().Name})");
            }
        }

        // ---------- UnityWebRequest ----------
        async UniTask RunUnityWebRequestBatch()
        {
            CancelRunning();
            var ct = _cts.Token;
            var start = DateTime.UtcNow;

            var tasks = new UniTask[concurrentRequests];
            for (int i = 0; i < concurrentRequests; i++)
            {
                int idx = i;
                tasks[i] = PerformUnityWebRequestAsync(testUrl, idx, method, payload, ct);
            }

            try
            {
                await UniTask.WhenAll(tasks);
                var totalMs = (DateTime.UtcNow - start).TotalMilliseconds;
                Log($"[UnityWebRequest] Total: {totalMs:F1} ms, Avg: {totalMs / concurrentRequests:F1} ms");
            }
            catch (OperationCanceledException)
            {
                Log("[UnityWebRequest] Canceled");
            }
            catch (Exception ex)
            {
                Log($"[UnityWebRequest] Exception: {ex.GetType().Name}: {ex.Message}");
            }
        }

        async UniTask PerformUnityWebRequestAsync(string url, int index, string method, string payload, CancellationToken ct)
        {
            using var req = UnityWebRequest.Get(url);
            try
            {
                await req.SendWebRequest().WithCancellation(ct);

#if UNITY_2020_2_OR_NEWER
                bool success = req.result == UnityWebRequest.Result.Success;
#else
                bool success = !(req.isNetworkError || req.isHttpError);
#endif
                byte[] data = req.downloadHandler != null ? req.downloadHandler.data ?? Array.Empty<byte>() : Array.Empty<byte>();
                int length = data.Length;
                string sha256 = ComputeSha256Hex(data);
                Log($"[UnityWebRequest] #{index} {(success ? "SUCCESS" : "FAIL")} ({req.responseCode}) BodyLen={length} SHA256={sha256}");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log($"[UnityWebRequest] #{index} FAIL ({ex.GetType().Name})");
            }
        }

        // ---------- Helpers ----------
        private static string ComputeSha256Hex(byte[] data)
        {
            if (data == null || data.Length == 0)
                return "0".PadLeft(64, '0'); // 空 body 的 sha256 表示为 64 个 0（可按需修改）

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(data);
            // 转为小写 hex
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();

        }
    }
}
