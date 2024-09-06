namespace FootballQuizAPI.DTO
{
    public class HomePageDTO
    {
        public string Username { get; set; }
        public string? Email { get; set; }
        public long XP { get; set; }
        public string? CountryCode { get; set; }
        public byte Level { get; set; } = 1;
        public byte Heart { get; set; } = 5;
        public byte ExtraHeart { get; set; } = 25;
        public byte Hint { get; set; } = 10;
        public byte Chest { get; set; }
        public long CurrentXP { get; set; }
        public long XPForNextLevel { get; set; }
        public int UserRank { get; set; }
        
    }
}
