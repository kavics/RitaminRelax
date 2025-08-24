using Microsoft.AspNetCore.Mvc;
using SenseNet.BusinessSolutions.Common;
using SenseNet.ContentRepository.Security.ApiKeys;
using SenseNet.ContentRepository.Storage.Security;
using User = SenseNet.ContentRepository.User;

namespace RitaminRelax.Controllers;

/// <summary>
/// Response model for login operations
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets the username of the authenticated user
    /// </summary>
    public string UserName { get; set; }
    
    /// <summary>
    /// Gets or sets the generated authentication token
    /// </summary>
    public string Token { get; set; }
}

/// <summary>
/// Controller for handling user authentication
/// </summary>
[ApiController]
[Route("[controller]")]
public class LoginController(IApiKeyManager apiKeyManager, ILogger<LoginController> logger) : ControllerBase
{
    /// <summary>
    /// Authenticates a user and returns login information with token in headers
    /// </summary>
    /// <param name="userName">The username for authentication</param>
    /// <param name="password">The password for authentication</param>
    /// <returns>Login response containing user information and token</returns>
    [HttpPost]
    public async Task<LoginResponse> Post([FromForm] string userName, [FromForm] string password)
    {
        var loggedInUser = await BsTools.LoginByNameOrEmailAsync<User>(userName, password, null, HttpContext, logger, HttpContext.RequestAborted);
        if (loggedInUser == null)
        {
            Response.StatusCode = 401; // Unauthorized
            return new LoginResponse();
        }

        SenseNet.ContentRepository.User.Current = loggedInUser;

        var apiKey = await GetOrCreateApiKeyAsync(loggedInUser.Id, loggedInUser.Email).ConfigureAwait(false);

        var token = apiKey?.Value;

        // Add token to response headers
        Response.Headers["ApiKey"] = token;
        
        return new LoginResponse
        {
            UserName = userName,
            Token = token
        };
    }

    private async Task<ApiKey> GetOrCreateApiKeyAsync(int userId, string userNameOrEmail)
    {
        ApiKey? apiKey;
        string apiKeyDisplay;

        using (new SystemAccount())
        {
            var apiKeys =
                await apiKeyManager.GetApiKeysByUserAsync(userId, HttpContext.RequestAborted);
            apiKey = apiKeys.OrderByDescending(x => x.ExpirationDate).FirstOrDefault();
        }

        if (apiKey == null)
        {
            using (new SystemAccount())
            {
                apiKey = await apiKeyManager.CreateApiKeyAsync(userId, DateTime.UtcNow.AddYears(1),
                    HttpContext.RequestAborted);
                apiKeyDisplay = $"{apiKey.Value.Substring(0, 4)}...";
                logger.LogInformation("Creating new API key for user {User}: {ApiKeyTruncated}", userNameOrEmail, apiKeyDisplay);
            }
        }
        else
        {
            apiKeyDisplay = $"{apiKey.Value.Substring(0, 4)}...";
        }

        logger.LogInformation("User {User} requested API key: {ApiKeyTruncated}", userNameOrEmail, apiKeyDisplay);

        return apiKey;
    }
}