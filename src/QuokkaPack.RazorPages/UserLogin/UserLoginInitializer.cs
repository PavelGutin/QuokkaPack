using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace QuokkaPack.RazorPages.UserLogin
{
    public class UserLoginInitializer : IUserLoginInitializer
    {
        private readonly IDownstreamApi _api;

        public UserLoginInitializer(IDownstreamApi api)
        {
            _api = api;
        }

        public async Task InitializeAsync(ClaimsPrincipal user)
        {
            try
            {

                await _api.CallApiForUserAsync("DownstreamApi", options =>
                {
                    options.RelativePath = "api/users/initialize";
                    options.HttpMethod = "POST";
                }, 
                user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize user: {ex.Message}");
            }
        }
    }
}
