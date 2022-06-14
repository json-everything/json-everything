using System.Reflection;
using System.Runtime.CompilerServices;

// Disable warning for missing XML comments 
#pragma warning disable CS1591

namespace JsonEverythingNet.Services.MarkdownGen.Reflection
{
    /// <summary>
    /// Collection of type information objects.
    /// </summary>
    public class TypeCollection
    {
        /// <summary>
        /// Reflection information for the class, its methods, properties and fields. 
        /// </summary>
        public class TypeInformation
        {
            /// <summary>
            /// The type that this class describes
            /// </summary>
            public Type Type { get; set; }
            /// <summary>
            /// Other types referencing this type.
            /// </summary>
            public HashSet<Type> ReferencesIn { get; set; } = new HashSet<Type>();
            /// <summary>
            /// Other types referenced by this type.
            /// </summary>
            public HashSet<Type> ReferencesOut { get; set; } = new HashSet<Type>();
            /// <summary>
            /// The list of property inforation of the class.
            /// </summary>
            public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();
            /// <summary>
            /// The list of method inforation of the class.
            /// </summary>
            public List<MethodBase> Methods { get; set; } = new List<MethodBase>();
            /// <summary>
            /// The list of field inforation of the class.
            /// </summary>
            public List<FieldInfo> Fields { get; set; } = new List<FieldInfo>();
        }

        /// <summary>
        /// Reflection settings that should be used when looking for referenced types.
        /// </summary>
        public ReflectionSettings Settings { get; set; } = ReflectionSettings.Default;

        /// <summary>
        /// All referenced types.
        /// </summary>
        public Dictionary<Type, TypeInformation> ReferencedTypes { get; set; } 
            = new Dictionary<Type, TypeInformation>();
        
        /// <summary>
        /// Types that had their data and functions examined.
        /// </summary>
        protected HashSet<Type> VisitedPropTypes { get; set; } = new HashSet<Type>();
        /// <summary>
        /// Types that need to have their properties, methods and fields examined.
        /// </summary>
        protected Queue<Type> PendingPropTypes { get; set; } = new Queue<Type>();

        /// <summary>
        /// Cached information from ExamineAssemblies call.
        /// Contains the set of assemblies that should be checked or ignored.
        /// </summary>
        protected Dictionary<Assembly, bool> CheckAssemblies { get; set; } = new Dictionary<Assembly, bool>();
        /// <summary>
        /// Cached information from the ExamineTypes call.
        /// Contains the set of types that should be ignored.
        /// </summary>
        protected HashSet<Type> IgnoreTypes { get; set; } = new HashSet<Type>();

        /// <summary>
        /// Get all types referenced by the specified type.
        /// Reflection information for the specified type is also returned.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static TypeCollection ForReferencedTypes(Type type, ReflectionSettings settings = null)
        {
            var typeCollection = new TypeCollection();
            typeCollection.GetReferencedTypes(type, settings);
            return typeCollection;
        }

        /// <summary>
        /// Get all types referenced by the types from specified assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static TypeCollection ForReferencedTypes(Assembly assembly, ReflectionSettings settings = null)
        {
            var typeCollection = new TypeCollection();
            typeCollection.GetReferencedTypes(assembly, settings);
            return typeCollection;
        }

        /// <summary>
        /// Get all types referenced by the types from the list of assemblies.
        /// </summary>
        /// <param name="assemblies"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static TypeCollection ForReferencedTypes(IEnumerable<Assembly> assemblies, ReflectionSettings settings = null)
        {
            var typeCollection = new TypeCollection();
            typeCollection.GetReferencedTypes(assemblies, settings);
            return typeCollection;
        }

        /// <summary>
        /// Get all types referenced by the specified type.
        /// Reflection information for the specified type is also returned.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="settings"></param>
        public void GetReferencedTypes(Type type, ReflectionSettings settings = null)
        {
            Init(settings);
            PendingPropTypes.Enqueue(type);
            ProcessTypeQueue();
        }

        /// <summary>
        /// Get all types referenced by the types from specified assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="settings"></param>
        public void GetReferencedTypes(Assembly assembly, ReflectionSettings settings = null)
        {
            Init(settings);
            foreach (var type in assembly.GetTypes())
            {
                PendingPropTypes.Enqueue(type);
            }
            ProcessTypeQueue();
        }

