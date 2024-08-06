using FootballQuizAPI.DAL;

namespace FootballQuizAPI.Models
{
    public class Question : BaseEntity
    {
        public string QuestionText { get; set; }
        public string Difficulty { get; set; }
        public string Answer { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public List<Choice> Choices { get; set; }
    }
}
