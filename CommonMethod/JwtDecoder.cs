using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Models.Helpers;

namespace CommonMethod
{
    public class JwtDecoder:IJwtDecoder
    {
        public JWTviewModel Decode(String JWT)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(JWT);
            var jti = token.Claims.First(claim => claim.Type == "nameid").Value;
            //var sub=token.Claims.First(claim => claim.Type == "sub").Value;
            var role=token.Claims.First(claim => claim.Type == "role").Value;

            return new JWTviewModel(){
               nid=jti,//Int64.Parse(sub)
               role=role
            };
        }
    }
}