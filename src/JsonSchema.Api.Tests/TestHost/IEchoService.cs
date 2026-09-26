namespace Json.Schema.Api.Tests.TestHost;

/// <summary>
/// A service injected into handlers, so discovery has to tell it apart from a request body.
/// </summary>
public interface IEchoService
{
	SimpleModel Echo(SimpleModel model);
}

public class EchoService : IEchoService
{
	public SimpleModel Echo(SimpleModel model) => model;
}
