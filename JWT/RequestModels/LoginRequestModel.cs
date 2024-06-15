using System.ComponentModel.DataAnnotations;

namespace JWT.RequestModels;

public class LoginRequestModel
{
    [MaxLength(100)] public string Username { get; set; } = null!;
    public string UserPassword { get; set; } = null!;
}