using Microsoft.AspNetCore.Mvc;
using RitaminRelax.Models;

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
public class LoginController : ControllerBase
{
    /// <summary>
    /// Authenticates a user and returns login information with token in headers
    /// </summary>
    /// <param name="userName">The username for authentication</param>
    /// <param name="password">The password for authentication</param>
    /// <returns>Login response containing user information and token</returns>
    [HttpPost]
    public LoginResponse Post(string userName, string password)
    {
        //UNDONE: Validate user credentials (BsTools)
        var token = GetToken(userName);
        
        // Add token to response headers
        Response.Headers.Add("ApiKey", token);
        
        return new LoginResponse
        {
            UserName = userName,
            Token = token
        };
    }
    
    /// <summary>
    /// Generates an authentication token for the specified user
    /// </summary>
    /// <param name="userName">The username to generate token for</param>
    /// <returns>Base64 encoded token string</returns>
    private string GetToken(string userName)
    {
        //UNDONE: Get apikey for user from database
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userName + "_token"));
    }
}