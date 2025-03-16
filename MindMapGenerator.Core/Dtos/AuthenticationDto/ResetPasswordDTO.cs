namespace MindMapGenerator.Core.Dtos.AuthenticationDto
{
    public class ResetPasswordDTO
    {
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }

        public string? Email { get; set; }

    }
}
