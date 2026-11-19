using ATM.Domain.Models.DTOs;

namespace ATM.Application.Services.Interfaces;

public interface IAdminDashboardService
{
  Task<AdminDashboardDto> GetDashboardAsync();
}
