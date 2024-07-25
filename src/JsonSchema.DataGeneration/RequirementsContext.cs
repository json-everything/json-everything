﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.More;

namespace Json.Schema.DataGeneration;

internal class RequirementsContext
{
	public const SchemaValueType AllTypes =
		SchemaValueType.Array |
		SchemaValueType.Boolean |
		SchemaValueType.Integer |
		SchemaValueType.Null |
		SchemaValueType.Number |
		SchemaValueType.Object |
		SchemaValueType.String;

	public bool IsFalse { get; set; }

	public SchemaValueType? Type { get; set; }
	public SchemaValueType InferredType { get; set; }

	public NumberRangeSet? NumberRanges { get; set; }
	public List<decimal>? Multiples { get; set; }
	public List<decimal>? AntiMultiples { get; set; }

	public NumberRangeSet? StringLengths { get; set; }
	// https://www.ocpsoft.org/tutorials/regular-expressions/and-in-regex/
	//public List<Regex>? Patterns { get; set; }
	//public List<Regex>? AntiPatterns { get; set; }
	public string? Pattern { get; set; }
	public string? Format { get; set; }

	public List<RequirementsContext>? SequentialItems { get; set; }
	public RequirementsContext? RemainingItems { get; set; }
	public NumberRangeSet? ItemCounts { get; set; }
	// TODO: unevaluatedItems

	public RequirementsContext? Contains { get; set; }
	public NumberRangeSet? ContainsCounts { get; set; }

	public Dictionary<string, RequirementsContext>? Properties { get; set; }
	public RequirementsContext? RemainingProperties { get; set; }
	public NumberRangeSet? PropertyCounts { get; set; }
	public List<string>? RequiredProperties { get; set; }
	public List<string>? AvoidProperties { get; set; }
	// TODO: unevaluatedProperties

	public JsonNode? Const { get; set; }
	public bool ConstIsSet { get; set; }
	public List<(bool, JsonNode?)>? EnumOptions { get; set; }

	public List<RequirementsContext>? Options { get; set; }

	public bool HasConflict { get; set; }

	public RequirementsContext() { }

	public RequirementsContext(RequirementsContext other, bool copyOptions = true)
	{
		Type = other.Type;
		InferredType = other.InferredType;
		IsFalse = other.IsFalse;

		if (other.NumberRanges != null)
			NumberRanges = new NumberRangeSet(other.NumberRanges);
		if (other.Multiples != null)
			Multiples = [.. other.Multiples];
		if (other.AntiMultiples != null)
			AntiMultiples = [.. other.AntiMultiples];

		if (other.StringLengths != null)
			StringLengths = new NumberRangeSet(other.StringLengths);
		//if (other.Patterns != null)
		//	Patterns = other.Patterns.ToList();
		//if (other.AntiPatterns != null)
		//	AntiPatterns = other.AntiPatterns.ToList();
		if (other.Pattern != null)
			Pattern = other.Pattern;

		if (other.ItemCounts != null)
			ItemCounts = new NumberRangeSet(other.ItemCounts);
		if (other.RemainingItems != null)
			RemainingItems = new RequirementsContext(other.RemainingItems);

		if (copyOptions && other.Options != null)
			Options = other.Options.Select(x => new RequirementsContext(x)).ToList();

		Const = other.Const;
		ConstIsSet = other.ConstIsSet;
		HasConflict = other.HasConflict;

		if (other.Properties != null)
			Properties = other.Properties.ToDictionary(x => x.Key, x => x.Value);
		if (other.RemainingProperties != null)
			RemainingProperties = new RequirementsContext(other.RemainingProperties);
		if (other.PropertyCounts != null)
			PropertyCounts = other.PropertyCounts;
		if (other.RequiredProperties != null)
			RequiredProperties = [.. other.RequiredProperties];
		if (other.AvoidProperties != null)
			AvoidProperties = [.. other.AvoidProperties];
	}

