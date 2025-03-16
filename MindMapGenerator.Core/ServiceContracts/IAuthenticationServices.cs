using MindMapGenerator.Core.Dtos.AuthenticationDto;

namespace MindMapGenerator.Core.ServiceContracts
{
    /// <summary>
    /// Defines the contract for authentication-related services.
    /// </summary>
    public interface IAuthenticationServices
    {
        /// <summary>
        /// Registers a new client asynchronously and returns an authentication response.
        /// </summary>
        /// <param name="clientRegisterDTO">The data transfer object containing client registration details.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="AuthenticationResponse"/> with registration details.</returns>
        Task<AuthenticationResponse> RegisterClientAsync(RegisterDTO clientRegisterDTO);

        /// <summary>
        /// Retrieves a user by their unique identifier asynchronously.
        /// </summary>
        /// <param name="userID">The unique identifier (GUID) of the user to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="UserDto"/> if found, or null if not found.</returns>
        Task<UserDto?> GetUserByIdAsync(Guid userID);
        /// <summary>
        /// Retrieves all users asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="IEnumerable{T}"/> if found, or null if not found.</returns>
        Task<IEnumerable<UserDto>> GetAllUsersAsync();

        /// <summary>
        /// Authenticates a user asynchronously using login credentials and returns an authentication response.
        /// </summary>
        /// <param name="loginDTO">The data transfer object containing login credentials (e.g., username and password).</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="AuthenticationResponse"/> with authentication details.</returns>
        Task<AuthenticationResponse> LoginAsync(LoginDTO loginDTO);

        /// <summary>
        /// Refreshes an authentication token asynchronously and returns a new authentication response.
        /// </summary>
        /// <param name="token">The refresh token to be used for generating a new authentication token.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="AuthenticationResponse"/> with refreshed token details.</returns>
        Task<AuthenticationResponse> RefreshTokenAsync(string token);

        /// <summary>
        /// Initiates the forgot password process asynchronously for a user.
        /// </summary>
        /// <param name="forgotPasswordDTO">The data transfer object containing forgot password details (e.g., email).</param>
        /// <returns>A task that represents the asynchronous operation, returning true if the process was successfully initiated, false otherwise.</returns>
        Task<bool> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO);

        /// <summary>
        /// Revokes an authentication token asynchronously.
        /// </summary>
        /// <param name="token">The token to be revoked.</param>
        /// <returns>A task that represents the asynchronous operation, returning true if the token was successfully revoked, false otherwise.</returns>
        Task<bool> RevokeTokenAsync(string token);

        /// <summary>
        /// Assigns a role to a user asynchronously and returns a confirmation message.
        /// </summary>
        /// <param name="model">The data transfer object containing the user and role information.</param>
        /// <returns>A task that represents the asynchronous operation, containing a string message indicating the result of the operation.</returns>
        Task<string> AddRoleToUserAsync(AddRoleDTO model);
    }
}