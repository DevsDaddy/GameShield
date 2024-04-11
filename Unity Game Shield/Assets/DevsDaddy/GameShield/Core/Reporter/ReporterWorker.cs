using System;
using System.Collections;
using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using UnityEngine.Networking;

namespace DevsDaddy.GameShield.Core.Reporter
{
    /// <summary>
    /// Reporting Worker Class
    /// </summary>
    public static class ReporterWorker
    {
        /// <summary>
        /// Send Report to Server
        /// </summary>
        /// <param name="data"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        public static void Send(ReportData data, Action<string> onComplete = null, Action<string> onError = null) {
            EventMessenger.Main.Publish(new RequestCoroutine {
                Coroutine = SendReport(data, onComplete, onError),
                Id = "GameShieldReportProcessing"
            });
        }
        
        /// <summary>
        /// Send Report
        /// </summary>
        /// <param name="data"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        private static IEnumerator SendReport(ReportData data, Action<string> onComplete = null, Action<string> onError = null) {
            string url = $"{GameShield.Main.GetBackendUrl()}/{data.Gateway}";
            if (string.IsNullOrEmpty(url)) {
                onError?.Invoke($"{GeneralStrings.LOG_PREFIX} Failed to send report. Backend URL and Gateway are empty");
                yield break;
            }

            // Request
            var req = new UnityWebRequest(url, "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(data));
            req.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            
            yield return req.SendWebRequest();

            if (req.isDone && req.downloadHandler.isDone)
            {
                if (string.IsNullOrEmpty(req.downloadHandler.text)) {
                    onError?.Invoke($"{GeneralStrings.LOG_PREFIX} Failed to parse server response at reporting processing. Response are empty.");
                    yield break;
                }
                
                onComplete?.Invoke(req.downloadHandler.text);
            }
            else
            {
                onError?.Invoke($"{GeneralStrings.LOG_PREFIX} Reporting Processing Error: {req.error}");
            }
        }
    }
}