using System.Reflection;

namespace ApiDocsGenerator.MarkdownGen.Reflection;

/// <summary>
///     DocXmlReader extension methods to retrieve type properties, methods, and fields
///     using reflection information.
/// </summary>
public static class DocXmlReaderExtensions
{
	/// <summary>
	///     Get comments for the collection of properties.
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="propInfos"></param>
	/// <returns></returns>
	public static async Task<IEnumerable<(PropertyInfo Info, CommonComments Comments)>> Comments(
		this DocXmlReader reader,
		IEnumerable<PropertyInfo> propInfos)
	{
		return await Task.WhenAll(propInfos.Select(async info => (info, await reader.GetMemberComments(info))));
	}

	/// <summary>
	///     Get comments for the collection of methods.
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="methodInfos"></param>
	/// <returns></returns>
	public static async Task<IEnumerable<(MethodBase Info, MethodComments Comments)>> Comments(
		this DocXmlReader reader,
		IEnumerable<MethodBase> methodInfos)
	{
		return (await Task.WhenAll(methodInfos.Select(async info => (info, Comments: (await reader.GetMethodComments(info, info.IsConstructor && info.GetParameters().Length == 0))!))))
			// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
			.Where(data => data.Comments != null);
	}

	/// <summary>
	///     Get comments for the collection of fields.
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="fieldInfos"></param>
	/// <returns></returns>
	public static async Task<IEnumerable<(FieldInfo Info, CommonComments Comments)>> Comments(
		this DocXmlReader reader,
		IEnumerable<FieldInfo> fieldInfos)
	{
		return await Task.WhenAll(fieldInfos.Select(async info => (info, await reader.GetMemberComments(info))));
	}
}
