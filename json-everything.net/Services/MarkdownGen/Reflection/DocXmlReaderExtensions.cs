using System.Reflection;

namespace JsonEverythingNet.Services.MarkdownGen.Reflection
{
    /// <summary>
    /// DocXmlReader extension methods to retrieve type properties, methods, and fields
    /// using reflection information.
    /// </summary>
    public static class DocXmlReaderExtensions
    {
        /// <summary>
        /// Get comments for the collection of properties.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="propInfos"></param>
        /// <returns></returns>
        public static IEnumerable<(PropertyInfo Info, CommonComments Comments)>
            Comments(this DocXmlReader reader, IEnumerable<PropertyInfo> propInfos)
        {
            return propInfos.Select(info => (info, reader.GetMemberComments(info)));
        }
        /// <summary>
        /// Get comments for the collection of methods.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="methodInfos"></param>
        /// <returns></returns>
        public static IEnumerable<(MethodBase Info, MethodComments Comments)>
            Comments(this DocXmlReader reader, IEnumerable<MethodBase> methodInfos)
        {
            return methodInfos
                .Select(info => 
                    (Info: info, Comments: reader.GetMethodComments(info, info.IsConstructor && info.GetParameters().Length == 0)))
                .Where(data => data.Comments != null);
        }
        /// <summary>
        /// Get comments for the collection of fields.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="fieldInfos"></param>
        /// <returns></returns>
        public static IEnumerable<(FieldInfo Info, CommonComments Comments)>
            Comments(this DocXmlReader reader, IEnumerable<FieldInfo> fieldInfos)
        {
            return fieldInfos.Select(info => (info, reader.GetMemberComments(info)));
        }
    }
}
