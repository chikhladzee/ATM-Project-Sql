using ATM.Application.Services.Interfaces;
using ATM.Domain.Models.DTOs;
using ATM.Domain.Models.Entities;
using ATM.Domain.Models.Enums;
using ATM.UI.Helpers;
using Spectre.Console;

namespace ATM.UI.Menus;

public class ClientMenu
{
  private readonly IAccountService _accountService;
  private readonly ITransactionService _transactionService;
  private readonly ILoanService _loanService;
  private readonly IClientDashboardService _dashboardService;

  public ClientMenu(
    IAccountService accountService,
    ITransactionService transactionService,
    ILoanService loanService,
    IClientDashboardService dashboardService)
  {
    _accountService = accountService;
    _transactionService = transactionService;
    _loanService = loanService;
    _dashboardService = dashboardService;
  }

  public async Task RunAsync(User user)
  {
    while (true)
    {
      AnsiConsole.Clear();

      AnsiConsole.Write(
        new FigletText("CLIENT PANEL")
          .Centered()
          .Color(Color.Green)
      );

      AnsiConsole.MarkupLine(
        $"[grey]Welcome, {Markup.Escape(user.FirstName)}![/]"
      );

      string choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
          .Title("[yellow]Select an operation:[/]")
          .AddChoices(
            "Dashboard",
            "Check Balance",
            "Deposit",
            "Withdraw",
            "Transaction History",
            "Request Loan",
            "My Loan Requests",
            "Logout"
          )
      );

      switch (choice)
      {
        case "Dashboard":
          await ShowDashboardAsync(user.Id);
          break;

        case "Check Balance":
          await CheckBalanceAsync(user.Id);
          break;

        case "Deposit":
          await DepositAsync(user.Id);
          break;

        case "Withdraw":
          await WithdrawAsync(user.Id);
          break;

        case "Transaction History":
          await ShowTransactionHistoryAsync(user.Id);
          break;

        case "Request Loan":
          await RequestLoanAsync(user.Id);
          break;

        case "My Loan Requests":
          await ShowMyLoansAsync(user.Id);
          break;

        case "Logout":
          return;
      }

      AnsiConsole.WriteLine();
      AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
      Console.ReadKey(true);
    }
  }

  private async Task CheckBalanceAsync(int userId)
  {
    try
    {
      decimal balance = await _accountService.GetBalanceAsync(userId);

      Panel panel = new Panel(
        new Markup($"[bold green]{balance:N2} GEL[/]")
      )
      {
        Header = new PanelHeader("Your Balance"),
        Border = BoxBorder.Rounded,
        Padding = new Padding(2, 1)
      };

      AnsiConsole.Write(panel);
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine(
        $"[red]{Markup.Escape(ex.Message)}[/]"
      );
    }
  }

  private async Task DepositAsync(int userId)
  {
    try
    {
      decimal amount = InputHelper.AskPositiveAmount("Enter deposit amount:");

      await _accountService.DepositAsync(userId, amount);

      AnsiConsole.MarkupLine(
        $"[green]Successfully deposited {amount:N2} GEL![/]"
      );
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine(
        $"[red]{Markup.Escape(ex.Message)}[/]"
      );
    }
  }

  private async Task WithdrawAsync(int userId)
  {
    try
    {
      decimal amount = InputHelper.AskPositiveAmount("Enter withdraw amount:");

      await _accountService.WithdrawAsync(userId, amount);

      AnsiConsole.MarkupLine(
        $"[green]Successfully withdrawn {amount:N2} GEL![/]"
      );
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine(
        $"[red]{Markup.Escape(ex.Message)}[/]"
      );
    }
  }

  private async Task ShowTransactionHistoryAsync(int userId)
  {
    try
    {
      List<TransactionDto> transactions = await _transactionService.GetHistoryAsync(userId);

      if (transactions.Count == 0)
      {
        AnsiConsole.MarkupLine(
          "[yellow]You don't have any transactions yet.[/]"
        );

        return;
      }

      Table table = new Table();

      table.Border(TableBorder.Rounded);

      table.AddColumn("ID");
      table.AddColumn("Type");
      table.AddColumn("Amount");
      table.AddColumn("Date");

      foreach (TransactionDto transaction in transactions)
      {
        string type = transaction.Type.ToString();

        string color = transaction.Type switch
        {
          TransactionType.Deposit => "green",
          TransactionType.Withdrawal => "red",
          TransactionType.Loan => "yellow",
          _ => "white"
        };

        table.AddRow(
          transaction.Id.ToString(),
          $"[{color}]{type}[/]",
          $"{transaction.Amount:N2} GEL",
          transaction.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
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


  private async Task RequestLoanAsync(int userId)
  {
    try
    {
      decimal amount = InputHelper.AskPositiveAmount("Enter the loan amount:");

      bool confirm = AnsiConsole.Confirm(
        $"Are you sure you want to request a loan of {amount:N2} GEL?"
      );

      if (!confirm)
      {
        AnsiConsole.MarkupLine("[yellow]Loan request cancelled.[/]");
        return;
      }

      await _loanService.RequestLoanAsync(userId, amount);

      AnsiConsole.MarkupLine(
        "[green]Your loan request has been submitted successfully![/]"
      );

      AnsiConsole.MarkupLine(
        "[grey]Status: Pending approval[/]"
      );
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine(
        $"[red]{Markup.Escape(ex.Message)}[/]"
      );
    }
  }

  private async Task ShowMyLoansAsync(int userId)
  {
    try
    {
      List<LoanRequestDto> loans = await _loanService.GetMyRequestsAsync(userId);

      if (loans.Count == 0)
      {
        AnsiConsole.MarkupLine(
          "[yellow]You haven't requested any loans yet.[/]"
        );

        return;
      }

      Table table = new Table();

      table.Border(TableBorder.Rounded);

      table.AddColumn("ID");
      table.AddColumn("Amount");
      table.AddColumn("Status");
      table.AddColumn("Created At");

      foreach (LoanRequestDto loan in loans)
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
        $"[red]{Markup.Escape(ex.Message)}[/]"
      );
    }
  }


  private async Task ShowDashboardAsync(int userId)
  {
    try
    {
      ClientDashboardDto dashboard = await _dashboardService.GetDashboardAsync(userId);

      AnsiConsole.Clear();

      AnsiConsole.Write(
        new FigletText("MY DASHBOARD")
              .Centered()
              .Color(Color.Green)
        );

      Panel balancePanel = new Panel(
        new Markup(
          $"[bold green]{dashboard.Balance:N2} GEL[/]"
          ))
      {
        Header = new PanelHeader("Current Balance"),
        Border = BoxBorder.Rounded,
        Padding = new Padding(2, 1)
      };

      AnsiConsole.Write(balancePanel);

      Table table = new Table();
      table.Border(TableBorder.Rounded);

      table.AddColumn("Statistic");
      table.AddColumn("Amount");

      table.AddRow(
        "Total Deposits",
        $"[green]+{dashboard.TotalDeposits:N2} GEL[/]"
        );

      table.AddRow(
        "Total Withdrawals",
        $"[red]-{dashboard.TotalWithdrawals:N2} GEL[/]"
        );

      table.AddRow(
        "Transaction Count",
        dashboard.TransactionCount.ToString()
        );

      AnsiConsole.Write(table);

      AnsiConsole.MarkupLine("\n[bold yellow]Recent Transactions[/]");

      if (dashboard.RecentTransactions.Count == 0)
      {
        AnsiConsole.MarkupLine(
          "[grey]No transactions yet.[/]"
          );

        return;
      }

      Table transactionsTable = new Table();
      transactionsTable.Border(TableBorder.Rounded);

      transactionsTable.AddColumn("Type");
      transactionsTable.AddColumn("Amount");
      transactionsTable.AddColumn("Date");

      foreach (var transaction in dashboard.RecentTransactions)
      {
        string color = transaction.Type switch
        {
          TransactionType.Deposit => "green",
          TransactionType.Withdrawal => "red",
          TransactionType.Loan => "yellow",
          _ => "white"
        };

        transactionsTable.AddRow(
          $"[{color}]{transaction.Type}[/]",
          $"{transaction.Amount:N2} GEL",
          transaction.CreatedAt.ToLocalTime()
            .ToString("dd.MM.yyyy HH:mm")
          );
      }

      AnsiConsole.Write(transactionsTable);
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine(
        $"[red]{Markup.Escape(ex.Message)}[/]");
    }
  }
}
