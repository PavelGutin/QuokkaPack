using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuokkaPack.Razor.Pages.Account;

public class LogoutModel : PageModel
{
    public IActionResult OnPost()
    {
        HttpContext.Session.Remove("JWT");
         HttpContext.Session.Clear();
        return RedirectToPage("/Index");
    }
}
