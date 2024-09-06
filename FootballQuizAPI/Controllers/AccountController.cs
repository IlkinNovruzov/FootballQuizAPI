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
                case "bestscore":
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

            var results = await query
                .GroupBy(q => q.UserId)
                .Select(g => new
                {
                    User = new GetUserDTO
                    {
                        UserName = g.First().User.UserName,
                        CountryCode = g.First().User.CountryCode
                    },
                    TotalXP = g.Sum(q => q.XP),
                    BestScore = g.Max(q => q.XP)
                })
                .OrderByDescending(g => period == "bestscore" ? g.BestScore : g.TotalXP)
                .Take(1000)
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

        [HttpGet("user/stats")]
        public async Task<ActionResult<object>> GetUserStats([FromHeader(Name = "Authorization")] string token)
        {
            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);
            if (user == null) return Unauthorized("Invalid token or user.");

            int id = user.Id;
            var countryCode = user.CountryCode;
            var now = DateTime.UtcNow;
            var userQuizzes = _context.QuizResults
                .Where(q => q.UserId == id);

            var todayXP = await userQuizzes
                .Where(q => q.DateTaken.Date == now.Date)
                .SumAsync(q => q.XP);

            var monthXP = await userQuizzes
                .Where(q => q.DateTaken.Year == now.Year && q.DateTaken.Month == now.Month)
                .SumAsync(q => q.XP);

            var yearXP = await userQuizzes
                .Where(q => q.DateTaken.Year == now.Year)
                .SumAsync(q => q.XP);

            var highestScore = await userQuizzes
                .OrderByDescending(q => q.XP)
                .Select(q => q.XP)
                .FirstOrDefaultAsync();

            var totalXP = await userQuizzes.SumAsync(q => q.XP);

            var todayRank = (await _context.QuizResults
                .Where(q => q.DateTaken.Date == now.Date)
                .GroupBy(q => q.UserId)
                .Select(g => new { UserId = g.Key, TotalXP = g.Sum(q => q.XP) })
                .OrderByDescending(g => g.TotalXP)
                .ToListAsync())
                .Select((g, index) => new { g.UserId, Rank = index + 1 })
                .FirstOrDefault(g => g.UserId == id);

            var monthRank = (await _context.QuizResults
               .Where(q => q.DateTaken.Year == now.Year && q.DateTaken.Month == now.Month)
               .GroupBy(q => q.UserId)
               .Select(g => new { UserId = g.Key, TotalXP = g.Sum(q => q.XP) })
               .OrderByDescending(g => g.TotalXP)
               .ToListAsync())
               .Select((g, index) => new { g.UserId, Rank = index + 1 })
               .FirstOrDefault(g => g.UserId == id);

            var yearRank = (await _context.QuizResults
              .Where(q => q.DateTaken.Year == now.Year)
              .GroupBy(q => q.UserId)
              .Select(g => new { UserId = g.Key, TotalXP = g.Sum(q => q.XP) })
              .OrderByDescending(g => g.TotalXP)
              .ToListAsync())
              .Select((g, index) => new { g.UserId, Rank = index + 1 })
              .FirstOrDefault(g => g.UserId == id);

            var totalRank = (await _context.QuizResults
             .GroupBy(q => q.UserId)
             .Select(g => new { UserId = g.Key, TotalXP = g.Sum(q => q.XP) })
             .OrderByDescending(g => g.TotalXP)
             .ToListAsync())
             .Select((g, index) => new { g.UserId, Rank = index + 1 })
             .FirstOrDefault(g => g.UserId == id);

            var highestScoreRank = (await _context.QuizResults
             .GroupBy(q => q.UserId)
             .Select(g => new { UserId = g.Key, HighestScore = g.Max(q => q.XP) })
             .OrderByDescending(g => g.HighestScore)
             .ToListAsync())
             .Select((g, index) => new { g.UserId, Rank = index + 1 })
             .FirstOrDefault(g => g.UserId == id);

            try
            {
                var userStats = new
                {
                    TodayXP = todayXP,
                    TodayRank = todayRank?.Rank,
                    MonthXP = monthXP,
                    MonthRank = monthRank?.Rank,
                    YearXP = yearXP,
                    YearRank = yearRank?.Rank,
                    HighestScore = highestScore,
                    HighestScoreRank = highestScoreRank?.Rank,
                    TotalXP = totalXP,
                    TotalRank = totalRank?.Rank,
                    CountryCode = countryCode
                };

                return Ok(userStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("user-info")]
        public async Task<ActionResult> GetUser([FromHeader(Name = "Authorization")] string token)
        {
            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);
            if (user == null) return Unauthorized(new { text = "Invalid token or user." });

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
                return NotFound(new { text = "User not found" });
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

            if (result.Succeeded)
            {
                var jwtToken = await _tokenService.GenerateToken(user);
                return Ok(new { jwtToken });

            }

            if (result.IsLockedOut)
            {
                return BadRequest(new { text = "User account locked out." });
            }

            return BadRequest(new { text = "Invalid login attempt." });
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
            var (currentLevel, xpForNextLevel, currentLevelXP) = CalculateLevelAndXP(user.XP);
            user.Level = currentLevel;
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

            return Ok(new { Message = "saved" });
        }

        [HttpGet("home")]
        public async Task<IActionResult> HomePage([FromHeader(Name = "Authorization")] string token)
        {
            var user = await _tokenService.ValidateTokenAndGetUserAsync(token);
            if (user == null) return Unauthorized("Invalid token or user.");

            var (currentLevel, xpForNextLevel, currentLevelXP) = CalculateLevelAndXP(user.XP);
            var userRank = 0;
            var userRankResult = await _context.Users.Where(u => u.XP > user.XP).CountAsync() + 1;
            if (userRankResult != 0 && user.XP != 0 && userRankResult < 10000) userRank = userRankResult;
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
                XPForNextLevel = xpForNextLevel,
                UserRank = userRank
            };
            return Ok(homePageModel);
        }


        private static (byte currentLevel, long xpForNextLevel, long currentLevelXP) CalculateLevelAndXP(long totalXP)
        {
            byte level = 1;
            long xpForNextLevel = 500;
            long remainingXP = totalXP;

            while (remainingXP >= xpForNextLevel)
            {
                remainingXP -= xpForNextLevel;
                level++;
                xpForNextLevel = (long)Math.Round(xpForNextLevel * 1.9);
            }

            return (level, xpForNextLevel, remainingXP);
        }
    }
}
