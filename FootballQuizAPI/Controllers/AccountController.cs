using FootballQuizAPI.DAL;
using FootballQuizAPI.DTO;
using FootballQuizAPI.Models;
using FootballQuizAPI.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballQuizAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, AppDbContext context, TokenService tokenService, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<List<IdentityUser>>> GetUsers(int page = 1, int pageSize = 10, string? countryCode = null)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(countryCode))
            {
                query = query.Where(u => u.CountryCode == countryCode);
            }

            var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var totalUsers = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var response = new
            {
                Users = users,
                TotalPages = totalPages,
                CurrentPage = page
            };

            return Ok(response);
        }

        [HttpPost("user-info")]
        public async Task<ActionResult> GetUser([FromHeader(Name = "Authorization")] string token)
        {
            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);
            if (user == null) return Unauthorized("Invalid token or user.");
            
            return Ok(user);
        }

            [HttpPost("register")]
        public async Task<IActionResult> CreateUser([FromBody] UpdateUserDTO dto)
        {
            var user = new User
            {
                UserName = dto.UserName.Trim(),
                CountryCode = dto.CountryCode,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                var roleRusult = await _userManager.AddToRoleAsync(user, "User");

                if (!roleRusult.Succeeded) return BadRequest("Error Role Assignment");

                return Ok(new { Message = "Register successful" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null) return NotFound("User not found");

            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);
            user.CountryCode = model.CountryCode;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

            if (result.Succeeded)
            {
                var jwtToken = await _tokenService.GenerateToken(user);
                return Ok(jwtToken);
            }

            if (result.IsLockedOut)
            {
                return BadRequest("User account locked out.");
            }

            return BadRequest("Invalid login attempt.");
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleUserDto googleUser)
        {
            var info = new UserLoginInfo("Google", googleUser.Sub, "Google");

            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(googleUser.Email);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = googleUser.Name.Trim(),
                        Email = googleUser.Email,
                        EmailConfirmed = true,
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        return BadRequest(result.Errors);
                    }
                }

                await _userManager.AddLoginAsync(user, info);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            var jwtToken = await _tokenService.GenerateToken(user);

            return Ok(new { jwtToken });
        }
    }
}
