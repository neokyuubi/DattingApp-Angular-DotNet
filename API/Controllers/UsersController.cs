using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // GET /api/users
    
    public class UsersController : ControllerBase
    {
        private readonly DataContext ctx;

        public UsersController(DataContext context)
        {
            ctx = context;
            
        }

        [HttpGet]
        public ActionResult<IEnumerable<AppUser>> GetUsers()
        {
            var users = ctx.Users.ToList();
            return users;
        }

        [HttpGet("{id}")]
        public ActionResult<AppUser> GetUser(int id)
        {
            var user = ctx.Users.Find(id);
            return user;
        }
    }
}