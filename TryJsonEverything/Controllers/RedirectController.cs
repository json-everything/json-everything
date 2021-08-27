using Microsoft.AspNetCore.Mvc;

namespace TryJsonEverything.Controllers
{
	public class RedirectController : Controller
	{
		public IActionResult Github()
		{
			return Redirect("https://github.com/gregsdennis/json-everything");
		}

		public IActionResult Slack()
		{
			return Redirect("https://join.slack.com/t/manateeopensource/shared_invite/enQtMzU4MjgzMjgyNzU3LWZjYzAzYzY3NjY1MjY3ODI0ZGJiZjc3Nzk1MDM5NTNlMjMyOTE0MzMxYWVjMjdiOGU1NDY5OGVhMGQ5YzY4Zjg");
		}

		public IActionResult Documentation()
		{
			return Redirect("https://gregsdennis.github.io/json-everything");
		}
	}
}
