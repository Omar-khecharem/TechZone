namespace TechZone.ViewModels.Admin
{
    public class AdminOperationResult
    {
        public bool Succeeded { get; init; }

        public string? Error { get; init; }

        public static AdminOperationResult Success() => new() { Succeeded = true };

        public static AdminOperationResult Failure(string error) =>
            new() { Succeeded = false, Error = error };
    }
}
