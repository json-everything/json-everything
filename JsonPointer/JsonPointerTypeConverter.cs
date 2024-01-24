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

		/// <summary>
		/// The standard values used by this converter.
		/// </summary>
		/// <remarks>
		///   This field is initialized using a so-called poor man's lazy in <see cref="GetStandardValues(ITypeDescriptorContext)"/>.
		/// </remarks>
		private static StandardValuesCollection? _standardValues;

		/// <inheritdoc/>
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		{
			return sourceType == typeof(string) || sourceType == typeof(JsonPointer) || base.CanConvertFrom(context, sourceType);
		}

		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
		{
			return destinationType == typeof(string) || destinationType == typeof(JsonPointer) || base.CanConvertTo(context, destinationType);
		}

		/// <inheritdoc/>
		public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			if (value is string s)
			{
				return JsonPointer.Parse(s);
			}
			else if (value is JsonPointer pointer)
			{
				return JsonPointer.Create(pointer.Segments);
			}

			throw GetConvertFromException(value);
		}

		/// <inheritdoc/>
		public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is JsonPointer pointer)
			{
				if (destinationType == typeof(string))
				{
					return pointer.ToString();
				}
				else if (destinationType == typeof(JsonPointer))
				{
					return JsonPointer.Create(pointer.Segments);
				}
			}

			throw GetConvertToException(value, destinationType);
		}

		/// <inheritdoc/>
		public override bool GetStandardValuesSupported(ITypeDescriptorContext? context)
		{
			return true;
		}

		/// <inheritdoc/>
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context)
		{
			return false;
		}

		/// <inheritdoc/>
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context)
		{
			return _standardValues ??= new StandardValuesCollection(new[] { JsonPointer.Empty });
		}

		/// <inheritdoc/>
		public override bool IsValid(ITypeDescriptorContext? context, object? value)
		{
			if (value is string s)
			{
				return JsonPointer.TryParse(s, out _);
			}
			return value is JsonPointer;
		}

  }
}
