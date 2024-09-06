using FootballQuizAPI.DAL;
using FootballQuizAPI.DTO;
using FootballQuizAPI.Models;
using FootballQuizAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FootballQuizAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public QuestionController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpGet("get-categories")]
        public async Task<ActionResult<List<IdentityUser>>> GetCategories(int page = 1, int pageSize = 7)
        {
            var query = _context.Categories.AsQueryable();

            var categories = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var totalCategories = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalCategories / (double)pageSize);

            var response = new
            {
                Categories = categories,
                TotalPages = totalPages,
                CurrentPage = page
            };

            return Ok(response);
        }

        [HttpPost("get-questions")]
        public async Task<IActionResult> GetQuestions([FromBody] List<DifficultyDTO> categoryDifficultyRequests)
        {
            if (categoryDifficultyRequests == null || categoryDifficultyRequests.Count == 0) return BadRequest("Kategori ve zorluk seviyeleri gönderilmedi.");

            var questionsList = new List<GetQuestionDTO>();

            foreach (var request in categoryDifficultyRequests)
            {
                var questionsQuery = _context.Questions
                    .Include(q => q.Choices)
                    .Where(q => q.CategoryId == request.CategoryId && q.Difficulty == request.Difficulty);

                if (await questionsQuery.CountAsync() == 0) continue;

                var questions = await questionsQuery
                    .OrderBy(q => Guid.NewGuid()) // Sırayı rastgele yap
                    .Take(5) // Belirtilen sayıda soruyu al
                    .Select(q => new GetQuestionDTO
                    {
                        Id = q.Id,
                        QuestionText = q.QuestionText,
                        Answer = q.Answer,
                        ImageUrl = q.ImageUrl,
                        Choices = q.Choices.Select(c => c.Text).ToList()
                    })
                    .ToListAsync();

                questionsList.AddRange(questions);
            }

            if (questionsList.Count == 0) return NotFound("Belirtilen kategoriler ve zorluk seviyeleri için soru bulunamadı.");

            // Soruları karıştır
            questionsList = questionsList.OrderBy(q => Guid.NewGuid()).ToList();

            return Ok(questionsList);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestion(int id)
        {
            var question = await _context.Questions.Include(q => q.Choices).FirstOrDefaultAsync(q => q.Id == id);

            if (question == null) return NotFound();

            var getQuestionDTO = new GetQuestionDTO
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                Answer = question.Answer,
                ImageUrl = question.ImageUrl,
                Choices = question.Choices.Select(c => c.Text).ToList()
            };

            return Ok(getQuestionDTO);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionDTO dto)
        {
            var test =await  _context.Questions.FirstOrDefaultAsync(q => q.Answer == dto.Answer);
            if(test != null) return BadRequest(new {Message="Sameeeeee"});
            var question = new Question
            {
                QuestionText = dto.QuestionText,
                Answer = dto.Answer,
                Difficulty = dto.Difficulty,
                CategoryId = dto.CategoryId,
                ImageUrl = dto.ImageUrl,
                IsActive = true
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            foreach (var item in dto.Choices)
            {
                var choice = new Choice
                {
                    QuestionId = question.Id,
                    Text = item
                };
                _context.Choices.Add(choice);
            }
            await _context.SaveChangesAsync();

            return Ok(new { ok = "Succsed" });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteQuestionAsync([FromRoute] int questionId)
        {
            var question = await _context.Questions.Include(q => q.Choices).FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null) return NotFound("Question not found.");

            _context.Questions.Remove(question);

            await _context.SaveChangesAsync();
            return Ok("Removed");
        }

        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAllQuestionsAsync()
        {
            var questions = await _context.Questions.Include(q => q.Choices).ToListAsync();

            if (questions.Count == 0) return NotFound("No questions found.");

            _context.Questions.RemoveRange(questions);

            await _context.SaveChangesAsync();

            return Ok("All questions removed.");
        }

        [HttpPost("createRandom20")]
        public async Task<IActionResult> CreateUsers()
        {
            var random = new Random();
            var players = new List<string> {
    "Lionel Messi", "Cristiano Ronaldo", "Neymar Jr.", "Kylian Mbappe", "Kevin De Bruyne", "Sergio Ramos",
    "Virgil van Dijk", "Mohamed Salah", "Sadio Mane", "Harry Kane", "Eden Hazard", "Luka Modric",
    "Toni Kroos", "Karim Benzema", "Gareth Bale", "Raheem Sterling", "Paul Pogba", "N'Golo Kante",
    "Sergio Aguero", "Luis Suarez", "Zlatan Ibrahimovic", "David de Gea", "Alisson Becker",
    "Manuel Neuer", "Jan Oblak", "Marc-Andre ter Stegen", "Robert Lewandowski", "Romelu Lukaku",
    "Antoine Griezmann", "Philippe Coutinho", "Marco Reus", "Paulo Dybala", "Jadon Sancho",
    "Timo Werner", "Thomas Muller", "Joshua Kimmich", "Frenkie de Jong", "Matthijs de Ligt",
    "Gerard Pique", "Thiago Silva", "Edinson Cavani", "Angel Di Maria", "Hugo Lloris",
    "Gianluigi Buffon", "Keylor Navas", "Wojciech Szczesny", "Hakim Ziyech", "Bruno Fernandes",
    "Joao Felix"
                 };

            for (int i = 0; i < 20; i++)
            {
                var question = new Question
                {
                    QuestionText = "Who is Player?",
                    Answer = "Cristiano Ronaldo",
                    Difficulty = "easy",
                    CategoryId = 1,
                    ImageUrl = "cr7.jpg",
                    IsActive = true
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();
                for (int j = 0; j < 3; j++)
                {
                    var choice = new Choice
                    {
                        QuestionId = question.Id,
                        Text = players[random.Next(players.Count)]
                    };
                    _context.Choices.Add(choice);
                }
                var c = new Choice
                {
                    QuestionId = question.Id,
                    Text = question.Answer
                };
                _context.Choices.Add(c);
                await _context.SaveChangesAsync();


            }

            return Ok("Created");
        }

        //[HttpPost("verify-answer")]
        //public async Task<IActionResult> VerifyAnswer([FromBody] AnswerDTO answer)
        //{
        //    var question = await _context.Questions.Include(q => q.Choices).FirstOrDefaultAsync(q => q.Id == answer.QuestionId);

        //    if (question == null) return NotFound("Question not found.");

        //    var isCorrect = question.Answer == answer.SelectedChoice;

        //    return Ok(new { isCorrect });
        //}

    }
}
