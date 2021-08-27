using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;

namespace TryJsonEverything
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
						webBuilder.ConfigureKestrel(serverOptions =>
						{
							serverOptions.Listen(IPAddress.Any, Convert.ToInt32(Environment.GetEnvironmentVariable("PORT")));
						});
					webBuilder.UseStartup<Startup>();
				});
	}
}
