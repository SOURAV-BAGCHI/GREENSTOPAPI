using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CommonMethod;
using GreenStop.API.Data;
using GreenStop.API.Helpers;
using GreenStop.API.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models.ViewModels;
using Microsoft.Data.SqlClient;
using Models.ViewModels.StoredProcedureViewModel;
using GreenStop.API.Models.ViewModels.StoredProcedureViewModel;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController:ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtDecoder _jwtDecoder;
        private readonly AppSettings _appSettings;
        IHttpClientFactory _clientFactory;

        public AdminController(ApplicationDbContext db,IJwtDecoder jwtDecoder,
        IOptions<AppSettings> appSettings,IHttpClientFactory clientFactory)
        {
            _db=db;
            _jwtDecoder=jwtDecoder;
            _appSettings=appSettings.Value;
            _clientFactory=clientFactory;
        }

        [HttpPut("[action]")]
        public IActionResult UpdatePaymentInfo([FromBody] PaymentOrderInfoViewModel formdata)
        {
            var data=_db.OrderDetails.Where(x=>x.OrderId==formdata.OrderId).ToList().FirstOrDefault();

            if(data!=null)
            {
                // var paymentInfo=new PaymentInfoViewModel(){
                //     Mode=formdata.Mode,
                //     Amt=formdata.Amt,
                //     TId=formdata.TId
                // };


                var paymentInfo=JsonSerializer.Deserialize<PaymentInfoViewModel>(data.PaymentInfo);
                paymentInfo.Amt=formdata.Amt;
                paymentInfo.TId=formdata.TId;


                data.PaymentInfo=JsonSerializer.Serialize(paymentInfo);
                data.PaymentStatus=true;

                _db.Entry(data).State=EntityState.Modified;
                _db.SaveChanges();

                // return Ok("Payment updated successfully");
                return Ok(new{Message="Payment updated successfully"});

            }
            return BadRequest("Data not found");
        }

        [HttpGet("[action]/{orderId}/{code}")]
        public IActionResult CheckOtp([FromRoute] String orderId,String code){
            var data=_db.OrderDetails.Where(x=>x.OrderId==orderId).Select(x=>x.OrderStatusLog).ToList().FirstOrDefault();

            if(!String.IsNullOrEmpty(data))
            {
                var statuslog=JsonSerializer.Deserialize<List<StatusLogViewModel>>(data);
                Boolean isCodeMatch=false;

               
                for(int i=0;i<statuslog.Count;i++)
                {

                    if(statuslog[i].Status==0)
                    {
                        isCodeMatch=statuslog[i].To==code;
                        i=statuslog.Count;
                        
                    }
                }

                // return Ok(statuslog[0].To==code);
                return Ok(isCodeMatch);


            }
            return BadRequest("Data not found");
        
        }

        [HttpPut("[action]")]
        public IActionResult SetCustomerServiceStatus([FromBody] CustomerServiceStatusViewModel formdata)
        {
            SqlParameter param1=new SqlParameter("@str_OrderId",formdata.OrderId);
            SqlParameter param2=new SqlParameter("@str_CustomerServiceStatus",formdata.Value);

            var result=  _db.Set<GeneralListViewModel>().FromSqlRaw(StoredProcedure.usp_OrderDetailsSetCustomerServiceStatus,param1,param2).ToList().FirstOrDefault();

            return Ok(result);
        }
        
        [HttpPut("[action]")]
        public IActionResult SetOrderAction([FromBody] OrderActionViewModel formdata)
        {
            String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
            String nid=_jwtDecoder.Decode(authHeader).nid;

            SqlParameter param1=new SqlParameter("@str_OrderId",formdata.orderid);
            SqlParameter param2=new SqlParameter("@str_UserIdBy",nid);
            SqlParameter param3=new SqlParameter("@str_UserIdTo",formdata.useridto);
            SqlParameter param4=new SqlParameter("@i_OrderStatusNew",formdata.status);
            
            var result=  _db.Set<GeneralListViewModel>().FromSqlRaw(StoredProcedure.usp_OrderDetails_SetAction,param1,param2,param3,param4).ToList().FirstOrDefault();

            var NotificationReqBody=new NotificationRequestViewModel()
            {
                OrderId=formdata.orderid,
                Status=formdata.status
            };

            var ObjNotification=JsonSerializer.Serialize(NotificationReqBody);
            // HttpRequestHelper requestHelper=new HttpRequestHelper();
            ParameterizedThreadStart parameterizedThreadStartRequestNotification = new ParameterizedThreadStart(RequestNotification);
            Thread ThreadNotification = new Thread(parameterizedThreadStartRequestNotification);
            ThreadNotification.Start(ObjNotification);
            return Ok(new {Success=result.Id,Message=result.Value});

        }

        [HttpGet("[action]/{roleid}")]
        public IActionResult GetEmployeeCount([FromRoute] String roleid)
        {
            String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
            var jwtdata=_jwtDecoder.Decode(authHeader);
            String nid=jwtdata.nid;
            String role=jwtdata.role;

            if(role!="ITAdmin")
            {
                return Unauthorized();
            }
            
            SqlParameter param1=new SqlParameter("@str_RoleId",roleid);

            var result=  _db.Set<GeneralListViewModel>().FromSqlRaw(StoredProcedure.usp_AspNetUsers_GetEmployeeCount,param1).ToList().FirstOrDefault();

            return Ok(new {Count=Convert.ToInt32(result.Id)});
        }
    
        [HttpGet("[action]/{roleid}/{pageno}")]
        public IActionResult GetEmployyDetails([FromRoute] String roleid,Int32 pageno)
        {
            String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
            var jwtdata=_jwtDecoder.Decode(authHeader);
            String nid=jwtdata.nid;
            String role=jwtdata.role;

            if(role!="ITAdmin")
            {
                return Unauthorized();
            }

            SqlParameter param1=new SqlParameter("@str_RoleId",roleid);
            SqlParameter param2=new SqlParameter("@i_pageno",pageno);
            SqlParameter param3=new SqlParameter("@i_rowstoreturn",_appSettings.PaginationRecordCount);
            
            var result=  _db.Set<EmployeeDetailsViewModel>().FromSqlRaw(StoredProcedure.usp_AspNetUsers_GetEmployee,param1,param2,param3).ToList();

                return Ok(result);
        }
    
        private async void RequestNotification(Object Obj)
        {
            var todoItemJson = new StringContent(
                Obj.ToString(),//JsonSerializer.Serialize(todoItem, _jsonSerializerOptions),
                Encoding.UTF8,
                "application/json");
            var _client = _clientFactory.CreateClient("notification");
            _client.BaseAddress=new Uri(_appSettings.NotificationServer);

            using var httpResponse =
            await _client.PostAsync("api/SendNotification/SendNotification", todoItemJson);
        }
    
    }
}