using System;
using System.ComponentModel;
using System.Globalization;

namespace Json.Pointer;

internal sealed class JsonPointer2TypeConverter : TypeConverter
{
    private static StandardValuesCollection? _standardValues;

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || sourceType == typeof(JsonPointer2) || base.CanConvertFrom(context, sourceType);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string) || destinationType == typeof(JsonPointer2) || base.CanConvertTo(context, destinationType);
    }

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
    {
        if (value is string s)
            return JsonPointer2.Parse(s);
            
        if (value is JsonPointer2 pointer)
            return pointer;

        throw GetConvertFromException(value);
    }

    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is JsonPointer2 pointer)
        {
            if (destinationType == typeof(string))
                return pointer.ToString();
                
            if (destinationType == typeof(JsonPointer2))
                return pointer;
        }

        throw GetConvertToException(value, destinationType);
    }

    public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => false;

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context)
    {
        return _standardValues ??= new StandardValuesCollection(new[] { JsonPointer2.Empty });
    }

    public override bool IsValid(ITypeDescriptorContext? context, object? value)
    {
        return value is string s
            ? JsonPointer2.TryParse(s, out _)
            : value is JsonPointer2;
    }
} 