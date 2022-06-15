using System.Reflection;
using System.Text;
using JsonEverythingNet.Services.MarkdownGen.Reflection;

namespace JsonEverythingNet.Services.MarkdownGen
{
	public static class CodegenExtensions
	{
		private const string _static = "static ";
		private const string _protected = "protected ";
		private const string _public = "public ";
		private const string _abstract = "abstract ";
		private const string _sealed = "sealed ";
		private const string _virtual = "virtual ";
		private const string _override = "override ";
		private const string _delegate = "delegate ";

		public static string GenerateCode(this MethodInfo method)
		{
			var sb = new StringBuilder();
		
			if (method.IsPublic) sb.Append(_public);
			else if (method.IsFamily && !method.IsFamilyAndAssembly) sb.Append(_protected);

			if (method.IsStatic) sb.Append(_static);
			else
			{
				// final and virtual means interface implementation and not virtual
				if (method.GetBaseDefinition().DeclaringType != method.DeclaringType)
				{
					if (method.IsFinal && !method.IsVirtual) sb.Append(_sealed);
					sb.Append(_override);
				}
				else if (method.IsAbstract) sb.Append(_abstract);
				else if (method.IsVirtual && !method.IsFinal) sb.Append(_virtual);
			}

			sb.Append(method.ToTypeNameString()); // return type
			sb.Append(' ');
			sb.Append(method.Name);
			sb.Append(method.ToParametersString());

			return sb.ToString();
		}

		public static string GenerateCode(this ConstructorInfo method)
		{
			var sb = new StringBuilder();
		
			if (method.IsPublic) sb.Append(_public);
			else if (method.IsFamily && !method.IsFamilyAndAssembly) sb.Append(_protected);

			if (method.IsStatic) sb.Append(_static);
			else if (method.IsAbstract) sb.Append(_abstract);
			else if (method.IsVirtual && !method.IsFinal) sb.Append(_virtual);

			sb.Append(method.DeclaringType!.Name);
			sb.Append(method.ToParametersString());

			return sb.ToString();
		}

		public static (string, MethodInfo?) GenerateDelegateCode(this Type del, Func<Type, Queue<string?>?, string?> converter)
		{
			var sb = new StringBuilder();
		
			var methods = del.GetMethods();
			var invoke = methods.FirstOrDefault(x => x.Name == nameof(MethodInfo.Invoke));
			if (invoke != null)
			{
				var description = invoke.GenerateCode()
					.Replace("virtual ", _delegate)
					.Replace(nameof(MethodInfo.Invoke), del.ToNameString(converter));
				sb.Append(description);
			}

			return (sb.ToString(), invoke);
		}
	}
}
