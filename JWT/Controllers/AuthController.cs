using JWT.Exceptions;
using JWT.RequestModels;
using JWT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWT.Controllers;



[Route("api/auth")]
[ApiController]
public class AuthController(IConfiguration configuration, IUserService service) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequestModel model)
    {
        try
        {
            return Ok(await service.LoginUserAsync(model, configuration));
        }
        catch (InvalidUserLoginDataException e)
        {
            return Unauthorized(e.Message);
        }
    }
    
    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterUserRequestModel model)
    {
        try
        {
            await service.RegisterUserAsync(model);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequestModel model)
    {
        try
        {
            var results = await service.RefreshUserToken(model,configuration);
            return Ok(results);
        }
        catch (InvalidRefreshTokenException e)
        {
            return Unauthorized(e.Message);
        }
    }
    
    [HttpGet("protected-information")]
    [Authorize]
    public IActionResult GetProtectedInformation()
    {
        return Ok("Your protected information");
    }
}