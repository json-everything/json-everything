using System;
using System.Collections.Generic;
using System.Linq;
using Json.Pointer;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	public class SchemaGeneratorContext
	{
		public Type Type { get; }
		public List<Attribute> Attributes { get; }
		public List<ISchemaKeywordIntent> Intents { get; } = new List<ISchemaKeywordIntent>();

		public SchemaGeneratorContext(Type type, List<Attribute> attributes)
		{
			Type = type;
			Attributes = attributes;
		}

		public void GenerateIntents()
		{
			var generator = GeneratorRegistry.Get(Type);
			generator?.AddConstraints(this);

			AttributeHandler.HandleAttributes( this);
		}

		internal void Optimize()
		{
			var allContexts = GetChildContexts().ToList();
			var defsByHashCode = allContexts.GroupBy(c => c.GetHashCode())
				.Where(g => g.Count() > 1)
				.ToDictionary(g => g.Key, g => g.First());

			foreach (var def in defsByHashCode)
			{
				var refIntent = new RefIntent(new Uri($"#/$defs/{def.Key}", UriKind.Relative));
				var refContext = new SchemaGeneratorContext(def.Value.Type, null);
				refContext.Intents.Add(refIntent);
				foreach (var intent in Intents)
				{
					intent.Replace(def.Key, refContext);
				}
			}
			var defsIntent = new DefsIntent(defsByHashCode.ToDictionary(d => d.Key.ToString(), d => d.Value));

			Intents.Add(defsIntent);
		}

		public IEnumerable<SchemaGeneratorContext> GetChildContexts()
		{
			return Intents.SelectMany(i => i.GetChildContexts());
		}

		public JsonSchemaBuilder Apply(JsonSchemaBuilder builder = null)
		{
			builder ??= new JsonSchemaBuilder();

			foreach (var intent in Intents)
			{
				intent.Apply(builder);
			}

			return builder;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			var other = (SchemaGeneratorContext) obj;
			return Type == other.Type &&
			       Intents.ContentsEqual(other.Intents);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Type.GetHashCode();
				hashCode = (hashCode * 397) ^ Intents.GetCollectionHashCode();
				return hashCode;
			}
		}
	}
}