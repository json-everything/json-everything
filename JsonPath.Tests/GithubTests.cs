using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Json.Path.Tests
{
	public class GithubTests
	{
		[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==10)].doc_count")]
		[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==11)].doc_count")]
		[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==12)].doc_count")]
		[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==13)].doc_count")]
		[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==14)].doc_count")]
		[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==15)].doc_count")]
		[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==16)].doc_count")]
		[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==17)].doc_count")]
		[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==18)].doc_count")]
		[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==19)].doc_count")]
		public void Issue150_ParseFailsWhenExpressionContainsLiteralNumberWith9(string pathText)
		{
			var path = JsonPath.Parse(pathText);
			Console.WriteLine(path);
		}
	}
}
