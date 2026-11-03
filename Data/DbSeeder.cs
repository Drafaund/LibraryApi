using LibraryApi.Common;
using LibraryApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public static class DbSeeder
{
    public const string AdminEmail = "admin@gmail.com";

    // A null adminPassword means "do not seed the admin user" (every environment except Development).
    public static async Task SeedAsync(
        AppDbContext db,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        string? adminPassword)
    {
        await SeedDomainAsync(db);
        await SeedRolesAsync(roleManager);

        if (!string.IsNullOrEmpty(adminPassword))
            await SeedAdminAsync(userManager, adminPassword);
    }

    private static async Task SeedDomainAsync(AppDbContext db)
    {
        var hasData = await db.Categories.AnyAsync()
            || await db.Authors.AnyAsync()
            || await db.Books.AnyAsync();
        if (hasData)
            return;

        var programming = new Category { Name = "Pemrograman" };
        var fiction = new Category { Name = "Fiksi" };

        var hunt = new Author { Name = "Andrew Hunt" };
        var thomas = new Author { Name = "David Thomas" };
        var hirata = new Author { Name = "Andrea Hirata" };

        db.Books.AddRange(
            new Book
            {
                Title = "The Pragmatic Programmer",
                Isbn = "9780135957059",
                Stock = 5,
                PublishedYear = 2019,
                Category = programming,
                BookAuthors =
                {
                    new BookAuthor { Author = hunt, IsPrimary = true },
                    new BookAuthor { Author = thomas, IsPrimary = false }
                }
            },
            new Book
            {
                Title = "Laskar Pelangi",
                Isbn = "9789793062792",
                Stock = 3,
                PublishedYear = 2005,
                Category = fiction,
                BookAuthors =
                {
                    new BookAuthor { Author = hirata, IsPrimary = true }
                }
            });

        await db.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in new[] { RoleNames.Admin, RoleNames.User })
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var created = await roleManager.CreateAsync(new IdentityRole(roleName));
            EnsureSucceeded(created, $"create role '{roleName}'");
        }
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, string adminPassword)
    {
        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser { UserName = AdminEmail, Email = AdminEmail };
            EnsureSucceeded(await userManager.CreateAsync(admin, adminPassword), "create the admin user");
        }

        if (!await userManager.IsInRoleAsync(admin, RoleNames.Admin))
            EnsureSucceeded(await userManager.AddToRoleAsync(admin, RoleNames.Admin), "assign the Admin role");
    }

    // Only the error codes are reported: descriptions of password errors are fine, the password itself never is.
    private static void EnsureSucceeded(IdentityResult result, string action)
    {
        if (result.Succeeded)
            return;

        var codes = string.Join(", ", result.Errors.Select(error => error.Code));
        throw new InvalidOperationException($"Seeding failed to {action}: {codes}.");
    }
}
