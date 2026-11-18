using ATM.Domain.Models.DTOs;

namespace ATM.Application.Services.Interfaces;

public interface IClientDashboardService
{
  Task<ClientDashboardDto> GetDashboardAsync(int userId);
}
