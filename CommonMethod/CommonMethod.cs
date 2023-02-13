using System;

namespace GreenStop.API.CommonMethod
{
    public class CommonMethods:ICommonMethods
    {

        public String GenerateOTP()
        {

            String OTP=DateTime.Now.Ticks.ToString();
            OTP=OTP.Substring(OTP.Length-4);
            //return OTP;
            return "1234";
        }

        public Boolean SendMessage()
        {
            return true;
        }
    }
}