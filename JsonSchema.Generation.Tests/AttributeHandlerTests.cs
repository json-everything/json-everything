using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace Json.Schema.Generation.Tests {
    public class AttributeHandlerTests {

        [Test]
        public void DirectAttributeHandler() {
            JsonSchema expected = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(
                    ("MyProperty", new JsonSchemaBuilder().Type(SchemaValueType.String).MaxLength(AttributeWithDirectHandler.MaxLength))
                );

            JsonSchema actual = new JsonSchemaBuilder().FromType<TypeWithCustomAttribute1>();

            Assert.AreEqual(expected, actual);
        }


        [Test]
        public void IndirectAttributeHandler() {
            AttributeHandler.RemoveHandler<CustomAttributeHandler>();
            AttributeHandler.AddHandler<CustomAttributeHandler>();

            JsonSchema expected = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(
                    ("MyProperty", new JsonSchemaBuilder().Type(SchemaValueType.String).MaxLength(AttributeWithIndirectHandler.MaxLength))
                );

            JsonSchema actual = new JsonSchemaBuilder().FromType<TypeWithCustomAttribute2>();

            Assert.AreEqual(expected, actual);
        }


        [AttributeUsage(AttributeTargets.Property)]
        public class AttributeWithDirectHandler : Attribute, IAttributeHandler {

            public const uint MaxLength = 100;
            
            void IAttributeHandler.AddConstraints(SchemaGeneratorContext context) {
                if (context.Attributes.Any(x => x.GetType() == typeof(AttributeWithDirectHandler))) {
                    context.Intents.Add(new Intents.MaxLengthIntent(MaxLength));
                }
            }

        }


        [AttributeUsage(AttributeTargets.Property)]
        public class AttributeWithIndirectHandler : Attribute {

            public const uint MaxLength = 200;

        }


        public class CustomAttributeHandler : IAttributeHandler {
            void IAttributeHandler.AddConstraints(SchemaGeneratorContext context) {
                if (context.Attributes.Any(x => x.GetType() == typeof(AttributeWithIndirectHandler))) {
                    context.Intents.Add(new Intents.MaxLengthIntent(AttributeWithIndirectHandler.MaxLength));
                }
            }
        }


        public class TypeWithCustomAttribute1 {

            [AttributeWithDirectHandler]
            public string MyProperty { get; set; }

        }


        public class TypeWithCustomAttribute2 {

            [AttributeWithIndirectHandler]
            public string MyProperty { get; set; }

        }

    }
}
