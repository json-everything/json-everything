using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TryJsonEverything.Models;

namespace TryJsonEverything.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index()
		{
			return View();
		}

		[Route("~/json-schema")]
		public IActionResult Schema()
		{
			return View();
		}

		[Route("~/json-path")]
		public IActionResult Path()
		{
			return View();
		}

		[Route("~/json-patch")]
		public IActionResult Patch()
		{
			return View();
		}

		[Route("~/json-logic")]
		public IActionResult Logic()
		{
			return View();
		}

		[Route("~/json-pointer")]
		public IActionResult Pointer()
		{
			return View();
		}

		[Route("~/privacy")]
		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
