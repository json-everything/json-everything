using System;
using System.ComponentModel;
using System.Globalization;

namespace Json.Path
{
	/// <summary>
	/// <see cref="TypeConverter"/> for <see cref="JsonPath"/>.
	/// </summary>
	internal sealed class JsonPathTypeConverter : TypeConverter
	{

		/// <inheritdoc/>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string);
		}

		/// <inheritdoc/>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string s)
			{
				return JsonPath.Parse(s);
			}

			return base.ConvertFrom(context, culture, value);
		}

		/// <inheritdoc/>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string) && value is JsonPath pointer)
			{
				return pointer.ToString();
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

	}
}
