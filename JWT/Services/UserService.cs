using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JWT.Contexts;
using JWT.Exceptions;
using JWT.Models;
using JWT.RequestModels;
using JWT.ResponseModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JWT.Services;


public interface IUserService
{
    Task<LoginResponseModel> LoginUserAsync(LoginRequestModel requestModel, IConfiguration configuration);
    Task<RefreshTokenResponseModel> RefreshUserToken(RefreshTokenRequestModel requestModel, IConfiguration configuration);
    Task RegisterUserAsync(RegisterUserRequestModel requestModel);
}

public class UserService(DatabaseContext context) : IUserService
{
    public async Task<LoginResponseModel> LoginUserAsync(LoginRequestModel requestModel, IConfiguration configuration)
    {
        var user = await context.Users.Where(u => u.UserName == requestModel.Username).FirstOrDefaultAsync();
    
        if (user is null)
        {
            throw new InvalidUserLoginDataException("Login or password is invalid");
        }
    
        var passwordHasher = new PasswordHasher<User>();
        if (passwordHasher.VerifyHashedPassword(user, user.UserPassword, requestModel.UserPassword) ==
            PasswordVerificationResult.Failed)
        {
            throw new InvalidUserLoginDataException("Login or password is invalid");
        }
        
        var stringToken = CreateJwtToken(configuration, requestModel.Username);
        var stringRefToken = CreateJwtRefreshToken(configuration, requestModel.Username);
        
        return new LoginResponseModel()
        {
            Token = stringToken,
            RefreshToken = stringRefToken
        };
        
        
    }


    public async Task RegisterUserAsync(RegisterUserRequestModel requestModel)
    {
        var isUserNameUnique = !(await context.Users.AnyAsync(u => u.UserName == requestModel.Username));
        
        if (!isUserNameUnique)
        {
            throw new UserNameTakenException($"User with the name of {requestModel.Username} already exists");
        }

        if (!requestModel.Username.Contains('@') || !requestModel.Username.Contains('.'))
        {
            throw new InvalidUserNameException("Email doesn't containt @ or .");
        }
        
        var newUser = new User()
        {
            UserName = requestModel.Username
        };
        
        var passwordHasher = new PasswordHasher<User>();
        var userPassword = passwordHasher.HashPassword(newUser, requestModel.UserPassword);

        newUser.UserPassword = userPassword;

        await context.Users.AddAsync(newUser);
        await context.SaveChangesAsync();

    }

    public async Task<RefreshTokenResponseModel> RefreshUserToken(RefreshTokenRequestModel requestModel,
        IConfiguration configuration)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        try
        {
            tokenHandler.ValidateToken(requestModel.RefreshToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JWT:RefIssuer"],
                ValidAudience = configuration["JWT:RefAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:RefKey"]!))
            }, out SecurityToken validatedToken);

            
            var oldRefreshToken = tokenHandler.ReadJwtToken(requestModel.RefreshToken);
            
            var stringToken =
                CreateJwtToken(configuration, oldRefreshToken.Claims.First(c => c.Type == "username").Value);
            
            var stringRefToken = CreateJwtRefreshToken(configuration,oldRefreshToken.Claims.First(c =>c.Type == "username").Value);
            
            
            
            return new RefreshTokenResponseModel()
            {
                Token = stringToken,
                RefreshToken = stringRefToken
            };

        }
        catch
        {
            throw new InvalidRefreshTokenException("Provided refresh token is invalid");
        }
    }

    private string CreateJwtToken(IConfiguration configuration, string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescription = new SecurityTokenDescriptor
        {
            Issuer = configuration["JWT:Issuer"],
            Audience = configuration["JWT:Audience"],
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]!)),
                SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(new List<Claim>
            {
                new Claim("username", username)
            })
        };
        
        var token = tokenHandler.CreateToken(tokenDescription);
        return tokenHandler.WriteToken(token);
    }

    private string CreateJwtRefreshToken(IConfiguration configuration, string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var refTokenDescription = new SecurityTokenDescriptor
        {
            Issuer = configuration["JWT:RefIssuer"],
            Audience = configuration["JWT:RefAudience"],
            Expires = DateTime.UtcNow.AddDays(3),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:RefKey"]!)),
                SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(new List<Claim>
            {
                new Claim("username", username)
            })
        };

        var refToken = tokenHandler.CreateToken(refTokenDescription);
        return tokenHandler.WriteToken(refToken);
    }
}