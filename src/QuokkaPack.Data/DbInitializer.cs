using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.Data
{
    public static class DbInitializer
    {
        public static async Task SeedDefaultUserAsync(
            UserManager<IdentityUser> userManager,
            AppDbContext context)
        {
            // Check if the default user already exists
            const string defaultEmail = "demo@quokkapack.com";
            const string defaultPassword = "Demo123!";

            var existingUser = await userManager.FindByEmailAsync(defaultEmail);
            if (existingUser != null)
            {
                // User already exists, no need to seed
                return;
            }

            // Create the IdentityUser
            var identityUser = new IdentityUser
            {
                UserName = defaultEmail,
                Email = defaultEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(identityUser, defaultPassword);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create default user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Get the seeded MasterUser ID
            var masterUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            // Check if this MasterUser already has an IdentityUserId
            var masterUser = await context.MasterUsers
                .Include(mu => mu.Logins)
                .FirstOrDefaultAsync(mu => mu.Id == masterUserId);

            if (masterUser != null)
            {
                // Link the IdentityUser to the existing MasterUser
                masterUser.IdentityUserId = identityUser.Id;

                // Add login record
                masterUser.Logins.Add(new AppUserLogin
                {
                    Provider = "local",
                    ProviderUserId = identityUser.Id,
                    Email = identityUser.Email,
                    MasterUserId = masterUserId
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
