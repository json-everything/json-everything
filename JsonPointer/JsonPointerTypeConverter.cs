using System;
using System.ComponentModel;
using System.Globalization;

namespace Json.Pointer
{
	/// <summary>
	/// <see cref="TypeConverter"/> for <see cref="JsonPointer"/>.
	/// </summary>
	internal sealed class JsonPointerTypeConverter : TypeConverter
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
				return JsonPointer.Parse(s);
			}

			return base.ConvertFrom(context, culture, value);
		}

		/// <inheritdoc/>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string) && value is JsonPointer pointer)
			{
				return pointer.ToString();
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

	}
}
