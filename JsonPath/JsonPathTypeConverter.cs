using System;
using System.ComponentModel;
using System.Globalization;

namespace Json.Path;

internal sealed class JsonPathTypeConverter : TypeConverter
{

	private static StandardValuesCollection? _standardValues;

	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
	{
		return sourceType == typeof(string) || sourceType == typeof(JsonPath) || base.CanConvertFrom(context, sourceType);
	}

	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
	{
		return destinationType == typeof(string) || destinationType == typeof(JsonPath) || base.CanConvertTo(context, destinationType);
	}

	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (value is string s)
			return JsonPath.Parse(s);

		if (value is JsonPath path)
			return new JsonPath(path.Scope, path.Segments);

		throw GetConvertFromException(value);
	}

	public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
	{
		if (value is JsonPath path)
		{
			if (destinationType == typeof(string))
				return path.ToString();
			
			if (destinationType == typeof(JsonPath))
				return new JsonPath(path.Scope, path.Segments);
		}

		throw GetConvertToException(value, destinationType);
	}

	public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

	public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => false;

	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context)
	{
		return _standardValues ??= new StandardValuesCollection(new[] { JsonPath.Root });
	}

	public override bool IsValid(ITypeDescriptorContext? context, object? value)
	{
		if (value is string s)
			return JsonPath.TryParse(s, out _);

		return value is JsonPath;
	}

}