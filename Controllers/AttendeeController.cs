

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Schedify.Controllers;

[Authorize(Roles = "Attendee")]
public class AttendeeController : Controller
{
    [Route("attendee/events")]
    public ActionResult Events() => View();
}