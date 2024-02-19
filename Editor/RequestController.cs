using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GlitchSDK.IntegrationManager.Editor
{
    public class RequestController
    {
        UnityWebRequest request;

        Dictionary<string, string> headers;

        readonly IntegrationManager manager;

        public RequestController(IntegrationManager manager)
        {
            this.manager = manager;
        }

        public void SetAuthToken(string token)
        {
            headers = new()
            {
                { "Accept", "application/vnd.github+json" },
                { "Authorization", $"Bearer {token}" }, // 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
                { "X-GitHub-Api-Version", "2022-11-28" },
            };
        }

        public void Get(string url)
        {
            request = new(url, "GET");
            DownloadHandlerBuffer dH = new();
            request.downloadHandler = dH;
            foreach (string key in headers.Keys) request.SetRequestHeader(key, headers[key]);

            UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
            asyncOp.completed += OnGetRequestCompleted;
        }

        void OnGetRequestCompleted(AsyncOperation op)
        {
            UnityWebRequest result = (op as UnityWebRequestAsyncOperation).webRequest;
            manager.OnGetRequestCompleted(result);
            request.Dispose();
        }
    }
}
