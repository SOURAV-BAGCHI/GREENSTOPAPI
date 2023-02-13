using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CommonMethod;
using CommonMethodLib;
using GreenStop.API.Data;
using GreenStop.API.Helpers;
using GreenStop.API.Models;
using GreenStop.API.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models.ViewModels;
using Models.ViewModels.StoredProcedureViewModel;
using DinkToPdf;
using DinkToPdf.Contracts;
using SelectPdf;
using System.Net.Http;

namespace GreenStop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController:ControllerBase
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
        private readonly IJwtDecoder _jwtDecoder;
        private readonly AppSettings _appSettings;
        private readonly IWebHostEnvironment _env;
        private IConverter _converter;
        IHttpClientFactory _clientFactory;
        public OrderController(ApplicationDbContext db,IJwtDecoder jwtDecoder,
        IOptions<AppSettings> appSettings,IWebHostEnvironment env,IConverter converter,
        IHttpClientFactory clientFactory)
        {
            _db=db;
            _jwtDecoder=jwtDecoder;
            _appSettings=appSettings.Value;
            _env=env;
            _converter = converter;
            _clientFactory=clientFactory;
        }

        [HttpPost("[action]")]
        // [Authorize(Policy="RequireCustomerLoggedIn")]
        public async Task<IActionResult> Create([FromBody] OrderModel formData)
        {
            String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
            String nid=_jwtDecoder.Decode(authHeader).nid;

            DateTime now=DateTime.Now;
            List<StatusLogViewModel> orderStatusLog=new List<StatusLogViewModel>();

            // formData.OrderId="WEB"+now.Ticks.ToString();
            formData.OrderId="WEB"+(now.Year-2000).ToString()+now.Month.ToString()+now.Day.ToString()+now.Hour.ToString()+now.Minute.ToString()+now.Second.ToString()+now.Millisecond.ToString();
            formData.UserId=nid;
            formData.CreateDate=now;
            formData.OrderStatus=(Int32)Status.OrderPlaced;
            orderStatusLog.Add(new StatusLogViewModel{
                Status=0,
                By="GREENSTOP",
                On=now,
                To=now.Ticks.ToString().Substring(now.Ticks.ToString().Length-4)
            });
            orderStatusLog.Add(new StatusLogViewModel{
                Status=formData.OrderStatus,
                By=nid,
                On=now,
                To="GREENSTOP"
            });
            formData.OrderStatusLog=JsonSerializer.Serialize(orderStatusLog);
            formData.CustomerServiceStatus="0";

            await _db.OrderDetails.AddAsync(formData);
            await _db.SaveChangesAsync();

             var Obj= JsonSerializer.Serialize(formData);
            

            ParameterizedThreadStart parameterizedThreadStartSendOrderDetails = new ParameterizedThreadStart(SendOrderDetails);
            Thread Thread1 = new Thread(parameterizedThreadStartSendOrderDetails);
            Thread1.Start(Obj);

            var NotificationReqBody=new NotificationRequestViewModel()
            {
                OrderId=formData.OrderId,
                Status=formData.OrderStatus
            };

            var ObjNotification=JsonSerializer.Serialize(NotificationReqBody);
            // HttpRequestHelper requestHelper=new HttpRequestHelper();
            ParameterizedThreadStart parameterizedThreadStartRequestNotification = new ParameterizedThreadStart(RequestNotification);
            Thread ThreadNotification = new Thread(parameterizedThreadStartRequestNotification);
            ThreadNotification.Start(ObjNotification);

            return Ok(new{OrderId=formData.OrderId});

        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetOrderList([FromBody] OrderListRequestViewModel formdata)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            
            DateTime? Date=null;
            if(String.IsNullOrEmpty(formdata.date))
            {
                Date= DateTime.ParseExact(formdata.date, "dd-MM-yyyy", provider);
            }
            // DateTime Date=(String.IsNullOrEmpty(formdata.date)?DateTime.Now:DateTime.ParseExact(formdata.date, "dd-MM-yyyy", provider));

            String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
            var token=_jwtDecoder.Decode(authHeader);

            String nid=token.nid;
            String role=token.role;

            SqlParameter param1=new SqlParameter("@str_role",role);
            SqlParameter param2=new SqlParameter("@dt_date",Date);
            SqlParameter param3=new SqlParameter("@i_timeslot",formdata.timeslotid);
            SqlParameter param4=new SqlParameter("@i_offset",formdata.lastrecord);
            SqlParameter param5=new SqlParameter("@i_record_size",_appSettings.RecordCountPerPage);
            SqlParameter param6=new SqlParameter("@i_list_type",formdata.listtype);
            SqlParameter param7=new SqlParameter("@str_reqdId",nid);
            
            var result= await _db.Set<OrderListViewModel>().FromSqlRaw(StoredProcedure.usp_Order_GetList,param1,param2,param3,param4,param5,param6,param7).ToListAsync();

            if(result.Count<_appSettings.RecordCountPerPage)
            {
                formdata.lastrecord=0;

            }
            else
            {
                formdata.lastrecord+=result.Count;
            }

            return Ok(new{Record=result,lastrecord=formdata.lastrecord});
        }

        [HttpGet("[action]")]
        public IActionResult GetOrderDetailsMinCount()
        {
            String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
            String nid=_jwtDecoder.Decode(authHeader).nid;

            SqlParameter param1=new SqlParameter("@str_userid",nid);
            
            var result=  _db.Set<GeneralListViewModel>().FromSqlRaw(StoredProcedure.usp_OrderDetails_GetOrderDetailsMinCount,param1).ToList().FirstOrDefault();

            return Ok(new {Count=Convert.ToInt32(result.Id)});
        }

        [HttpGet("[action]/{pageno}")]
        // [Authorize(Policy="RequireCustomerLoggedIn")]
        public async Task<IActionResult> GetOrderDetailsMin([FromRoute] Int32 pageno)
        {
            String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
            String nid=_jwtDecoder.Decode(authHeader).nid;

            List<OrderDetailsSmall> resultSet=new List<OrderDetailsSmall>();

            SqlParameter param1=new SqlParameter("@str_userid",nid);
            SqlParameter param2=new SqlParameter("@i_pageno",pageno);
            SqlParameter param3=new SqlParameter("@i_rowstoreturn",_appSettings.PaginationRecordCount);
            
            // var data=await _db.OrderDetails.Where(x=>x.UserId==nid).OrderByDescending(x=>x.CreateDate).ToListAsync();
            var data= await _db.Set<OrderModel>().FromSqlRaw(StoredProcedure.usp_OrderDetails_GetOrderDetailsMin,param1,param2,param3).ToListAsync();

            Dictionary<Int32,String> timeslots=new Dictionary<int, string>();
            if(data !=null)
            {
                timeslots.Add(1,"11 am - 12 pm");
                timeslots.Add(2,"12 pm - 1 pm");
                timeslots.Add(3,"1 pm - 2 pm");
                timeslots.Add(4,"2 pm - 3 pm");
                timeslots.Add(5,"3 pm - 4 pm");
                timeslots.Add(6,"4 pm - 5 pm");
                timeslots.Add(7,"5 pm - 6 pm");
                timeslots.Add(8,"6 pm - 7 pm");                                                                                                                
                timeslots.Add(9,"7 pm - 8 pm");
                timeslots.Add(10,"8 pm - 9 pm");
                timeslots.Add(11,"9 pm - 10 pm");
            }

            foreach(var m in data)
            {
                var x=JsonSerializer.Deserialize<List<ItemDetailsViewModel>>(m.ItemDetails);
                var code="0";
                if(m.OrderStatus==(Int32)Status.OrderPickupDone)
                {
                    var statuslog=JsonSerializer.Deserialize<List<StatusLogViewModel>>(m.OrderStatusLog);
                    for(int i=0;i<statuslog.Count;i++)
                    {

                        if(statuslog[i].Status==0)
                        {
                            code=statuslog[i].To;
                            i=statuslog.Count;
                            
                        }
                    }
                }
                resultSet.Add(new OrderDetailsSmall(){
                    name_list=x.Select(p=>p.name).ToList(),
                    note_list=x.Select(p=>p.note).ToList(),
                    total=m.SubTotal+m.DeliveryCharges,
                    status=m.OrderStatus,
                    orderdate=m.CreateDate,
                    deliverydate=m.DeliveryDate,
                    deliverytime=timeslots[m.DeliveryTimeId],
                    orderid=m.OrderId,
                    code=code,//m.OrderId.Substring(m.OrderId.Length-4),
                    qty=x.Select(p=>p.quantity).ToList()
                });
            }

            return Ok(resultSet);

        }
    
        [HttpPut("[action]")]
        public IActionResult SetOrderCancel([FromBody] GeneralListViewModel formdata)
        {
            String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
            String nid=_jwtDecoder.Decode(authHeader).nid;


            SqlParameter param1=new SqlParameter("@str_OrderId",formdata.Id);
            SqlParameter param2=new SqlParameter("@str_UserIdBy",nid);
            SqlParameter param3=new SqlParameter("@str_UserIdTo",nid);
            SqlParameter param4=new SqlParameter("@i_OrderStatusNew",9);
            
            var result=  _db.Set<GeneralListViewModel>().FromSqlRaw(StoredProcedure.usp_OrderDetails_SetAction,param1,param2,param3,param4).ToList().FirstOrDefault();

            return Ok(new {Success=result.Id,Message=result.Value});
            

        }

        [HttpGet("[action]/{orderid}/{useridby}/{useridto}/{status}")]
        public IActionResult SetOrderAction([FromRoute] String orderid,String useridby,String useridto,Int32 status)
        {
            
            SqlParameter param1=new SqlParameter("@str_OrderId",orderid);
            SqlParameter param2=new SqlParameter("@str_UserIdBy",useridby);
            SqlParameter param3=new SqlParameter("@str_UserIdTo",useridto);
            SqlParameter param4=new SqlParameter("@i_OrderStatusNew",status);
            
            var result=  _db.Set<GeneralListViewModel>().FromSqlRaw(StoredProcedure.usp_OrderDetails_SetAction,param1,param2,param3,param4).ToList().FirstOrDefault();
            
            var NotificationReqBody=new NotificationRequestViewModel()
            {
                OrderId=orderid,
                Status=status
            };

            var Obj=JsonSerializer.Serialize(NotificationReqBody);

            ParameterizedThreadStart parameterizedThreadStartRequestNotification = new ParameterizedThreadStart(RequestNotification);
            Thread Thread1 = new Thread(parameterizedThreadStartRequestNotification);
            Thread1.Start(Obj);
            return Ok(new {Success=result.Id,Message=result.Value});
        }

        

        [HttpGet("[action]")]
        public IActionResult GetCustomerInfo(){
            
            String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
            String nid=_jwtDecoder.Decode(authHeader).nid;

            var data=_db.OrderDetails.Where(x=>x.UserId==nid).OrderByDescending(x=>x.CreateDate).Take(1).Select(x=>x.CustomerInfo).ToList().FirstOrDefault();

            if(!String.IsNullOrEmpty(data))
            {
                
                return Ok(data);
            }
            return BadRequest("Data not found");

        
        }


        [HttpGet("[action]/{date}/{deliverytimeid}/{pageno}")]
        public IActionResult GetCurrentOrderDetails([FromRoute] String date,Int32 deliverytimeid,Int32 pageno)
        {
            String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
            var jwtdata=_jwtDecoder.Decode(authHeader);
            String nid=jwtdata.nid;
            String role=jwtdata.role;

            DateTime  dateTime=new DateTime(2000, 01, 01); ;
            if(!String.IsNullOrEmpty(date) && date!="none")
            {
                CultureInfo provider = CultureInfo.InvariantCulture;  
            
                dateTime = DateTime.ParseExact(date, "dd-MM-yyyy", provider);
                // dateTime=DateTime.Now;
            }

            // if(dateTime!=null)
            // {
                SqlParameter param1=new SqlParameter("@dt_orderdate",dateTime);
                SqlParameter param2=new SqlParameter("@str_userid",nid);
                SqlParameter param3=new SqlParameter("@i_deliverytimeid",deliverytimeid);
                SqlParameter param4=new SqlParameter("@str_role",role);
                SqlParameter param5=new SqlParameter("@i_pageno",pageno);
                SqlParameter param6=new SqlParameter("@i_rowstoreturn",_appSettings.PaginationRecordCount);
            
                var result=  _db.Set<OrderModel>().FromSqlRaw(StoredProcedure.usp_GetCurrentOrderDetails,param1,param2,param3,param4,param5,param6).ToList();

                return Ok(new {Result=result});
            
            // }
            // else
            // {
            //     var result=  _db.Set<OrderModel>().FromSqlRaw("usp_GetCurrentOrderDetails").ToList();

            //     return Ok(new {Result=result});
            // }

            

        }
        
        [HttpGet("[action]/{date}/{deliverytimeid}")]
        public IActionResult GetCurrentOrderDetailsCount([FromRoute] String date,Int32 deliverytimeid)
        {
            String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
            var jwtdata=_jwtDecoder.Decode(authHeader);
            String nid=jwtdata.nid;
            String role=jwtdata.role;
            DateTime  dateTime=new DateTime(2000, 01, 01); ;
            if(!String.IsNullOrEmpty(date) && date!="none")
            {
                CultureInfo provider = CultureInfo.InvariantCulture;  
            
                dateTime = DateTime.ParseExact(date, "dd-MM-yyyy", provider);
            }

            SqlParameter param1=new SqlParameter("@dt_orderdate",dateTime);
            SqlParameter param2=new SqlParameter("@str_userid",nid);
            SqlParameter param3=new SqlParameter("@i_deliverytimeid",deliverytimeid);
            SqlParameter param4=new SqlParameter("@str_role",role);

            var result=  _db.Set<GeneralListViewModel>().FromSqlRaw(StoredProcedure.usp_GetCurrentOrderDetailsCount,param1,param2,param3,param4).ToList().FirstOrDefault();

            return Ok(new {Count=Convert.ToInt32(result.Id)});

        }

        // [HttpGet("[action]/{date}")]
        // public IActionResult GetOrderDetailsHistory([FromRoute] String date)
        // {
        //     DateTime ? dateTime=null;
        //     if(!String.IsNullOrEmpty(date) && date!="none")
        //     {
        //         CultureInfo provider = CultureInfo.InvariantCulture;  
            
        //         dateTime = DateTime.ParseExact(date, "dd-MM-yyyy", provider);
        //         // dateTime=DateTime.Now;
        //     }

        //      if(dateTime!=null)
        //     {
        //         SqlParameter param1=new SqlParameter("@dt_orderdate",dateTime);
                
        //         var result=  _db.Set<OrderModel>().FromSqlRaw(StoredProcedure.usp_GetOrderDetailsHistory,param1).ToList();

        //         return Ok(new {Result=result});
        //      }
        //     else
        //     {
        //         var result=  _db.Set<OrderModel>().FromSqlRaw("usp_GetOrderDetailsHistory").ToList();

        //         return Ok(new {Result=result});
        //     }
        // }

        [HttpGet("[action]/{date}/{deliverytimeid}/{pageno}")]
        public IActionResult GetOrderDetailsHistory([FromRoute] String date,Int32 deliverytimeid,Int32 pageno)
        {
            String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
            var jwtdata=_jwtDecoder.Decode(authHeader);
            String nid=jwtdata.nid;
            String role=jwtdata.role;
            DateTime  dateTime=new DateTime(2000, 01, 01); ;
            if(!String.IsNullOrEmpty(date) && date!="none")
            {
                CultureInfo provider = CultureInfo.InvariantCulture;  
            
                dateTime = DateTime.ParseExact(date, "dd-MM-yyyy", provider);
            }

                SqlParameter param1=new SqlParameter("@dt_orderdate",dateTime);
                SqlParameter param2=new SqlParameter("@str_userid",nid);
                SqlParameter param3=new SqlParameter("@i_deliverytimeid",deliverytimeid);
                SqlParameter param4=new SqlParameter("@str_role",role);
                SqlParameter param5=new SqlParameter("@i_pageno",pageno);
                SqlParameter param6=new SqlParameter("@i_rowstoreturn",_appSettings.PaginationRecordCount);
            
                var result=  _db.Set<OrderModel>().FromSqlRaw(StoredProcedure.usp_GetOrderDetailsHistory,param1,param2,param3,param4,param5,param6).ToList();

                return Ok(new {Result=result});

        }


        [HttpGet("[action]/{date}/{deliverytimeid}")]
        public IActionResult GetOrderDetailsHistoryCount([FromRoute] String date,Int32 deliverytimeid)
        {
            String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
            var jwtdata=_jwtDecoder.Decode(authHeader);
            String nid=jwtdata.nid;
            String role=jwtdata.role;
            DateTime  dateTime=new DateTime(2000, 01, 01); ;
            if(!String.IsNullOrEmpty(date) && date!="none")
            {
                CultureInfo provider = CultureInfo.InvariantCulture;  
            
                dateTime = DateTime.ParseExact(date, "dd-MM-yyyy", provider);
            }

            SqlParameter param1=new SqlParameter("@dt_orderdate",dateTime);
            SqlParameter param2=new SqlParameter("@str_userid",nid);
            SqlParameter param3=new SqlParameter("@i_deliverytimeid",deliverytimeid);
            SqlParameter param4=new SqlParameter("@str_role",role);

            var result=  _db.Set<GeneralListViewModel>().FromSqlRaw(StoredProcedure.usp_GetOrderDetailsHistoryCount,param1,param2,param3,param4).ToList().FirstOrDefault();

            return Ok(new {Count=Convert.ToInt32(result.Id)});

        }

        [HttpGet("[action]/{OrderId}")]
        public async Task<IActionResult> GetOrderDetails([FromRoute] String OrderId)
        {
            var OrderDetails=await _db.OrderDetails.FindAsync(OrderId);

            return Ok(OrderDetails);
        }

        [HttpGet("[action]")]
        public IActionResult TestNotificationSend()
        {
            var NotificationReqBody=new NotificationRequestViewModel()
            {
                OrderId="WEB2145987520",
                Status=9
            };

            var Obj=JsonSerializer.Serialize(NotificationReqBody);

            ParameterizedThreadStart parameterizedThreadStartSendOrderDetails = new ParameterizedThreadStart(RequestNotification);
            Thread Thread1 = new Thread(parameterizedThreadStartSendOrderDetails);
            Thread1.Start(Obj);

            return Ok("Send");
        }

        [HttpGet("[action]/{OrderId}")]
        public async Task<IActionResult> GetOrderDocument([FromRoute] String OrderId)
        {
            var data= await _db.OrderDetails.FindAsync(OrderId);
            String EmailTemplate=CreateHtmlString(data);

            String contentRootPath = _env.ContentRootPath;
            String Message=String.Empty;

            String logoPath=_appSettings.Site+"/GS_Logo.png";
            String localLogoPath=Path.Combine(_env.WebRootPath,"GS_Logo.png") ;

            EmailTemplate = EmailTemplate.Replace(logoPath, localLogoPath);

            string contentType = "application/pdf";
            var fileName="WEB_"+data.OrderId.Substring(data.OrderId.Length-4)+"_"+data.DeliveryDate.ToString("dd-MM-yyyy")+".pdf";

            HtmlToPdf oHtmlToPdf=new HtmlToPdf();
            PdfDocument oPdfDocument=oHtmlToPdf.ConvertHtmlString(EmailTemplate);

            using (var stream = new MemoryStream())
            {
                oPdfDocument.Save(stream);
                var content = stream.ToArray();
                return File(content, contentType, fileName);
            }

        }
       
        private void SendOrderDetails(Object Obj)
        {
            var data=JsonSerializer.Deserialize<OrderModel>(Obj.ToString());
            String EmailTemplate=CreateHtmlString(data);
            // data.CreateDate=data.CreateDate.AddMinutes(330);
            // var customerInfo=JsonSerializer.Deserialize<CustomerInfoViewModel>(data.CustomerInfo);
            // var itemDetails=JsonSerializer.Deserialize<List<ItemDetailsViewModel>>(data.ItemDetails);

            // Dictionary<Int32,String> timeslots=new Dictionary<int, string>();
            // if(data !=null)
            // {
            //     timeslots.Add(1,"11 am - 12 pm");
            //     timeslots.Add(2,"12 pm - 1 pm");
            //     timeslots.Add(3,"1 pm - 2 pm");
            //     timeslots.Add(4,"2 pm - 3 pm");
            //     timeslots.Add(5,"3 pm - 4 pm");
            //     timeslots.Add(6,"4 pm - 5 pm");
            //     timeslots.Add(7,"5 pm - 6 pm");
            //     timeslots.Add(8,"6 pm - 7 pm");                                                                                                                
            //     timeslots.Add(9,"7 pm - 8 pm");
            //     timeslots.Add(10,"8 pm - 9 pm");
            //     timeslots.Add(11,"9 pm - 10 pm");
            // }

            // String contentRootPath = _env.ContentRootPath;
            // String path = Path.Combine(contentRootPath , "Content","EmailTemplates","Order_details.html");
            // String EmailTemplate = String.Empty;
            // EmailTemplate = CommonMethodd.ReadHtmlFile(path);
            // String logoPath=_appSettings.Site+"/GS_Logo.png";

            // EmailTemplate = EmailTemplate.Replace("@@LOGO@@",logoPath);
            // EmailTemplate = EmailTemplate.Replace("@@CLIENTNAME@@",customerInfo.Name);
            // EmailTemplate = EmailTemplate.Replace("@@ADDRESSLINE1@@",customerInfo.AddressLine1);
            // EmailTemplate = EmailTemplate.Replace("@@ADDRESSLINE2@@",customerInfo.AddressLine2);
            // EmailTemplate = EmailTemplate.Replace("@@LANDMARK@@",customerInfo.Landmark);
            // EmailTemplate = EmailTemplate.Replace("@@PINCODE@@",customerInfo.Pincode);
            // EmailTemplate = EmailTemplate.Replace("@@PHONE@@",customerInfo.Phone);
            // EmailTemplate = EmailTemplate.Replace("@@ALTERNATEPHONE@@",customerInfo.AlternatePhone);

            // EmailTemplate = EmailTemplate.Replace("@@ORDERID@@",data.OrderId);

            // EmailTemplate = EmailTemplate.Replace("@@ORDERDATETIME@@",data.CreateDate.ToString("dd/MM/yyyy hh:mm tt"));
            // EmailTemplate = EmailTemplate.Replace("@@DELIVERYDATE@@",data.DeliveryDate.ToString("dd/MM/yyyy"));
            // EmailTemplate = EmailTemplate.Replace("@@TIMESLOT@@",timeslots[data.DeliveryTimeId]);   //just do it alredy

            // EmailTemplate=EmailTemplate.Replace("[CLIENTS.WEBSITE]",_appSettings.ClientSite);

            // Double price=0.0;
            // var builder = new StringBuilder();
            // using (var xmlwriter = XmlWriter.Create(builder))
            // {
            //     xmlwriter.WriteStartElement("table");
            //     xmlwriter.WriteAttributeString("class", "tbl-s100p");

            //     xmlwriter.WriteStartElement("thead");
            //     xmlwriter.WriteAttributeString("class", "tbl-header");

            //         xmlwriter.WriteStartElement("tr");
            //             xmlwriter.WriteStartElement("td");
            //                 xmlwriter.WriteElementString("b","Item");
            //             xmlwriter.WriteEndElement();
            //             xmlwriter.WriteStartElement("td");
            //                 xmlwriter.WriteElementString("b", "Qty");
            //             xmlwriter.WriteEndElement();
            //             xmlwriter.WriteStartElement("td");
            //                 xmlwriter.WriteElementString("b", "Amt (Rs.)");
            //             xmlwriter.WriteEndElement();
            //         xmlwriter.WriteEndElement();


            //     xmlwriter.WriteEndElement();

            //     xmlwriter.WriteStartElement("tbody");
            //     foreach(var m in itemDetails)
            //     {
            //         xmlwriter.WriteStartElement("tr");
            //             xmlwriter.WriteStartElement("td");
            //                 xmlwriter.WriteElementString("span", m.name);
            //             xmlwriter.WriteEndElement();
            //             xmlwriter.WriteStartElement("td");
            //                 xmlwriter.WriteElementString("span", m.quantity.ToString());
            //             xmlwriter.WriteEndElement();
            //             xmlwriter.WriteStartElement("td");
            //                 xmlwriter.WriteElementString("span", m.price.ToString());
            //             xmlwriter.WriteEndElement();
            //         xmlwriter.WriteEndElement();

            //         price+=(m.price*m.quantity);
            //     }
            //     xmlwriter.WriteEndElement();


            //     xmlwriter.WriteEndElement();
            // }

            // String xmlHeader = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";
            // String XmlString = builder.ToString().Substring(xmlHeader.Length);
            
            // EmailTemplate = EmailTemplate.Replace("@@ORDERDETAILS@@", XmlString);
            // EmailTemplate = EmailTemplate.Replace("@@ITEMTOTAL@@", price.ToString());
            
            // EmailTemplate = EmailTemplate.Replace("@@DELIVERYCHARGE@@",data.DeliveryCharges.ToString());
            // EmailTemplate = EmailTemplate.Replace("@@TOTALBILLVALUE@@", (price+data.DeliveryCharges).ToString());
            // EmailTemplate=EmailTemplate.Replace("@@TERMSANDCONDITIONLINK@@",_appSettings.ClientSite+"termsandconditions");

            List<String> emailAddressList=new List<string>()
            {
                "sbagchi19@gmail.com",
                "csgst113@gmail.com",
                "acgst113@gmail.com",
                "kdgst113@gmail.com"
            };

            var attachment=CreatePDF(EmailTemplate);
            var OrderFileName="WEB_"+data.OrderId.Substring(data.OrderId.Length-4)+"_"+data.DeliveryDate.ToString("dd-MM-yyyy");//"Order_"+data.OrderId+".pdf"

            var IsMailSend=CommonMethodd.SendMail(_appSettings.UserName,_appSettings.Password, EmailTemplate, emailAddressList, "Order Details",_appSettings.Host,attachment,OrderFileName,_appSettings.Port,_appSettings.EnableSsl);
            
        }

        private string CreateHtmlString(OrderModel data)
        {
            data.CreateDate=data.CreateDate.AddMinutes(330);
            var customerInfo=JsonSerializer.Deserialize<CustomerInfoViewModel>(data.CustomerInfo);
            var itemDetails=JsonSerializer.Deserialize<List<ItemDetailsViewModel>>(data.ItemDetails);

            Dictionary<Int32,String> timeslots=new Dictionary<int, string>();
            if(data !=null)
            {
                timeslots.Add(1,"11 am - 12 pm");
                timeslots.Add(2,"12 pm - 1 pm");
                timeslots.Add(3,"1 pm - 2 pm");
                timeslots.Add(4,"2 pm - 3 pm");
                timeslots.Add(5,"3 pm - 4 pm");
                timeslots.Add(6,"4 pm - 5 pm");
                timeslots.Add(7,"5 pm - 6 pm");
                timeslots.Add(8,"6 pm - 7 pm");                                                                                                                
                timeslots.Add(9,"7 pm - 8 pm");
                timeslots.Add(10,"8 pm - 9 pm");
                timeslots.Add(11,"9 pm - 10 pm");
            }

            String contentRootPath = _env.ContentRootPath;
            String path = Path.Combine(contentRootPath , "Content","EmailTemplates","Order_details.html");
            String EmailTemplate = String.Empty;
            EmailTemplate = CommonMethodd.ReadHtmlFile(path);
            String logoPath=_appSettings.Site+"/GS_Logo.png";

            EmailTemplate = EmailTemplate.Replace("@@LOGO@@",logoPath);
            EmailTemplate = EmailTemplate.Replace("@@CLIENTNAME@@",customerInfo.Name);
            EmailTemplate = EmailTemplate.Replace("@@ADDRESSLINE1@@",customerInfo.AddressLine1);
            EmailTemplate = EmailTemplate.Replace("@@ADDRESSLINE2@@",customerInfo.AddressLine2);
            EmailTemplate = EmailTemplate.Replace("@@LANDMARK@@",customerInfo.Landmark);
            EmailTemplate = EmailTemplate.Replace("@@PINCODE@@",customerInfo.Pincode);
            EmailTemplate = EmailTemplate.Replace("@@PHONE@@",customerInfo.Phone);
            EmailTemplate = EmailTemplate.Replace("@@ALTERNATEPHONE@@",customerInfo.AlternatePhone);

            EmailTemplate = EmailTemplate.Replace("@@ORDERID@@",data.OrderId);

            EmailTemplate = EmailTemplate.Replace("@@ORDERDATETIME@@",data.CreateDate.ToString("dd/MM/yyyy hh:mm tt"));
            EmailTemplate = EmailTemplate.Replace("@@DELIVERYDATE@@",data.DeliveryDate.ToString("dd/MM/yyyy"));
            EmailTemplate = EmailTemplate.Replace("@@TIMESLOT@@",timeslots[data.DeliveryTimeId]);   //just do it alredy

            EmailTemplate=EmailTemplate.Replace("[CLIENTS.WEBSITE]",_appSettings.ClientSite);

            Double price=0.0;
            var builder = new StringBuilder();
            using (var xmlwriter = XmlWriter.Create(builder))
            {
                xmlwriter.WriteStartElement("table");
                xmlwriter.WriteAttributeString("class", "tbl-s100p");

                xmlwriter.WriteStartElement("thead");
                xmlwriter.WriteAttributeString("class", "tbl-header");

                    xmlwriter.WriteStartElement("tr");
                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteElementString("b","Item");
                        xmlwriter.WriteEndElement();
                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteElementString("b", "Qty");
                        xmlwriter.WriteEndElement();
                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteElementString("b", "Price (Rs.)");
                        xmlwriter.WriteEndElement();
                    xmlwriter.WriteEndElement();


                xmlwriter.WriteEndElement();

                xmlwriter.WriteStartElement("tbody");
                String OrderItemName=String.Empty;
                foreach(var m in itemDetails)
                {
                    OrderItemName=m.name+(m.note.Length>0?" ( "+m.note+" )":"");

                    xmlwriter.WriteStartElement("tr");
                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteElementString("span", OrderItemName);
                        xmlwriter.WriteEndElement();
                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteElementString("span", m.quantity.ToString());
                        xmlwriter.WriteEndElement();
                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteElementString("span", m.price.ToString());
                        xmlwriter.WriteEndElement();
                    xmlwriter.WriteEndElement();

                    price+=(m.price*m.quantity);
                }
                xmlwriter.WriteEndElement();


                xmlwriter.WriteEndElement();
            }

            String xmlHeader = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";
            String XmlString = builder.ToString().Substring(xmlHeader.Length);
            
            EmailTemplate = EmailTemplate.Replace("@@ORDERDETAILS@@", XmlString);
            EmailTemplate = EmailTemplate.Replace("@@ITEMTOTAL@@", price.ToString());
            
            EmailTemplate = EmailTemplate.Replace("@@DELIVERYCHARGE@@",data.DeliveryCharges.ToString());
            EmailTemplate = EmailTemplate.Replace("@@TOTALBILLVALUE@@", (price+data.DeliveryCharges).ToString());
            EmailTemplate=EmailTemplate.Replace("@@TERMSANDCONDITIONLINK@@",_appSettings.ClientSite+"termsandconditions");

            return EmailTemplate;
        }
        private byte[] CreatePDF(String template){

            byte [] pdf=null;
            try
            {
                String contentRootPath = _env.ContentRootPath;
                String Message=String.Empty;

                String logoPath=_appSettings.Site+"/GS_Logo.png";
                String localLogoPath=Path.Combine(_env.WebRootPath,"GS_Logo.png") ;

                template = template.Replace(logoPath, localLogoPath);

                HtmlToPdf oHtmlToPdf=new HtmlToPdf();
                PdfDocument oPdfDocument=oHtmlToPdf.ConvertHtmlString(template);
                pdf=oPdfDocument.Save();
                oPdfDocument.Close();
            }
            catch(Exception)
            {

            }
            finally{

            }
            

            return pdf;
        }
        private MemoryStream CreatePDFStream(String template){
            String contentRootPath = _env.ContentRootPath;
            String Message=String.Empty;

            String logoPath=_appSettings.Site+"/GS_Logo.png";
            String localLogoPath=Path.Combine(_env.WebRootPath,"GS_Logo.png") ;

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "Order Details"
            };

            template = template.Replace(logoPath, localLogoPath);

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = template,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet =  "" }
            };

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            var pdf = _converter.Convert(doc);
            var stream = new MemoryStream(pdf);

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    
        private String CretePDF(String template,String Filename)
        {
            String contentRootPath = _env.ContentRootPath;
            String Message=String.Empty;
        //    String path = System.IO.Path.Combine(contentRootPath , "Content","EmailTemplates","Order_details_gs.html");
            
            String outputFolder= System.IO.Path.Combine(contentRootPath,"Content","PDFs");
            String outputPath= System.IO.Path.Combine(outputFolder,Filename);//System.IO.Path.Combine(contentRootPath,"Content","PDFs",DateTime.Now.Ticks.ToString()+".pdf");

            String logoPath=_appSettings.Site+"/GS_Logo.png";
            String localLogoPath=Path.Combine(_env.WebRootPath,"GS_Logo.png") ;
            if(!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "Order Details",
                Out = outputPath
            };

            template = template.Replace(logoPath, localLogoPath);

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = template,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet =  "" },
                // HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                // FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" }
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            _converter.Convert(pdf);

            return outputPath;

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