namespace MindMapGenerator.Core.Dtos.AuthenticationDto
{
    public class AddRoleDTO
    {
        public Guid UserID { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
