using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace QuokkaPack.Razor.Pages;

public class SetupModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public SetupModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public string Username { get; set; }

    [BindProperty]
    public string Password { get; set; }

    public string ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()  
    {
        var apiBase = _configuration["DownstreamApi:BaseUrl"] ?? "http://localhost:7100";
        var http = _httpClientFactory.CreateClient();

        var response = await http.PostAsJsonAsync($"{apiBase.TrimEnd('/')}/api/setup/init", new
        {
            Username,
            Password
        });

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("/Index");
        }

        var errors = await response.Content.ReadFromJsonAsync<List<string>>();
        ErrorMessage = errors != null ? string.Join("<br>", errors) : "An error occurred during setup.";
        return Page();
    }
}
