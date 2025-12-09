using BlazorMonaco;
using BlazorMonaco.Editor;

namespace JsonEverythingNet.Services
{
	public class EditorOptions
	{
		private readonly ThemeService _themeService;

		public EditorOptions(ThemeService themeService)
		{
			_themeService = themeService;
		}

		public StandaloneEditorConstructionOptions Basic() =>
			new()
			{
				AutomaticLayout = true,
				Language = "json",
				Theme = _themeService.MonacoTheme,
				SelectOnLineNumbers = true,
				Scrollbar = new EditorScrollbarOptions
				{
					AlwaysConsumeMouseWheel = false
				},
				ScrollBeyondLastLine = false,
				TabSize = 2
			};

		public StandaloneEditorConstructionOptions Readonly()
		{
			var options = Basic();
			options.ReadOnly = true;

			return options;
		}
	}
}