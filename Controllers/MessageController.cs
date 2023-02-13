using System;
using System.Linq;
using System.Threading.Tasks;
using GreenStop.API.CommonMethod;
using GreenStop.API.Data;
using GreenStop.API.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GreenStop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController:ControllerBase
    {
        readonly private ICommonMethods _commonMethods;
        private IMemoryCache _cache;
        private readonly ApplicationDbContext _db;
        public MessageController(ICommonMethods commonMethods,IMemoryCache cache,
        ApplicationDbContext db)
        {
            _commonMethods=commonMethods;
            _cache=cache;
            _db=db;
        }

        // [HttpPost("[action]/{Phone}")]
        // public IActionResult SendOTP(String Phone)
        // {
        //     String OTP=this.commonMethods.GenerateOTP();
        //     Boolean MessageSent=this.commonMethods.SendMessage();

        //     if(MessageSent)
        //     {
        //         return Ok();
        //     }
        //     else
        //     {
        //         return BadRequest();
        //     }
        // }
        [HttpPost("[action]/{phone}")]
        public IActionResult SendOTP([FromRoute] String phone)
        {
            String cacheEntry=String.Empty;
            if (_cache.TryGetValue(phone, out cacheEntry))
            {
                _cache.Remove(phone);     
            }
            if(String.IsNullOrEmpty(cacheEntry))
            {cacheEntry=_commonMethods.GenerateOTP();}

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                // Keep in cache for this time, reset time if accessed.
                .SetSlidingExpiration(TimeSpan.FromMinutes(1));
            
            _cache.Set(phone, cacheEntry, cacheEntryOptions);

            // send sms on thread
            return Ok();
            
        }

        [HttpPost("[action]")]
        public IActionResult VerifyOTP([FromBody] OTPVerifyViewModel formData)
        {
            String cacheEntry=String.Empty;
            if (_cache.TryGetValue(formData.phone, out cacheEntry))
            {
                if(cacheEntry.Equals(formData.otp))
                {
                    SqlParameter param=new SqlParameter("@str_Username",formData.phone);
                    var result= _db.Set<UserValidityCheckViewModel>().FromSqlRaw(StoredProcedure.usp_AspNetUsers_CheckIfUserExist,param).ToList().FirstOrDefault();
                     _cache.Remove(formData.phone); 
                    return Ok(result.Available);
                }
                return BadRequest();
            }
            else
            {
                return Unauthorized();
            }
        }
        
        [HttpGet("[action]/{phone}")]
        public IActionResult CheckNumberAvailability([FromRoute] String phone)
        {
            SqlParameter param=new SqlParameter("@str_Username",phone);
            var result= _db.Set<UserValidityCheckViewModel>().FromSqlRaw(StoredProcedure.usp_AspNetUsers_CheckIfUserExist,param).ToList().FirstOrDefault();
                
            return Ok(result.Available);   
        }
    }
}