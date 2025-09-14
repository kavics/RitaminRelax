using System.ComponentModel.DataAnnotations;

namespace RitaminRelax.Models;

/// <summary>
/// View model for user login form
/// </summary>
public class LoginViewModel
{
    /// <summary>
    /// Gets or sets the username or email address for login
    /// </summary>
    [Required(ErrorMessage = "A név vagy email cím megadása kötelezõ")]
    [Display(Name = "Név vagy email cím")]
    public string NameOrEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for login
    /// </summary>
    [Required(ErrorMessage = "A jelszó megadása kötelezõ")]
    [DataType(DataType.Password)]
    [Display(Name = "Jelszó")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to remember the user login
    /// </summary>
    [Display(Name = "Emlékezzen rám")]
    public bool RememberMe { get; set; } = false;
}