        /// <summary>
        /// Get all types referenced by the types from specified assemblies.
        /// Reflection information for the specified type is also returned.
        /// </summary>
        /// <param name="assemblies"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public void GetReferencedTypes(IEnumerable<Assembly> assemblies, ReflectionSettings settings = null)
        {
            Init(settings);
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    PendingPropTypes.Enqueue(type);
                }
            }
            ProcessTypeQueue();
        }

        protected void Init(ReflectionSettings settings)
        {
            Settings = settings ?? ReflectionSettings.Default;
            ReferencedTypes = new Dictionary<Type, TypeInformation>();
            VisitedPropTypes = new HashSet<Type>();
            PendingPropTypes = new Queue<Type>();
            CheckAssemblies = new Dictionary<Assembly, bool>();
            IgnoreTypes = new HashSet<Type>();
        }

        protected void ProcessTypeQueue()
        {
            while (PendingPropTypes.Count > 0)
            {
                var theType = PendingPropTypes.Dequeue();
                UnwrapType(null, theType);
                GetReferencedBy(theType);
            }
        }

        /// <summary>
        /// Returns true for types that should be checked.
        /// </summary>
        /// <param name="type">Check this type?</param>
        /// <returns></returns>
        bool CheckType(Type type)
        {
	        return true;
            if (IgnoreTypes.Contains(type)) return false;
            // Check if type assembly should be checked or ignored
            if (Settings.AssemblyFilter != null)
            {
                if (!CheckAssemblies.ContainsKey(type.Assembly))
                {
                    CheckAssemblies.Add(type.Assembly, Settings.AssemblyFilter(type.Assembly));
                }
                if (!CheckAssemblies[type.Assembly]) return false;
            }
            // Ignore compiler-generated types
            // If we have filtering function then ask if type is OK
            if (!IsCompilerGenerated(type) &&
                (type.DeclaringType == null || 
                type.DeclaringType.Name != "<PrivateImplementationDetails>") &&
                (Settings.TypeFilter == null ||
                Settings.TypeFilter(type))) return true;
            IgnoreTypes.Add(type);
            return false;
        }

        protected void GetReferencedBy(Type type)
        {
            if (VisitedPropTypes.Contains(type) || !CheckType(type)) return;
            VisitedPropTypes.Add(type);
            var thisTypeInfo = ReferencedTypes[type];
            foreach (var info in type.GetProperties(Settings.PropertyFlags))
            {
                if (Settings.PropertyFilter != null && !Settings.PropertyFilter(info)) continue;
                thisTypeInfo.Properties.Add(info);
                UnwrapType(type, info.PropertyType);
                if (info.GetMethod?.GetParameters()?.Length > 0)
                {
                    UnwrapType(type, info.GetMethod.GetParameters()[0].ParameterType);
                }
                else if (info.SetMethod?.GetParameters()?.Length > 1)
                {
                    UnwrapType(type, info.SetMethod.GetParameters()[1].ParameterType);
                }
            }
            foreach (var info in type.GetMethods(Settings.MethodFlags))
            {
                if (info.IsSpecialName) continue;
                if (Settings.MethodFilter != null && !Settings.MethodFilter(info)) continue;
                thisTypeInfo.Methods.Add(info);
                UnwrapType(type, info.ReturnType);
                if (!(info.GetParameters()?.Length > 0)) continue;
                foreach(var parameter in info.GetParameters()) UnwrapType(type, parameter.ParameterType);
            }
            foreach (var info in type.GetConstructors(Settings.MethodFlags))
            {
                if (Settings.MethodFilter != null && !Settings.MethodFilter(info)) continue;
                thisTypeInfo.Methods.Add(info);
                if (!(info.GetParameters()?.Length > 0)) continue;
                foreach (var parameter in info.GetParameters()) UnwrapType(type, parameter.ParameterType);
            }
            foreach (var info in type.GetFields(Settings.FieldFlags))
            {
                if (IsCompilerGenerated(info)) continue;
                if (Settings.FieldFilter != null && !Settings.FieldFilter(info)) continue;
                thisTypeInfo.Fields.Add(info);
                UnwrapType(type, info.FieldType);
            }
            foreach (var info in type.GetNestedTypes(Settings.NestedTypeFlags))
            {
                UnwrapType(type, info);
            }
        }

        /// <summary>
        /// Recursively "unwrap" the generic type or array. If type is not generic and not an array
        /// then do nothing.
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="type"></param>
        public void UnwrapType(Type parentType, Type type)
        {
            if (ReferencedTypes.ContainsKey(type))
            {
                if (parentType == null) return;
                ReferencedTypes[type].ReferencesIn.Add(parentType);
                ReferencedTypes[parentType].ReferencesOut.Add(type);
                return;
            }
            // Some types could be wrapped in generic types that should not be checked
            var checkType = CheckType(type);

            if (type.IsConstructedGenericType) // List<int>
            {
                UnwrapType(parentType, type.GetGenericTypeDefinition());
                if (!(type.GenericTypeArguments?.Length > 0)) return;
                foreach (var argType in type.GenericTypeArguments) UnwrapType(parentType, argType);
            }
            else if (type.IsGenericParameter)  // void Method<T>()   <-- T in generic class
            {
                return;
            }
            else if (type.IsGenericTypeDefinition) // List<>
            {
                if (checkType) AddTypeToCheckProps(parentType, type);
            }
            else if (type.IsGenericType) // List<int>
            {
                if (type.ContainsGenericParameters)
                {
                    foreach (var argType in type.GenericTypeArguments) UnwrapType(parentType, argType);
                }
                return;
            }
            else if (type.IsArray || type.IsByRef) // SomeType[] or ref SomeType
            {
                UnwrapType(parentType, type.GetElementType());
            }
            else
            {
                if (checkType) AddTypeToCheckProps(parentType, type);
            }
        }

        void AddTypeToCheckProps(Type parentType, Type type)
        {
            var newRef = new TypeInformation() { Type = type };
            newRef.ReferencesIn.Add(parentType);
            if (parentType != null) ReferencedTypes[parentType].ReferencesOut.Add(type);
            ReferencedTypes.Add(type, newRef);
            PendingPropTypes.Enqueue(type);
        }

        bool IsCompilerGenerated(Type type)
        {
            return type.Name.Contains('<') ||
                   type.CustomAttributes.Any(attr => attr.AttributeType == typeof(CompilerGeneratedAttribute));
        }
        bool IsCompilerGenerated(FieldInfo fieldInfo)
        {
            return fieldInfo.FieldType.Name.Contains('<') ||
                   fieldInfo.CustomAttributes.Any(attr => attr.AttributeType == typeof(CompilerGeneratedAttribute));
        }
    }
}
