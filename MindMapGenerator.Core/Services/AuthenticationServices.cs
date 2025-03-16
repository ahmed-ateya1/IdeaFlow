﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MindMapGenerator.Core.Domain.IdentityEntities;
using MindMapGenerator.Core.Dtos.AuthenticationDto;
using MindMapGenerator.Core.Helper;
using MindMapGenerator.Core.ServiceContracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MindMapGenerator.Core.Services
{
    public class AuthenticationServices : IAuthenticationServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly JwtDTO _jwt;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;
        public AuthenticationServices(UserManager<ApplicationUser> userManager,
                                      RoleManager<ApplicationRole> roleManager,
                                      IOptions<JwtDTO> jwt,
                                      IEmailSender emailSender,
                                      IHttpContextAccessor httpContextAccessor,
                                      SignInManager<ApplicationUser> signInManager,
                                      IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwt = jwt.Value;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _signInManager = signInManager;
            _environment = environment;
        }
        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host.Value}/";
        }

        private string EmailBody(string Title, string content, string email, string otpCode)
        {
            return $@"
    <html>
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                color: #333;
                line-height: 1.6;
                background-color: #f4f4f4;
                margin: 0;
                padding: 0;
            }}
            .container {{
                max-width: 600px;
                margin: 20px auto;
                padding: 20px;
                border: 1px solid #ddd;
                border-radius: 8px;
                background-color: #ffffff;
                box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            }}
            .header {{
                text-align: center;
                padding-bottom: 20px;
            }}
            .header h1 {{
                color: #007BFF;
                font-size: 24px;
                margin: 0;
            }}
            .content {{
                padding: 20px;
                background-color: #ffffff;
                border-radius: 8px;
                text-align: center;
            }}
            .otp-code {{
                font-size: 24px;
                font-weight: bold;
                color: #007BFF;
                margin: 20px 0;
                padding: 10px;
                border: 2px dashed #007BFF;
                display: inline-block;
                letter-spacing: 2px;
            }}
            .footer {{
                text-align: center;
                margin-top: 20px;
                font-size: 12px;
                color: #777;
            }}
            .footer a {{
                color: #007BFF;
                text-decoration: none;
            }}
        </style>
        </head>
        <body>
            <div class='container'>
            <div class='header'>
                <h1>{Title}</h1>
            </div>
            <div class='content'>
                <p>Hello {email},</p>
                <p>{content}</p>
                <div class='otp-code'>{otpCode}</div>
                <p>This code will expire in 10 minutes. If you did not request this, please ignore this email.</p>
                <p>Best regards,<br/>Mind Map Generator Team</p>
            </div>
            <div class='footer'>
                <p>&copy; 2025 . All rights reserved.</p>
                <p><a href='#'>Privacy Policy</a> | <a href='#'>Contact Support</a></p>
            </div>
            </div>
         </body>
         </html>";
        }

        public async Task<AuthenticationResponse> RegisterClientAsync(RegisterDTO clientRegisterDTO)
        {
            if (await _userManager.FindByEmailAsync(clientRegisterDTO.Email) != null)
                return new AuthenticationResponse { Message = "Email is already registered!" };

            var user = new ApplicationUser
            {
                UserName = clientRegisterDTO.Email,
                Email = clientRegisterDTO.Email,
                FullName = clientRegisterDTO.FullName,
            };

            var result = await _userManager.CreateAsync(user, clientRegisterDTO.Password);

            if (!result.Succeeded)
                return CreateErrorResponse(result.Errors);

            var roleName = clientRegisterDTO.Role == RolesOption.ADMIN ? "ADMIN" : "USER";
            var addRoleResult = await AddRoleToUserAsync(new AddRoleDTO { RoleName = roleName, UserID = user.Id });
            if (!string.IsNullOrEmpty(addRoleResult))
                return new AuthenticationResponse { Message = addRoleResult };

            var authenticationUser = await GenerateJwtToken(user);

            if (user.RefreshTokens.Any(x => x.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.FirstOrDefault(x => x.IsActive);
                authenticationUser.RefreshToken = activeRefreshToken.Token;
                authenticationUser.RefreshTokenExpiration = activeRefreshToken.ExpiredOn;
            }
            else
            {
                var otpCode = OtpHelper.GenerateOtp();
                user.OTPCode = otpCode;
                user.OTPExpiration = DateTime.UtcNow.AddMinutes(10);
                await _userManager.UpdateAsync(user);


                string emailBody = EmailBody("Confirm Your Email", "Thank you for registering with Mind Map Generator App. To complete your registration, please confirm your email address by Using this code: ", user.Email, otpCode);
                await _emailSender.SendEmailAsync(user.Email, "Confirm Email", emailBody);

                var newRefreshToken = GenerateRefreshToken();
                authenticationUser.RefreshToken = newRefreshToken.Token;
                authenticationUser.RefreshTokenExpiration = newRefreshToken.ExpiredOn;
                user.RefreshTokens.Add(newRefreshToken);
                await _userManager.UpdateAsync(user);
            }
            authenticationUser.UserID = user.Id;
            authenticationUser.FullName = user.FullName;


            return authenticationUser;
        }
        public async Task<AuthenticationResponse> GenerateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id.ToString())
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredentials);

            return new AuthenticationResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Email = user.Email,
                Roles = roles.ToList(),
                Message = "Success Operation",
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(_jwt.DurationInDays),
                IsAuthenticated = true
            };
        }

        public async Task<string> AddRoleToUserAsync(AddRoleDTO model)
        {
            var role = await _roleManager.FindByNameAsync(model.RoleName);
            if (role == null)
            {
                var createRoleResult = await _roleManager.CreateAsync(new ApplicationRole { Name = model.RoleName });
                if (!createRoleResult.Succeeded)
                    return string.Join(", ", createRoleResult.Errors.Select(e => e.Description));
            }

            var user = await _userManager.FindByIdAsync(model.UserID.ToString());
            if (user == null)
                return "User not found.";

            if (await _userManager.IsInRoleAsync(user, model.RoleName))
                return "User is already assigned to this role.";

            var addRoleResult = await _userManager.AddToRoleAsync(user, model.RoleName);
            if (!addRoleResult.Succeeded)
                return string.Join(", ", addRoleResult.Errors.Select(e => e.Description));

            return null;
        }

        public async Task<AuthenticationResponse> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, loginDTO.Password))
            {
                return new AuthenticationResponse() { Message = "Email or Password is incorrect!" };
            }

            var authenticationUser = await GenerateJwtToken(user);

            if (user.RefreshTokens.Any(x => x.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.FirstOrDefault(x => x.IsActive);

                authenticationUser.RefreshToken = activeRefreshToken.Token;
                authenticationUser.RefreshTokenExpiration = activeRefreshToken.ExpiredOn;
            }
            else
            {
                var newRefreshToken = GenerateRefreshToken();

                authenticationUser.RefreshToken = newRefreshToken.Token;
                authenticationUser.RefreshTokenExpiration = newRefreshToken.ExpiredOn;
                user.RefreshTokens.Add(newRefreshToken);
                await _userManager.UpdateAsync(user);
            }
            authenticationUser.UserID = user.Id;
            authenticationUser.FullName = user.FullName;

            return authenticationUser;
        }

        private AuthenticationResponse CreateErrorResponse(IEnumerable<IdentityError> errors)
        {
            var errorMessages = string.Join(", ", errors.Select(e => e.Description));
            return new AuthenticationResponse { Message = errorMessages };
        }

        private RefreshToken GenerateRefreshToken()
        {
            byte[] bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return new RefreshToken()
            {
                CreatedOn = DateTime.UtcNow,
                ExpiredOn = DateTime.UtcNow.AddDays(120),
                Token = Convert.ToBase64String(bytes)
            };
        }

        public async Task<AuthenticationResponse> RefreshTokenAsync(string token)
        {
            var user = _userManager.Users
                 .SingleOrDefault(x => x.RefreshTokens.Any(rt => rt.Token == token));

            if (user == null)
            {
                return new AuthenticationResponse() { Message = "Invalid token" };
            }

            var refreshToken = user.RefreshTokens.Single(rt => rt.Token == token);

            if (!refreshToken.IsActive)
            {
                return new AuthenticationResponse() { Message = "Inactive token" };
            }

            refreshToken.RevokedOn = DateTime.UtcNow;

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);

            await _userManager.UpdateAsync(user);

            var authenticationUser = await GenerateJwtToken(user);

            authenticationUser.RefreshToken = newRefreshToken.Token;
            authenticationUser.RefreshTokenExpiration = newRefreshToken.ExpiredOn;

            return authenticationUser;
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var user = _userManager.Users
                .SingleOrDefault(x => x.RefreshTokens.Any(x => x.Token == token));

            if (user == null)
                return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive)
                return false;

            refreshToken.RevokedOn = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return true;
        }
        private string GetCurrentEmail()
        {
            var email = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                throw new UnauthorizedAccessException("User is not authenticated");

            return email;
        }

        public async Task<bool> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDTO.Email!);

            if (user is null)
                return false;


            var otpCode = OtpHelper.GenerateOtp();


            user.OTPCode = otpCode;
            user.OTPExpiration = DateTime.UtcNow.AddMinutes(10);
            await _userManager.UpdateAsync(user);
            string emailBody = EmailBody("Reset Your Password", "We received a request to reset your password. Please use the following OTP code to proceed:", user.Email, otpCode);
            await _emailSender.SendEmailAsync(user.Email, "Reset Password OTP", emailBody);
            return true;
        }
        public Task<UserDto?> GetUserByIdAsync(Guid userID)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == userID);
            if (user == null)
                return Task.FromResult<UserDto?>(null);

            var userdto = new UserDto
            {
                UserID = user.Id,
                Email = user.Email,
                FullName = user.FullName,
            };
            return Task.FromResult<UserDto?>(userdto);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = users.Select(user => new UserDto
            {
                UserID = user.Id,
                Email = user.Email,
                FullName = user.FullName
            });
            return userDtos;
        }
    }
}
