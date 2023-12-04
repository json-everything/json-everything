using System.Text.Json.Nodes;
using BlazorMonaco;
using Yaml2JsonNode;

namespace JsonEverythingNet.Services;

public static class EditorExtensions
{
	public static void SetLanguage(this MonacoEditor editor, string language)
	{
		editor.GetModel()
			.ContinueWith(x => MonacoEditorBase.SetModelLanguage(x.Result, language));
	}

	public static async Task SetLanguageAsync(this MonacoEditor editor, string language)
	{
		var model = await editor.GetModel();
		await MonacoEditorBase.SetModelLanguage(model, language);
	}

	public static async Task DetectLanguage(this MonacoEditor editor)
	{
		var text = await editor.GetValue();

		if (TryParseJson(text))
		{
			await editor.UpdateOptions(new GlobalEditorOptions { TabSize = 2 });
			await editor.SetLanguageAsync("json");
		}
		else if (TryParseYaml(text))
		{
			await editor.UpdateOptions(new GlobalEditorOptions { TabSize = 2 });
			await editor.SetLanguageAsync("yaml");
		}
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