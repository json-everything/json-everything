using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Json.Logic;
using Json.Patch;
using Json.Path;
using Json.Pointer;
using Json.Schema;
using Json.Schema.Data;
using Json.Schema.DataGeneration;
using Json.Schema.UniqueKeys;
using TryJsonEverything.Models;

namespace TryJsonEverything.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		[Route("~/json-schema")]
		public IActionResult Schema()
		{
			return View(new LibraryVersionCollectionModel
			{
				GetLibraryVersion<JsonSchema>(),
				GetLibraryVersion<DataKeyword>(),
				GetLibraryVersion<UniqueKeysKeyword>(),
				GetLibraryVersion<Bound>()
			});
		}

		[Route("~/json-path")]
		public IActionResult Path()
		{
			return View(GetLibraryVersion<JsonPath>());
		}

		[Route("~/json-patch")]
		public IActionResult Patch()
		{
			return View(GetLibraryVersion<JsonPatch>());
		}

		[Route("~/json-logic")]
		public IActionResult Logic()
		{
			return View(GetLibraryVersion<Rule>());
		}

		[Route("~/json-pointer")]
		public IActionResult Pointer()
		{
			return View(GetLibraryVersion<JsonPointer>());
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

		private static LibraryVersionModel GetLibraryVersion<T>()
		{
			var attribute = typeof(T).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
			var version = Regex.Match(attribute!.InformationalVersion, @"\d+\.\d+\.\d+").Value;
			return new LibraryVersionModel
			{
				Name = typeof(T).Assembly.GetName().Name!,
				Version = version
			};
		}
	}
}
