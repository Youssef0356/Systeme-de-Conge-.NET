using Microsoft.AspNetCore.Identity;
using testingdatabase.Models;

namespace testingdatabase.Services
{
    public class DatabaseInitializer
    {
        public static async Task SeedDataAsync(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            if (userManager == null || roleManager == null)
            {
                throw new ArgumentNullException("userManager or roleManager is null");
            }

            await CreateRoles(roleManager);

            await CreateAdminUser(userManager);
        }

        private static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "admin", "employee" };

            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task CreateAdminUser(UserManager<ApplicationUser> userManager)
        {
            var adminUser = await userManager.FindByEmailAsync("admin@perfaxis.com");

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "admin@perfaxis.com",
                    Email = "admin@perfaxis.com",

                };

                string defaultPassword = "@Perfaxis123*";
                var result = await userManager.CreateAsync(user, defaultPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "admin");
                    Console.WriteLine("Admin user created successfully! Please update the initial password!");
                    Console.WriteLine("Email:" + user.Email + "- Initial password :" + defaultPassword);
                }
                else
                {
                    Console.WriteLine("Unable to create Admin User:" + result.Errors.First().Description);
                }
            }
        }
    }
}
