namespace TFApp.Pages;

public class RegisterModel : PageModel
{
    private readonly ILogger<RegisterModel> _logger;
    private readonly TFAppContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RegisterModel(
        TFAppContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<RegisterModel> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    [BindProperty]
    public User User { get; set; }

    public static readonly string SessionKey = "UserId";

    public async Task<IActionResult> OnGetAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        var key = context?.Session.GetString(SessionKey);

        if (_context.User != null)
        {
            // 既存ユーザだったらDBから取ってきてその情報をフォームに埋めて表示
            User = await _context.User.FindAsync(key) ?? default!;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new User
        {
            UserId = Guid.NewGuid().ToString(),
            Name = User.Name,
            City = User.City,
        };

        if (user != null)
        {
            // DBに保存
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("UserIdをDBに保存しました");

            // セッションにUserIdをセット
            var context = _httpContextAccessor.HttpContext;
            context?.Session.SetString(SessionKey, user.UserId);

            _logger.LogInformation("UserIdをセッションに保存しました");

        }
        else
        {
            _logger.LogError("DBへの保存が失敗しました");
        }

        return RedirectToPage("./Index");
    }
}
