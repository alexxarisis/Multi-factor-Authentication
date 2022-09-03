using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

using Backend.Models.JWT.Util;
using Backend.Models.JWT.Tokens;
using Backend.Models.Services;
using Backend.Models.Services.MFA;
using Backend.Models.User;
using Backend.Models.Security;
using Backend.Models.Responses;

namespace Backend.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController  : ControllerBase
    {
        private readonly MyResponse myResponse;
        private readonly DatabaseConnector db = new();
        private readonly MFAServiceFactory mfaServiceFactory = new();

        public AuthController(RsaSecurityKey key)
        {
            myResponse = new MyResponse(key);
        }

        /// <summary>
        /// Creates a new account, if it does not already exist, and
        /// grants access to the new user.
        /// </summary>
        /// <param name="request">Username, email and password (in order)
        /// in JSON format.</param>
        /// <returns>Access and Refresh tokens.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (request.IsInvalid())
            {
                return BadRequest("Found null or empty strings.");
            }
            if (await db.UsernameExistsAsync(request.UserName))
            {
                return Conflict("Username already exists"); //409
            }
            if (await db.EmailExistsAsync(request.Email))
            {
                return Conflict("Email already exists"); //409
            }
            var account = new Account(request);
            if (!await db.CreateAccountAsync(account))
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                "The server could not create the account.");
            }

            int id = await db.FindUserIdByUserNameAsync(account.UserName);
            var userData = new JWTData(id);

            Response.Redirect("mfa/init");
            myResponse.SetMessage("Account created successfully.");
            myResponse.AddToken(JWTType.ACCESS, userData);
            myResponse.AddToken(JWTType.REFRESH, userData);
            return StatusCode(StatusCodes.Status201Created,
                JsonConvert.SerializeObject(myResponse));
        }

        /// <summary>
        /// Validates and grants access to the account or, if MFA is enabled, 
        /// sends a one time password to the user's email.
        /// </summary>
        /// <param name="request">Username and password (in order)
        /// in JSON format</param>
        /// <returns>MFA Access and MFA Refresh tokens if MFA is on,
        /// Access and Refresh tokens otherwise.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (request.IsInvalid())
            {
                return BadRequest("Found null or empty strings.");
            }
            if (!await db.UsernameExistsAsync(request.UserName))
            {
                return NotFound("Account not found!");
            }

            var salt = await db.GetUserSaltAsync(request.UserName);
            var hasher = new SecurePasswordGenerator();
            var hashedPassword = await hasher.HashPasswordAsync(request.Password, salt);

            if (!await db.CredentialsMatchAsync(request.UserName, hashedPassword))
            {
                return BadRequest("Invalid password!");
            }

            int id = await db.FindUserIdByUserNameAsync(request.UserName);
            var userData = new JWTData(id);

            if (!await db.UserHasEnabledMFAAsync(id))
            {
                Response.Redirect("mfa/init");
                myResponse.SetMessage("Logged in!");
                myResponse.AddToken(JWTType.ACCESS, userData);
                myResponse.AddToken(JWTType.REFRESH, userData);
                return Ok(JsonConvert.SerializeObject(myResponse));
            }
            
            int oneTimePassword = hasher.CreateOneTimePassword();
            string userEmail = await db.FindUserEmailByIdAsync(id);
            _ = Task.Run(() => mfaServiceFactory.CreateService(MFAServiceType.EMAIL)
                             .SendVerificationCode(userEmail, oneTimePassword));
            byte[] hashedOneTimePassword = await hasher.HashPasswordAsync(oneTimePassword.ToString());
            await db.StoreUserMFACodeAsync(id, hashedOneTimePassword);

            Response.Redirect("mfa/login");
            myResponse.SetMessage("Check your email for the code.");
            myResponse.AddToken(JWTType.MFA_ACCESS, userData);
            myResponse.AddToken(JWTType.MFA_OTP_REFRESH, userData);
            return Ok(JsonConvert.SerializeObject(myResponse));
        }

        /// <summary>
        /// Returns new Access and Refresh tokens.
        /// </summary>
        /// <returns>Access and Refresh tokens.</returns>
        [HttpPost("refresh")]
        [Authorize(Roles = "Refresh")]
        public IActionResult RefreshAccess()
        {
            int id = new JWTReader(Request).GetID();
            var userData = new JWTData(id);

            myResponse.AddToken(JWTType.ACCESS, userData);
            myResponse.AddToken(JWTType.REFRESH, userData);
            return Ok(JsonConvert.SerializeObject(myResponse));
        }
    }
}