using Newtonsoft.Json.Linq;

namespace IPA.Bcfier.App.Services
{
    public class ErrorLogsService
    {
        private static SemaphoreSlim _errorLogsSemaphore = new SemaphoreSlim(1);

        public async Task SaveErrorAsync(object errorObject)
        {
            await _errorLogsSemaphore.WaitAsync();
            try
            {
                var errorLogsPath = await GetErrorLogsPathAsync();
                var errorLogsRaw = File.ReadAllText(errorLogsPath);
                var errorLogs = JArray.Parse(errorLogsRaw);

                if (errorLogs.Count > 9)
                {
                    // We're always saving the last 10 errors
                    errorLogs.RemoveAt(0);
                }

                var serializedError = JObject.FromObject(errorObject);
                errorLogs.Add(serializedError);

                await File.WriteAllTextAsync(errorLogsPath, errorLogs.ToString());
            }
            finally
            {
                _errorLogsSemaphore.Release();
            }
        }

        public async Task<object> GetErrorLogAsync()
        {
            await _errorLogsSemaphore.WaitAsync();
            try
            {
                var errorLogsPath = await GetErrorLogsPathAsync();
                var errorLogsRaw = await File.ReadAllTextAsync(errorLogsPath);
                var errorLogs = JArray.Parse(errorLogsRaw);
                return errorLogs;
            }
            finally
            {
                _errorLogsSemaphore.Release();
            }
        }

        private static async Task<string> GetErrorLogsPathAsync()
        {
            var errorLogsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "IPA.BCFier",
                        "errors.json");

            if (!File.Exists(errorLogsPath))
            {
                using var initialFileStream = File.CreateText(errorLogsPath);
                await initialFileStream.WriteAsync("[]");
            }

            return errorLogsPath;
        }
    }
}
