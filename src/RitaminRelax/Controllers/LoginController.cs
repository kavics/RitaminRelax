using Microsoft.AspNetCore.Mvc;
using RitaminRelax.Models;
using SenseNet.BusinessSolutions.Common;
using SenseNet.ContentRepository.Security.ApiKeys;
using SenseNet.ContentRepository.Storage.Security;
using User = SenseNet.ContentRepository.User;

namespace RitaminRelax.Controllers;

/// <summary>
/// MVC Controller for handling user authentication
/// </summary>
public class LoginController(IApiKeyManager apiKeyManager, ILogger<LoginController> logger) : Controller
{
    /// <summary>
    /// Display login form
    /// </summary>
    /// <returns>Login view</returns>
    public IActionResult Login()
    {
        var model = new LoginViewModel();
        return View(model);
    }

    /// <summary>
    /// Process login form submission
    /// </summary>
    /// <param name="model">Login form data</param>
    /// <returns>Redirect or return to login form</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Véletlenszerű késleltetés a brute-force támadások csökkentésére
        var rnd = new Random();
        await System.Threading.Tasks.Task.Delay(rnd.Next(500, 800), HttpContext.RequestAborted).ConfigureAwait(false);

        if (ModelState.IsValid)
        {
            try
            {
                // Try to authenticate using SenseNet authentication
                var loggedInUser = await BsTools.LoginByNameOrEmailAsync<User>(model.NameOrEmail, model.Password, null, HttpContext, logger, HttpContext.RequestAborted);
                
                if (loggedInUser != null)
                {
                    // API key létrehozása vagy lekérése a felhasználóhoz
                    var apiKey = await GetOrCreateApiKeyAsync(loggedInUser.Id, model.NameOrEmail);
                    if (apiKey == null)
                    {
                        logger.LogError("Nem sikerült API key-t létrehozni a felhasználóhoz: {Email}", model.NameOrEmail);
                        ModelState.AddModelError("", "Hiba történt a bejelentkezés során");
                        return View(model);
                    }

                    // API key mentése cookie-ba
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,      // JavaScript nem férhet hozzá (biztonság)
                        Secure = Request.IsHttps, // Csak HTTPS-en keresztül production-ben
                        SameSite = SameSiteMode.Strict,
                        Expires = model.RememberMe ? apiKey.ExpirationDate : DateTime.UtcNow.AddHours(1) // 1 óra ha nincs "emlékezzen rám"
                    };

                    Response.Cookies.Append("RRApiKey", apiKey.Value, cookieOptions);

                    logger.LogInformation("Sikeres bejelentkezés: {Email} (User ID: {UserId})", model.NameOrEmail, loggedInUser.Id);

                    TempData["SuccessMessage"] = $"Sikeres bejelentkezés! Üdvözöljük, {loggedInUser.DisplayName ?? model.NameOrEmail}";
                    return RedirectToAction("Index", "Booking");
                }
                else
                {
                    ModelState.AddModelError("", "Hibás név/email vagy jelszó.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Login error for user: {Email}", model.NameOrEmail);
                ModelState.AddModelError("", "Hiba történt a bejelentkezés során. Kérjük, próbálja újra.");
            }
        }

        return View(model);
    }

    /// <summary>
    /// Logout user
    /// </summary>
    /// <returns>Redirect to home page</returns>
    [HttpPost]
    public IActionResult Logout()
    {
        // API key cookie törlése
        if (Request.Cookies.ContainsKey("RRApiKey"))
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1) // Múltbeli dátum a cookie törléshez
            };
            Response.Cookies.Append("RRApiKey", "", cookieOptions);
        }

        logger.LogInformation("Felhasználó kijelentkezett");
        TempData["SuccessMessage"] = "Sikeresen kijelentkezett.";
        return RedirectToAction("Index", "Booking");
    }

    /// <summary>
    /// API key létrehozása vagy lekérése a felhasználóhoz
    /// </summary>
    private async Task<ApiKey> GetOrCreateApiKeyAsync(int userId, string userNameOrEmail)
    {
        using var loggerScope = logger.BeginScope(new Dictionary<string, object>
            {
                { "UserId", userId },
                { "UserNameOrEmail", userNameOrEmail }
            });

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
                apiKey = await apiKeyManager.CreateApiKeyAsync(userId, DateTime.UtcNow.AddMonths(3),
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