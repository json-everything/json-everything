using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;

namespace Json.JsonE.Tests;

public class ClientTests
{
	[Test]
	public void Issue931_NegativeIndex()
	{
		var template = JsonNode.Parse(""" 
		                              { 
		                                 "$eval": "array[-1]"
		                              } 
		                              """);

		var context = JsonNode.Parse(""" 
		                             { 
		                                "array": [1]
		                             } 
		                             """);

		// throws InterpreterException "index out of bounds"
		var output = JsonE.Evaluate(template, context);
		Assert.That(output.AsJsonString(), Is.EqualTo("1"));
	}
}