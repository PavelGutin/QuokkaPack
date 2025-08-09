namespace QuokkaPack.Blazor.Auth
{
    public interface ITokenStore
    {
        ValueTask SetAsync(string token);
        Task<string?> GetAsync();
        ValueTask ClearAsync();
    }
}
