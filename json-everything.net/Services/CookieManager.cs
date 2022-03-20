using System.Text;
using Microsoft.JSInterop;

namespace JsonEverythingNet.Services;

public class CookieManager
{
	private readonly IJSRuntime _jsRuntime;
	private readonly Dictionary<string, string> _cache;

	public CookieManager(IJSRuntime jsRuntime)
	{
		_jsRuntime = jsRuntime;
		_cache = new Dictionary<string, string>();
	}

	public async Task Set(string key, string value)
	{
		var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(value), Base64FormattingOptions.None);
		await SetValue(key, base64, 30);
	}

	public string? Get(string key)
	{
		var base64 = GetValue(key, null);
		if (base64 == null) return null;

		var value = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
		return value;
	}

	public async Task Initialize()
	{
		var cValue = await GetCookie();
		if (string.IsNullOrEmpty(cValue)) return;

		var cookies = cValue.Split(';');
		foreach (var val in cookies)
		{
			if (string.IsNullOrEmpty(val)) continue;

			var index = val.IndexOf('=');
			var key = val[..(index)].Trim();
			var value = val[(index+1)..];
			if (string.IsNullOrEmpty(value)) continue;

			_cache[key] = value;
		}
	}

	private async Task SetValue(string key, string value, int? days = null)
	{
		_cache[key] = value;

		var curExp = days is > 0
			? DateToUtc(days.Value)
			: string.Empty;

		await SetCookie($"{key}={value}; expires={curExp}; path=/");
	}

	private string? GetValue(string key, string? defaultValue = "")
	{
		if (_cache.TryGetValue(key, out var value)) return value;

		return defaultValue;
	}

	private async Task SetCookie(string value)
	{
		await _jsRuntime.InvokeVoidAsync("eval", $"document.cookie = \"{value}\"");
	}

	private async Task<string> GetCookie()
	{
		return await _jsRuntime.InvokeAsync<string>("eval", "document.cookie");
	}

	private static string DateToUtc(int days) => DateTime.Now.AddDays(days).ToUniversalTime().ToString("R");
}