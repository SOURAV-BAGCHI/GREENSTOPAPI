using System;
using System.Linq;
using System.Threading.Tasks;
using GreenStop.API.Data;
using GreenStop.API.Helpers;
using GreenStop.API.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GreenStop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatagoryItemController:ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private IMemoryCache _cache;
        private readonly AppSettings _appSettings;
        public CatagoryItemController(ApplicationDbContext db,IMemoryCache memoryCache,
        IOptions<AppSettings> appSettings)
        {
            _db=db;
            _cache = memoryCache;
            _appSettings=appSettings.Value;
        }

        [HttpGet("[action]")]
        public IActionResult GetCatagoryItemList()
        {
            String cacheEntry=String.Empty;
            
             // Look for cache key.
            if (!_cache.TryGetValue(_appSettings.CacheKey_CatagoryItem, out cacheEntry))
            {

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                // Keep in cache for this time, reset time if accessed.
                .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                var result= _db.Set<CatagoryItemDetailsViewModel>().FromSqlRaw(StoredProcedure.usp_CatagoryItemDetails_Get).ToList().FirstOrDefault();
                cacheEntry=result.CatagoryItemDetails;
                // Save data in cache.
                _cache.Set(_appSettings.CacheKey_CatagoryItem, cacheEntry, cacheEntryOptions);
            
            }
            return Ok(cacheEntry);
        }
    }
}