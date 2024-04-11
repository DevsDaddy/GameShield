using System.Collections.Generic;

namespace DevsDaddy.GameShield.Core.Modules.Captcha
{
    [System.Serializable]
    public class RewardedCaptchaData
    {
        public int[] RequiredOrder;
        public int[] CurrentFieldOrder;
    }
}