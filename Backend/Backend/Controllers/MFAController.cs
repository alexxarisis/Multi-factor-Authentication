using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

using Backend.Models.JWT.Util;
using Backend.Models.JWT.Tokens;
using Backend.Models.Services;
using Backend.Models.Services.MFA;
using Backend.Models.Security;
using Backend.Models.Responses;

namespace Backend.Controllers
{
    [ApiController]
    [Route("mfa")]
    public class MFAController : ControllerBase
    {
        private readonly MyResponse myResponse;
        private readonly DatabaseConnector db = new();
        private readonly MFAServiceFactory mfaServiceFactory = new();

        public MFAController(RsaSecurityKey key)
        {
            myResponse = new(key);
        }

        /// <summary>
        /// Validates the user's one time password and grants access
        /// to the account.
        /// </summary>
        /// <param name="oneTimePassword">The one time password
        /// sent to the user's email.</param>
        /// <returns>Access and Refresh tokens.</returns>
        [HttpPost("login")]
        [Authorize(Roles = "MFA Access")]
        public async Task<IActionResult> LoginMFA([FromBody] string oneTimePassword)
        {
            int id = new JWTReader(Request).GetID();
            var hashedOneTimePassword = await new SecurePasswordGenerator().HashPasswordAsync(oneTimePassword);
            if (!await db.MFACodesMatchAsync(id, hashedOneTimePassword))
            {
                return BadRequest("Wrong OTP!"); // this status code?
            }

            var userData = new JWTData(id);
            myResponse.AddToken(JWTType.ACCESS, userData);
            myResponse.AddToken(JWTType.REFRESH, userData);
            return Ok(JsonConvert.SerializeObject(myResponse));
        }

        /// <summary>
        /// Sends a one time password to the user's email 
        /// and grants the user the ability to activate MFA on the account.
        /// </summary>
        /// <returns>MFA Activate and MFA Refresh tokens.</returns>
        [HttpPost("init")]
        [Authorize(Roles = "Access")]
        public async Task<IActionResult> InitializeMFA()
        {
            int id = new JWTReader(Request).GetID();
            var hasher = new SecurePasswordGenerator();

            int oneTimePassword = hasher.CreateOneTimePassword();
            var userEmail = await db.FindUserEmailByIdAsync(id);
            _ = Task.Run(() => mfaServiceFactory.CreateService(MFAServiceType.EMAIL)
                             .SendVerificationCode(userEmail, oneTimePassword));

            var hashedOneTimePassword = await hasher.HashPasswordAsync(oneTimePassword.ToString());
            if (await db.UserHasRegisteredMFAAsync(id))
            {
                await db.StoreUserMFACodeAsync(id, hashedOneTimePassword);
            }
            else
            {
                await db.RegisterUserAndMFACodeAsync(id, hashedOneTimePassword);
            }

            var userData = new JWTData(id);
            Response.Redirect("mfa/activate");
            myResponse.SetMessage("A one-time password will be send on your email.\n" +
                "This might take a minute or two!");
            myResponse.AddToken(JWTType.MFA_ACTIVATE, userData);
            myResponse.AddToken(JWTType.MFA_OTP_REFRESH, userData);
            return Ok(JsonConvert.SerializeObject(myResponse));
        }

