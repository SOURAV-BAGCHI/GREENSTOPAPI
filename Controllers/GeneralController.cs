using System;
using System.Linq;
using System.Threading.Tasks;
using GreenStop.API.Data;
using GreenStop.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models.ViewModels.StoredProcedureViewModel;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeneralController:ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly AppSettings _appSettings;

        public GeneralController(ApplicationDbContext db,IOptions<AppSettings> appSettings)
        {
            _db=db;
            _appSettings=appSettings.Value;
        }

        [HttpGet("[action]")]
    //    [Authorize(Policy="RequireAdministratorRole")]
        public async Task<IActionResult> GetRoles()
        {
            var result= await _db.Set<GeneralListViewModel>().FromSqlRaw(StoredProcedure.usp_AspNetRoles_GetRoles).ToListAsync();
            return Ok(result);
        }

        [HttpGet("[action]/{roleid}/{lastrecord}")]
    //    [Authorize(Policy="RequireAdministratorRole")]
        public async Task<IActionResult> GetEmployeeByRole([FromRoute] String roleid,Int32 lastrecord)
        {

            SqlParameter param1=new SqlParameter("@str_RoleId",roleid);
            SqlParameter param2=new SqlParameter("@i_Offset",lastrecord);
            SqlParameter param3=new SqlParameter("@i_recordCountPerPage",_appSettings.RecordCountPerPage);

            var result= await _db.Set<GeneralListViewModel>().FromSqlRaw(StoredProcedure.usp_EmployeeByRole_GetList,param1,param2,param3).ToListAsync();

            if(result.Count<_appSettings.RecordCountPerPage)
            {
                lastrecord=0;

            }
            else
            {
                lastrecord+=result.Count;
            }

            return Ok(new{Record=result,lastrecord=lastrecord});
        }

        [HttpGet("[action]")]
        public IActionResult GetServerDateTime()
        {
            return Ok(new{DateTime=DateTime.Now});
        }
    
    }
}