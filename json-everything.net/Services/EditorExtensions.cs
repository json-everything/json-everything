using BlazorMonaco;

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
}