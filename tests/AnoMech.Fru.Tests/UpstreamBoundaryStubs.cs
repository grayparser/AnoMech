namespace AnoMech.Core
{
    public static class DiagnosticLog
    {
        public static void Info(string message) { }
        // Production scheduler catches callback exceptions. Surface them as test failures.
        public static void Warn(string message) => throw new InvalidOperationException(message);
    }
}
