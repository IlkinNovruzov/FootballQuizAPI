using FootballQuizAPI.DAL;

namespace FootballQuizAPI.Models
{
    public class Category:BaseEntity
    {
        public string Name { get; set; }
        public byte UnlockLevel { get; set; }
        public string UnlockLevels { get; set; }
        public List<Question> Questions { get; set; }
    }
}
