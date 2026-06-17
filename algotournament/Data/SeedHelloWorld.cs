using algotournament.Data;
using algotournament.Models;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Data
{
    public static class SeedHelloWorld
    {
        public static async Task SeedHelloWorldProblemAsync(ApplicationDbContext context)
        {
            // Check if Hello World problem already exists
            var existingProblem = await context.Problems
                .FirstOrDefaultAsync(p => p.Slug == "hello-world");

            if (existingProblem != null)
            {
                return; // Problem already exists
            }

            // Get admin user
            var adminUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "admin@algotournament.com");

            if (adminUser == null)
            {
                return; // No admin user found
            }

            // Create Hello World problem
            var helloWorldProblem = new Problem
            {
                TitleVi = "Xin Chào Thế Giới",
                TitleEn = "Hello World",
                Slug = "hello-world",
                StatementVi = "# Xin Chào Thế Giới\n\nViết một chương trình in ra \"Hello, World!\" ra màn hình.\n\nĐây là bài toán đơn giản nhất để bắt đầu với lập trình thi đấu.",
                StatementEn = "# Hello World\n\nWrite a program that prints \"Hello, World!\" to the standard output.\n\nThis is the simplest problem to get started with competitive programming.",
                InputDescriptionVi = "Không cần nhập dữ liệu.",
                InputDescriptionEn = "No input is required.",
                OutputDescriptionVi = "In ra \"Hello, World!\" (không có ngoặc kép) theo sau là xuống dòng.",
                OutputDescriptionEn = "Print \"Hello, World!\" (without quotes) followed by a newline.",
                ConstraintsVi = "Không có giới hạn",
                ConstraintsEn = "None",
                ExplanationVi = "Đây là bài toán cơ bản nhất để làm quen với lập trình.",
                ExplanationEn = "This is the basic problem to get started with programming.",
                SampleInput = "",
                SampleOutput = "Hello, World!",
                TimeLimitMs = 1000,
                MemoryLimitMB = 256,
                DifficultyRating = 800,
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = adminUser.Id,
                IsEnglishTranslated = true,
                EnglishTranslatedAt = DateTime.UtcNow
            };

            context.Problems.Add(helloWorldProblem);
            await context.SaveChangesAsync();

            // Create test cases
            var testCases = new List<TestCase>
            {
                new TestCase
                {
                    ProblemId = helloWorldProblem.Id,
                    Input = "",
                    ExpectedOutput = "Hello, World!\n",
                    Points = 10,
                    IsSample = true,
                    Order = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new TestCase
                {
                    ProblemId = helloWorldProblem.Id,
                    Input = "",
                    ExpectedOutput = "Hello, World!\n",
                    Points = 10,
                    IsSample = false,
                    Order = 2,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.TestCases.AddRange(testCases);
            await context.SaveChangesAsync();
        }
    }
}
