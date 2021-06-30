using Json.Schema.Generation.Intents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Json.Schema.Generation.Generators
{
	internal class ObjectSchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return true;
		}

		public void AddConstraints(SchemaGeneratorContext context)
		{
			context.Intents.Add(new TypeIntent(SchemaValueType.Object));

			var props = new Dictionary<string, SchemaGeneratorContext>();
			var required = new List<string>();
			var propertiesToGenerate = context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.CanRead && p.CanWrite);
			var fieldsToGenerate = context.Type.GetFields(BindingFlags.Public | BindingFlags.Instance);
			var hiddenPropertiesToGenerate = context.Type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(p => p.GetCustomAttribute<JsonIncludeAttribute>() != null);
			var hiddenFieldsToGenerate = context.Type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(p => p.GetCustomAttribute<JsonIncludeAttribute>() != null);
			var membersToGenerate = propertiesToGenerate.Cast<MemberInfo>()
				.Concat(fieldsToGenerate)
				.Concat(hiddenPropertiesToGenerate)
				.Concat(hiddenFieldsToGenerate);

			membersToGenerate = context.Configuration.PropertyOrder switch
			{
				PropertyOrder.AsDeclared => membersToGenerate.OrderBy(m => m, MemberInfoMetadataTokenComparer.ForType(context.Type)),
				PropertyOrder.ByName => membersToGenerate.OrderBy(m => m.Name),
				_ => membersToGenerate
			};

			foreach (var member in membersToGenerate)
			{
				var memberAttributes = member.GetCustomAttributes().ToList();
				var ignoreAttribute = memberAttributes.OfType<JsonIgnoreAttribute>().FirstOrDefault();
				if (ignoreAttribute != null) continue;

				var memberContext = SchemaGenerationContextCache.Get(member.GetMemberType(), memberAttributes, context.Configuration);

				var name = context.Configuration.PropertyNamingMethod(member.Name);
				var nameAttribute = memberAttributes.OfType<JsonPropertyNameAttribute>().FirstOrDefault();
				if (nameAttribute != null)
					name = nameAttribute.Name;

				if (memberAttributes.OfType<ObsoleteAttribute>().Any())
					memberContext.Intents.Add(new DeprecatedIntent(true));

				props.Add(name, memberContext);

				if (memberAttributes.OfType<RequiredAttribute>().Any())
					required.Add(name);
			}


			if (props.Count > 0)
			{
				context.Intents.Add(new PropertiesIntent(props));

				if (required.Count > 0)
					context.Intents.Add(new RequiredIntent(required));
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
				if (member == null)
				{
					return false;
				}

#if NET5_0_OR_GREATER
				return member.HasMetadataToken();
#else
				try { var token = member.MetadataToken; return true; }
				catch (InvalidOperationException) { return false; }
#endif
			}

			private static int GetMetadataToken(MemberInfo? member)
			{
				return HasMetadataToken(member) ? member!.MetadataToken : int.MaxValue;
			}

			public override int Compare(MemberInfo? x, MemberInfo? y)
			{
				if (x == y)
				{
					return 0;
				}

				if (x == null)
				{
					return 1;
				}

				if (y == null)
				{
					return -1;
				}

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

					if (xIndex < 0 && yIndex < 0)
					{
						return Comparer<int>.Default.Compare(xTypeToken, yTypeToken);
					}

					if (xIndex < 0)
					{
						return 1;
					}

					if (yIndex < 0)
					{
						return -1;
					}

					return Comparer<int>.Default.Compare(xIndex, yIndex);
				}

				// Members were declared in the same type. Use the metadata tokens for the members
				// to determine the sort order.
				var xToken = GetMetadataToken(x);
				var yToken = GetMetadataToken(y);

				return Comparer<int>.Default.Compare(xToken, yToken);
			}

		}

	}
}