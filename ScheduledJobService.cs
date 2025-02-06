public class ScheduledJobService : BackgroundService
{
    private readonly ILogger<ScheduledJobService> _logger;
    private readonly HttpClient _httpClient;

    public ScheduledJobService(ILogger<ScheduledJobService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Executing scheduled request...");
                
                // send a POST request to the server
                var response = await _httpClient.PostAsync("http://<app-name>:<app-port>/execute", null, stoppingToken);
                
                _logger.LogInformation($"Response: {response.StatusCode}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ScheduledJobService: {ex.Message}");
            }

            // wait for 1 minute
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
