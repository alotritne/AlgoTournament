namespace algotournament.Services.Abstractions
{
    public interface IDuelJudgeNotifier
    {
        Task NotifySubmissionJudgedAsync(int submissionId);
    }
}
