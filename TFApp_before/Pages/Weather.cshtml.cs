namespace TFApp.Pages;

public class WeatherModel : PageModel
{
    private readonly TFAppContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterModel> _logger;

    public WeatherModel(
        TFAppContext context,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<RegisterModel> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId { get; private set; }

    public Weather? Weather { get; set; }

    public async Task OnGetAsync()
    {
        // セッションからUserIdを取り出す
        var context = _httpContextAccessor.HttpContext;
        var key = context?.Session.GetString(RegisterModel.SessionKey);
        UserId = key;

        if (_context.User != null)
        {
            // セッションと同じユーザーをDBから取得
            var user = await _context.User.FindAsync(key);

            // weather-apiをたたく
            if (user != null)
            {
                var client = _httpClientFactory.CreateClient("weather");
                client.DefaultRequestHeaders.Add("x-api-key", _configuration.GetValue<string>("ApiKey"));
                var response = await client.GetAsync($"api/weather/{user.City}");

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    Weather = JsonSerializer.Deserialize<Weather>(responseBody);

                    _logger.LogInformation("weather-apiのコールに成功しました");
                }
            }
            else
            {
                Weather = null;

                _logger.LogError("ユーザーの登録処理が失敗しました");
            }
        }
    }
}