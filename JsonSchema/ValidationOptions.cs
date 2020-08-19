using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema
{
	public class ValidationOptions
	{
		public static ValidationOptions Default { get; } = new ValidationOptions();

		public Draft ValidateAs { get; set; }

		internal IEnumerable<IJsonSchemaKeyword> FilterKeywords(IEnumerable<IJsonSchemaKeyword> keywords, Uri metaSchemaId, SchemaRegistry registry)
		{
			while (metaSchemaId != null)
			{
				if (metaSchemaId == MetaSchemas.Draft6_Id || metaSchemaId == MetaSchemas.Draft7_Id)
					return DisallowSiblingRef(keywords);
				if (metaSchemaId == MetaSchemas.Draft2019_09_Id)
					return AllowSiblingRef(keywords);
				var metaSchema = registry.Get(metaSchemaId);
				if (metaSchema == null) return ByOption(keywords);
				metaSchemaId = metaSchema.Keywords.OfType<SchemaKeyword>().FirstOrDefault()?.Schema;
			}

			return ByOption(keywords);
		}

		private IEnumerable<IJsonSchemaKeyword> ByOption(IEnumerable<IJsonSchemaKeyword> keywords)
		{
			switch (ValidateAs)
			{
				case Draft.Draft6:
				case Draft.Draft7:
					return DisallowSiblingRef(keywords);
				case Draft.Unspecified:
				case Draft.Draft2019_09:
				default:
					return AllowSiblingRef(keywords);
			}
		}

		private static IEnumerable<IJsonSchemaKeyword> DisallowSiblingRef(IEnumerable<IJsonSchemaKeyword> keywords)
		{
			var refKeyword = keywords.OfType<RefKeyword>().SingleOrDefault();

			return refKeyword != null ? new[] {refKeyword} : keywords;
		}

		private static IEnumerable<IJsonSchemaKeyword> AllowSiblingRef(IEnumerable<IJsonSchemaKeyword> keywords)
		{
			return keywords;
		}
	}
}