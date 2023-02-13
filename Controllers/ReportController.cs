using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ClosedXML.Excel;
using CommonMethod;
using GreenStop.API.Data;
using GreenStop.API.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GreenStop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController:ControllerBase
    {
         private readonly ApplicationDbContext _db;
         private readonly IJwtDecoder _jwtDecoder;
        public ReportController(ApplicationDbContext db,IJwtDecoder jwtDecoder)
        {
            _db=db;
            _jwtDecoder=jwtDecoder;
        }

        [HttpGet("[action]/{date}")]
        public IActionResult GetTime_slot_wise_items_to_cook([FromRoute] String date)
        {
            try
            {
                // String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
                // var jwtdata=_jwtDecoder.Decode(authHeader);
                // String nid=jwtdata.nid;
                // String role=jwtdata.role;
                CultureInfo provider = CultureInfo.InvariantCulture;  
                DateTime  dateTime=DateTime.ParseExact(date, "dd-MM-yyyy", provider);
                Int32 OrderStatusCancelled=9;
                Int32 OrderStatusPlaced=1;
                IDictionary<Int32,Int32> columnNumber=new Dictionary<int, int>();
                IDictionary<Int32,Int32> rowNumber=new Dictionary<int, int>();
                IDictionary<Int32,Dictionary<Int32,Int32>> orderItemRecord=new Dictionary<int, Dictionary<int, int>>();
                IDictionary<Int32,Int32> orderListTotal=new Dictionary<Int32,Int32>();

                var myCustomStyleRed = XLWorkbook.DefaultStyle;
                // myCustomStyleRed.Fill.SetPatternColor(XLColor.Red);
                myCustomStyleRed.Font.SetFontColor(XLColor.Red);
                myCustomStyleRed.Font.SetBold(true);
                // myCustomStyleRed.Font.SetFontSize(16);

                var myCustomStyleTitle = XLWorkbook.DefaultStyle;
                myCustomStyleTitle.Font.SetFontColor(XLColor.Black);
                myCustomStyleTitle.Font.SetBold(true);
                // myCustomStyleTitle.Font.SetFontSize(16);

                var myCustomStyleGreen = XLWorkbook.DefaultStyle;
                myCustomStyleGreen.Font.SetFontColor(XLColor.Green);

                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = "Time_slot_wise_items_to_cook_"+dateTime.ToString("dd/MM/yyyy")+".xlsx";
                    

                var ItemList=_db.ItemDetails
                .Select(x=>new{
                    x.Id,
                    x.Name1,
                    x.Name2
                }).ToList();

                var DeliveryTimeList=_db.DeliveryTimeDetails.ToList();

                var OrderList=_db.OrderDetails
                                .Where(x=>x.DeliveryDate==dateTime && x.OrderStatus>OrderStatusPlaced && x.OrderStatus<OrderStatusCancelled)
                                .Select(x=>new {
                                    x.ItemDetails,x.DeliveryTimeId
                                }).ToList();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Time slot wise items to cook");
                    var currentRow = 1;
                    var currentCol=2;

                    worksheet.Cell(currentRow, 1).Value = "Time slot wise items to cook";
                    worksheet.Cell(currentRow,1).Style=myCustomStyleTitle;
                    currentRow=3;
                    worksheet.Cell(currentRow, 1).Value = "Delivery on:";
                    worksheet.Cell(currentRow, 3).Value = "Report on:";
                    worksheet.Cell(currentRow, 4).Value = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");
                    currentRow=4;
                    worksheet.Cell(currentRow, 1).Value = dateTime.ToString("dd/MM/yyyy");

                    currentRow=6;
                    worksheet.Cell(currentRow, 1).Value = "Time Slot";
                    worksheet.Cell(currentRow, 1).Style=myCustomStyleRed;

                    currentCol=2;
                    foreach(var item in ItemList)
                    {
                        worksheet.Cell(currentRow, currentCol).Value = item.Name1;
                        worksheet.Cell(currentRow+1, currentCol).Value = item.Id;
                        worksheet.Cell(currentRow+1, currentCol).Style=myCustomStyleGreen;

                        columnNumber.Add(item.Id,currentCol++);
                    }
                    currentRow=9;
                    foreach(var time_item in DeliveryTimeList)
                    {
                        worksheet.Cell(currentRow, 1).Value = time_item.TimeSlot;
                        rowNumber.Add(time_item.DeliveryTimeId,currentRow);
                        currentRow++;
                    }

                    worksheet.Cell(currentRow, 1).Value ="#Tot";
                    rowNumber.Add(101,currentRow);  //101 is just a default number for marking #Tot Row

                    foreach(var odr in OrderList)
                    {
                        var itemDetailsList=JsonSerializer.Deserialize<List<ItemDetailsViewModel>>(odr.ItemDetails);
                        foreach(var item in itemDetailsList)
                        {
                            if(orderItemRecord.ContainsKey(odr.DeliveryTimeId))
                            {
                                if(orderItemRecord[odr.DeliveryTimeId].ContainsKey(item.id))
                                {
                                    orderItemRecord[odr.DeliveryTimeId][item.id]+=item.quantity;
                                }
                                else
                                {
                                    //orderItemRecord[odr.DeliveryTimeId]=new Dictionary<int, int>();
                                    orderItemRecord[odr.DeliveryTimeId].Add(item.id,item.quantity);
                                }
                            }
                            else
                            {
                                var tempDict=new Dictionary<int,int>();
                                tempDict.Add(item.id,item.quantity);
                                orderItemRecord.Add(odr.DeliveryTimeId,tempDict);
                            }
                        }
                    }

                    foreach(var item in orderItemRecord)
                    {
                        foreach(var subitem in item.Value)
                        {
                            worksheet.Cell(rowNumber[item.Key], columnNumber[subitem.Key]).Value = subitem.Value;
                            
                            // for listng total item at the ending
                            if(orderListTotal.ContainsKey(subitem.Key))
                            {
                                orderListTotal[subitem.Key]+=subitem.Value;
                            }
                            else
                            {
                                orderListTotal.Add(subitem.Key,subitem.Value);
                            }
                        }
                    }

                    foreach(var item in orderListTotal)
                    {
                        worksheet.Cell(rowNumber[101], columnNumber[item.Key]).Value = item.Value;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, contentType, fileName);
                    }
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
            
        }
    
        [HttpGet("[action]/{date}")]
        public IActionResult GetTime_slot_wise_items_to_deliver([FromRoute] String date)
        {
            try
            {
                // String authHeader = Convert.ToString(Request.HttpContext.Request.Headers["Authorization"]).Substring(7);
                // var jwtdata=_jwtDecoder.Decode(authHeader);
                // String nid=jwtdata.nid;
                // String role=jwtdata.role;
                CultureInfo provider = CultureInfo.InvariantCulture;  
                DateTime  dateTime=DateTime.ParseExact(date, "dd-MM-yyyy", provider);
                Int32 OrderStatusCancelled=9;
                Int32 OrderStatusPlaced=1;
                var myCustomStyleRed = XLWorkbook.DefaultStyle;
                // myCustomStyleRed.Fill.SetPatternColor(XLColor.Red);
                myCustomStyleRed.Font.SetFontColor(XLColor.Red);
                myCustomStyleRed.Font.SetBold(true);
                // myCustomStyleRed.Font.SetFontSize(16);

                var myCustomStyleTitle = XLWorkbook.DefaultStyle;
                myCustomStyleTitle.Font.SetFontColor(XLColor.Black);
                myCustomStyleTitle.Font.SetBold(true);
                // myCustomStyleTitle.Font.SetFontSize(16);

                var myCustomStyleGreen = XLWorkbook.DefaultStyle;
                myCustomStyleGreen.Font.SetFontColor(XLColor.Green);

                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = "Time_slot_wise_items_to_deliver_"+dateTime.ToString("dd/MM/yyyy")+".xlsx";
                
                 var DeliveryTimeList=_db.DeliveryTimeDetails.ToList();
                var OrderList=_db.OrderDetails
                                .Where(x=>x.DeliveryDate==dateTime && x.OrderStatus>OrderStatusPlaced && x.OrderStatus<OrderStatusCancelled)
                                .Select(x=>new {
                                   x.OrderId, 
                                   x.ItemDetails,
                                   x.DeliveryTimeId,
                                   x.CustomerInfo,
                                   x.SubTotal,
                                   x.Tax,
                                   x.DeliveryCharges
                                }).ToList();

                 using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Time slot wise items to deliver");
                    var currentRow = 1;
                    var currentCol=2;

                    worksheet.Cell(currentRow, 1).Value = "Time slot wise items to deliver with details";
                    worksheet.Cell(currentRow,1).Style=myCustomStyleTitle;
                    currentRow=3;
                    worksheet.Cell(currentRow, 1).Value = "Delivery on:";
                    worksheet.Cell(currentRow, 3).Value = "Report on:";
                    worksheet.Cell(currentRow, 4).Value = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");
                    currentRow=4;
                    worksheet.Cell(currentRow, 1).Value = dateTime.ToString("dd/MM/yyyy");

                    currentRow=6;
                    worksheet.Cell(currentRow, 1).Value = "Time Slot";
                    worksheet.Cell(currentRow, 1).Style=myCustomStyleRed;

                    currentRow=8;
                    currentCol=2;

                    worksheet.Cell(currentRow, currentCol++).Value = "#Cust";
                    worksheet.Cell(currentRow, currentCol++).Value = "#Pincode";
                    worksheet.Cell(currentRow, currentCol++).Value = "#Pro";
                    worksheet.Cell(currentRow, currentCol++).Value = "#Qty";
                    worksheet.Cell(currentRow, currentCol++).Value = "#Amt";
                    worksheet.Cell(currentRow, currentCol++).Value = "#Del chg";
                    worksheet.Cell(currentRow, currentCol++).Value = "#Pack chg";
                    worksheet.Cell(currentRow, currentCol++).Value = "#CGST";
                    worksheet.Cell(currentRow, currentCol++).Value = "#SGST";
                    worksheet.Cell(currentRow, currentCol++).Value = "#Tot";
                    worksheet.Cell(currentRow, currentCol++).Value = "#Id";

                    foreach(var time_item in DeliveryTimeList)
                    {
                        var subOrderList=OrderList.Where(x=>x.DeliveryTimeId==time_item.DeliveryTimeId).ToList();
                        
                        currentRow+=2;
                        currentCol=1;

                        worksheet.Cell(currentRow, currentCol).Value = time_item.TimeSlot;
                        worksheet.Cell(currentRow, currentCol++).Style = myCustomStyleGreen;
                        foreach(var odr in subOrderList)
                        {
                            var customerInfo=JsonSerializer.Deserialize<CustomerInfoViewModel>(odr.CustomerInfo);
                            var odrDetails=JsonSerializer.Deserialize<List<ItemDetailsViewModel>>(odr.ItemDetails);

                            worksheet.Cell(currentRow, currentCol++).Value = customerInfo.Name;
                            worksheet.Cell(currentRow, currentCol++).Value = customerInfo.Pincode;

                            foreach(var o in odrDetails)
                            {
                                worksheet.Cell(currentRow, currentCol++).Value = o.name;
                                worksheet.Cell(currentRow, currentCol++).Value = o.quantity;
                                worksheet.Cell(currentRow++, currentCol++).Value = o.price;

                                currentCol-=3;
                            }

                            currentCol+=3;
                            worksheet.Cell(--currentRow, currentCol++).Value = odr.DeliveryCharges;
                            worksheet.Cell(currentRow, currentCol++).Value = odr.Tax;
                            worksheet.Cell(currentRow, currentCol++).Value = odr.Tax;
                            worksheet.Cell(currentRow, currentCol++).Value = odr.Tax;
                            worksheet.Cell(currentRow, currentCol++).Value = odr.SubTotal+odr.DeliveryCharges;
                            worksheet.Cell(currentRow, currentCol++).Value = odr.OrderId;

                            currentRow++;
                            currentCol-=11;

                        }

                       
                        
                        
                    }
                    
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, contentType, fileName);
                    }
                }

            }
            catch(Exception Ex)
            {
                
                return BadRequest(Ex.Message.ToString());
            }
        }
    }
}