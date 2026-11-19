using ATM.Application.Services.Interfaces;
using ATM.Domain.Models.DTOs;
using ATM.Domain.Models.Entities;
using ATM.Domain.Models.Enums;
using Spectre.Console;

namespace ATM.UI.Menus;

public class AdminMenu
{
  private readonly IAdminDashboardService _dashboardService;
  private readonly ILoanService _loanService;
  private readonly IUserService _userService;

  public AdminMenu(
    IUserService userService,
    ILoanService loanService,
    IAdminDashboardService dashboardService)
  {
    _userService = userService;
    _loanService = loanService;
    _dashboardService = dashboardService;
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
            "Dashboard",
            "View Users",
            "View Loan Requests",
            "Approve Loan",
            "Reject Loan",
            "Logout"
          )
      );

      switch (choice)
      {
        case "Dashboard":
          await ShowDashboardAsync();
          break;

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

      string filter = AnsiConsole.Prompt(
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
        "Pending" => loans.Where(loan => loan.Status == LoanStatus.Pending
        ).ToList(),

        "Approved" => loans.Where(loan => loan.Status == LoanStatus.Approved
        ).ToList(),

        "Rejected" => loans.Where(loan => loan.Status == LoanStatus.Rejected
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
      table.AddRow(
        loan.Id.ToString(),
        loan.UserId.ToString(),
        $"{loan.Amount:N2} GEL",
        loan.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
      );

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

    if (selected == "Back") return null;

    int selectedId = pendingLoans[
      choices.IndexOf(selected)
    ].Id;

    return selectedId;
  }


  private async Task ShowDashboardAsync()
  {
    try
    {
      AdminDashboardDto dashboard =
        await _dashboardService.GetDashboardAsync();

      AnsiConsole.Clear();

      AnsiConsole.Write(
        new FigletText("DASHBOARD")
          .Centered()
          .Color(Color.Cyan)
      );

      AnsiConsole.MarkupLine(
        "[bold grey]GENERAL STATISTICS[/]"
      );

      Grid generalGrid = new Grid();

      for (int i = 0; i < 5; i++)
        generalGrid.AddColumn();

      generalGrid.AddRow(
        new Markup(
          $"[grey]TOTAL CLIENTS[/]\n[bold cyan]{dashboard.TotalClients}[/]"
        ),
        new Markup(
          $"[grey]TRANSACTIONS[/]\n[bold cyan]{dashboard.TotalTransactions}[/]"
        ),
        new Markup(
          $"[grey]TOTAL DEPOSITS[/]\n[bold green]{dashboard.TotalDeposits:N2} GEL[/]"
        ),
        new Markup(
          $"[grey]TOTAL WITHDRAWALS[/]\n[bold red]{dashboard.TotalWithdrawals:N2} GEL[/]"
        ),
        new Markup(
          $"[grey]LOAN REQUESTS[/]\n[bold yellow]{dashboard.TotalLoanRequests}[/]"
        )
      );

      AnsiConsole.Write(generalGrid);

      AnsiConsole.WriteLine();

      AnsiConsole.MarkupLine(
        "[bold grey]LOAN STATISTICS[/]"
      );

      Grid loanGrid = new Grid();

      for (int i = 0; i < 4; i++)
        loanGrid.AddColumn();

      loanGrid.AddRow(
        new Markup(
          $"[grey]PENDING[/]\n[bold yellow]{dashboard.PendingLoans}[/]"
        ),
        new Markup(
          $"[grey]APPROVED[/]\n[bold green]{dashboard.ApprovedLoans}[/]"
        ),
        new Markup(
          $"[grey]REJECTED[/]\n[bold red]{dashboard.RejectedLoans}[/]"
        ),
        new Markup(
          $"[grey]APPROVAL RATE[/]\n[bold cyan]{dashboard.LoanApprovalRate:N1}%[/]"
        )
      );

      AnsiConsole.Write(loanGrid);

      ShowFinancialChart(dashboard.DailyFinancialStats);
      ShowTopLoanApplicants(dashboard.TopLoanApplicants);

      ShowRecentTransactions(dashboard.RecentTransactions);
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine(
        $"[red]{Markup.Escape(ex.Message)}[/]"
      );
    }
  }

  private void ShowFinancialChart(
    List<DailyFinancialStatsDto> dailyStats)
  {
    AnsiConsole.MarkupLine(
      "\n[bold grey]FINANCIAL ACTIVITY — LAST 10 DAYS[/]"
    );

    if (dailyStats.Count == 0)
    {
      AnsiConsole.MarkupLine("[yellow]No financial data available.[/]");
      return;
    }

    decimal maxAmount = dailyStats
      .SelectMany(x => new[] { x.TotalDeposits, x.TotalWithdrawals })
      .DefaultIfEmpty(0)
      .Max();

    if (maxAmount == 0)
    {
      AnsiConsole.MarkupLine("[grey]No deposits or withdrawals yet.[/]");
      return;
    }

    const int chartHeight = 8;

    for (int row = chartHeight; row >= 1; row--)
    {
      foreach (DailyFinancialStatsDto day in dailyStats)
      {
        int depositHeight = (int)Math.Ceiling(
          day.TotalDeposits / maxAmount * chartHeight
        );

        int withdrawalHeight = (int)Math.Ceiling(
          day.TotalWithdrawals / maxAmount * chartHeight
        );

        string depositBar = depositHeight >= row
          ? "[green]██[/]"
          : "  ";

        string withdrawalBar = withdrawalHeight >= row
          ? "[red]██[/]"
          : "  ";

        AnsiConsole.Markup(
          $" {depositBar}{withdrawalBar} "
        );
      }

      AnsiConsole.WriteLine();
    }

    foreach (DailyFinancialStatsDto day in dailyStats)
      AnsiConsole.Markup(
        $" [white]{day.Date:dd MMM}[/] "
      );

    AnsiConsole.WriteLine();

    AnsiConsole.MarkupLine(
      "[green]██[/] Deposits    [red]██[/] Withdrawals"
    );
  }

  private void ShowTopLoanApplicants(
    List<TopLoanApplicantDto> applicants)
  {
    AnsiConsole.WriteLine();

    AnsiConsole.MarkupLine(
      "[bold grey]TOP 5 LOAN APPLICANTS[/]"
    );

    if (applicants.Count == 0)
    {
      AnsiConsole.MarkupLine(
        "[yellow]No loan applicants found.[/]"
      );

      return;
    }

    Grid grid = new Grid();

    grid.AddColumn();
    grid.AddColumn();
    grid.AddColumn();

    grid.AddRow(
      new Markup("[grey]RANK[/]"),
      new Markup("[grey]USERNAME[/]"),
      new Markup("[grey]REQUESTS[/]")
    );

    for (int i = 0; i < applicants.Count; i++)
    {
      TopLoanApplicantDto applicant = applicants[i];

      string rankColor = i switch
      {
        0 => "yellow",
        1 => "silver",
        2 => "orange1",
        _ => "white"
      };

      grid.AddRow(
        new Markup($"[{rankColor}]#{i + 1}[/]"),
        new Markup(
          $"[bold]{Markup.Escape(applicant.Username)}[/]"
        ),
        new Markup(
          $"[cyan]{applicant.RequestCount}[/]"
        )
      );
    }

    AnsiConsole.Write(grid);
  }


  private void ShowRecentTransactions(
    List<TransactionDto> transactions)
  {
    AnsiConsole.WriteLine();

    AnsiConsole.MarkupLine(
      "[bold grey]RECENT TRANSACTIONS[/]"
    );

    if (transactions.Count == 0)
    {
      AnsiConsole.MarkupLine(
        "[yellow]No transactions found.[/]"
      );

      return;
    }

    Grid grid = new Grid();

    grid.AddColumn();
    grid.AddColumn();
    grid.AddColumn();
    grid.AddColumn();

    grid.AddRow(
      new Markup("[grey]ID[/]"),
      new Markup("[grey]TYPE[/]"),
      new Markup("[grey]AMOUNT[/]"),
      new Markup("[grey]DATE[/]")
    );

    foreach (TransactionDto transaction in transactions)
    {
      string typeColor = transaction.Type switch
      {
        TransactionType.Deposit => "green",
        TransactionType.Withdrawal => "red",
        TransactionType.Loan => "yellow",
        _ => "white"
      };

      grid.AddRow(
        new Markup($"[grey]#{transaction.Id}[/]"),
        new Markup($"[{typeColor}]{transaction.Type}[/]"),
        new Markup(
          $"[bold]{transaction.Amount:N2} GEL[/]"
        ),
        new Markup(
          $"[grey]{transaction.CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}[/]"
        )
      );
    }

    AnsiConsole.Write(grid);
  }
}
