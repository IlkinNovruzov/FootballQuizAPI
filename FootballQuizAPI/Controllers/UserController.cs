using FootballQuizAPI.DAL;
using FootballQuizAPI.DTO;
using FootballQuizAPI.Models;
using FootballQuizAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;

namespace FootballQuizAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, AppDbContext context, TokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _tokenService = tokenService;
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


        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] string code)
        {
            var random = new Random();
            var username = "User" + random.Next(1000000, 9999999);
            var password = random.Next(10000, 99999).ToString();
            var defaultCountryCode = "US";

            var user = new User
            {
                UserName = username,
                CountryCode = code,
                Level = 1
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                var roleRusult = await _userManager.AddToRoleAsync(user, "User");

                if (!roleRusult.Succeeded) return BadRequest("Error Role Assignment");

                return Ok(new { username, password, country = defaultCountryCode });
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




        [HttpPost("createRandom100")]
        public async Task<IActionResult> CreateUsers()
        {
            var random = new Random();
            var countryCodes = new[]
            {
        "us", "de", "fr", "gb", "it", "es", "jp", "cn", "kr", "in",
        "br", "za", "mx", "au", "ca", "ru", "se", "no", "fi", "dk",
        "pl", "nl", "be", "ch", "at", "cz", "hu", "sk", "ro", "bg",
        "hr", "si", "lt", "lv", "ee", "ie", "pt", "tr", "il", "sa",
        "ae", "ng", "ke", "gh", "eg", "ma", "dz", "tn", "qa", "om",
        "kw", "bh", "sg", "my", "ph", "vn", "th", "id", "pk"
           };

            var userCreationResults = new List<object>();

            for (int i = 0; i < 100; i++)
            {
                var username = "User" + random.Next(1000000, 9999999);
                var password = random.Next(10000, 99999).ToString();
                var countryCode = countryCodes[random.Next(countryCodes.Length)];

                var user = new User
                {
                    UserName = username,
                    CountryCode = countryCode,
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");

                    if (!roleResult.Succeeded)
                    {
                        return BadRequest("Error Role Assignment");
                    }

                    userCreationResults.Add(new { username, password, country = countryCode });
                }
                else
                {
                    userCreationResults.Add(new { username, error = result.Errors });
                }
            }

            return Ok(userCreationResults);
        }


    }
}
