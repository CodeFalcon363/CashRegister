using CashRegister.Application.Interfaces;
using CashRegister.Domain.Entities;
using CashRegister.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CashRegister.Infrastructure.Data;


// Seeds the database with default branch and users on first run.
// Default users: BRN001Inputter, BRN001Authorizer, Management (Password123!), Admin (Admin123!)
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

        await context.Database.MigrateAsync();

        if (await context.Branches.AnyAsync())
        {
            return;
        }

        var branch = new Branch("001", "Main Branch");
        context.Branches.Add(branch);
        await context.SaveChangesAsync();

        var users = new List<User>
        {
            new User("BRN001Inputter", "BRN001Inputter@bank.com", passwordService.HashPassword("Password123!"), UserRole.Inputer, branch.Id),
            new User("BRN001Authorizer", "BRN001Authorizer@bank.com", passwordService.HashPassword("Password123!"), UserRole.Authorizer, branch.Id),
            new User("Management", "Management@bank.com", passwordService.HashPassword("Password123!"), UserRole.Viewer, null),
            new User("Admin", "Admin@bank.com", passwordService.HashPassword("Admin123!"), UserRole.Admin, null)
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }
}
