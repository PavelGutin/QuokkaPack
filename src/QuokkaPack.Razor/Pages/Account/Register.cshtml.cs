using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using static QuokkaPack.Razor.Pages.Account.LoginModel;

namespace QuokkaPack.Razor.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegisterModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public RegisterInputModel Input { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public void OnGet() { }

        //TODO: add more robust error handling
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _httpClientFactory.CreateClient("QuokkaApi");

            var response = await client.PostAsJsonAsync("/api/auth/register", Input);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    if (errorResponse?.Errors != null && errorResponse.Errors.Any())
                    {
                        ErrorMessage = string.Join(", ", errorResponse.Errors.Select(e => e.Description));
                    }
                    else
                    {
                        ErrorMessage = $"Registration failed: {errorContent}";
                    }
                }
                catch
                {
                    ErrorMessage = $"Registration failed: {errorContent}";
                }
                return Page();
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result?.Token is null)
            {
                ErrorMessage = "Registration failed.";
                return Page();
            }
            HttpContext.Session.SetString("JWT", result.Token);

            return RedirectToPage("/Index");
        }

        public IActionResult OnPostExternalLogin(string provider)
        {
            // Redirect to your external login handler or API
            return Redirect($"/auth/external-login?provider={provider}");
        }

        public class RegisterInputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public class ErrorResponse
        {
            public IEnumerable<ErrorDetail> Errors { get; set; } = [];
        }

        public class ErrorDetail
        {
            public string Code { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }
}
