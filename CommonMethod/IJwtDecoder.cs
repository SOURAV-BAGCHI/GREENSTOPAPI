using System;
using Models.Helpers;

namespace CommonMethod
{
    public interface IJwtDecoder
    {
        JWTviewModel Decode (String JWT);
    }
}