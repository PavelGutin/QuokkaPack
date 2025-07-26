using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace QuokkaPack.Razor.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LoginModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        ErrorMessage = null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var client = _httpClientFactory.CreateClient("QuokkaApi");

        var response = await client.PostAsJsonAsync("/api/auth/login", Input);

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = "Invalid email or password.";
            return Page();
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (result?.Token is null)
        {
            ErrorMessage = "Login failed.";
            return Page();
        }

        HttpContext.Session.SetString("JWT", result.Token);
        return RedirectToPage("/Index");
    }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
