using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MindMapGenerator.Core.Domain.IdentityEntities;
using MindMapGenerator.Core.Domain.RepositoryContract;
using MindMapGenerator.Core.Dtos;
using MindMapGenerator.Core.Dtos.AuthenticationDto;
using MindMapGenerator.Core.ServiceContracts;
using System.Security.Claims;

namespace MindMapGenerator.API.Controllers
{
    /// <summary>
    /// Manages user accounts, including registration, login, password reset, roles, and token handling.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthenticationServices _authenticationServices;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly ILogger<AccountController> _logger; // Added ILogger

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="authenticationServices">Authentication service.</param>
        /// <param name="userManager">User manager service.</param>
        /// <param name="emailSender">Email sender service.</param>
        /// <param name="signInManager">Sign-in manager service.</param>
        /// <param name="unitOfWork">Unit of work service.</param>
        /// <param name="passwordHasher">Password hasher service.</param>
        /// <param name="logger">Logger instance for logging controller actions.</param>
        public AccountController(
            IAuthenticationServices authenticationServices,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            SignInManager<ApplicationUser> signInManager,
            IUnitOfWork unitOfWork,
            IPasswordHasher<ApplicationUser> passwordHasher,
            ILogger<AccountController> logger) 
        {
            _authenticationServices = authenticationServices;
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new client account.
        /// </summary>
        /// <param name="registerDTO">Client registration details.</param>
        /// <returns>Authentication response with token and status.</returns>
        /// <response code="200">Client registered successfully.</response>
        /// <response code="400">Invalid input or request.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("register-Client")]
        public async Task<ActionResult<AuthenticationResponse>> RegisterCleintAsync([FromBody] RegisterDTO registerDTO)
        {
            _logger.LogInformation("Registering new client with email: {Email}", registerDTO.Email);

            if (!ModelState.IsValid)
            {
                var errors = string.Join("|", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                _logger.LogWarning("Invalid model state for client registration: {Errors}", errors);
                return BadRequest(errors);
            }

            var result = await _authenticationServices.RegisterClientAsync(registerDTO);
            if (!result.IsAuthenticated)
            {
                _logger.LogWarning("Client registration failed: {Message}", result.Message);
                return Problem(result.Message);
            }

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshToken(result.RefreshToken, result.RefreshTokenExpiration);
                _logger.LogInformation("Refresh token set for client: {Email}", registerDTO.Email);
            }

            _logger.LogInformation("Client registered successfully: {Email}", registerDTO.Email);
            return Ok(result);
        }

        /// <summary>
        /// Logs in a user or company.
        /// </summary>
        /// <param name="loginDTO">Login credentials (email, password).</param>
        /// <returns>Authentication response with token and status.</returns>
        /// <response code="200">Login successful.</response>
        /// <response code="400">Invalid credentials or input.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> LoginAsync([FromBody] LoginDTO loginDTO)
        {
            _logger.LogInformation("Login attempt for email: {Email}", loginDTO.Email);

            if (!ModelState.IsValid)
            {
                var errors = string.Join("|", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                _logger.LogWarning("Invalid login model state: {Errors}", errors);
                return BadRequest(errors);
            }

            var result = await _authenticationServices.LoginAsync(loginDTO);
            if (!result.IsAuthenticated)
            {
                _logger.LogWarning("Login failed for email: {Email}. Reason: {Message}", loginDTO.Email, result.Message);
                return BadRequest(result.Message);
            }

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshToken(result.RefreshToken, result.RefreshTokenExpiration);
                _logger.LogInformation("Refresh token set for email: {Email}", loginDTO.Email);
            }

            _logger.LogInformation("Login successful for email: {Email}", loginDTO.Email);
            return Ok(result);
        }

        

        /// <summary>
        /// Sends a password reset OTP to the user's email.
        /// </summary>
        /// <param name="forgotPassword">Email to send the password reset link to.</param>
        /// <returns>Status message.</returns>
        /// <response code="200">Password reset OTP sent successfully.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPassword)
        {
            _logger.LogInformation("Forgot password request for email: {Email}", forgotPassword.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for forgot password: {Email}", forgotPassword.Email);
                return BadRequest(ModelState);
            }

            await _authenticationServices.ForgotPassword(forgotPassword);
            _logger.LogInformation("Password reset OTP sent for email: {Email}", forgotPassword.Email);
            return Ok("If the email is associated with an account, an OTP will be sent.");
        }

        /// <summary>
        /// Resets the user's password using an OTP.
        /// </summary>
        /// <param name="resetPassword">Reset password request details, including OTP.</param>
        /// <returns>Status message.</returns>
        /// <response code="200">Password reset successfully.</response>
        /// <response code="400">Invalid OTP or request.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPassword)
        {
            _logger.LogInformation("Reset password request for email: {Email}", resetPassword.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for reset password: {Email}", resetPassword.Email);
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(resetPassword.Email!);
            if (user == null)
            {
                _logger.LogWarning("No user found for reset password with email: {Email}", resetPassword.Email);
                return BadRequest("Invalid Request");
            }

            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var hashedPassword = passwordHasher.HashPassword(user, resetPassword.Password!);
            user.PasswordHash = hashedPassword;
            user.OTPCode = null;
            user.OTPExpiration = null;

            await _userManager.UpdateAsync(user);
            _logger.LogInformation("Password reset successfully for email: {Email}", resetPassword.Email);
            return Ok("Password has been reset successfully.");
        }

        /// <summary>
        /// Changes the user's password.
        /// </summary>
        /// <param name="model">Change password request details.</param>
        /// <returns>Status message.</returns>
        /// <response code="200">Password changed successfully.</response>
        /// <response code="400">Invalid current password.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Change password request for userID: {UserID}", userId);

            if (userId == null)
            {
                _logger.LogWarning("UserID not found in claims for change password request");
                return BadRequest(new { message = "User not found" });
            }

            var user = await _userManager.FindByEmailAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for change password with userID: {UserID}", userId);
                return NotFound(new { message = "User not found" });
            }

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.OldPassword);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Incorrect current password for userID: {UserID}", userId);
                return BadRequest(new { message = "Current password is incorrect" });
            }

            var newHashedPassword = _passwordHasher.HashPassword(user, model.NewPassword);
            user.PasswordHash = newHashedPassword;

            await _userManager.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Password changed successfully for userID: {UserID}", userId);
            return Ok(new { message = "Password changed successfully" });
        }

        /// <summary>
        /// Verifies the OTP code.
        /// </summary>
        /// <param name="request">OTP verification request.</param>
        /// <returns>Status message.</returns>
        /// <response code="200">OTP verified successfully.</response>
        /// <response code="400">Invalid or expired OTP.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            _logger.LogInformation("Verifying OTP for email: {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("No user found for OTP verification with email: {Email}", request.Email);
                return BadRequest("Invalid email.");
            }

            if (user.OTPCode != request.Otp || user.OTPExpiration < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired OTP for email: {Email}", request.Email);
                return BadRequest("Invalid or expired OTP.");
            }

            user.EmailConfirmed = true;
            user.OTPCode = null;
            user.OTPExpiration = null;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("OTP verified successfully for email: {Email}", request.Email);
            return Ok("Verify successfully.");
        }
        /// <summary>
        /// Revokes a refresh token.
        /// </summary>
        /// <param name="revokTokenDTO">Token details to revoke.</param>
        /// <returns>Status message.</returns>
        [HttpPost("revokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokTokenDTO revokTokenDTO)
        {
            var token = revokTokenDTO.Token ?? Request.Cookies["refreshToken"];
            _logger.LogInformation("Revoking token: {Token}", token);

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token is required but missing");
                return BadRequest("Token is required!");
            }

            var result = await _authenticationServices.RevokeTokenAsync(token);
            if (!result)
            {
                _logger.LogWarning("Invalid token for revocation: {Token}", token);
                return BadRequest("Invalid token");
            }

            _logger.LogInformation("Token revoked successfully: {Token}", token);
            return Ok();
        }
        /// <summary>
        /// Adds a new role to the user.
        /// </summary>
        /// <param name="model">Role details to assign.</param>
        /// <returns>Status message.</returns>
        [HttpPost("addrole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleDTO model)
        {
            _logger.LogInformation("Adding role to user with email: {UserID}", model.UserID);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for adding role: {UserID}", model.UserID);
                return BadRequest(ModelState);
            }

            var result = await _authenticationServices.AddRoleToUserAsync(model);
            if (!string.IsNullOrEmpty(result))
            {
                _logger.LogWarning("Failed to add role for email: {UserID}. Reason: {Result}", model.UserID, result);
                return BadRequest(result);
            }

            _logger.LogInformation("Role added successfully for email: {UserID}", model.UserID);
            return Ok(model);
        }

        /// <summary>
        /// Checks if the email is already in use.
        /// </summary>
        /// <param name="email">The email to check.</param>
        /// <returns>True if the email is in use; otherwise, false.</returns>
        /// <response code="200">Check successful, result returned.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("check-email")]
        public async Task<IActionResult> IsEmailInUse([FromQuery] string email)
        {
            _logger.LogInformation("Checking if email is in use: {Email}", email);

            var isInUse = await _unitOfWork.Repository<ApplicationUser>().AnyAsync(u => u.Email == email);
            _logger.LogInformation("Email check complete: {Email} is in use: {IsInUse}", email, isInUse);
            return Ok(new { isInUse });
        }
        /// <summary>
        /// Retrieves user information based on the provided user ID.
        /// </summary>
        /// <param name="userID">The unique identifier (GUID) of the user to retrieve.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing an <see cref="ApiResponse"/> with the user details if found,
        /// or a not found response if the user does not exist.
        /// </returns>
        /// <response code="200">Returns the user information when the user is found.</response>
        /// <response code="404">Returned when no user matches the provided user ID.</response>
        [HttpGet("getUserInfo/{userID}")]
        public async Task<ActionResult<ApiResponse>> GetUserInfo(Guid userID)
        {
            _logger.LogInformation("Fetching user info for userID: {UserID}", userID);

            var user = await _authenticationServices.GetUserByIdAsync(userID);
            if (user == null)
            {
                _logger.LogWarning("User not found for userID: {UserID}", userID);
                return NotFound(new ApiResponse
                {
                    Message = "User not found",
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    IsSuccess = false
                });
            }

            _logger.LogInformation("User info retrieved successfully for userID: {UserID}", userID);
            return Ok(new ApiResponse
            {
                Message = "User found",
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
                Result = user
            });
        }

        /// <summary>
        /// Retrieves all users from the system asynchronously.
        /// </summary>
        /// <remarks>
        /// This endpoint logs the process of fetching users and returns an <see cref="ApiResponse"/> containing the result.
        /// If no users are found, a 404 Not Found response is returned; otherwise, a 200 OK response is returned with the list of users.
        /// </remarks>
        /// <returns>
        /// A task that represents the asynchronous operation, containing an <see cref="ActionResult{T}"/> with an <see cref="ApiResponse"/>.
        /// The response includes a success flag, message, status code, and the list of users if successful.
        /// </returns>
        /// <response code="200">Returns the list of users when retrieval is successful.</response>
        /// <response code="404">Returned when no users are found in the system.</response>
        [HttpGet("getUsers")]
        public async Task<ActionResult<ApiResponse>> GetUsers()
        {
            _logger.LogInformation("Fetching all users");
            var users = await _authenticationServices.GetAllUsersAsync();
            if (users == null)
            {
                _logger.LogWarning("No users found");
                return NotFound(new ApiResponse
                {
                    Message = "No users found",
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    IsSuccess = false
                });
            }
            _logger.LogInformation("Users retrieved successfully");
            return Ok(new ApiResponse
            {
                Message = "Users found",
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
                Result = users
            });
        }

        /// <summary>
        /// Refreshes the user's authentication token.
        /// </summary>
        /// <returns>New authentication token.</returns>
        [HttpGet("refreshtoken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            _logger.LogInformation("Refreshing token with refresh token: {RefreshToken}", refreshToken);

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh token missing or invalid");
                return BadRequest("Refresh token is missing or invalid.");
            }

            var result = await _authenticationServices.RefreshTokenAsync(refreshToken);
            if (!result.IsAuthenticated)
            {
                _logger.LogWarning("Token refresh failed: {Message}", result.Message);
                return BadRequest(result.Message);
            }

            SetRefreshToken(result.RefreshToken, result.RefreshTokenExpiration);
            _logger.LogInformation("Token refreshed successfully");
            return Ok(result);
        }

        /// <summary>
        /// Sets the refresh token cookie in the response.
        /// </summary>
        /// <param name="refreshToken">The refresh token string.</param>
        /// <param name="expires">The expiration time for the token.</param>
        private void SetRefreshToken(string refreshToken, DateTime expires)
        {
            var cookieOption = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                Expires = expires,
                SameSite = SameSiteMode.Lax,
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOption);
            _logger.LogDebug("Refresh token cookie set with expiration: {Expires}", expires);
        }
    }
}
