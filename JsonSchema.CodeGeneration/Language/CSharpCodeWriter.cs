using System;
using System.Text;
using Humanizer;
using Json.Schema.CodeGeneration.Model;

namespace Json.Schema.CodeGeneration.Language;

internal class CSharpCodeWriter : ICodeWriter
{
	public string? TransformName(string? original) => original?.Underscore().Pascalize();

	public void Write(StringBuilder builder, TypeModel model)
	{
		WriteDeclaration(builder, model);
	}

	private void WriteUsage(StringBuilder builder, TypeModel model)
	{
		if (model.IsSimple)
		{
			if (ReferenceEquals(model, CommonModels.String))
				builder.Append("string");
			else if (ReferenceEquals(model, CommonModels.Integer))
				builder.Append("int");
			else if (ReferenceEquals(model, CommonModels.Number))
				builder.Append("double");
			else if (ReferenceEquals(model, CommonModels.Boolean))
				builder.Append("bool");
			else
				throw new ArgumentOutOfRangeException(nameof(model));
			return;
		}

		if (model.Name != null)
		{
			builder.Append(TransformName(model.Name));
			return;
		}

		// only arrays and dictionaries can opt out of names
		switch (model)
		{
			case ArrayModel arrayModel:
				WriteUsage(builder, arrayModel);
				break;
			case DictionaryModel dictionaryModel:
				WriteUsage(builder, dictionaryModel);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(model));
		}
	}

	private void WriteDeclaration(StringBuilder builder, TypeModel model)
	{
		if (model.Name == null) return;
		if (model.IsSimple) return;

		switch (model)
		{
			case EnumModel enumModel:
				WriteDeclaration(builder, enumModel);
				break;
			case ArrayModel arrayModel:
				WriteDeclaration(builder, arrayModel);
				break;
			case ObjectModel objectModel:
				WriteDeclaration(builder, objectModel);
				break;
			case DictionaryModel dictionaryModel:
				WriteDeclaration(builder, dictionaryModel);
				break;
			default:
				if (ReferenceEquals(model, CommonModels.String))
					builder.Append(CommonModels.String.Name);
				else if (ReferenceEquals(model, CommonModels.Integer))
					builder.Append(CommonModels.Integer.Name);
				else if (ReferenceEquals(model, CommonModels.Number))
					builder.Append(CommonModels.Number.Name);
				else if (ReferenceEquals(model, CommonModels.Boolean))
					builder.Append(CommonModels.Boolean.Name);
				else
					throw new ArgumentOutOfRangeException(nameof(model));
				break;
		}
	}

	private void WriteDeclaration(StringBuilder builder, EnumModel model)
	{
		void WriteValue(EnumValue value)
		{
			builder.Append("\t");
			builder.Append(TransformName(value.Name));
			builder.Append(" = ");
			builder.Append(value.Value);
		}

		builder.Append("public enum ");
		builder.AppendLine(TransformName(model.Name));
		builder.AppendLine("{");
		for (var i = 0; i < model.Values.Length - 1; i++)
		{
			var value = model.Values[i];
			WriteValue(value);
			builder.AppendLine(",");
		}
		WriteValue(model.Values[model.Values.Length - 1]);
		builder.AppendLine();
		builder.AppendLine("}");
	}

	private void WriteUsage(StringBuilder builder, ArrayModel model)
	{
		WriteUsage(builder, model.Items);
		builder.Append("[]");
	}

	private void WriteDeclaration(StringBuilder builder, ArrayModel model)
	{
		builder.Append("public class ");
		builder.Append(TransformName(model.Name));
		builder.Append(" : List<");
		WriteUsage(builder, model.Items);
		builder.AppendLine(">");
		builder.AppendLine("{");
		builder.AppendLine("}");
	}

	private void WriteDeclaration(StringBuilder builder, ObjectModel model)
	{
		builder.Append("public ");
		if (!model.IsOpen)
			builder.Append("sealed ");
		builder.Append("class ");
		builder.AppendLine(TransformName(model.Name));
		builder.AppendLine("{");
		foreach (var property in model.Properties)
		{
			var propertyName = TransformName(property.Name);
			if (propertyName != property.Name)
			{
				builder.Append("\t[JsonPropertyName(\"");
				builder.Append(property.Name);
				builder.AppendLine("\")]");
			}
			builder.Append("\tpublic ");
			WriteUsage(builder, property.Type);
			builder.Append(" ");
			builder.Append(TransformName(property.Name));
			builder.Append(" { ");
			if (property.CanRead)
				builder.Append("get; ");
			if (property.CanWrite)
				builder.Append("set; ");
			builder.AppendLine("}");
		}
		builder.AppendLine("}");
	}

	private void WriteUsage(StringBuilder builder, DictionaryModel model)
	{
		builder.Append("Dictionary<");
		WriteUsage(builder, model.Keys);
		builder.Append(", ");
		WriteUsage(builder, model.Items);
		builder.Append(">");
	}

	private void WriteDeclaration(StringBuilder builder, DictionaryModel model)
	{
		builder.Append("public class ");
		builder.Append(TransformName(model.Name));
		builder.Append(" : Dictionary<");
		WriteUsage(builder, model.Keys);
		builder.Append(", ");
		WriteUsage(builder, model.Items);
		builder.AppendLine(">");
		builder.AppendLine("{");
		builder.AppendLine("}");
	}
}