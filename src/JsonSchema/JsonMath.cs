using System;
using System.Text.Json;

namespace Json.Schema;

/// <summary>
/// Provides static methods for performing mathematical operations and comparisons on JSON numeric values represented by
/// <see cref="System.Text.Json.JsonElement"/> instances.
/// </summary>
/// <remarks>The methods in this class are designed to work directly with the textual representation of JSON
/// numbers, enabling precise comparison and divisibility checks without converting to .NET numeric types. This is
/// useful for scenarios where exact JSON number semantics are required, such as schema validation or custom JSON
/// processing. All methods are thread-safe and do not modify the input <see cref="JsonElement"/> instances.</remarks>
public static class JsonMath
{
	/// <summary>
	/// Compares two JSON number values represented by <see cref="JsonElement"/> instances and determines their relative
	/// order.
	/// </summary>
	/// <remarks>This method performs a numeric comparison based on the JSON number representations, handling
	/// differences in sign, digit sequence, and exponent. Both parameters must represent valid JSON numbers; otherwise,
	/// the result is undefined.</remarks>
	/// <param name="a">The first <see cref="JsonElement"/> containing a JSON number to compare.</param>
	/// <param name="b">The second <see cref="JsonElement"/> containing a JSON number to compare.</param>
	/// <returns>A signed integer that indicates the relative order of the numbers: less than zero if <paramref name="a"/> is less
	/// than <paramref name="b"/>; zero if they are equal; greater than zero if <paramref name="a"/> is greater than
	/// <paramref name="b"/>.</returns>
	public static int NumberCompare(JsonElement a, JsonElement b)
	{
		var aSpan = a.GetRawText().AsSpan();
		var bSpan = b.GetRawText().AsSpan();

		var (aNeg, aDigitStart, aDigitLen, aExp) = NormalizeNumber(aSpan);
		var (bNeg, bDigitStart, bDigitLen, bExp) = NormalizeNumber(bSpan);

		if (aNeg != bNeg)
			return aNeg ? -1 : 1;

		int signMultiplier = aNeg ? -1 : 1;

		if (aExp != bExp)
			return aExp > bExp ? signMultiplier : -signMultiplier;

		int aIndex = aDigitStart;
		int bIndex = bDigitStart;
		int aDigitsCompared = 0;
		int bDigitsCompared = 0;

		int aEnd = aSpan.Length;
		int bEnd = bSpan.Length;
		for (int i = aDigitStart; i < aSpan.Length; i++)
		{
			if (aSpan[i] == 'e' || aSpan[i] == 'E')
			{
				aEnd = i;
				break;
			}
		}
		for (int i = bDigitStart; i < bSpan.Length; i++)
		{
			if (bSpan[i] == 'e' || bSpan[i] == 'E')
			{
				bEnd = i;
				break;
			}
		}

		while (aDigitsCompared < aDigitLen && bDigitsCompared < bDigitLen)
		{
			while (aIndex < aEnd && aSpan[aIndex] == '.')
				aIndex++;
			
			while (bIndex < bEnd && bSpan[bIndex] == '.')
				bIndex++;

			if (aIndex >= aEnd || bIndex >= bEnd)
				break;

			char aChar = aSpan[aIndex];
			char bChar = bSpan[bIndex];

			if (aChar != bChar)
				return aChar > bChar ? signMultiplier : -signMultiplier;

			aIndex++;
			bIndex++;
			aDigitsCompared++;
			bDigitsCompared++;
		}

		if (aDigitLen != bDigitLen)
			return aDigitLen > bDigitLen ? signMultiplier : -signMultiplier;

		return 0;
	}

