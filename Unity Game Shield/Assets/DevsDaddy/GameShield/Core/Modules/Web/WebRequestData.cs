using System.Collections.Generic;
using DevsDaddy.Shared.CryptoLibrary.Core;
using UnityEngine.Networking;

namespace DevsDaddy.GameShield.Core.Modules.Web
{
    [System.Serializable]
    public class WebRequestData
    {
        // Request Data
        public long RequestId = 0;
        public string Url = "";
        public string Method = "GET";
        public Dictionary<string, string> Headers = new Dictionary<string, string>();
        public byte[] UploadData;
        public ICryptoProvider Provider;

        // Handlers
        public DownloadHandler DownloadHandler;
    }

    public class WebRequestResponse
    {
        public long Code;
        public string DecryptedText;
        public byte[] DecryptedBinary;
        public string RawText;
        public byte[] RawBinary;
    }
}