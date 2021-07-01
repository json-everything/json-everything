 using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Json.Schema.Generation.Intents;
 using Json.Schema.Generation.Refiners;

 namespace Json.Schema.Generation
{
	/// <summary>
	/// Provides meta-data about the generation process.
	/// </summary>
	public class SchemaGeneratorContext
	{
		private class ContextCount
		{
			public SchemaGeneratorContext Context { get; }
			public int Count { get; set; }

			public ContextCount(SchemaGeneratorContext context)
			{
				Context = context;
				Count = 1;
			}
		}

		private class MemberInfoMetadataTokenComparer : Comparer<MemberInfo>
		{

			private readonly int[] _typeOrder;

			private MemberInfoMetadataTokenComparer(Type type)
			{
				var typeStack = new Stack<Type>();

				do
				{
					typeStack.Push(type);
					type = type.BaseType!;
				} while (type != null);

				_typeOrder = typeStack.Select(GetMetadataToken).ToArray();
			}

			public static MemberInfoMetadataTokenComparer ForType(Type type)
			{
				return new MemberInfoMetadataTokenComparer(type ?? throw new ArgumentNullException(nameof(type)));
			}

			private static bool HasMetadataToken(MemberInfo? member)
			{
				if (member == null) return false;

#if NET5_0_OR_GREATER
				return member.HasMetadataToken();
#else
				try
				{
					var token = member.MetadataToken; return true;
				}
				catch (InvalidOperationException)
				{
					return false;
				}
#endif
			}

			private static int GetMetadataToken(MemberInfo? member)
			{
				return HasMetadataToken(member) ? member!.MetadataToken : int.MaxValue;
			}

			public override int Compare(MemberInfo? x, MemberInfo? y)
			{
				if (x == y) return 0;

				if (x == null) return 1;

				if (y == null) return -1;

				// Get metadata tokens for the types that declared the members.
				var xTypeToken = GetMetadataToken(x.DeclaringType);
				var yTypeToken = GetMetadataToken(y.DeclaringType);

				if (xTypeToken != yTypeToken)
				{
					// Members were declared in different types. Find the _typeOrder indices for
					// the types so that we can identify which one we consider to be the
					// least-derived type.
					var xIndex = Array.IndexOf(_typeOrder, xTypeToken);
					var yIndex = Array.IndexOf(_typeOrder, yTypeToken);

					if (xIndex < 0 && yIndex < 0) return Comparer<int>.Default.Compare(xTypeToken, yTypeToken);

					if (xIndex < 0) return 1;

					if (yIndex < 0) return -1;

					return Comparer<int>.Default.Compare(xIndex, yIndex);
				}

				// Members were declared in the same type. Use the metadata tokens for the members
				// to determine the sort order.
				var xToken = GetMetadataToken(x);
				var yToken = GetMetadataToken(y);

				return Comparer<int>.Default.Compare(xToken, yToken);
			}

		}

		/// <summary>
		/// The CLR type currently being processed.
		/// </summary>
		public Type Type { get; }
		/// <summary>
		/// The set of attributes.  Will be populated when an attribute has a property.
		/// </summary>
		public List<Attribute> Attributes { get; }
		/// <summary>
		/// The current set of keyword intents.
		/// </summary>
		public List<ISchemaKeywordIntent> Intents { get; } = new List<ISchemaKeywordIntent>();
		/// <summary>
		/// The generator configuration.
		/// </summary>
		public SchemaGeneratorConfiguration Configuration { get; }
		/// <summary>
		/// <see cref="IComparer{MemberInfo}"/> for ordering members when generating a schema with <see cref="PropertyOrder.AsDeclared"/> ordering enabled.
		/// </summary>
		private IComparer<MemberInfo> _memberInfoComparer;
		internal IComparer<MemberInfo> AsDeclaredMemberInfoComparer => _memberInfoComparer ??= new MemberInfoMetadataTokenComparer(Type);

		internal SchemaGeneratorContext(Type type, List<Attribute> attributes, SchemaGeneratorConfiguration configuration)
		{
			Type = type;
			Attributes = attributes;
			Configuration = configuration;
			AsDeclaredMemberInfoComparer = new Lazy<IComparer<MemberInfo>>(() => MemberInfoMetadataTokenComparer.ForType(Type));
		}

