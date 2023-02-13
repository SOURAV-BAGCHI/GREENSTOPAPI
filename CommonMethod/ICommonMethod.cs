using System;

namespace GreenStop.API.CommonMethod
{
    public interface ICommonMethods
    {
        String GenerateOTP();
        Boolean SendMessage();
    }
}