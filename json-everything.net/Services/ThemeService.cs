namespace JsonEverythingNet.Services;

public class ThemeService
{
	public event Action? OnThemeChanged;
	
	private bool _isDarkMode = true;
	
	public bool IsDarkMode => _isDarkMode;
	
	public string MonacoTheme => _isDarkMode ? "vs-dark" : "vs";
	
	public void SetTheme(bool isDarkMode)
	{
		if (_isDarkMode != isDarkMode)
		{
			_isDarkMode = isDarkMode;
			OnThemeChanged?.Invoke();
		}
	}
}
