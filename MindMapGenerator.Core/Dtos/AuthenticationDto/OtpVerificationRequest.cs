using System.ComponentModel.DataAnnotations;

namespace MindMapGenerator.Core.Dtos.AuthenticationDto
{
    public class OtpVerificationRequest
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Otp { get; set; }
    }
}
