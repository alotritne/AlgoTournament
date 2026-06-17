using algotournament.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Season> Seasons { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Contest> Contests { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<TestCase> TestCases { get; set; }
        public DbSet<ContestProblem> ContestProblems { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<SubmissionResult> SubmissionResults { get; set; }
        public DbSet<ContestParticipant> ContestParticipants { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<JudgeQueue> JudgeQueues { get; set; }
        public DbSet<Ranking> Rankings { get; set; }
        public DbSet<Discussion> Discussions { get; set; }
        public DbSet<DiscussionReply> DiscussionReplies { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<DuelRoom> DuelRooms { get; set; }
        public DbSet<DuelParticipant> DuelParticipants { get; set; }
        public DbSet<DuelMatch> DuelMatches { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Season configuration
            builder.Entity<Season>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // Tournament configuration
            builder.Entity<Tournament>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.AccessCode).HasMaxLength(50);
                entity.HasOne(e => e.Season)
                    .WithMany(s => s.Tournaments)
                    .HasForeignKey(e => e.SeasonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Contest configuration
            builder.Entity<Contest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasOne(e => e.Tournament)
                    .WithMany(t => t.Contests)
                    .HasForeignKey(e => e.TournamentId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.StartTime);
                entity.HasIndex(e => e.EndTime);
            });

            // Problem configuration
            builder.Entity<Problem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TitleVi).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.Property(e => e.StatementVi).IsRequired();
                entity.Property(e => e.TimeLimitMs).HasDefaultValue(1000);
                entity.Property(e => e.MemoryLimitMB).HasDefaultValue(256);
                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TestCase configuration
            builder.Entity<TestCase>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Input).IsRequired();
                entity.Property(e => e.ExpectedOutput).IsRequired();
                entity.Property(e => e.Points).HasDefaultValue(10);
                entity.HasOne(e => e.Problem)
                    .WithMany(p => p.TestCases)
                    .HasForeignKey(e => e.ProblemId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.ProblemId, e.Order });
            });

            // ContestProblem configuration (many-to-many)
            builder.Entity<ContestProblem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Contest)
                    .WithMany(c => c.ContestProblems)
                    .HasForeignKey(e => e.ContestId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Problem)
                    .WithMany(p => p.ContestProblems)
                    .HasForeignKey(e => e.ProblemId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.ContestId, e.ProblemId }).IsUnique();
                entity.HasIndex(e => new { e.ContestId, e.Order });
            });

            // Submission configuration
            builder.Entity<Submission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SourceCode).IsRequired();
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Submissions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Problem)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(e => e.ProblemId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Contest)
                    .WithMany(c => c.Submissions)
                    .HasForeignKey(e => e.ContestId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.DuelMatch)
                    .WithMany(m => m.Submissions)
                    .HasForeignKey(e => e.DuelMatchId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ProblemId);
                entity.HasIndex(e => e.SubmittedAt);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.DuelMatchId);
            });

            // SubmissionResult configuration
            builder.Entity<SubmissionResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Submission)
                    .WithMany(s => s.Results)
                    .HasForeignKey(e => e.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.TestCase)
                    .WithMany()
                    .HasForeignKey(e => e.TestCaseId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.SubmissionId, e.TestCaseId }).IsUnique();
            });

            // ContestParticipant configuration
            builder.Entity<ContestParticipant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Contest)
                    .WithMany(c => c.Participants)
                    .HasForeignKey(e => e.ContestId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.ContestParticipations)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.ContestId, e.UserId }).IsUnique();
                entity.HasIndex(e => new { e.ContestId, e.TotalScore, e.PenaltyTime });
            });

            // Announcement configuration
            builder.Entity<Announcement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Contest)
                    .WithMany()
                    .HasForeignKey(e => e.ContestId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.ExpiresAt);
            });

            // JudgeQueue configuration
            builder.Entity<JudgeQueue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Submission)
                    .WithOne()
                    .HasForeignKey<JudgeQueue>(e => e.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.QueuedAt);
            });

            // Ranking configuration
            builder.Entity<Ranking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Contest)
                    .WithMany()
                    .HasForeignKey(e => e.ContestId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.ContestId, e.UserId }).IsUnique();
                entity.HasIndex(e => new { e.ContestId, e.Rank });
                entity.HasIndex(e => new { e.ContestId, e.TotalScore, e.PenaltyTime });
            });

            // ApplicationUser configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.Handle).IsRequired().HasMaxLength(50).HasDefaultValue("");
                entity.HasIndex(e => e.Handle).IsUnique();
                entity.Property(e => e.Rating).HasDefaultValue(1200);
                entity.Property(e => e.IsBanned).HasDefaultValue(false);
            });

            // Discussion configuration
            builder.Entity<Discussion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Problem)
                    .WithMany()
                    .HasForeignKey(e => e.ProblemId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.AuthorId);
                entity.HasIndex(e => e.ProblemId);
                entity.HasIndex(e => e.UpdatedAt);
            });

            // DiscussionReply configuration
            builder.Entity<DiscussionReply>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.HasOne(e => e.Discussion)
                    .WithMany(d => d.Replies)
                    .HasForeignKey(e => e.DiscussionId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.DiscussionId);
                entity.HasIndex(e => e.AuthorId);
            });

            // BlogPost configuration
            builder.Entity<BlogPost>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.Property(e => e.Content).IsRequired();
                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.AuthorId);
                entity.HasIndex(e => e.PublishedAt);
            });

            // DuelRoom configuration
            builder.Entity<DuelRoom>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RoomCode).IsRequired().HasMaxLength(6);
                entity.HasIndex(e => e.RoomCode).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.HostUserId);
                entity.HasIndex(e => e.ExpiresAt);
                entity.HasOne(e => e.Problem)
                    .WithMany()
                    .HasForeignKey(e => e.ProblemId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.HostUser)
                    .WithMany()
                    .HasForeignKey(e => e.HostUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DuelParticipant configuration
            builder.Entity<DuelParticipant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Room)
                    .WithMany(r => r.Participants)
                    .HasForeignKey(e => e.DuelRoomId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.DuelRoomId, e.UserId }).IsUnique();
            });

            // DuelMatch configuration
            builder.Entity<DuelMatch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Room)
                    .WithOne(r => r.Match)
                    .HasForeignKey<DuelMatch>(e => e.DuelRoomId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.WinnerUser)
                    .WithMany()
                    .HasForeignKey(e => e.WinnerUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.StartedAt);
                entity.HasIndex(e => e.Player1UserId);
                entity.HasIndex(e => e.Player2UserId);
            });
        }
    }
}