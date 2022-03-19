using System.Reflection;
using System.Text.RegularExpressions;

#pragma warning disable 8618

namespace JsonEverythingNet.Services;

public class LibraryVersion
{
	public string Name { get; set; }
	public string Version { get; set; }

	public static LibraryVersion GetFor<T>()
	{
		var attribute = typeof(T).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		var version = Regex.Match(attribute!.InformationalVersion, @"\d+\.\d+\.\d+").Value;
		return new LibraryVersion
		{
			Name = typeof(T).Assembly.GetName().Name!,
			Version = version
		};
	}
}