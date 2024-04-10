using System;

namespace DevsDaddy.GameShield.Core.Reporter
{
    [System.Serializable]
    public class ReportData
    {
        public string Gateway = "";
        public bool IsUserReport = false;
        public DateTime LocalTime = DateTime.Now;
        public string Message = "";
        public string ModuleType = "";
        public string Code = "";
    }
}