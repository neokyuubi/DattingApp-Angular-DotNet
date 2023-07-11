using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ReportController: BaseApiController
    {
        private readonly DataContext _context;
        public ReportController(DataContext context)
		{
            _context = context;
		}

		[Authorize]
		[HttpGet("auth")]
		public ActionResult<string>GetSecrect()
		{
			return "secrect text";
		}

		[HttpGet("not-found")]
		public ActionResult<AppUser>GetNotFound()
		{
			var thing = _context.Users.Find(-1);
			if (thing == null)
			{
				return NotFound();
			}
			return thing;
		}

		[HttpGet("server-error")]
		public ActionResult<string>GetServerError()
		{
			var thing = _context.Users.Find(-1);
			var thingToReturn = thing.ToString();
			return thingToReturn;
		}

		// [HttpGet("auth")]
		// public ActionResult<string>GetSecrect()
		// {
		// 	return "secrect text";
		// }

		// [HttpGet("auth")]
		// public ActionResult<string>GetSecrect()
		// {
		// 	return "secrect text";
		// }
    }
}