using System;
using System.Collections.Generic;
using System.Linq;
using BTDB.EventStoreLayer;
using BTDB.StreamLayer;
using NUnit.Framework;

namespace BTDBTest
{
    [TestFixture]
    public class TypeSerializersTest
    {
        ITypeSerializers _ts;
        ITypeSerializersMapping _mapping;

        [SetUp]
        public void Setup()
        {
            _ts = new TypeSerializers();
            _mapping = _ts.CreateMapping();
        }

        [Test]
        public void CanSerializeString()
        {
            var writer = new ByteBufferWriter();
            var storedDescriptorCtx = _mapping.StoreNewDescriptors(writer, "Hello");
            storedDescriptorCtx.FinishNewDescriptors(writer);
            storedDescriptorCtx.StoreObject(writer, "Hello");
            storedDescriptorCtx.CommitNewDescriptors();
            var reader = new ByteBufferReader(writer.Data);
            var obj = _mapping.LoadObject(reader);
            Assert.AreEqual("Hello", obj);
        }

        [Test]
        public void CanSerializeInt()
        {
            var writer = new ByteBufferWriter();
            var storedDescriptorCtx = _mapping.StoreNewDescriptors(writer, 12345);
            storedDescriptorCtx.FinishNewDescriptors(writer);
            storedDescriptorCtx.StoreObject(writer, 12345);
            storedDescriptorCtx.CommitNewDescriptors();
            var reader = new ByteBufferReader(writer.Data);
            var obj = _mapping.LoadObject(reader);
            Assert.AreEqual(12345, obj);
        }

        [Test]
        public void CanSerializeSimpleTypes()
        {
            CanSerializeSimpleValue((byte)42);
            CanSerializeSimpleValue((sbyte)-20);
            CanSerializeSimpleValue((short)-1234);
            CanSerializeSimpleValue((ushort)1234);
            CanSerializeSimpleValue((uint)123456789);
            CanSerializeSimpleValue(-123456789012L);
            CanSerializeSimpleValue(123456789012UL);
        }

        void CanSerializeSimpleValue(object value)
        {
            var writer = new ByteBufferWriter();
            var storedDescriptorCtx = _mapping.StoreNewDescriptors(writer, value);
            storedDescriptorCtx.FinishNewDescriptors(writer);
            storedDescriptorCtx.StoreObject(writer, value);
            storedDescriptorCtx.CommitNewDescriptors();
            var reader = new ByteBufferReader(writer.Data);
            var obj = _mapping.LoadObject(reader);
            Assert.AreEqual(value, obj);
        }

        public class SimpleDto
        {
            public string StringField { get; set; }
            public int IntField { get; set; }
        }

        [Test]
        public void CanSerializeSimpleDto()
        {
            var writer = new ByteBufferWriter();
            var value = new SimpleDto { IntField = 42, StringField = "Hello" };
            var storedDescriptorCtx = _mapping.StoreNewDescriptors(writer, value);
            storedDescriptorCtx.FinishNewDescriptors(writer);
            storedDescriptorCtx.StoreObject(writer, value);
            storedDescriptorCtx.CommitNewDescriptors();
            var reader = new ByteBufferReader(writer.Data);
            _mapping.LoadTypeDescriptors(reader);
            var obj = (SimpleDto)_mapping.LoadObject(reader);
            Assert.AreEqual(value.IntField, obj.IntField);
            Assert.AreEqual(value.StringField, obj.StringField);
        }

        void TestSerialization(object value)
        {
            var writer = new ByteBufferWriter();
            var storedDescriptorCtx = _mapping.StoreNewDescriptors(writer, value);
            storedDescriptorCtx.FinishNewDescriptors(writer);
            storedDescriptorCtx.StoreObject(writer, value);
            storedDescriptorCtx.CommitNewDescriptors();
            var reader = new ByteBufferReader(writer.Data);
            _mapping.LoadTypeDescriptors(reader);
            Assert.AreEqual(value, _mapping.LoadObject(reader));
            Assert.True(reader.Eof);
            _mapping = _ts.CreateMapping();
            reader = new ByteBufferReader(writer.Data);
            _mapping.LoadTypeDescriptors(reader);
            Assert.AreEqual(value, _mapping.LoadObject(reader));
            Assert.True(reader.Eof);
        }

        public class ClassWithList : IEquatable<ClassWithList>
        {
            public List<int> List { get; set; }

            public bool Equals(ClassWithList other)
            {
                if (List==other.List) return true;
                if (List==null || other.List==null) return false;
                if (List.Count != other.List.Count) return false;
                return List.Zip(other.List, (i1, i2) => i1 == i2).All(p => p);
            }
        }

        [Test]
        public void ListCanHaveContent()
        {
            TestSerialization(new ClassWithList { List = new List<int> { 1, 2, 3 } });
        }

        [Test]
        public void ListCanBeEmpty()
        {
            TestSerialization(new ClassWithList { List = new List<int>()});
        }

        [Test]
        public void ListCanBeNull()
        {
            TestSerialization(new ClassWithList { List = null });
        }

    }
}