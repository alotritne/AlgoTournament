using System.ComponentModel.DataAnnotations;

namespace algotournament.Models
{
    public class Problem
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Slug là bắt buộc")]
        public string Slug { get; set; } = string.Empty;
        
        // Vietnamese content (primary)
        [Required(ErrorMessage = "Tiêu đề tiếng Việt là bắt buộc")]
        public string TitleVi { get; set; } = string.Empty;
        [Required(ErrorMessage = "Nội dung đề bài tiếng Việt là bắt buộc")]
        public string StatementVi { get; set; } = string.Empty; // Markdown
        public string InputDescriptionVi { get; set; } = string.Empty;
        public string OutputDescriptionVi { get; set; } = string.Empty;
        public string ConstraintsVi { get; set; } = string.Empty;
        public string ExplanationVi { get; set; } = string.Empty;
        
        // English content (translated) - Optional
        public string? TitleEn { get; set; }
        public string? StatementEn { get; set; } // Markdown
        public string? InputDescriptionEn { get; set; }
        public string? OutputDescriptionEn { get; set; }
        public string? ConstraintsEn { get; set; }
        public string? ExplanationEn { get; set; }
        
        // Legacy fields for backward compatibility
        [Obsolete("Use TitleVi instead")]
        public string Title { get; set; } = string.Empty;
        [Obsolete("Use StatementVi instead")]
        public string Statement { get; set; } = string.Empty;
        [Obsolete("Use InputDescriptionVi instead")]
        public string InputDescription { get; set; } = string.Empty;
        [Obsolete("Use OutputDescriptionVi instead")]
        public string OutputDescription { get; set; } = string.Empty;
        [Obsolete("Use ConstraintsVi instead")]
        public string Constraints { get; set; } = string.Empty;
        
        // Sample tests (language-independent)
        public string SampleInput { get; set; } = string.Empty;
        public string SampleOutput { get; set; } = string.Empty;
        
        // Technical settings
        public int TimeLimitMs { get; set; } = 1000; // 1 second default
        public int MemoryLimitMB { get; set; } = 256; // 256MB default
        public int DifficultyRating { get; set; } = 1200;
        public bool IsPublic { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        
        // Translation status
        public bool IsEnglishTranslated { get; set; } = false;
        public DateTime? EnglishTranslatedAt { get; set; }
        
        // Helper properties for getting content based on language
        public string GetTitle(string language = "vi")
        {
            language = language?.ToLower() ?? "vi";
            
            if (language == "en")
            {
                // Return English title if available, otherwise Vietnamese
                return !string.IsNullOrEmpty(TitleEn) ? TitleEn : TitleVi;
            }
            
            return TitleVi;
        }
        
        public string GetStatement(string language = "vi")
        {
            return language.ToLower() == "en" && !string.IsNullOrEmpty(StatementEn) ? StatementEn : StatementVi;
        }
        
        public string GetInputDescription(string language = "vi")
        {
            return language.ToLower() == "en" && !string.IsNullOrEmpty(InputDescriptionEn) ? InputDescriptionEn : InputDescriptionVi;
        }
        
        public string GetOutputDescription(string language = "vi")
        {
            return language.ToLower() == "en" && !string.IsNullOrEmpty(OutputDescriptionEn) ? OutputDescriptionEn : OutputDescriptionVi;
        }
        
        public string GetConstraints(string language = "vi")
        {
            return language.ToLower() == "en" && !string.IsNullOrEmpty(ConstraintsEn) ? ConstraintsEn : ConstraintsVi;
        }
        
        public string GetExplanation(string language = "vi")
        {
            return language.ToLower() == "en" && !string.IsNullOrEmpty(ExplanationEn) ? ExplanationEn : ExplanationVi;
        }
        
        public bool HasEnglishTranslation()
        {
            return !string.IsNullOrEmpty(TitleEn) && !string.IsNullOrEmpty(StatementEn);
        }
        
        // Navigation properties
        public virtual ApplicationUser Creator { get; set; } = null!;
        public virtual ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
        public virtual ICollection<ContestProblem> ContestProblems { get; set; } = new List<ContestProblem>();
        // public virtual ICollection<DuelRoom> DuelRooms { get; set; } = new List<DuelRoom>(); // Commented out - causes foreign key conflict
    }
}