		internal void GenerateIntents()
		{
			var generator = Configuration.Generators.FirstOrDefault(x => x.Handles(Type)) ?? GeneratorRegistry.Get(Type);
			generator?.AddConstraints(this);

			AttributeHandler.HandleAttributes(this);

			var refiners = Configuration.Refiners.ToList();
			refiners.Add(NullabilityRefiner.Instance);
			foreach (var refiner in refiners.Where(x => x.ShouldRun(this)))
			{
				refiner.Run(this);
			}
		}

		internal void Optimize()
		{
			var thisHash = GetHashCode();
			var allContexts = GetChildContexts();
			var defsByHashCode = allContexts.Where(g => g.Value.Count > 1 &&
			                                            (g.Value.Context.Intents.Count != 1 ||
			                                            !(g.Value.Context.Intents[0] is TypeIntent)))
				.ToDictionary(g => g.Key, g => g.Value.Context);

			var currentNames = new List<string>();
			var defs = new Dictionary<string, SchemaGeneratorContext>();
			var contextContainers = GetNestedContainers().ToList();
			foreach (var def in defsByHashCode)
			{
				var name = def.Value.GetDefName(currentNames);
				var refIntent = new RefIntent(new Uri(def.Key == thisHash ? "#" : $"#/$defs/{name}", UriKind.Relative));
				var refContext = new SchemaGeneratorContext(def.Value.Type, null!, Configuration);
				refContext.Intents.Add(refIntent);
				foreach (var intent in contextContainers)
				{
					intent.Replace(def.Key, refContext);
				}
				if (def.Key == thisHash) continue;
				defs[name] = def.Value;
			}

			if (defs.Any())
			{
				var defsIntent = new DefsIntent(defs);
				Intents.Add(defsIntent);
			}
		}

		private IEnumerable<IContextContainer> GetNestedContainers()
		{
			var contextContainers = Intents.OfType<IContextContainer>().ToList();
			var i = 0;

			while (i < contextContainers.Count)
			{
				var current = contextContainers[i];
				var unknownContainers = current.GetContexts()
					.SelectMany(c => c.Intents.OfType<IContextContainer>())
					.Except(contextContainers);
				contextContainers.AddRange(unknownContainers);
				i++;
			}

			return contextContainers;
		}

		private string GetDefName(List<string> currentNames)
		{
			var name = GetName(Type);
			var regex = new Regex($@"^{name}\d*$");
			var count = currentNames.Count(n => regex.IsMatch(n));
			if (count != 0)
				name += count;

			currentNames.Add(name);
			return name;
		}

		private static string GetName(Type type)
		{
			if (type.IsInteger()) return "integer";
			if (type.IsNumber()) return "number";
			if (type == typeof(string)) return "string";
			if (type.IsArray()) return "array";
			if (type == typeof(bool)) return "boolean";
			return type.Name;
		}

		private Dictionary<int, ContextCount> GetChildContexts()
		{
			var contextsToCheck = new List<SchemaGeneratorContext>{this};
			var contextsReceived = new Dictionary<int, ContextCount>();
			while (contextsToCheck.Any())
			{
				var context = contextsToCheck[0];
				contextsToCheck.RemoveAt(0);

				var hash = context.GetHashCode();
				if (contextsReceived.TryGetValue(hash, out var count))
				{
					count.Count++;
					continue;
				}

				contextsToCheck.AddRange(context.Intents.OfType<IContextContainer>().SelectMany(i => i.GetContexts()));
				contextsReceived[hash] = new ContextCount(context);
			}

			return contextsReceived;
		}

		/// <summary>
		/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
		/// </summary>
		/// <param name="builder">The schema builder.</param>
		/// <returns>The schema builder (for fluent syntax support).</returns>
		public JsonSchemaBuilder Apply(JsonSchemaBuilder? builder = null)
		{
			builder ??= new JsonSchemaBuilder();

			foreach (var intent in Intents)
			{
				intent.Apply(builder);
			}

			return builder;
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			var other = (SchemaGeneratorContext) obj;
			return Type == other.Type &&
				   Intents.ContentsEqual(other.Intents);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Type.GetHashCode();
				hashCode = (hashCode * 397) ^ (Attributes?.GetAttributeSetHashCode() ?? 0);
				return hashCode;
			}
		}
	}
}
