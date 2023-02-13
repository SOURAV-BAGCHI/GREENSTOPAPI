using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenStop.API.Helpers;
using GreenStop.API.Models;
using GreenStop.API.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GreenStop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController:ControllerBase
    {
         //core services of entity framework

        //To register user
        private readonly UserManager<ApplicationUser> _userManager;
        //To signin user
        private readonly SignInManager<ApplicationUser> _signManager;
        //To create JWT Token from app settings present in the appsettings.json
        private readonly AppSettings _appSettings;


        public AccountController(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signManager, 
        IOptions<AppSettings> appsettings,IMemoryCache cache)
        {
            _userManager=userManager;
            _signManager=signManager;
            _appSettings=appsettings.Value;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel formdata)
        {
             // will hold all the errors related to registration
            List<String> errorList= new List<String>();
            
            var user=new ApplicationUser(){
                Email=String.Empty,
                UserName=formdata.Phone,
                DisplayName=formdata.DisplayName,
                SecurityStamp=Guid.NewGuid().ToString()
            };

            var result=await _userManager.CreateAsync(user,formdata.Password);
            var role=(String.IsNullOrEmpty(formdata.Role)?"Customer":formdata.Role);
            if(result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user,role);
               //Send confirmation email
                return Ok(new{phone=user.UserName,StatusCode=1,message="Registration successful"});

            } 
            else
            {
                foreach(var err in result.Errors)
                {
                    ModelState.AddModelError("",err.Description);
                    errorList.Add(err.Description);
                }
            }

            return BadRequest(new JsonResult(errorList));
        }
        
        // [HttpGet("[action]/{phone}")]
        // public IActionResult CheckNumberAvailability([FromRoute] String phone)
        // {
        //     SqlParameter param=new SqlParameter("@str_Username",phone);
        //     var result= _db.Set<UserValidityCheckViewModel>().FromSqlRaw(StoredProcedure.usp_AspNetUsers_CheckIfUserExist,param).ToList().FirstOrDefault();
                
        //     return Ok(result.Available);   
        // }
    }
}