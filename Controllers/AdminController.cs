



using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Schedify.ViewModels;
using Schedify.Data;
using Schedify.Services;
using System.Threading.Tasks;

namespace Schedify.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{

    private readonly ApplicationDbContext _context;
    private readonly AdminService _adminService;
    private readonly UserService _userService;
    private readonly ResourceService _resourceService;
    private readonly IWebHostEnvironment _environment;

    public AdminController(ApplicationDbContext context, AdminService adminService, UserService userService, ResourceService resourceService, IWebHostEnvironment environment)
    {
        _context = context;
        _adminService = adminService;
        _userService = userService;
        _resourceService = resourceService;
        _environment = environment;
    }

    [Route("admin/dashboard")]
    public async Task<ActionResult> Dashboard()
    {
        var AvatarFileName = await _userService.GetUserAvatarFileNameAsync();
        var summary = await _adminService.GetDashboardSummaryAsync(null, null);
        var events = await _adminService.GetDashboardEventsAsync(null, null);
        
        var firstEvent = events?.GetEvents?.FirstOrDefault();
        Guid? selectedId = firstEvent?.Id ?? Guid.Empty;
        var perEvents = await _adminService.GetEventDashboardByIdAsync(selectedId);

        var viewModel = new DashboardViewModel
        {
            AvatarFileName = AvatarFileName,
            Summary = summary,
            Events = events,
            SelectedEventDetails = perEvents,
        };
        return View(viewModel);
    }

    [HttpGet("admin/summary-filter-date")]
    public async Task<IActionResult> SummaryFilterDate(DateTime? summaryStartDate, DateTime? summaryEndDate)
    {
        var summary = await _adminService.GetDashboardSummaryAsync(summaryStartDate, summaryEndDate);
        var events = await _adminService.GetDashboardEventsAsync(summaryStartDate, summaryEndDate);

        var firstEvent = events?.GetEvents?.FirstOrDefault();
        Guid? selectedId = firstEvent?.Id ?? Guid.Empty;
        var perEvents = await _adminService.GetEventDashboardByIdAsync(selectedId);

        var viewModel = new DashboardViewModel
        {
            Summary = summary,
            Events = events!,
            SelectedEventDetails = perEvents,
        };
        return PartialView("~/Views/Admin/Partials/_UpdateSummaryPartial.cshtml", viewModel);
    }

    [HttpGet("admin/select-event-dropdown/{EventId}")]
    public async Task<IActionResult> SummaryFilterDate(Guid? EventId)
    {
        var perEvents = await _adminService.GetEventDashboardByIdAsync(EventId);

        var viewModel = new DashboardViewModel
        {
            SelectedEventDetails = perEvents,
        };
        return PartialView("~/Views/Admin/Partials/_UpdateEventFromDropdownPartial.cshtml", viewModel);
    }

    // [HttpGet("admin/event-filter-date")]
    // public async Task<IActionResult> EventFilterDate(DateTime? eventsStartDate, DateTime? eventsEndDate)
    // {
    //     var events = await _adminService.GetDashboardEventsAsync(eventsStartDate, eventsEndDate);
    //     var firstEvent = events?.GetEvents?.FirstOrDefault();
    //     Guid? selectedId = firstEvent?.Id ?? Guid.Empty;
    //     var perEvents = await _adminService.GetEventDashboardByIdAsync(selectedId);

    //     var viewModel = new DashboardViewModel { Events = events, SelectedEventDetails = perEvents };
    //     return PartialView("~/Views/Admin/Partials/_UpdateEventsPartial.cshtml", viewModel);
    // }

    // [Route("admin/activity-logs")]
    // public ActionResult ActivityLogs() => View();

    // [Route("admin/profile")]
    // public ActionResult Profile() => View();
}