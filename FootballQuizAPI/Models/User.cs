using Microsoft.AspNetCore.Identity;

namespace FootballQuizAPI.Models
{
    public class User:IdentityUser<int>
    {
        public long XP { get; set; }
        public string? CountryCode { get; set; }
        public byte Level { get; set; } = 1;
        public byte Heart { get; set; } = 5;
        public byte ExtraHeart { get; set; } = 25;
        public byte Hint { get; set; } = 10;
        public byte Chest { get; set; }
        public List<QuizResult> QuizResults { get; set; }
    }
}
