using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // GET /api/[users] 
    
    public class UsersController : ControllerBase
    {
        private readonly DataContext ctx;

        public UsersController(DataContext context)
        {
            ctx = context;
            
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            return await ctx.Users.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(int id)
        {
            return await ctx.Users.FindAsync(id);
        }
    }
}