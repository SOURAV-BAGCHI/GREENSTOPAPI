using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GreenStop.API.Data;
using GreenStop.API.Helpers;
using GreenStop.API.Models;
using GreenStop.API.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models.ViewModels.StoredProcedureViewModel;

namespace GreenStop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController:ControllerBase
    {
        private enum Status{
            Default,    //0 -otp assigned
            OrderPlaced,    //1
            OrderApproved,  //2
            OrderAssignedToKitchen, //3
            DeliveryAgentAssigned,  //4
            KitchenOrderReady,  //5
            OrderPickupDone,    //6
            OrderDelivered, //7
            OrderComplete,  //8
            OrderCancelled  //9
        }
        private readonly ApplicationDbContext _db;
        private readonly AppSettings _appSettings;
        public FeedbackController(ApplicationDbContext db,IOptions<AppSettings> appSettings)
        {
            _db=db;
            _appSettings=appSettings.Value;
        } 

        [HttpPost("[action]")]
        public async Task<IActionResult> SetFeedback([FromBody] FeedbackViewModel formData)
        {
            var OrderDetails=await _db.OrderDetails.FindAsync(formData.OrderId);
            var ErrorMessage=String.Empty;

            if(OrderDetails!=null)
            {
                if(OrderDetails.OrderStatus==(Int32)Status.OrderComplete)
                {
                    var FeedbackCount=_db.UserFeedbacks.Where(x=>x.OrderId==formData.OrderId).Count();
                    if(FeedbackCount==0)
                    {
                        var UserFeedback=new UserFeedbackModel()
                        {
                            Rating=formData.Rating,
                            CustomerName=JsonSerializer.Deserialize<CustomerInfoViewModel>(OrderDetails.CustomerInfo).Name,
                            ReviewTitle=formData.ReviewTitle,
                            Review=formData.Review,
                            OrderId=formData.OrderId,
                            FeedbackDate=DateTime.Now
                        };

                        await _db.UserFeedbacks.AddAsync(UserFeedback);
                        await _db.SaveChangesAsync();

                        return Ok(1);
                    }
                    ErrorMessage="Review already present";

                }
                else
                {
                    ErrorMessage="Review for this order cannot be posted";
                }
                
            }
            else
            {
                ErrorMessage="Invalid order number";
            }

            return BadRequest();
        }
    
        [HttpGet("[action]")]
        public IActionResult GetFeedbackCount()
        {
            // SqlParameter param4=new SqlParameter("@str_role",role);

            var result=  _db.Set<GeneralListViewModel>().FromSqlRaw(StoredProcedure.usp_UserFeedbacks_GetCount).ToList().FirstOrDefault();

            return Ok(new {Count=Convert.ToInt32(result.Id)});
        }
    
        [HttpGet("[action]/{pageno}")]
        public IActionResult GetFeedbacks([FromRoute] Int32 pageno)
        {
            SqlParameter param1=new SqlParameter("@i_pageno",pageno);
            SqlParameter param2=new SqlParameter("@i_rowstoreturn",_appSettings.PaginationRecordCount);
        
            var result=  _db.Set<UserFeedbackModel>().FromSqlRaw(StoredProcedure.usp_UserFeedbacks_GetUserFeedbacks,param1,param2).ToList();

            // return Ok(new {Result=result});
            return Ok(result);
        }
    }
}