	public IEnumerable<RequirementsContext> GetAllVariations()
	{
		RequirementsContext CreateVariation(RequirementsContext option)
		{
			var variation = new RequirementsContext(this, copyOptions: false);
			variation.And(option);
			return variation;
		}

		if (Options == null)
			yield return this;
		else
		{
			using var enumerator = Options.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current!.Options != null)
				{
					foreach (var variation in enumerator.Current.GetAllVariations())
					{
						yield return CreateVariation(variation);
					}
				}
				else
					yield return CreateVariation(enumerator.Current);
			}
		}
	}

	// Create a requirements object that doesn't meet this context's requirement
	// Only need to break one requirement for this to work, not all
	public RequirementsContext Break()
	{
		bool BreakBoolean(RequirementsContext context)
		{
			if (IsTrue())
			{
				context.IsFalse = true;
				return true;
			}

			if (IsFalse)
			{
				context.IsFalse = false;
				return true;
			}

			return false;
		}

		bool BreakType(RequirementsContext context)
		{
			if (Type == null) return false;
			context.Type = ~AllTypes ^ ~Type;
			return true;
		}

		bool BreakNumberRange(RequirementsContext context)
		{
			if (NumberRanges == null || !NumberRanges.Ranges.Any()) return false;
			context.NumberRanges = NumberRanges?.GetComplement();
			return true;
		}

		bool BreakMultiples(RequirementsContext context)
		{
			if (Multiples == null && AntiMultiples == null) return false;
			context.Multiples = AntiMultiples;
			context.AntiMultiples = Multiples;
			return true;
		}

		bool BreakStringLength(RequirementsContext context)
		{
			if (StringLengths == null || !StringLengths.Ranges.Any()) return false;
			context.StringLengths = StringLengths?.GetComplement();
			return true;
		}

		bool BreakPatterns(RequirementsContext context)
		{
			//if (Patterns == null && AntiPatterns == null) return false;
			//context.Patterns = AntiPatterns;
			//context.AntiPatterns = Patterns;
			//return true;
			if (Pattern != null)
				throw new NotSupportedException("Cannot generate string against negative pattern");

			return false;
		}

		bool BreakItems(RequirementsContext context)
		{
			if (RemainingItems == null) return false;
			context.RemainingItems = RemainingItems.Break();
			return true;
		}

		bool BreakItemCount(RequirementsContext context)
		{
			if (ItemCounts == null) return false;
			context.ItemCounts = ItemCounts?.GetComplement();
			return true;
		}

		bool BreakProperties(RequirementsContext context)
		{
			if (RemainingProperties != null)
			{
				context.RemainingProperties = RemainingProperties.Break();
				return true;
			}

			if (Properties != null)
			{
				context.Properties = Properties.ToDictionary(x => x.Key, x => x.Value.Break());
				context.RequiredProperties ??= [];
				context.RequiredProperties.AddRange(context.Properties.Where(x => !x.Value.IsFalse).Select(x => x.Key));
				return true;
			}

			return false;
		}

		bool BreakRequired(RequirementsContext context)
		{
			if (RequiredProperties == null && AvoidProperties == null) return false;
			context.RequiredProperties = AvoidProperties;
			context.AvoidProperties = RequiredProperties;
			return true;
		}

		bool BreakPropertyCounts(RequirementsContext context)
		{
			if (PropertyCounts == null) return false;
			context.PropertyCounts = PropertyCounts?.GetComplement();
			return true;
		}

		bool BreakContains(RequirementsContext context)
		{
			if (Contains == null) return false;
			context.Contains = Contains.Break();
			return true;
		}

		bool BreakContainsCount(RequirementsContext context)
		{
			if (ContainsCounts == null) return false;
			context.ContainsCounts = ContainsCounts?.GetComplement();
			return true;
		}

		bool BreakConst(RequirementsContext context)
		{
			if (!ConstIsSet) return false;
			context.Const = Const == null ? 1 : null;
			context.ConstIsSet = true;
			return true;
		}

		var allBreakers = new[]
		{
			BreakBoolean,
			BreakType,
			BreakNumberRange,
			BreakMultiples,
			BreakStringLength,
			BreakPatterns,
			BreakItems,
			BreakItemCount,
			BreakProperties,
			BreakRequired,
			BreakPropertyCounts,
			BreakContains,
			BreakContainsCount,
			BreakConst
		};
		var breakers = JsonSchemaExtensions.Randomizer.Shuffle(allBreakers);

		var broken = new RequirementsContext(this);
		using var enumerator = breakers.GetEnumerator();
		while (enumerator.MoveNext() && !enumerator.Current!(broken)) { }

		return broken;
	}

	public void And(RequirementsContext other)
	{
		IsFalse |= other.IsFalse;

		if (Type == null)
			Type = other.Type;
		else if (other.Type != null)
			Type &= other.Type;

		InferredType |= other.InferredType;

		if (NumberRanges == null || !NumberRanges.Ranges.Any())
			NumberRanges = other.NumberRanges;
		else if (other.NumberRanges != null)
			NumberRanges *= other.NumberRanges;

		if (Multiples == null)
			Multiples = other.Multiples;
		else if (other.Multiples != null)
			Multiples.AddRange(other.Multiples);

		if (AntiMultiples == null)
			AntiMultiples = other.AntiMultiples;
		else if (other.AntiMultiples != null)
			AntiMultiples.AddRange(other.AntiMultiples);

		if (StringLengths == null || !StringLengths.Ranges.Any())
			StringLengths = other.StringLengths;
		else if (other.StringLengths != null)
			StringLengths *= other.StringLengths;

		if (Format == null)
			Format = other.Format;
		else if (other.Format != null)
			HasConflict = Format != other.Format;

		if (!ConstIsSet)
		{
			Const = other.Const;
			ConstIsSet = other.ConstIsSet;
		}
		else if (other.ConstIsSet)
			HasConflict = Const.IsEquivalentTo(other.Const);

		//if (Patterns == null)
		//	Patterns = other.Patterns;
		//else if (other.Patterns != null)
		//	Patterns.AddRange(other.Patterns);

		//if (AntiPatterns == null)
		//	AntiPatterns = other.AntiPatterns;
		//else if (other.AntiPatterns != null)
		//	AntiPatterns.AddRange(other.AntiPatterns);

		if (Pattern == null)
			Pattern = other.Pattern;
		else if (other.Pattern != null)
			throw new NotSupportedException("Generator only supports `pattern` on a single branch.");

		if (ItemCounts == null || !ItemCounts.Ranges.Any())
			ItemCounts = other.ItemCounts;
		else if (other.ItemCounts != null)
			ItemCounts *= other.ItemCounts;

		// sequentialItems?

		if (RemainingItems == null)
			RemainingItems = other.RemainingItems;
		else if (other.RemainingItems != null)
			RemainingItems.And(other.RemainingItems);

		if (PropertyCounts == null || !PropertyCounts.Ranges.Any())
			PropertyCounts = other.PropertyCounts;
		else if (other.PropertyCounts != null)
			PropertyCounts *= other.PropertyCounts;

		if (Properties == null)
			Properties = other.Properties;
		else if (other.Properties != null)
		{
			var allKeys = Properties.Keys.Union(other.Properties.Keys);
			foreach (var key in allKeys)
			{
				Properties.TryGetValue(key, out var thisProperty);
				other.Properties.TryGetValue(key, out var otherProperty);

				if (thisProperty == null)
					Properties[key] = otherProperty!;
				else if (otherProperty != null)
					thisProperty.And(otherProperty);
			}
		}

		if (RemainingProperties == null)
			RemainingProperties = other.RemainingProperties;
		else if (other.RemainingProperties != null)
			RemainingProperties.And(other.RemainingProperties);

		if (RequiredProperties == null)
			RequiredProperties = other.RequiredProperties;
		else if (other.RequiredProperties != null)
			RequiredProperties.AddRange(other.RequiredProperties);

		if (AvoidProperties == null)
			AvoidProperties = other.AvoidProperties;
		else if (other.AvoidProperties != null)
			AvoidProperties.AddRange(other.AvoidProperties);

		if (Contains == null)
			Contains = other.Contains;
		else if (other.Contains != null)
			// is this right?
			Contains.And(other.Contains);

		if (ContainsCounts == null || !ContainsCounts.Ranges.Any())
			ContainsCounts = other.ContainsCounts;
		else if (other.ContainsCounts != null)
			ContainsCounts *= other.ContainsCounts;
	}

	private bool IsTrue()
	{
		return !IsFalse &&
		       Type == null &&
		       NumberRanges == null &&
		       Multiples == null &&
		       AntiMultiples == null &&
		       StringLengths == null &&
		       //Patterns == null &&
		       //AntiPatterns == null &&
		       Pattern == null &&
		       Format == null &&
		       SequentialItems == null &&
		       RemainingItems == null &&
		       ItemCounts == null &&
		       Contains == null &&
		       ContainsCounts == null &&
		       Properties == null &&
		       RemainingProperties == null &&
		       PropertyCounts == null &&
		       RequiredProperties == null &&
		       AvoidProperties == null &&
		       Const == null &&
		       !ConstIsSet &&
		       EnumOptions == null &&
		       Options == null;
	}
}