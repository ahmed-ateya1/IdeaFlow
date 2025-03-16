using Microsoft.AspNetCore.Http;
using MindMapGenerator.Core.Helper;

namespace MindMapGenerator.Core.Dtos.AuthenticationDto
{
    public class RegisterDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public RolesOption Role { get; set; } = RolesOption.USER;
    }
}
