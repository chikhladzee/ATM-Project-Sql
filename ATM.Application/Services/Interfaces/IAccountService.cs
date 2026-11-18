namespace ATM.Application.Services.Interfaces;

public interface IAccountService
{
  Task<decimal> GetBalanceAsync(int userId);
  Task DepositAsync(int userId, decimal amount);
  Task WithdrawAsync(int userId, decimal amount);
}
