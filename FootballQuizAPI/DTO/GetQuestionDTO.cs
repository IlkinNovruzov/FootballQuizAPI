using FootballQuizAPI.Models;

namespace FootballQuizAPI.DTO
{
    public class CreateQuestionDTO
    {
        public string QuestionText { get; set; }
        public string Difficulty { get; set; }
        public string Answer { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public List<string> Choices { get; set; }

    }
    public class GetQuestionDTO
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string Answer { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> Choices { get; set; }
    }
}
