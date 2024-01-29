using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Json.More;

public class TypeResolverOptionsManager
{
	private readonly JsonSerializerOptions _baseOptions;
	private JsonSerializerOptions? _serializerOptions;
#if NET8_0_OR_GREATER
	private readonly IJsonTypeInfoResolver _baseResolver;
	private IJsonTypeInfoResolver _typeInfoResolver;
#endif
	private readonly object _serializerOptionsLock = new();
	
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

	public TypeResolverOptionsManager(JsonSerializerOptions? baseOptions = null)
	{
		_baseOptions = baseOptions ?? new JsonSerializerOptions();
	}

#if NET8_0_OR_GREATER
	public IJsonTypeInfoResolver TypeInfoResolver => _typeInfoResolver;

	public event EventHandler TypeInfoResolverUpdated;
	
	public TypeResolverOptionsManager(IJsonTypeInfoResolver baseResolver, params IJsonTypeInfoResolver[] resolvers)
	{
		_baseOptions = new JsonSerializerOptions();
		_baseResolver = baseResolver;
		_typeInfoResolver = JsonTypeInfoResolver.Combine([baseResolver, .. resolvers]);
	}
	
	public TypeResolverOptionsManager(JsonSerializerOptions baseOptions, IJsonTypeInfoResolver baseResolver, params IJsonTypeInfoResolver[] resolvers)
	{
		_baseOptions = baseOptions;
		_baseResolver = baseResolver;
		_typeInfoResolver = JsonTypeInfoResolver.Combine([baseResolver, .. resolvers]);
	}

	public void RebuildTypeResolver(params IJsonTypeInfoResolver[] resolvers)
	{
		lock (_serializerOptionsLock)
		{
			_typeInfoResolver = JsonTypeInfoResolver.Combine([_baseResolver, .. resolvers]);
			_serializerOptions = null;
		}

		TypeInfoResolverUpdated?.Invoke(this, EventArgs.Empty);
	}
#endif
}