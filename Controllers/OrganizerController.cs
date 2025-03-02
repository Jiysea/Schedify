using Microsoft.AspNetCore.Mvc;

namespace Schedify.Controllers;

public class OrganizerController : Controller
{
    [Route("organizer/events")]
    [HttpGet]
    public ActionResult Events() => View();

    [Route("organizer/resources")]
    [HttpGet]
    public ActionResult Resources() => View();

    [Route("organizer/feedbacks")]
    [HttpGet]
    public ActionResult Feedbacks() => View();

    [Route("organizer/stats")]
    [HttpGet]
    public ActionResult Stats() => View();


    [Route("organizer/venues")]
    public IActionResult Venues()
    {
        if (Request.Headers["HX-Request"] == "true")
        {
            return PartialView("~/Views/Organizer/Partials/_VenuesPartial.cshtml", new { });
        }

        return RedirectToAction("Resources", "Organizer");
    }
    [Route("organizer/equipments")]
    public IActionResult Equipments()
    {
        if (Request.Headers["HX-Request"] == "true")
        {
            return PartialView("~/Views/Organizer/Partials/_EquipmentsPartial.cshtml", new { });
        }

        return RedirectToAction("Resources", "Organizer");

    }
    [Route("organizer/furnitures")]
    public IActionResult Furnitures()
    {

        if (Request.Headers["HX-Request"] == "true")
        {
            return PartialView("~/Views/Organizer/Partials/_FurnituresPartial.cshtml", new { });
        }

        return RedirectToAction("Resources", "Organizer");

    }
    [Route("organizer/personnels")]
    public IActionResult Personnels()
    {

        if (Request.Headers["HX-Request"] == "true")
        {
            return PartialView("~/Views/Organizer/Partials/_PersonnelsPartial.cshtml", new { });
        }

        return RedirectToAction("Resources", "Organizer");

    }
    [Route("organizer/caterings")]
    public IActionResult Caterings()
    {

        if (Request.Headers["HX-Request"] == "true")
        {
            return PartialView("~/Views/Organizer/Partials/_CateringsPartial.cshtml", new { });
        }

        return RedirectToAction("Resources", "Organizer");

    }

    [Route("organizer/by-events")]
    public IActionResult ByEvents()
    {

        if (Request.Headers["HX-Request"] == "true")
        {
            return PartialView("~/Views/Organizer/Partials/_CateringsPartial.cshtml", new { });
        }

        return RedirectToAction("Resources", "Organizer");

    }
}
