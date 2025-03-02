

using Microsoft.AspNetCore.Mvc;

namespace Schedify.Controllers;

public class AttendeeController : Controller
{
    [Route("attendee/events")]
    public ActionResult Events() => View();
}