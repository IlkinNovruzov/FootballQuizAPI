using FootballQuizAPI.DAL;
using FootballQuizAPI.DTO;
using FootballQuizAPI.Models;
using FootballQuizAPI.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

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

        [HttpGet("top1000")]
        public async Task<ActionResult<IEnumerable<object>>> GetTop1000([FromQuery] string period, int page = 1, int pageSize = 10, string? countryCode = null)
        {
            var now = DateTime.UtcNow;
            IQueryable<QuizResult> query = _context.QuizResults.Include(q => q.User);

            if (!string.IsNullOrEmpty(countryCode)) query = query.Where(q => q.User.CountryCode == countryCode);

            switch (period.ToLower())
            {
                case "day":
                    query = query.Where(q => q.DateTaken.Date == now.Date);
                    break;
                case "month":
                    query = query.Where(q => q.DateTaken.Year == now.Year && q.DateTaken.Month == now.Month);
                    break;
                case "year":
                    query = query.Where(q => q.DateTaken.Year == now.Year);
                    break;
                case "alltime":
                    break;
                default:
                    return BadRequest("Invalid period specified.");
            }

            var totalUsers = await query
                .GroupBy(q => q.UserId)
                .CountAsync();

            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            // İlk 1000 kullanıcıyı getir
            var results = await query
                .GroupBy(q => q.UserId)
                .Select(g => new
                {
                    User = g.FirstOrDefault() != null ? g.FirstOrDefault().User : null,
                    TotalXP = g.Sum(q => q.XP) // Kullanıcının toplam XP'sini hesapla
                })
                .OrderByDescending(g => g.TotalXP) // Kullanıcıları XP'ye göre sırala
                .Take(1000) // İlk 1000 kullanıcıyı al
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new
            {
                Users = results,
                TotalPages = totalPages,
                CurrentPage = page,
                TotalUsers = totalUsers
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
                UserName = dto.UserName,
                CountryCode = dto.CountryCode,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "User");

                if (!roleResult.Succeeded)
                {
                    return BadRequest(new
                    {
                        Message = "User creation succeeded, but role assignment failed.",
                        Errors = roleResult.Errors.Select(e => e.Description)
                    });
                }

                var jwtToken = await _tokenService.GenerateToken(user);
                if (jwtToken == null) return StatusCode(500, "User creation and role assignment succeeded, but token generation failed.");

                return Ok(new { jwtToken });
            }

            return BadRequest(new
            {
                Message = "User creation failed.",
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO model, [FromHeader(Name = "Authorization")] string token)
        {
            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);
            if (user == null) return Unauthorized("Invalid token or user.");

            user.UserName = model.UserName;
            if (user.PasswordHash != null)
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);
            user.CountryCode = model.CountryCode;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) return Ok();

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
                return Ok(new { jwtToken });

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
                        UserName = googleUser.Name.Replace(" ", ""),
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

            return Ok(new { token = jwtToken, countryCode = user.CountryCode });
        }

        [HttpPost("result")]
        public async Task<IActionResult> AddQuizResult([FromBody] long xp, [FromHeader(Name = "Authorization")] string token)
        {
            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);
            if (user == null) return Unauthorized("Invalid token or user.");

            user.XP += xp;
            var quizResult = new QuizResult
            {
                XP = xp,
                IsActive = true,
                UserId = user.Id,
                DateTaken = DateTime.UtcNow
            };
            _context.QuizResults.Add(quizResult);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok("saved");
        }

        [HttpGet("home")]
        public async Task<IActionResult> HomePage([FromHeader(Name = "Authorization")] string token)
        {
            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);
            if (user == null) return Unauthorized("Invalid token or user.");

            var (currentLevel, xpForNextLevel, currentLevelXP) = CalculateLevelAndXP(user.XP);

            var homePageModel = new HomePageDTO
            {
                Username = user.UserName,
                Email = user.Email,
                CountryCode = user.CountryCode,
                Level = currentLevel,
                Heart = user.Heart,
                ExtraHeart = user.ExtraHeart,
                Chest = user.Chest,
                Hint = user.Hint,
                CurrentXP = currentLevelXP,
                XPForNextLevel = xpForNextLevel
            };
            return Ok(homePageModel);
        }


        private static (byte currentLevel, long xpForNextLevel, long currentLevelXP) CalculateLevelAndXP(long totalXP)
        {
            byte level = 1;
            long xpForNextLevel = 100;
            long remainingXP = totalXP;

            while (remainingXP >= xpForNextLevel)
            {
                remainingXP -= xpForNextLevel;
                level++;
                xpForNextLevel = (long)Math.Round(xpForNextLevel * 1.5);
            }

            return (level, xpForNextLevel, remainingXP);
        }
    }
}
