using System.Reflection;

namespace JsonEverythingNet.Services.MarkdownGen.Reflection
{
    /// <summary>
    /// Settings used by TypeCollection to retrieve reflection info.
    /// </summary>
    public class ReflectionSettings
    {
        /// <summary>
        /// Default reflection settings.
        /// </summary>
        public static ReflectionSettings Default => new()
        {
            PropertyFlags = 
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.Static,

            MethodFlags =
                BindingFlags.DeclaredOnly |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.Static,

            FieldFlags =
                BindingFlags.DeclaredOnly |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.Static,

            NestedTypeFlags =
                BindingFlags.DeclaredOnly |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.Static
        };

        /// <summary>
        /// Binding flags to use when retrieving properties of the type.
        /// </summary>
        public BindingFlags PropertyFlags { get; set; }
        /// <summary>
        /// Binding flags to use when retrieving methods of the type.
        /// </summary>
        public BindingFlags MethodFlags { get; set; }
        /// <summary>
        /// Binding flags to use when retrieving fields of the type.
        /// </summary>
        public BindingFlags FieldFlags { get; set; }
        /// <summary>
        /// Binding flags to use when retrieving nested types of the type.
        /// </summary>
        public BindingFlags NestedTypeFlags { get; set; }

        /// <summary>
        /// Function that checks if specified types of assembly should be added to the set of the 
        /// referenced types.
        /// Return true if referenced types of the assembly should be examined.
        /// Return false if assembly types should be ignored.
        /// Default implementation checks if documentation XML file exists for the assembly and if
        /// it does then returns true.
        /// </summary>
        public Func<Assembly, bool> AssemblyFilter { get; set; } =
            (assembly) => File.Exists(Path.ChangeExtension(assembly.Location, ".xml"));

        /// <summary>
        /// Checks if specified type should be added to the set of referenced types.
        /// Return true if type and types referenced by it should be examined.
        /// Function should return false if type should be ignored.
        /// Default implementation returns true for all types.
        /// </summary>
        public Func<Type,bool> TypeFilter { get; set; }

        /// <summary>
        /// Checks if specified property should be added to the list of properties and the
        /// set of referenced types.
        /// Return true if property and types referenced by it should be examined.
        /// Function should return false if property should be ignored.
        /// Default implementation returns true for all properties.
        /// </summary>
        public Func<PropertyInfo, bool> PropertyFilter { get; set; }
        /// <summary>
        /// Checks if specified method should be added to the list of methods and the
        /// set of referenced types.
        /// Return true if the method and types referenced by it should be examined.
        /// Function should return false if method should be ignored.
        /// Default implementation returns true for all methods.
        /// </summary>
        public Func<MethodBase, bool> MethodFilter { get; set; }
        /// <summary>
        /// Checks if specified field should be added to the list of fields and the
        /// set of referenced types.
        /// Return true if field and types referenced by it should be examined.
        /// Function should return false if field should be ignored.
        /// Default implementation returns true for all fields.
        /// </summary>
        public Func<FieldInfo, bool> FieldFilter { get; set; }
    }
}
