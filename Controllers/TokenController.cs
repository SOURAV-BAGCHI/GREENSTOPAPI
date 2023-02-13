using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GreenStop.API.Data;
using GreenStop.API.Helpers;
using GreenStop.API.Models;
using GreenStop.API.Models.ViewModels.StoredProcedureViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Models.ViewModels.StoredProcedureViewModel;
using Models.ViewModels;

namespace GreenStop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController:ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly AppSettings _appSettings;
        public TokenController(ApplicationDbContext db,IOptions<AppSettings> appSettings,
        UserManager<ApplicationUser> userManager)
        {
            _db=db;
            _appSettings=appSettings.Value;
            _userManager=userManager;
        }
        
        [HttpPost("[action]")]
        public async Task<IActionResult> Auth([FromBody] TokenRequestModel model) //Granttype can be "password" or "refresh_token"
        {
             // this method will be called in case of login using password and also during refresh token request
           if(model==null)
            {
                return new StatusCodeResult(500);
            }

            switch(model.GrantType)
            {
                case "otplogin":
                    return  GenerateTokenModelvOtp(model);
                case "password":
                    return await GenerateTokenModel(model);
                case "refresh_token":
                    return  RefreshToken(model);
                default:
                    // not supported. return a Http 401 unauthorized
                    //return new UnauthorizedResult();
                    return BadRequest();
                    
            }
        }

        private IActionResult GenerateTokenModelvOtp(TokenRequestModel model)
        {
            var newRToken=CreateRefreshToken(_appSettings.ClientId,"0");
            var serializedRToken=JsonSerializer.Serialize(newRToken);

            SqlParameter param1=new SqlParameter("@str_RefreshTokenModel",serializedRToken);
            SqlParameter param2=new SqlParameter("@str_Username",model.Username);
            var user= _db.Set<UserDetailsViewModel>().FromSqlRaw(StoredProcedure.usp_AspNetUsers_OTPLoginAndRefToken,param1,param2).ToList().FirstOrDefault();
            //user.RefreshToken=
            // user.RefreshToken.Replace(@"\", "");
            // user.RefreshToken=Regex.Replace(user.RefreshToken, @"\\", "");
            var newuser=new UserDetailsViewModel2(){
                Id=user.Id,
                Username=model.Username,
                DisplayName=user.DisplayName,
                Role=user.Role,
                RefreshToken=user.RefreshToken
            };

            var nRToken=JsonSerializer.Deserialize<List<TokenModel>>(user.RefreshToken).FirstOrDefault();
            var accessToken= CreateAccessToken(newuser,ZipRefreshToken(nRToken));

            return Ok(new{authToken=accessToken});

        }
        private async Task<IActionResult> GenerateTokenModel(TokenRequestModel model)
        {
            var user= await _userManager.FindByNameAsync(model.Username);
            if(user!=null && await _userManager.CheckPasswordAsync(user,model.Password))
            {
                var newRToken=CreateRefreshToken(_appSettings.ClientId,user.Id);
                var serializedRToken=JsonSerializer.Serialize(newRToken);
                SqlParameter param1=new SqlParameter("@str_RefreshTokenModel",serializedRToken);
                SqlParameter param2=new SqlParameter("@str_Id",user.Id);

                var RToken=_db.Set<RefreshTokenSPViewModel>().FromSqlRaw(StoredProcedure.usp_Token_InsertUpdate,param1,param2).ToList().FirstOrDefault();
               
                var roles= await _userManager.GetRolesAsync(user);
                var newuser=new UserDetailsViewModel2()
                {
                    Id=user.Id,
                    DisplayName=user.DisplayName,
                    Role=roles.FirstOrDefault(),
                    RefreshToken=String.Empty,
                    Username=model.Username
                };

                var accessToken= CreateAccessToken(newuser,ZipRefreshToken(JsonSerializer.Deserialize<List<TokenModel>>(RToken.NEWREFTOKEN).FirstOrDefault()));
                return Ok(new{authToken=accessToken});
            }
             ModelState.AddModelError(String.Empty,"Username/Password not found");
            return Unauthorized(new{LoginError="Please check login credentials.Invalid Username/Password was entered"});
        }

        private IActionResult RefreshToken(TokenRequestModel model)
        {   
            try
            {
                Int64 Id=0;
                String value=String.Empty;
                UnzipRefreshToken(model.RefreshToken,out Id,out value);
                var newRToken=CreateRefreshToken(_appSettings.ClientId,"0");
                newRToken.Id=Id;
                newRToken.Value=value;
                var serializedRToken=JsonSerializer.Serialize(newRToken);
                SqlParameter param1=new SqlParameter("@str_RefreshTokenModel",serializedRToken);
                SqlParameter param2=new SqlParameter("@str_Id","0");

                var RToken=_db.Set<RefreshTokenSPViewModel>().FromSqlRaw(StoredProcedure.usp_Token_InsertUpdate,param1,param2).ToList().FirstOrDefault();

                if(RToken==null)
                {
                    // return new UnauthorizedResult();
                    return BadRequest();
                }
                else
                {   
                    SqlParameter param=new SqlParameter("@str_Username",model.Username);
                    var user= _db.Set<UserDetailsViewModel>().FromSqlRaw(StoredProcedure.usp_AspNetUsers_GetUserDetails,param).ToList().FirstOrDefault();
                    
                    var newuser=new UserDetailsViewModel2(){
                        Id=user.Id,
                        Username=model.Username,
                        DisplayName=user.DisplayName,
                        Role=user.Role,
                        RefreshToken=user.RefreshToken
                    };
                    var accessToken= CreateAccessToken(newuser,ZipRefreshToken(JsonSerializer.Deserialize<List<TokenModel>>(RToken.NEWREFTOKEN).FirstOrDefault()));
                    return Ok(new{authToken=accessToken});
                }
                
            }
            catch (System.Exception)
            {
                
                // return new UnauthorizedResult();
                return BadRequest();
            }
            
        }

        [HttpGet("[action]")]
        public IActionResult GetRefreshToken()
        {
            var newRToken=CreateRefreshToken(_appSettings.ClientId,"123456870");
            newRToken.Id=DateTime.Now.Ticks;

            return Ok(JsonSerializer.Serialize(newRToken));
        }
         // Create access token
        private TokenResponseModel CreateAccessToken(UserDetailsViewModel2 user,String refreshToken)
        {
        //    var roles= await _userManager.GetRolesAsync(user);
            var key= new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));
            Double TokenExpiryTime=Convert.ToDouble(_appSettings.ExpireTime);

            var tokenHandler=new JwtSecurityTokenHandler();
            var tokenDescriptor=new SecurityTokenDescriptor{
                    //contains the claims reqd in the future
                    Subject =new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub,user.DisplayName), // holds the user's identity
                        new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()) ,// holds unique identifier for our json web token
                        new Claim(ClaimTypes.NameIdentifier,user.Id), // holds user id
                        new Claim(ClaimTypes.Role,user.Role) ,  // holds the role of user
                        new Claim("LoggedOn",DateTime.Now.ToString())
                    }),
                    SigningCredentials=new SigningCredentials(key,SecurityAlgorithms.HmacSha256Signature),
                    Issuer=_appSettings.Site,
                    Audience=_appSettings.Audience,
                    Expires=DateTime.UtcNow.AddMinutes(TokenExpiryTime)
                };

            // Generate JWT Token
            var newToken=tokenHandler.CreateToken(tokenDescriptor);
            var encodedToken=tokenHandler.WriteToken(newToken);

            return new TokenResponseModel(){
                token=encodedToken,
                expiration=newToken.ValidTo,
                roles=user.Role,
                username=user.Username,
                refresh_token=refreshToken,
                displayName=user.DisplayName
            }  ;  
        }
        private TokenModel CreateRefreshToken(String clientId,String userId)
        {
            return new TokenModel(){
                Id=0,
                ClientId=clientId,
                UserId=userId,
                CreatedDate=DateTime.UtcNow,
                ExpiryTime=DateTime.UtcNow.AddMinutes(Convert.ToDouble(_appSettings.RefreshTokenExpireTime)),
                Value=Guid.NewGuid().ToString("N")
            };
        }
    
        private String ZipRefreshToken(TokenModel token)
        {
            return token.Id.ToString()+token.Value;
        }

        private void UnzipRefreshToken(String ZipRToken,out Int64 Id,out String value)
        {
           Int32 index= ZipRToken.Length-32;
            
            Id=Int64.Parse(ZipRToken.Substring(0,index));
            value=ZipRToken.Substring(index);
            return ;
        }
    
    }
}