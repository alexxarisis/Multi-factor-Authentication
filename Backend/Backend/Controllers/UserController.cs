using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Models.JWT.Util;
using Backend.Models.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("user")]
    [Authorize(Roles = "Access")]
    public class UserController : ControllerBase
    {
        private readonly DatabaseConnector db = new();

        // Are we gonna update all together or seperately?
        [HttpPatch("update")]
        public IActionResult Update()
        {
            // update info
            return Ok("Updated profile");
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete()
        {
            int id = new JWTReader(Request).GetID();
            await db.DeleteAccountAsync(id);

            return StatusCode(StatusCodes.Status410Gone,
                "Account deleted!");
        }
    }
}
