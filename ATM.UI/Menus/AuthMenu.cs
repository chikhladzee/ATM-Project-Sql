using ATM.Application.Services.Interfaces;
using ATM.Domain.Models.Entities;
using Spectre.Console;

namespace ATM.UI.Menus;

public class AuthMenu
{
  private readonly IUserService _userService;

  public AuthMenu(IUserService userService)
  {
    _userService = userService;
  }

  public async Task<User?> RunAsync()
  {
    while (true)
    {
      AnsiConsole.Clear();

      AnsiConsole.Write(
        new FigletText("ATM SYSTEM")
          .Centered()
          .Color(Color.Green)
      );

      string choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
          .Title("[yellow]Welcome to ATM System[/]")
          .AddChoices(
            "Login",
            "Register",
            "Exit"
          )
      );

      switch (choice)
      {
        case "Login":
        {
          User? user = await LoginAsync();

          if (user != null)
            return user;

          break;
        }

        case "Register":
          await RegisterAsync();
          break;

        case "Exit":
          return null;
      }

      AnsiConsole.WriteLine();
      AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
      Console.ReadKey(true);
    }
  }

  private async Task<User?> LoginAsync()
  {
    AnsiConsole.Clear();

    AnsiConsole.MarkupLine("[green]Login[/]");

    string username = AnsiConsole.Ask<string>("Username:");

    string password = AnsiConsole.Prompt(
      new TextPrompt<string>("Password:")
        .Secret()
    );

    User? user = await _userService.LoginAsync(username, password);

    if (user == null)
    {
      AnsiConsole.MarkupLine(
        "[red]Invalid username or password![/]"
      );

      return null;
    }

    AnsiConsole.MarkupLine(
      $"[green]Welcome, {user.FirstName}![/]"
    );

    return user;
  }

  private async Task RegisterAsync()
  {
    AnsiConsole.Clear();

    AnsiConsole.MarkupLine("[green]Registration[/]");

    string firstName = AnsiConsole.Ask<string>("First name:");
    string lastName = AnsiConsole.Ask<string>("Last name:");
    string username = AnsiConsole.Ask<string>("Username:");

    string password = AnsiConsole.Prompt(
      new TextPrompt<string>("Password:")
        .Secret()
    );

    string confirmPassword = AnsiConsole.Prompt(
      new TextPrompt<string>("Confirm password:")
        .Secret()
    );

    if (password != confirmPassword)
    {
      AnsiConsole.MarkupLine(
        "[red]Passwords do not match![/]"
      );

      return;
    }

    try
    {
      User user = await _userService.RegisterAsync(
        firstName,
        lastName,
        username,
        password
      );

      AnsiConsole.MarkupLine(
        $"[green]Registration successful! Welcome {user.FirstName}![/]"
      );
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
    }
  }
}
