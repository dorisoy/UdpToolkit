using System;
using UdpToolkit.Core;
using UdpToolkit.Serialization.MsgPack;
using UdpToolkit.Tests.Resources;
using Xunit;

namespace UdpToolkit.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void Serializer_SerializeModel_Success()
        {
            ISerializer serializer = new Serializer();

            var model = new Message(1, 1);
            var bytes = serializer.Serialize(model);
            
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void Serializer_DeserializeModel_Success()
        {
            ISerializer serializer = new Serializer();
            
            var bytes = new byte[] { 146 , 1, 1 };
            var obj = serializer.Deserialize(typeof(Message), new ArraySegment<byte>(bytes, 0, bytes.Length));

            var message = obj as Message;
            
            Assert.NotNull(message);
            Assert.Equal(1, message.HubId);
            Assert.Equal(1, message.RpcId);
        }
    }
}
