using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Json.More;

/// <summary>
/// Manages a <see cref="JsonSerializer"/> object that incorporates the type resolvers
/// found in a <see cref="JsonSerializerContext"/>.
/// </summary>
public class TypeResolverOptionsManager
{
	private readonly JsonSerializerOptions _baseOptions;
	private JsonSerializerOptions? _serializerOptions;
#if NET8_0_OR_GREATER
	private readonly IJsonTypeInfoResolver _baseResolver;
	private IJsonTypeInfoResolver _typeInfoResolver;
#endif
	private readonly object _serializerOptionsLock = new();
	
	/// <summary>
	/// Gets the serializer options.
	/// </summary>
	public JsonSerializerOptions SerializerOptions
	{
		get
		{
			lock (_serializerOptionsLock)
			{
				_serializerOptions ??= new JsonSerializerOptions(_baseOptions)
				{
#if NET8_0_OR_GREATER
					TypeInfoResolver = _typeInfoResolver
#endif
				};

				return _serializerOptions!;
			}
		}
	}

#if NET8_0_OR_GREATER
	/// <summary>
	/// Gets the type info resolver for the associated context.
	/// </summary>
	public IJsonTypeInfoResolver TypeInfoResolver => _typeInfoResolver;

	/// <summary>
	/// Raised when the type info resolver is updated.  (See <see cref="RebuildTypeResolver"/>)
	/// </summary>
	public event EventHandler? TypeInfoResolverUpdated;
	
	/// <summary>
	/// Creates a new instance of the <see cref="TypeResolverOptionsManager"/> class.
	/// </summary>
	/// <param name="baseResolver">
	/// The base resolver.  This will generally be the `Default` property on your
	/// <see cref="JsonSerializerContext"/>.
	/// </param>
	/// <param name="resolvers">Any additional resolvers to be included.</param>
	public TypeResolverOptionsManager(IJsonTypeInfoResolver baseResolver, params IJsonTypeInfoResolver[] resolvers)
	{
		_baseOptions = new JsonSerializerOptions();
		_baseResolver = baseResolver;
		_typeInfoResolver = JsonTypeInfoResolver.Combine([baseResolver, .. resolvers]);
	}

	/// <summary>
	/// Creates a new instance of the <see cref="TypeResolverOptionsManager"/> class
	/// that includes a custom options object to use as a base.
	/// </summary>
	/// <param name="baseOptions">The base options.</param>
	/// <param name="baseResolver">
	/// The base resolver.  This will generally be the `Default` property on your
	/// <see cref="JsonSerializerContext"/>.
	/// </param>
	/// <param name="resolvers">Any additional resolvers to be included.</param>
	public TypeResolverOptionsManager(JsonSerializerOptions baseOptions, IJsonTypeInfoResolver baseResolver, params IJsonTypeInfoResolver[] resolvers)
	{
		_baseOptions = baseOptions;
		_baseResolver = baseResolver;
		_typeInfoResolver = JsonTypeInfoResolver.Combine([baseResolver, .. resolvers]);
	}

	/// <summary>
	/// Rebuilds the type resolver with a new set of resolvers.
	/// </summary>
	/// <param name="resolvers">The resolvers to incorporate</param>
	/// <remarks>
	/// This should be built with the same set of resolvers that the options manager was
	/// created with, except for the base resolver.
	/// </remarks>
	public void RebuildTypeResolver(params IJsonTypeInfoResolver[] resolvers)
	{
		lock (_serializerOptionsLock)
		{
			_typeInfoResolver = JsonTypeInfoResolver.Combine([_baseResolver, .. resolvers]);
			_serializerOptions = null;
		}

		TypeInfoResolverUpdated?.Invoke(this, EventArgs.Empty);
	}
#else
	/// <summary>
	/// Creates a new instance with optional base options.
	/// </summary>
	/// <param name="baseOptions">(optional) The base options.</param>
	public TypeResolverOptionsManager(JsonSerializerOptions? baseOptions = null)
	{
		_baseOptions = baseOptions ?? new JsonSerializerOptions();
	}
#endif
}