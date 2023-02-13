using GreenStop.API.Data;
using Microsoft.AspNetCore.Mvc;

namespace GreenStop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController:ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CartController(ApplicationDbContext db)
        {
            _db=db;
        }

        [HttpGet("[action]/{cartitem}")]
        public IActionResult CheckCartAvailability([FromRoute] string cartitem)
        {
            var id_list=cartitem.Split(',');

            
            return Ok();
        }
    }
}