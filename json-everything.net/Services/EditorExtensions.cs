using System.Text.Json.Nodes;
using BlazorMonaco;
using BlazorMonaco.Editor;
using Microsoft.JSInterop;
using Yaml2JsonNode;

namespace JsonEverythingNet.Services;

public static class EditorExtensions
{
	public static void SetLanguage(this StandaloneCodeEditor editor, string language, IJSRuntime jsRuntime)
	{
		editor.GetModel()
			.ContinueWith(x => Global.SetModelLanguage(jsRuntime, x.Result, language));
	}

	public static async Task SetLanguageAsync(this StandaloneCodeEditor editor, string language, IJSRuntime jsRuntime)
	{
		var model = await editor.GetModel();
		await Global.SetModelLanguage(jsRuntime, model, language);
	}

	public static async Task<string> DetectLanguage(this StandaloneCodeEditor editor, IJSRuntime jsRuntime)
	{
		var text = await editor.GetValue();

		if (TryParseJson(text))
		{
			await editor.UpdateOptions(new EditorUpdateOptions { TabSize = 2 });
			await editor.SetLanguageAsync("json", jsRuntime);
			return "json";
		}
		else if (TryParseYaml(text))
		{
			await editor.UpdateOptions(new EditorUpdateOptions { TabSize = 2 });
			await editor.SetLanguageAsync("yaml", jsRuntime);
			return "yaml";
		}

		return string.Empty;
	}

	private static bool TryParseJson(string text)
	{
		try
		{
			JsonNode.Parse(text);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static bool TryParseYaml(string text)
	{
		try
		{
			YamlSerializer.Parse(text);
			return true;
		}
		catch
		{
			return false;
		}
	}
}