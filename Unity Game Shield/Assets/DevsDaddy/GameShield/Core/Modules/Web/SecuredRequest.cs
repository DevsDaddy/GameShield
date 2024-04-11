using System;
using System.Collections;
using System.Collections.Generic;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.CryptoLibrary.Core;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using UnityEngine.Networking;

namespace DevsDaddy.GameShield.Core.Modules.Web
{
    /// <summary>
    /// Secured Web Requests Module
    /// </summary>
    public class SecuredRequest : IShieldModule
    {
        private Options _currentOptions;
        private bool _initialized = false;
        private bool _isPaused = false;
        
        // Web Requests Pull
        private int simplePostId = 0;
        private List<WebRequestData> _requests = new List<WebRequestData>();

        /// <summary>
        /// Setup Module
        /// </summary>
        /// <param name="config"></param>
        /// <param name="reinitialize"></param>
        public void SetupModule(IShieldModuleConfig config = null, bool reinitialize = false) {
            if (!Application.isPlaying) return;
            
            // Change Configuration
            _currentOptions = (Options)config ?? new Options();
            EventMessenger.Main.Publish(new SecurityModuleConfigChanged {
                Module = this,
                Config = _currentOptions
            });
            
            // Initialize Module
            if (!_initialized && !reinitialize)
                Initialize();
        }

        /// <summary>
        /// Disconnect Module
        /// </summary>
        public void Disconnect() {
            // Fire Disconnected Complete
            EventMessenger.Main.Publish(new SecurityModuleDisconnected {
                Module = this
            });
        }
        
        /// <summary>
        /// Toggle Pause for Current Detector
        /// </summary>
        /// <param name="isPaused"></param>
        public void PauseDetector(bool isPaused) {
            if(isPaused == _isPaused) return;
            _isPaused = isPaused;
            EventMessenger.Main.Publish(new SecurityModulePause {
                Module = this,
                IsPaused = _isPaused
            });
        }

        /// <summary>
        /// Check if Detector Paused
        /// </summary>
        /// <returns></returns>
        public bool IsPaused() {
            return _isPaused;
        }

        /// <summary>
        /// Initialize Module
        /// </summary>
        private void Initialize() {
            // Fire Initialization Complete
            EventMessenger.Main.Publish(new SecurityModuleInitialized {
                Module = this
            });
        }

        /// <summary>
        /// Send Web Request
        /// </summary>
        /// <param name="data"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        public void SendRequest(WebRequestData data, Action<WebRequestResponse> onComplete = null, Action<string> onError = null) {
            // Add Requests to List
            data.RequestId = _requests.Count + 1;
            _requests.Add(data);

            // Send Request
            EventMessenger.Main.Publish(new RequestCoroutine {
                Coroutine = WebRequest(data, onComplete, onError),
                Id = "SecuredRequest" + data.RequestId
            });
        }

