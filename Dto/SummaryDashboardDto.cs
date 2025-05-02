using Schedify.Models;

namespace Schedify.Dto;

public class SummaryDashboardDto
{
    public int TotalEvents { get; set; }
    public int TotalUsers { get; set; }
    public int TotalBooked { get; set; }
    public decimal TotalRevenue { get; set; }
    
    public List<MonthlyEventCountDto> MonthlyEvents { get; set; } = new();
    public List<MonthlyUserCountDto> MonthlyUsers { get; set; } = new();
    public List<MonthlyBookingCountDto> MonthlyBookings { get; set; } = new();
    public List<MonthlyRevenueCountDto> MonthlyRevenue { get; set; } = new();
}

public class MonthlyEventCountDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MonthlyUserCountDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MonthlyBookingCountDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MonthlyRevenueCountDto
{
    public string Month { get; set; } = string.Empty;
    public decimal NetWorth { get; set; }
}