	private static (bool isNegative, int digitStart, int digitLength, int exponent) NormalizeNumber(ReadOnlySpan<char> span)
	{
		bool isNegative = false;
		int index = 0;

		if (span[index] == '-')
		{
			isNegative = true;
			index++;
		}

		int mantissaEnd = span.Length;
		for (int i = index; i < span.Length; i++)
		{
			if (span[i] == 'e' || span[i] == 'E')
			{
				mantissaEnd = i;
				break;
			}
		}

		int decimalPos = -1;
		for (int i = index; i < mantissaEnd; i++)
		{
			if (span[i] == '.')
			{
				decimalPos = i;
				break;
			}
		}

		int firstNonZero = -1;
		for (int i = index; i < mantissaEnd; i++)
		{
			if (span[i] >= '1' && span[i] <= '9')
			{
				firstNonZero = i;
				break;
			}
		}

		if (firstNonZero == -1)
		{
			return (false, index, 0, 0);
		}

		int exponentValue;
		if (decimalPos == -1)
		{
			int digitsAfterFirstNonZero = 0;
			for (int i = firstNonZero; i < mantissaEnd; i++)
			{
				if (span[i] >= '0' && span[i] <= '9')
					digitsAfterFirstNonZero++;
			}
			exponentValue = digitsAfterFirstNonZero - 1;
		}
		else if (firstNonZero < decimalPos)
		{
			int digitsBeforeDecimal = 0;
			for (int i = firstNonZero; i < decimalPos; i++)
			{
				if (span[i] >= '0' && span[i] <= '9')
					digitsBeforeDecimal++;
			}
			exponentValue = digitsBeforeDecimal - 1;
		}
		else
		{
			int zerosAfterDecimal = 0;
			for (int i = decimalPos + 1; i < firstNonZero; i++)
			{
				if (span[i] == '0')
					zerosAfterDecimal++;
			}
			exponentValue = -(zerosAfterDecimal + 1);
		}

		if (mantissaEnd < span.Length)
		{
			int explicitExp = 0;
			bool expNegative = false;
			int expStart = mantissaEnd + 1;
			
			if (expStart < span.Length && span[expStart] == '-')
			{
				expNegative = true;
				expStart++;
			}
			else if (expStart < span.Length && span[expStart] == '+')
			{
				expStart++;
			}

			for (int i = expStart; i < span.Length; i++)
			{
				if (span[i] >= '0' && span[i] <= '9')
					explicitExp = explicitExp * 10 + (span[i] - '0');
			}

			if (expNegative)
				explicitExp = -explicitExp;

			exponentValue += explicitExp;
		}

		int lastNonZero = -1;
		for (int i = mantissaEnd - 1; i >= index; i--)
		{
			if (span[i] >= '1' && span[i] <= '9')
			{
				lastNonZero = i;
				break;
			}
		}

		int significantDigits = 0;
		for (int i = firstNonZero; i <= lastNonZero; i++)
		{
			if (span[i] >= '0' && span[i] <= '9')
				significantDigits++;
		}

		return (isNegative, firstNonZero, significantDigits, exponentValue);
	}

	/// <summary>
	/// Determines whether the numeric value represented by the specified JSON element is evenly divisible by the value of
	/// another JSON element.
	/// </summary>
	/// <remarks>Both parameters must represent valid JSON numbers. If the divisor is zero or not a valid number,
	/// the method returns false. If the dividend is zero, the method returns true.</remarks>
	/// <param name="dividend">The JSON element representing the dividend. Must contain a valid numeric value.</param>
	/// <param name="divisor">The JSON element representing the divisor. Must contain a valid numeric value.</param>
	/// <returns>true if the dividend is evenly divisible by the divisor; otherwise, false.</returns>
	public static bool Divides(JsonElement dividend, JsonElement divisor)
	{
		var dividendSpan = dividend.GetRawText().AsSpan();
		var divisorSpan = divisor.GetRawText().AsSpan();

		var (_, dividendStart, dividendLen, dividendExp) = NormalizeNumber(dividendSpan);
		var (_, divisorStart, divisorLen, divisorExp) = NormalizeNumber(divisorSpan);

		if (divisorLen == 0)
			return false;

		if (dividendLen == 0)
			return true;

		Span<byte> dividendDigits = stackalloc byte[dividendLen];
		Span<byte> divisorDigits = stackalloc byte[divisorLen];

		ExtractDigits(dividendSpan, dividendStart, dividendLen, dividendDigits);
		ExtractDigits(divisorSpan, divisorStart, divisorLen, divisorDigits);

		int dividendScale = dividendLen - 1 - dividendExp;
		int divisorScale = divisorLen - 1 - divisorExp;
		int scaleDiff = dividendScale - divisorScale;

		if (scaleDiff > 0)
		{
			Span<byte> extendedDivisor = stackalloc byte[divisorLen + scaleDiff];
			divisorDigits.CopyTo(extendedDivisor);
			extendedDivisor.Slice(divisorLen).Fill(0);
			divisorDigits = extendedDivisor;
			divisorLen += scaleDiff;
		}
		else if (scaleDiff < 0)
		{
			Span<byte> extendedDividend = stackalloc byte[dividendLen + (-scaleDiff)];
			dividendDigits.CopyTo(extendedDividend);
			extendedDividend.Slice(dividendLen).Fill(0);
			dividendDigits = extendedDividend;
			dividendLen += (-scaleDiff);
		}

		return ModuloDigits(dividendDigits.Slice(0, dividendLen), divisorDigits.Slice(0, divisorLen)) == 0;
	}