        /// <summary>
        /// Send Post Request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="provider"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        public void Post(string url, ICryptoProvider provider, Action<WebRequestResponse> onComplete = null, Action<string> onError = null) {
            simplePostId++;
            EventMessenger.Main.Publish(new RequestCoroutine {
                Coroutine = PostRequest(url, provider,null, null, onComplete, onError),
                Id = "SecuredPostRequest" + simplePostId
            });
        }

        /// <summary>
        /// Send Post Request with Headers
        /// </summary>
        /// <param name="url"></param>
        /// <param name="provider"></param>
        /// <param name="headers"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        public void Post(string url, ICryptoProvider provider, Dictionary<string, string> headers, Action<WebRequestResponse> onComplete = null, Action<string> onError = null) {
            simplePostId++;
            EventMessenger.Main.Publish(new RequestCoroutine {
                Coroutine = PostRequest(url, provider, headers, null, onComplete, onError),
                Id = "SecuredPostRequest" + simplePostId
            });
        }

        /// <summary>
        /// Send Post Request with Headers and Body
        /// </summary>
        /// <param name="url"></param>
        /// <param name="provider"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        public void Post(string url, ICryptoProvider provider, Dictionary<string, string> headers, Dictionary<string, string> body, Action<WebRequestResponse> onComplete = null, Action<string> onError = null) {
            simplePostId++;
            EventMessenger.Main.Publish(new RequestCoroutine {
                Coroutine = PostRequest(url, provider, headers, body, onComplete, onError),
                Id = "SecuredPostRequest" + simplePostId
            });
        }

        /// <summary>
        /// Put Request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="provider"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        public void Put(string url, ICryptoProvider provider, Action<WebRequestResponse> onComplete = null,
            Action<string> onError = null) {
            simplePostId++;
            EventMessenger.Main.Publish(new RequestCoroutine {
                Coroutine = PutRequest(url, provider, null, null, null, onComplete, onError),
                Id = "SecuredPostRequest" + simplePostId
            });
        }
        
        /// <summary>
        /// Put Request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="provider"></param>
        /// <param name="headers"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        public void Put(string url, ICryptoProvider provider, Dictionary<string, string> headers, Action<WebRequestResponse> onComplete = null,
            Action<string> onError = null) {
            simplePostId++;
            EventMessenger.Main.Publish(new RequestCoroutine {
                Coroutine = PutRequest(url, provider, headers, null, null, onComplete, onError),
                Id = "SecuredPostRequest" + simplePostId
            });
        }
        
        /// <summary>
        /// Put Request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="provider"></param>
        /// <param name="body"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        public void Put(string url, ICryptoProvider provider, string body, Action<WebRequestResponse> onComplete = null,
            Action<string> onError = null) {
            simplePostId++;
            EventMessenger.Main.Publish(new RequestCoroutine {
                Coroutine = PutRequest(url, provider, null, body, null, onComplete, onError),
                Id = "SecuredPostRequest" + simplePostId
            });
        }
        
        /// <summary>
        /// Put Request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="provider"></param>
        /// <param name="body"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        public void Put(string url, ICryptoProvider provider, byte[] body, Action<WebRequestResponse> onComplete = null,
            Action<string> onError = null) {
            simplePostId++;
            EventMessenger.Main.Publish(new RequestCoroutine {
                Coroutine = PutRequest(url, provider, null, null, body, onComplete, onError),
                Id = "SecuredPostRequest" + simplePostId
            });
        }
        
        /// <summary>
        /// Put Request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="provider"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        public void Put(string url, ICryptoProvider provider, Dictionary<string, string> headers, string body, Action<WebRequestResponse> onComplete = null,
            Action<string> onError = null) {
            simplePostId++;
            EventMessenger.Main.Publish(new RequestCoroutine {
                Coroutine = PutRequest(url, provider, headers, body, null, onComplete, onError),
                Id = "SecuredPostRequest" + simplePostId
            });
        }
        
        /// <summary>
        /// Put Request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="provider"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        public void Put(string url, ICryptoProvider provider, Dictionary<string, string> headers, byte[] body, Action<WebRequestResponse> onComplete = null,
            Action<string> onError = null) {
            simplePostId++;
            EventMessenger.Main.Publish(new RequestCoroutine {
                Coroutine = PutRequest(url, provider, headers, null, body, onComplete, onError),
                Id = "SecuredPostRequest" + simplePostId
            });
        }
        
        /// <summary>
        /// Post Request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="provider"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        private IEnumerator PostRequest(string url, ICryptoProvider provider, Dictionary<string, string> headers = null,
            Dictionary<string, string> body = null, Action<WebRequestResponse> onComplete = null,
            Action<string> onError = null) {
            // Check Parameters
            if (provider == null) {
                Debug.LogError("Failed to send Secured Request. Encryption module can't be null");
                yield break;
            }
            if (string.IsNullOrEmpty(url)) {
                Debug.LogError("Failed to send Secured Request. Url can't be null");
                yield break;
            }
            
            // Work with Body
            Dictionary<string, string> encryptedBody;
            if (body != null && body.Count > 0) {
                encryptedBody = new Dictionary<string, string>();
                foreach (var reqData in body) {
                    encryptedBody.Add(reqData.Key, provider.EncryptString(reqData.Value));
                }
            }
            else {
                encryptedBody = body;
            }
            
            // Send Request
            UnityWebRequest req = UnityWebRequest.Post(url, encryptedBody);
            if (headers != null && headers.Count > 0) {
                foreach (var header in headers) {
                    req.SetRequestHeader(header.Key, header.Value);
                }
            }
            
            req.SetRequestHeader("Encrypted", provider.GetType().Name);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success) {
                WebRequestResponse response = new WebRequestResponse {
                    RawText = req.downloadHandler.text,
                    RawBinary = req.downloadHandler.data,
                    Code = req.responseCode,
                    DecryptedBinary = provider.DecryptBinary(req.downloadHandler.data),
                    DecryptedText = provider.DecryptString(req.downloadHandler.text)
                };
                onComplete?.Invoke(response);
            }
            else {
                onError?.Invoke($"Failed to process web request. Code: {req.responseCode}. Error: {req.result.ToString()}");
            }
        }
        
        /// <summary>
        /// Send Put Request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="provider"></param>
        /// <param name="headers"></param>
        /// <param name="textbody"></param>
        /// <param name="bytebody"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        private IEnumerator PutRequest(string url, ICryptoProvider provider, Dictionary<string, string> headers = null,
            string textbody = null, byte[] bytebody = null, Action<WebRequestResponse> onComplete = null,
            Action<string> onError = null) {
            // Check Parameters
            if (provider == null) {
                Debug.LogError("Failed to send Secured Request. Encryption module can't be null");
                yield break;
            }
            if (string.IsNullOrEmpty(url)) {
                Debug.LogError("Failed to send Secured Request. Url can't be null");
                yield break;
            }
            
            // Work with Body
            if (!string.IsNullOrEmpty(textbody))
                textbody = provider.EncryptString(textbody);
            
            if(bytebody != null && bytebody.Length > 0)
                bytebody = provider.EncryptBinary(bytebody);

                // Send Request
            UnityWebRequest req;
            if (!string.IsNullOrEmpty(textbody)) {
                req = UnityWebRequest.Put(url, textbody);
            }
            else if(bytebody != null && bytebody.Length > 0){
                req = UnityWebRequest.Put(url, bytebody);
            }
            else {
                req = UnityWebRequest.Put(url, "");
            }
            if (headers != null && headers.Count > 0) {
                foreach (var header in headers) {
                    req.SetRequestHeader(header.Key, header.Value);
                }
            }
            
            req.SetRequestHeader("Encrypted", provider.GetType().Name);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success) {
                WebRequestResponse response = new WebRequestResponse {
                    RawText = req.downloadHandler.text,
                    RawBinary = req.downloadHandler.data,
                    Code = req.responseCode,
                    DecryptedBinary = provider.DecryptBinary(req.downloadHandler.data),
                    DecryptedText = provider.DecryptString(req.downloadHandler.text)
                };
                onComplete?.Invoke(response);
            }
            else {
                onError?.Invoke($"Failed to process web request. Code: {req.responseCode}. Error: {req.result.ToString()}");
            }
        }

        /// <summary>
        /// Web Request
        /// </summary>
        /// <param name="data"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        private IEnumerator WebRequest(WebRequestData data, Action<WebRequestResponse> onComplete = null, Action<string> onError = null) {
            // Check Parameters
            if (data.Provider == null) {
                Debug.LogError("Failed to send Secured Request. Encryption module can't be null");
                yield break;
            }
            if (string.IsNullOrEmpty(data.Url)) {
                Debug.LogError("Failed to send Secured Request. Url can't be null");
                yield break;
            }
            
            // Work with Request Body
            UploadHandler uploadHandler = null;
            if (data.UploadData != null && data.UploadData.Length > 0) {
                uploadHandler = new UploadHandlerRaw(data.Provider.EncryptBinary(data.UploadData));
            }

            // Send Request
            UnityWebRequest req = new UnityWebRequest(data.Url, data.Method, data.DownloadHandler, uploadHandler);
            foreach (var header in data.Headers) {
                req.SetRequestHeader(header.Key, header.Value);
            }
            req.SetRequestHeader("Encrypted", data.Provider.GetType().Name);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success) {
                WebRequestResponse response = new WebRequestResponse {
                    RawText = req.downloadHandler.text,
                    RawBinary = req.downloadHandler.data,
                    Code = req.responseCode,
                    DecryptedBinary = data.Provider.DecryptBinary(req.downloadHandler.data),
                    DecryptedText = data.Provider.DecryptString(req.downloadHandler.text)
                };
                onComplete?.Invoke(response);
            }
            else {
                onError?.Invoke($"Failed to process web request. Code: {req.responseCode}. Error: {req.result.ToString()}");
            }
        }

        /// <summary>
        /// Get Module Information
        /// </summary>
        /// <returns></returns>
        public ModuleInfo GetModuleInfo() {
            return new ModuleInfo {
                Name = "Secured Requests",
                Description = "This module allows you to exchange encrypted requests with the server with an additional layer of protection",
                DocumentationLink = "https://github.com/DevsDaddy/GameShield/wiki/Modules-Overview#secured-requests"
            };
        }
        
        // Module Options Configuration
        [System.Serializable]
        public class Options : IShieldModuleConfig
        {
            
        }
    }
}