        /// <summary>
        /// Validates the one time password and enables MFA for the user.
        /// </summary>
        /// <param name="oneTimePassword">The one time password sent 
        /// to the user's email</param>
        /// <returns>Recovery code, Access and Refresh tokens</returns>
        [HttpPost("activate")]
        [Authorize(Roles = "MFA Activate")]
        public async Task<IActionResult> ActivateMFA([FromBody] string oneTimePassword)
        {
            int id = new JWTReader(Request).GetID();
            var hasher = new SecurePasswordGenerator();
            var hashedOneTimePassword = await hasher.HashPasswordAsync(oneTimePassword);

            if (!await db.MFACodesMatchAsync(id, hashedOneTimePassword))
            {
                return BadRequest("Invalid one time code.");
            }

            var recoveryCode = hasher.CreateOneTimePassword();
            var hashedRecoveryCode = await hasher.HashPasswordAsync(recoveryCode.ToString());
            await db.StoreUserMFARecoveryCodeAsync(id, hashedRecoveryCode);
            await db.ChangeUserMFASettingsAsync(id, true);

            var userData = new JWTData(id);
            var recoveryReponse = new RecoveryResponse(myResponse);
            recoveryReponse.SetMessage("Two factor authentication is now activated.");
            recoveryReponse.AddToken(JWTType.ACCESS, userData);
            recoveryReponse.AddToken(JWTType.REFRESH, userData);
            recoveryReponse.SetRecoveryKey(recoveryCode.ToString());
            return Ok(JsonConvert.SerializeObject(recoveryReponse));
        }

        /// <summary>
        /// Sends a new one time password to the user's email.
        /// </summary>
        /// <returns>MFA Access token</returns>
        [HttpPost("refresh-otp")]
        [Authorize(Roles = "MFA OTP Refresh")]
        public async Task<IActionResult> ResendMFACode()
        {
            int id = new JWTReader(Request).GetID();
            var hasher = new SecurePasswordGenerator();

            int oneTimePassword = hasher.CreateOneTimePassword();
            var userEmail = await db.FindUserEmailByIdAsync(id);
            _ = Task.Run(() => mfaServiceFactory.CreateService(MFAServiceType.EMAIL)
                             .SendVerificationCode(userEmail, oneTimePassword));

            byte[] hashedOneTimePassword = await hasher.HashPasswordAsync(oneTimePassword.ToString());
            await db.StoreUserMFACodeAsync(id, hashedOneTimePassword);

            var userData = new JWTData(id);
            myResponse.SetMessage("A code has been sent to your email.");
            myResponse.AddToken(JWTType.MFA_ACCESS, userData);
            return Ok(JsonConvert.SerializeObject(myResponse));
        }

        /// <summary>
        /// Disables MFA on the account and grants access.
        /// </summary>
        /// <param name="recoveryCode">The recovery code of this account.</param>
        /// <returns></returns>
        [HttpPost("recover")]
        [Authorize(Roles = "MFA Access")]
        public async Task<IActionResult> RecoverMFA([FromBody] string recoveryCode)
        {
            int id = new JWTReader(Request).GetID();
            var hasher = new SecurePasswordGenerator();
            var hashedRecoveryCode = await hasher.HashPasswordAsync(recoveryCode);

            if (!await db.MFARecoveryCodesMatchAsync(id, hashedRecoveryCode))
            {
                return BadRequest("Invalid recovery code.");
            }

            var newRecoveryCode = hasher.CreateOneTimePassword();
            var newHashedRecoveryCode = await hasher.HashPasswordAsync(newRecoveryCode.ToString());
            await db.StoreUserMFARecoveryCodeAsync(id, newHashedRecoveryCode);
            await db.ChangeUserMFASettingsAsync(id, false);

            var userData = new JWTData(id);
            myResponse.SetMessage("Two factor authentication is now de-activated.\n" +
                "New recovery code: " + newRecoveryCode.ToString());
            myResponse.AddToken(JWTType.ACCESS, userData);
            myResponse.AddToken(JWTType.REFRESH, userData);
            return Ok(JsonConvert.SerializeObject(myResponse));
        }

        /// <summary>
        /// Disables MFA setting for user.
        /// </summary>
        /// <returns>Success message.</returns>
        [HttpPost("disable")]
        [Authorize(Roles = "Access")]
        public async Task<IActionResult> DisableMFA()
        {
            int id = new JWTReader(Request).GetID();
            await db.ChangeUserMFASettingsAsync(id, false);

            return Ok("MFA is now disabled!");
        }
    }
}
