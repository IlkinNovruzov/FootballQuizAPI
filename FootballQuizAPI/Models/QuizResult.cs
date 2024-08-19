using FootballQuizAPI.DAL;

namespace FootballQuizAPI.Models
{
    public class QuizResult:BaseEntity
    {
        public long XP { get; set; } 
        public DateTime DateTaken { get; set; } 
        public int UserId { get; set; } 
        public User User { get; set; }
    }
}
