using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text.Json;
using CommonMethodLib;
using DinkToPdf;
using DinkToPdf.Contracts;
using GreenStop.API.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController:ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private IConverter _converter;
        public TestController(IWebHostEnvironment env,IConverter converter)
        {
            _env=env;
            _converter=converter;
        }
        [HttpGet("[action]")]
        public String GetCustomerInfo()
        {
            CustomerInfoViewModel info=new CustomerInfoViewModel()
            {
                Name="Trikal",
                AddressLine1="10 dwarkanath ghosh lane",
                AddressLine2="Chetla",
                Landmark="Near Chetla petrol pump",
                Phone="9876543210",
                Pincode="700027",
                AlternatePhone=""
            };

            return JsonSerializer.Serialize(info);
        }

        [HttpGet("[action]")]
        public String GetItemDetails()
        {
            List<ItemDetailsViewModel> ItemList=new List<ItemDetailsViewModel>();
            ItemList.Add(new ItemDetailsViewModel(){
                id=1,
                name="Fish Thali",
                price=90,
                quantity=10,
                note="More oil and less water"
            });

            ItemList.Add(new ItemDetailsViewModel(){
                id=2,
                name="Chicken Thali",
                price=80,
                quantity=10,
                note="Only leg piece is prefferable"
            });

            ItemList.Add(new ItemDetailsViewModel(){
                id=2,
                name="Chicken Thali",
                price=80,
                quantity=2,
                note=String.Empty
            });

            return JsonSerializer.Serialize(ItemList);
        }

        [HttpGet("[action]")]
        public String GetPaymentInfo()
        {
            PaymentInfoViewModel infoViewModel=new PaymentInfoViewModel(){
                Amt=1860,
                Mode=1,
                TId=String.Empty
            };

            return JsonSerializer.Serialize(infoViewModel);
        }

        [HttpGet("[action]")]
        public IActionResult CreateGSPDFMemoryStream()
        {
            try
            {
                String AbsolutePath= System.IO.Path.Combine(_env.WebRootPath,"Images/");
            String contentRootPath = _env.ContentRootPath;
            String Message=String.Empty;
            String path = System.IO.Path.Combine(contentRootPath , "Content","EmailTemplates","Order_details.html");
            // String outputPath= System.IO.Path.Combine(contentRootPath,"Content","PDFs",DateTime.Now.Ticks.ToString()+".pdf");
            // String outputFolder= System.IO.Path.Combine(contentRootPath,"Content","PDFs");


            var EmailTemplate = CommonMethodd.ReadHtmlFile(path);
            var imagePath=System.IO.Path.Combine(_env.WebRootPath,"GS_Logo.png");
            EmailTemplate = EmailTemplate.Replace("@@LOGO@@",imagePath);
            
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "PDF Report"
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = EmailTemplate,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet =  "" },
                // HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                // FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" }
            };

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            var pdf = _converter.Convert(doc);
            var stream = new MemoryStream(pdf);

            stream.Seek(0, SeekOrigin.Begin);

            return File(stream,MediaTypeNames.Application.Pdf,"file1.pdf");
            }
            catch(Exception Ex)
            {
                return Ok(Ex.Message.ToString());
            }
            
        }

        [HttpGet("[action]")]
        public IActionResult  CretePDF()
        {
            try
            {
                String AbsolutePath= System.IO.Path.Combine(_env.WebRootPath,"Images/");
            String contentRootPath = _env.ContentRootPath;
            String Message=String.Empty;
            String path = System.IO.Path.Combine(contentRootPath , "Content","EmailTemplates","Order_details.html");
            // String outputPath= System.IO.Path.Combine(contentRootPath,"Content","PDFs",DateTime.Now.Ticks.ToString()+".pdf");
            // String outputFolder= System.IO.Path.Combine(contentRootPath,"Content","PDFs");

            String template=CommonMethodd.ReadHtmlFile(path);
            String Filename="tEST.pdf";

        //    String path = System.IO.Path.Combine(contentRootPath , "Content","EmailTemplates","Order_details_gs.html");
            
            String outputFolder= System.IO.Path.Combine(contentRootPath,"Content","PDFs");
            String outputPath= System.IO.Path.Combine(outputFolder,Filename);//System.IO.Path.Combine(contentRootPath,"Content","PDFs",DateTime.Now.Ticks.ToString()+".pdf");

            
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

            template = template.Replace("@@LOGO@@", localLogoPath);

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

            return Ok(outputPath);
            }
            catch(Exception Ex)
            {
                return Ok(Ex.Message.ToString());
            }
            

        } 

        [HttpGet("[action]")]
        public IActionResult Unauth()
        {
            return Unauthorized();
        }
    }
}