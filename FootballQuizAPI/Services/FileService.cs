namespace FootballQuizAPI.Services
{
    public static class FileService
    {
        public static bool IsImage(this IFormFile? file)
        {
            if (file == null || file.Length == 0) return false;

            // Extension control
            string fileExtension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(fileExtension) ||
                !fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) &&
                !fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) &&
                !fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase) &&
                !fileExtension.Equals(".webp", StringComparison.OrdinalIgnoreCase) &&
                !fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (file.Length > 30 * 1024 * 1024) // 30 MB
            {
                return false;
            }
            // MIME control
            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
        //public static bool IsImage(this IFormFile file)
        //{
        //    return file.ContentType == "image/jpg" ||
        //        file.ContentType == "image/jpeg" ||
        //        file.ContentType == "image/png" ||
        //        file.ContentType == "image/webp" ||
        //        file.ContentType == "image/gif";
        //}

        public static bool IsCV(this IFormFile CVfile)
        {
            return CVfile.ContentType == "application/pdf" ||
             CVfile.ContentType == "application/doc" ||
             CVfile.ContentType == "application/docx" ||
             CVfile.ContentType == "application/pptx";

        }
        public static async Task<string> SaveAsync(this IFormFile file)
        {
            string externalPath = @"C:\Users\ilkin\OneDrive\Masaüstü\WorldFootballQuiz\FootballQuiz\public\images";

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            string resultPath = Path.Combine(externalPath, fileName);

            using (FileStream fileStream = new FileStream(resultPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return fileName;
        }

    }
}