using Microsoft.JSInterop;

namespace JsonEverythingNet.Services;

public static class JsExtensions
{
	public static async Task Call(this IJSRuntime js, string function, Action<Exception>? onError = null)
	{
		try
		{
			await js.InvokeVoidAsync(function);
		}
		catch (Exception e)
		{
			if (onError is null)
			{
				Console.WriteLine(e);
				throw;
			}

			onError(e);
		}
	}

	public static async Task Call(this IJSRuntime js, string function, object[] parameters, Action<Exception>? onError = null)
	{
		try
		{
			await js.InvokeVoidAsync(function, parameters);
		}
		catch (Exception e)
		{
			if (onError is null)
			{
				Console.WriteLine(e);
				throw;
			}

			onError(e);
		}
	}
}