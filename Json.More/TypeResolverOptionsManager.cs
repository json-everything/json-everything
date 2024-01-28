using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Json.More;

public class TypeResolverOptionsManager
{
	private JsonSerializerOptions? _serializerOptions;
	private JsonSerializerOptions? _serializerOptionsUnsafeRelaxedJsonEscaping;
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
				_serializerOptions ??= new JsonSerializerOptions
				{
#if NET8_0_OR_GREATER
					TypeInfoResolver = _typeInfoResolver
#endif
				};

				return _serializerOptions!;
			}
		}
	}

	public JsonSerializerOptions SerializerOptionsUnsafeRelaxedJsonEscaping
	{
		get
		{
			lock (_serializerOptionsLock)
			{
				_serializerOptionsUnsafeRelaxedJsonEscaping ??= new JsonSerializerOptions
				{
					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
#if NET8_0_OR_GREATER
					TypeInfoResolver = _typeInfoResolver
#endif
				};

				return _serializerOptionsUnsafeRelaxedJsonEscaping!;
			}
		}
	}

#if NET8_0_OR_GREATER
	public IJsonTypeInfoResolver TypeInfoResolver => _typeInfoResolver;
	
	public TypeResolverOptionsManager(IJsonTypeInfoResolver baseResolver, params IJsonTypeInfoResolver[] resolvers)
	{
		_baseResolver = baseResolver;
		_typeInfoResolver = JsonTypeInfoResolver.Combine([baseResolver, .. resolvers]);
	}

	public void RebuildTypeResolver(params IJsonTypeInfoResolver[] resolvers)
	{
		lock (_serializerOptionsLock)
		{
			_typeInfoResolver = JsonTypeInfoResolver.Combine([_baseResolver, .. resolvers]);
			_serializerOptions = null;
			_serializerOptionsUnsafeRelaxedJsonEscaping = null;
		}
	}
#endif
}