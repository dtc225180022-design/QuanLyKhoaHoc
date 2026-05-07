using Microsoft.AspNetCore.Mvc;

namespace QuanLyKhoaHoc.Controllers;

public class ErrorController : Controller
{
    [Route("Error/{statusCode}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        ViewBag.StatusCode = statusCode;
        return statusCode switch
        {
            404 => View("NotFound"),
            403 => View("Forbidden"),
            _ => View("Error")
        };
    }
}
