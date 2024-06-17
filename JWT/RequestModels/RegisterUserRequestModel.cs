using System.ComponentModel.DataAnnotations;

namespace JWT.RequestModels;

public class RegisterUserRequestModel
{
    [MaxLength(100)]
    [MinLength(4)]
    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string UserPassword { get; set; } = null!;
}