	private static int ModuloDigits(Span<byte> dividend, Span<byte> divisor)
	{
		ulong divisorValue = 0;
		for (int i = 0; i < divisor.Length; i++)
		{
			divisorValue = divisorValue * 10 + divisor[i];
			if (divisorValue > int.MaxValue)
				return ModuloDigitsLarge(dividend, divisor);
		}

		if (divisorValue == 0)
			return -1;

		ulong remainder = 0;
		for (int i = 0; i < dividend.Length; i++)
		{
			remainder = (remainder * 10 + dividend[i]) % divisorValue;
		}

		return (int)remainder;
	}

	private static int ModuloDigitsLarge(Span<byte> dividend, Span<byte> divisor)
	{
		Span<byte> remainder = stackalloc byte[divisor.Length + 1];
		int remainderLen = 0;

		for (int i = 0; i < dividend.Length; i++)
		{
			remainder[remainderLen++] = dividend[i];

			while (remainderLen > 1 && remainder[0] == 0)
			{
				for (int k = 0; k < remainderLen - 1; k++)
					remainder[k] = remainder[k + 1];
				remainderLen--;
			}

			while (CompareDigits(remainder, remainderLen, divisor) >= 0)
			{
				SubtractDigits(remainder, ref remainderLen, divisor);
			}
		}

		if (IsZero(remainder, remainderLen))
			return 0;

		return 1;
	}

	private static void ExtractDigits(ReadOnlySpan<char> span, int start, int length, Span<byte> output)
	{
		int outputIndex = 0;
		int digitsExtracted = 0;
		
		int end = span.Length;
		for (int i = 0; i < span.Length; i++)
		{
			if (span[i] == 'e' || span[i] == 'E')
			{
				end = i;
				break;
			}
		}

		for (int i = start; i < end && digitsExtracted < length; i++)
		{
			if (span[i] >= '0' && span[i] <= '9')
			{
				output[outputIndex++] = (byte)(span[i] - '0');
				digitsExtracted++;
			}
		}
	}

	private static bool IsZero(Span<byte> digits, int length)
	{
		for (int i = 0; i < length; i++)
		{
			if (digits[i] != 0)
				return false;
		}
		return true;
	}

	private static int CompareDigits(Span<byte> a, int aLen, Span<byte> b)
	{
		int aStart = 0;
		while (aStart < aLen && a[aStart] == 0)
			aStart++;

		int bStart = 0;
		while (bStart < b.Length && b[bStart] == 0)
			bStart++;

		int aSignificant = aLen - aStart;
		int bSignificant = b.Length - bStart;

		if (aSignificant != bSignificant)
			return aSignificant.CompareTo(bSignificant);

		for (int i = 0; i < aSignificant; i++)
		{
			if (a[aStart + i] != b[bStart + i])
				return a[aStart + i].CompareTo(b[bStart + i]);
		}

		return 0;
	}

	private static void SubtractDigits(Span<byte> a, ref int aLen, Span<byte> b)
	{
		int borrow = 0;
		
		for (int i = 0; i < aLen; i++)
		{
			int aPos = aLen - 1 - i;
			int bPos = b.Length - 1 - i;
			
			int aVal = a[aPos];
			int bVal = bPos >= 0 ? b[bPos] : 0;
			
			int diff = aVal - bVal - borrow;
			
			if (diff < 0)
			{
				diff += 10;
				borrow = 1;
			}
			else
			{
				borrow = 0;
			}
			
			a[aPos] = (byte)diff;
		}

		while (aLen > 0 && a[0] == 0)
		{
			for (int k = 0; k < aLen - 1; k++)
				a[k] = a[k + 1];
			aLen--;
		}

		if (aLen == 0)
		{
			a[0] = 0;
			aLen = 1;
		}
	}
}