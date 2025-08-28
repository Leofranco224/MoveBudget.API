using Microsoft.AspNetCore.Mvc;
using MoveBudget.API.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registra um novo usuário
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        var success = await _authService.RegisterAsync(dto);
        if (!success)
            return BadRequest("Username já existe");

        return Ok("Usuário registrado com sucesso");
    }

    /// <summary>
    /// Login do usuário, retorna JWT e Refresh Token
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (result == null)
            return Unauthorized("Usuário ou senha inválidos");

        return Ok(result);
    }

    /// <summary>
    /// Gera novo JWT usando Refresh Token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        var result = await _authService.RefreshTokenAsync(refreshToken);
        if (result == null)
            return Unauthorized("Refresh Token inválido ou expirado");

        return Ok(result);
    }
}