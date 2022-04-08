using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Path.Tests
{
	public class StackOverflowTests
	{
		// https://stackoverflow.com/q/71692141/878701
		[Test]
		public void Question71692141()
		{
			var data = JsonDocument.Parse(@"{
    ""type"": ""object"",
    ""properties"": {
        ""period"": {
            ""description"": ""The period in which the rule applies"",
            ""type"": ""object"",
            ""properties"": {
                ""start"": {
                    ""type"": ""string"",
                    ""format"": ""date-time""
                },
                ""end"": {
                    ""type"": ""string"",
                    ""format"": ""date-time""
                }
            },
            ""required"": [
                ""start""
            ],
            ""x-updateIndicatorProperties"": [
                ""start""
            ]
        },
        ""productType"": {
            ""type"": ""string""
        },
        ""x-updateIndicatorProperties"": [
            ""productType""
        ]
    }
}");
			var path = JsonPath.Parse("$..['x-updateIndicatorProperties']");

			var results = path.Evaluate(data.RootElement);

			Assert.AreEqual(2, results.Matches.Count);
		}
	}
}