namespace algotournament.Models.Dtos
{
    public class DuelParticipantDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Handle { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool IsReady { get; set; }
        public byte SlotIndex { get; set; }
        public bool IsConnected { get; set; }
    }

    public class DuelLobbyStateDto
    {
        public string RoomCode { get; set; } = string.Empty;
        public int ProblemId { get; set; }
        public string ProblemTitle { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public DuelRoomStatus Status { get; set; }
        public int? MatchId { get; set; }
        public List<DuelParticipantDto> Participants { get; set; } = new();
        public string? CurrentUserId { get; set; }
    }

    public class DuelMatchStateDto
    {
        public int MatchId { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public int ProblemId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndsAt { get; set; }
        public DuelMatchStatus Status { get; set; }
        public DuelParticipantDto Player1 { get; set; } = new();
        public DuelParticipantDto Player2 { get; set; } = new();
        public string CurrentUserId { get; set; } = string.Empty;
        public byte CurrentUserSlot { get; set; }
        public string? MyLastStatus { get; set; }
        public string? OpponentLastStatus { get; set; }
        public int MySubmissionCount { get; set; }
        public int OpponentSubmissionCount { get; set; }
        public int DurationMinutes { get; set; }
    }

    public class DuelPublicSubmissionStatus
    {
        public const string Submitting = "Submitting";
        public const string Compiling = "Compiling";
        public const string Running = "Running";
        public const string Accepted = "Accepted";
        public const string NotAccepted = "Not Accepted";
    }

    public class DuelHistoryItemDto
    {
        public int MatchId { get; set; }
        public DateTime Date { get; set; }
        public string OpponentHandle { get; set; } = string.Empty;
        public string ProblemTitle { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public int MyScore { get; set; }
        public int OpponentScore { get; set; }
    }

    public class DuelResultDto
    {
        public int MatchId { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string ProblemTitle { get; set; } = string.Empty;
        public bool IsDraw { get; set; }
        public string? WinnerUserId { get; set; }
        public string? WinnerHandle { get; set; }
        public string? LoserHandle { get; set; }
        public DuelPlayerResultDto Player1 { get; set; } = new();
        public DuelPlayerResultDto Player2 { get; set; } = new();
        public string CurrentUserId { get; set; } = string.Empty;
    }

    public class DuelPlayerResultDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Handle { get; set; } = string.Empty;
        public int? RuntimeMs { get; set; }
        public int? MemoryKb { get; set; }
        public int WrongAttempts { get; set; }
        public int SubmissionCount { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public bool IsWinner { get; set; }
        public bool SolvedFirst { get; set; }
    }
}
