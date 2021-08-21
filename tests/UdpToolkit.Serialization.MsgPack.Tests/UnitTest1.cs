#pragma warning disable
namespace UdpToolkit.Serialization.MsgPack.Tests
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using MessagePack;
    using Xunit;

    public class UnitTest1
    {
        public interface ISubscription
        {
            byte[] Serialize(object data);
            
            object Deserialize(byte[] bytes);
        }

        public interface ISubscription<TEvent>
        {
        }

        public class Subscription<TTestDto> : ISubscription<TTestDto>, ISubscription
        {
            private readonly ISerializer _serializer;
            
            public Subscription(ISerializer serializer)
            {
                _serializer = serializer;
            }
            
            public byte[] Serialize(object data)
            {
                var dto = (TTestDto)data;
                return _serializer.Serialize(dto);
            }

            public object Deserialize(byte[] bytes)
            {
                return _serializer.Deserialize<TestDto>(bytes);
            }
        }
    
        [Fact]
        public void Test1()
        {
            var id = Guid.NewGuid();
            var storage = new Dictionary<int, ISubscription>();
            storage[0] = new Subscription<TestDto>(new Serializer());
            var subscription = storage[0];

            var bytes = subscription.Serialize(new TestDto(id));
            var obj = subscription.Deserialize(bytes);

            (obj as TestDto)
                .Should()
                .NotBeNull();
        }

        [MessagePackObject]
        public class TestDto
        {
            public TestDto(Guid id)
            {
                Id = id;
            }

            [Key(0)]
            public Guid Id { get; }
        }
    }
}