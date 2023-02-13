using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GreenStop.API.Data;
using GreenStop.API.Helpers;
using GreenStop.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GreenStop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatagoryItemsOperationController:ControllerBase
    {   
        private readonly ApplicationDbContext _db;
        private readonly AppSettings _appSettings;
        private IWebHostEnvironment _environment;
        public CatagoryItemsOperationController(ApplicationDbContext db,IOptions<AppSettings> appSettings,IWebHostEnvironment environment)
        {
            _db=db;
            _appSettings=appSettings.Value;
            _environment=environment;
        }

        [HttpGet("[action]")]
        public IActionResult GetCatagoryList()
        {
            var list=_db.CatagoryDetails.ToList();
            return Ok(list);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetCatagoryDetails([FromRoute] Int32 id)
        {
            var catagoryDetails=await _db.CatagoryDetails.FindAsync(id);

            if(catagoryDetails ==null)
            {
                return NotFound();
            }

            return Ok(catagoryDetails);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateCatagoryDetails([FromForm] CatagoryModel formData)
        {
            IFormFile File = null;
            if(Request.Form.Files.Count>0)
            {
                File=Request.Form.Files[0];
                CatagoryModel item=new CatagoryModel();
                item.Name=formData.Name;
                item.Image=formData.Image;
                item.Priority=formData.Priority;
                item.Available=formData.Available;

                if(File!=null && File.Length>0)
                {
                    string wwwPath = _environment.WebRootPath;
                    // string contentPath = _environment.ContentRootPath;
                    string path = Path.Combine(wwwPath, "Images");
                    var absolutePath = Path.Combine(path,formData.Image); //ContentDispositionHeaderValue.Parse(Image.ContentDisposition).FileName.Trim('"');
                
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    if(System.IO.File.Exists(absolutePath))
                    {
                        System.IO.File.Delete(absolutePath);
                    }

                    using (var stream = new FileStream(absolutePath, FileMode.Create))
                    {
                        File.CopyTo(stream);
                    }

                    await _db.CatagoryDetails.AddAsync(item);
                    await _db.SaveChangesAsync();

                    return Ok();

                }
                else
                {
                    return BadRequest("Image file error");
                }


            }
            else
            {
                return BadRequest("Image file not present");
            }
        }

        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> UpdateCatagoryDetails([FromRoute] Int32 id,[FromForm] CatagoryModel formData)//,[FromForm] IFormFile File)
        {
            var item=await _db.CatagoryDetails.FindAsync(id);
            
            IFormFile File = null;
            if(Request.Form.Files.Count>0)
            {
                File=Request.Form.Files[0];
            }

            if(item==null)
            {
                return NotFound();
            }

            item.Name=formData.Name;
            item.Image=formData.Image;
            item.Priority=formData.Priority;
            item.Available=formData.Available;

            if(File!=null && File.Length>0)
            {
                string wwwPath = _environment.WebRootPath;
                // string contentPath = _environment.ContentRootPath;
                string path = Path.Combine(wwwPath, "Images");
                var absolutePath = Path.Combine(path,formData.Image); //ContentDispositionHeaderValue.Parse(Image.ContentDisposition).FileName.Trim('"');
            
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if(System.IO.File.Exists(absolutePath))
                {
                    System.IO.File.Delete(absolutePath);
                }

                using (var stream = new FileStream(absolutePath, FileMode.Create))
                {
                    File.CopyTo(stream);
                }
            
            }

            _db.Entry(item).State=Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> DeleteCatagoryDetails([FromRoute] Int32 id)
        {
            var CatagoryDetails=await _db.CatagoryDetails.FindAsync(id);

            if(CatagoryDetails==null)
            {
                return NotFound();
            }

            var ItemList=_db.ItemDetails.Where(x=>x.CatagoryId==id);

            foreach(var item in ItemList)
            {
                _db.Entry(item).State=Microsoft.EntityFrameworkCore.EntityState.Deleted;
            }

            _db.Entry(CatagoryDetails).State=Microsoft.EntityFrameworkCore.EntityState.Deleted;
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("[action]/{id}")]
        public IActionResult GetCatagoryItemListByCatagoryId([FromRoute] Int32 id)
        {
            var ItemList=_db.ItemDetails.Where(x=>x.CatagoryId==id).ToList();

            return Ok(ItemList);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetItemDetails([FromRoute] Int32 id)
        {
            var ItemDetails=await _db.ItemDetails.FindAsync(id);

            if(ItemDetails ==null)
            {
                return NotFound();
            }

            return Ok(ItemDetails);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateItemDetails([FromBody] ItemModel formData)
        {
            ItemModel ItemDetails=new ItemModel();

            ItemDetails.CatagoryId=formData.CatagoryId;
            ItemDetails.Name1=formData.Name1;
            ItemDetails.Name2=formData.Name2;
            ItemDetails.Detail=formData.Detail;
            ItemDetails.Detail2=formData.Detail2;
            ItemDetails.Type=formData.Type;
            ItemDetails.Description=formData.Description;
            ItemDetails.Description2=formData.Description2;
            ItemDetails.Price=formData.Price;
            ItemDetails.Available=formData.Available;

            await _db.ItemDetails.AddAsync(ItemDetails);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> UpdateItemDetails([FromRoute] Int32 id,[FromBody] ItemModel formData)
        {
            var ItemDetails=await _db.ItemDetails.FindAsync(id);

            if(ItemDetails ==null)
            {
                return NotFound();
            }

            ItemDetails.Name1=formData.Name1;
            ItemDetails.Name2=formData.Name2;
            ItemDetails.Detail=formData.Detail;
            ItemDetails.Detail2=formData.Detail2;
            ItemDetails.Type=formData.Type;
            ItemDetails.Description=formData.Description;
            ItemDetails.Description2=formData.Description2;
            ItemDetails.Price=formData.Price;
            ItemDetails.Available=formData.Available;

            _db.Entry(ItemDetails).State=Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _db.SaveChangesAsync();

            return Ok();
        }
    
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> DeleteItemDetail([FromRoute] Int32 id)
        {
            var ItemDetail=await _db.ItemDetails.FindAsync(id);

            if(ItemDetail==null)
            {
                return NotFound();
            }

            _db.Entry(ItemDetail).State=Microsoft.EntityFrameworkCore.EntityState.Deleted;
            await _db.SaveChangesAsync();
            return Ok();
        }
    
    }
}