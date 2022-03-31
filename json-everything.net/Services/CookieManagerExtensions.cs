using BlazorMonaco;

namespace JsonEverythingNet.Services;

public static class CookieManagerExtensions
{
	public static async Task SaveEditorValue(this CookieManager cookieManager, MonacoEditor editor, string cookieKey)
	{
		var text = await editor.GetValue();

		await cookieManager.Set(cookieKey, text);
	}

	public static async Task LoadEditorValue(this CookieManager cookieManager, MonacoEditor editor, string cookieKey)
	{
		var text = cookieManager.Get(cookieKey) ?? string.Empty;

		await editor.SetValue(text);
	}
}