using Json.Schema.Generation.Intents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Removes `null` from schema type array.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class NotNullAttribute : Attribute, IAttributeHandler
	{
		/// <summary>
		/// Creates a new <see cref="NotNullAttribute"/> instance.
		/// </summary>
		public NotNullAttribute()
		{
		}

		void IAttributeHandler.AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<NotNullAttribute>().FirstOrDefault();
			if (attribute == null) return;

			var cleanIntents = new List<ISchemaKeywordIntent>();

			foreach (var intent in context.Intents)
			{
				if (intent is TypeIntent tIntent && (tIntent.Type & SchemaValueType.Null) != 0)
				{
					var valueType = tIntent.Type & ~SchemaValueType.Null;

					if (valueType != 0)
					{
						cleanIntents.Add(new TypeIntent(valueType));
					}

					continue;

				}

				cleanIntents.Add(intent);
			}

			context.Intents.Clear();

			context.Intents.AddRange(cleanIntents);
		}
	}
}