using System.Text.RegularExpressions;

namespace Json.Schema.CodeGeneration.Model;

public static class ValidationExtensions
{
	public static void ValidateForCSharp(this TypeModel typeModel)
	{
		ValidateName(typeModel.Name);

		switch (typeModel)
		{
			case ArrayModel arrayModel:
				ValidateForCSharp(arrayModel.Items);
				break;
			case DictionaryModel dictionaryModel:
				ValidateForCSharp(dictionaryModel.Items);
				ValidateForCSharp(dictionaryModel.Keys);
				break;
			case EnumModel:
				break;
			case ObjectModel objectModel:
			{
				foreach (var propertyModel in objectModel.Properties)
				{
					ValidateForCSharp(propertyModel);
				}
				break;
			}
		}
	}

	private static void ValidateForCSharp(PropertyModel propertyModel)
	{
		ValidateName(propertyModel.Name);
		ValidateForCSharp(propertyModel.Type);
		if (propertyModel is { CanRead: false, CanWrite: false })
			throw new SchemaConversionException($"Property '{propertyModel.Name}' can neither be written to or read from.");
	}

	private static readonly Regex _nameRegex = new("^[a-zA-Z_][a-zA-Z0-9_]$");

	private static void ValidateName(string name)
	{
		if (!_nameRegex.IsMatch(name))
			throw new SchemaConversionException($"The name '{name}' is not a valid type or member name");
	}
}