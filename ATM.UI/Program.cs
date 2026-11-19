using ATM.Application.Services;
using ATM.Application.Services.Interfaces;
using ATM.Domain.Interfaces;
using ATM.Domain.Models.Entities;
using ATM.Domain.Models.Enums;
using ATM.Infrastructure.Data;
using ATM.Infrastructure.Repositories;
using ATM.UI.Menus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

ServiceCollection services = new ServiceCollection();

services.AddDbContext<ATMDbContext>(options =>
{
  options.UseSqlServer(
    @"Server=localhost,1433;
     Database=ATMDb;
     User Id=sa;
     Password=AtmProject2026@;
     TrustServerCertificate=True;"
  );
});

// Repositories
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IAccountRepository, AccountRepository>();
services.AddScoped<ITransactionRepository, TransactionRepository>();
services.AddScoped<ILoanRequestRepository, LoanRequestRepository>();

// Services
services.AddScoped<IUserService, UserService>();
services.AddScoped<IAccountService, AccountService>();
services.AddScoped<ITransactionService, TransactionService>();
services.AddScoped<ILoanService, LoanService>();
services.AddScoped<IClientDashboardService, ClientDashboardService>();
services.AddScoped<IAdminDashboardService, AdminDashboardService>();

// MENUS
services.AddScoped<AuthMenu>();
services.AddScoped<ClientMenu>();
services.AddScoped<AdminMenu>();

ServiceProvider serviceProvider = services.BuildServiceProvider();

while (true)
  using (IServiceScope scope = serviceProvider.CreateScope())
  {
    IUserService userService = scope.ServiceProvider
      .GetRequiredService<IUserService>();

    await userService.EnsureAdminExistsAsync(
      "admin",
      "Admin123!"
    );

    AuthMenu authMenu = scope.ServiceProvider
      .GetRequiredService<AuthMenu>();

    User? user = await authMenu.RunAsync();

    if (user == null)
      break;

    switch (user.Role)
    {
      case UserRole.Client:
      {
        ClientMenu clientMenu = scope.ServiceProvider
          .GetRequiredService<ClientMenu>();

        await clientMenu.RunAsync(user);
        break;
      }

      case UserRole.Admin:
      {
        AdminMenu adminMenu = scope.ServiceProvider
          .GetRequiredService<AdminMenu>();

        await adminMenu.RunAsync(user);
        break;
      }
    }
  }

AnsiConsole.Clear();
AnsiConsole.MarkupLine("[green]Goodbye![/]");
