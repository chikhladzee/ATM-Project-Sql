using ATM.Application.Services.Interfaces;
using ATM.Domain.Models.DTOs;
using ATM.Domain.Models.Entities;
using ATM.Domain.Models.Enums;
using Spectre.Console;

namespace ATM.UI.Menus;

public class AdminMenu
{
  private readonly IUserService _userService;
  private readonly ILoanService _loanService;

  public AdminMenu(
    IUserService userService,
    ILoanService loanService)
  {
    _userService = userService;
    _loanService = loanService;
  }

  public async Task RunAsync(User admin)
  {
    while (true)
    {
      AnsiConsole.Clear();

      AnsiConsole.Write(
        new FigletText("ADMIN PANEL")
          .Centered()
          .Color(Color.Red)
      );

      AnsiConsole.MarkupLine(
        $"[grey]Welcome, {Markup.Escape(admin.FirstName)}![/]"
      );

      string choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
          .Title("[yellow]Select an operation:[/]")
          .AddChoices(
            "View Users",
            "View Loan Requests",
            "Approve Loan",
            "Reject Loan",
            "Logout"
          )
      );

      switch (choice)
      {
        case "View Users":
          await ShowUsersAsync(admin.Id);
          break;

        case "View Loan Requests":
          await ShowLoanRequestsAsync(admin.Id);
          break;

        case "Approve Loan":
          await ApproveLoanAsync(admin.Id);
          break;

        case "Reject Loan":
          await RejectLoanAsync(admin.Id);
          break;

        case "Logout":
          return;
      }

      AnsiConsole.WriteLine();
      AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
      Console.ReadKey(true);
    }
  }

  private async Task ShowUsersAsync(int adminId)
  {
    try
    {
      List<UserDto> users = await _userService.GetAllUsersAsync(adminId);

      if (users.Count == 0)
      {
        AnsiConsole.MarkupLine(
          "[yellow]No users found.[/]"
        );

        return;
      }

      Table table = new Table();

      table.Border(TableBorder.Rounded);

      table.AddColumn("ID");
      table.AddColumn("First Name");
      table.AddColumn("Last Name");
      table.AddColumn("Username");
      table.AddColumn("Role");
      table.AddColumn("Created At");

      foreach (UserDto user in users)
      {
        string roleColor = user.Role switch
        {
          UserRole.Admin => "red",
          UserRole.Client => "green",
          _ => "white"
        };

        table.AddRow(
          user.Id.ToString(),
          Markup.Escape(user.FirstName),
          Markup.Escape(user.LastName),
          Markup.Escape(user.Username),
          $"[{roleColor}]{user.Role}[/]",
          user.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
        );
      }

      AnsiConsole.Write(table);
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine(
        $"[red]{Markup.Escape(ex.Message)}[/]"
      );
    }
  }

  private async Task ShowLoanRequestsAsync(int adminId)
  {
    try
    {
      List<LoanRequestDto> loans = await _loanService.GetAllRequestsAsync(adminId);

      if (loans.Count == 0)
      {
        AnsiConsole.MarkupLine(
          "[yellow]No loan requests found.[/]"
          );

        return;
      }

      var filter = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
          .Title("[yellow]Select loan status filter:[/]")
          .AddChoices(
            "All",
            "Pending",
            "Approved",
            "Rejected",
            "Back"
            )
        );

      if (filter == "Back")
        return;

      List<LoanRequestDto> filteredLoans = filter switch
      {
        "Pending" => loans.Where(
          loan => loan.Status == LoanStatus.Pending
          ).ToList(),

        "Approved" => loans.Where(
          loan => loan.Status == LoanStatus.Approved
          ).ToList(),

        "Rejected" => loans.Where(
          loan => loan.Status == LoanStatus.Rejected
          ).ToList(),

        _ => loans
      };

      if (filteredLoans.Count == 0)
      {
        AnsiConsole.MarkupLine(
          "[yellow]No requests found with this status.[/]"
          );

        return;
      }

      Table table = new Table();

      table.Border(TableBorder.Rounded);

      table.AddColumn("ID");
      table.AddColumn("User ID");
      table.AddColumn("Amount");
      table.AddColumn("Status");
      table.AddColumn("Created At");

      foreach (LoanRequestDto loan in filteredLoans)
      {
        string statusColor = loan.Status switch
        {
          LoanStatus.Approved => "green",
          LoanStatus.Rejected => "red",
          LoanStatus.Pending => "yellow",
          _ => "white"
        };

        table.AddRow(
          loan.Id.ToString(),
          loan.UserId.ToString(),
          $"{loan.Amount:N2} GEL",
          $"[{statusColor}]{loan.Status}[/]",
          loan.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
          );
      }

      AnsiConsole.Write(table);
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine(
        $"[red]{Markup.Escape(ex.Message)}[/]");
    }
  }

  private async Task ApproveLoanAsync(int adminId)
  {
    try
    {
      int? loanRequestId = await SelectPendingLoanAsync(adminId);

      if (loanRequestId == null)
        return;

      bool confirm = AnsiConsole.Confirm(
        $"Approve loan request #{loanRequestId}?"
      );

      if (!confirm)
      {
        AnsiConsole.MarkupLine(
          "[yellow]Operation cancelled.[/]"
        );

        return;
      }

      await _loanService.ApproveLoanAsync(
        adminId,
        loanRequestId.Value
      );

      AnsiConsole.MarkupLine(
        "[green]Loan approved successfully![/]"
      );
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine(
        $"[red]{Markup.Escape(ex.Message)}[/]"
      );
    }
  }

  private async Task RejectLoanAsync(int adminId)
  {
    try
    {
      int? loanRequestId = await SelectPendingLoanAsync(adminId);

      if (loanRequestId == null)
        return;

      bool confirm = AnsiConsole.Confirm(
        $"Reject loan request #{loanRequestId}?"
      );

      if (!confirm)
      {
        AnsiConsole.MarkupLine(
          "[yellow]Operation cancelled.[/]"
        );

        return;
      }

      await _loanService.RejectLoanAsync(
        adminId,
        loanRequestId.Value
      );

      AnsiConsole.MarkupLine(
        "[red]Loan rejected successfully![/]"
      );
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine(
        $"[red]{Markup.Escape(ex.Message)}[/]"
      );
    }
  }

  private async Task<int?> SelectPendingLoanAsync(int adminId)
  {
    List<LoanRequestDto> loans = await _loanService.GetAllRequestsAsync(adminId);

    List<LoanRequestDto> pendingLoans = loans
      .Where(loan => loan.Status == LoanStatus.Pending)
      .ToList();

    if (pendingLoans.Count == 0)
    {
      AnsiConsole.MarkupLine(
        "[yellow]There are no pending loan requests.[/]"
      );

      return null;
    }

    Table table = new Table();

    table.Border(TableBorder.Rounded);

    table.AddColumn("ID");
    table.AddColumn("User ID");
    table.AddColumn("Amount");
    table.AddColumn("Created At");

    foreach (LoanRequestDto loan in pendingLoans)
    {
      table.AddRow(
        loan.Id.ToString(),
        loan.UserId.ToString(),
        $"{loan.Amount:N2} GEL",
        loan.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
      );
    }

    AnsiConsole.Write(table);

    List<string> choices = pendingLoans
      .Select(loan => $"Request #{loan.Id} - {loan.Amount:N2} GEL")
      .ToList();

    choices.Add("Back");

    string selected = AnsiConsole.Prompt(
      new SelectionPrompt<string>()
        .Title("[yellow]Select a loan request:[/]")
        .PageSize(10)
        .AddChoices(choices)
    );

    if (selected == "Back")
    {
      return null;
    }

    int selectedId = pendingLoans[
      choices.IndexOf(selected)
    ].Id;

    return selectedId;
  }
}
