using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UdpToolkit.Framework.Hubs;
using UdpToolkit.Framework.Rpcs;
using UdpToolkit.Network.Packets;
using UdpToolkit.Network.Queues;
using UdpToolkit.Tests.Fakes;
using UdpToolkit.Tests.Resources;
using Xunit;

namespace UdpToolkit.Tests
{
    public class RpcProviderTests
    {
        [Theory]
        [MemberData(nameof(Cases))]
        public async Task RpcProvider_InvokeHubRpc_NotThrown(byte hubId, byte rpcId, object[] ctorArgs, object[] methodArgs)
        {
            var rpcTransformer = new RpcTransformer();
            var methods = MethodDescriptorStorage.HubMethods.ToArray();
                
            var rpcs = methods
                .Select(x  => rpcTransformer.Transform(x))
                .ToList()
                .ToDictionary(rpcDesc => 
                    new RpcDescriptorId(
                        hubId: rpcDesc.HubId,
                        rpcId: rpcDesc.RpcId));

            IRpcProvider rpcProvider = new RpcProvider(rpcs);

            var key = new RpcDescriptorId(hubId: hubId, rpcId: rpcId);
            
            rpcProvider.TryProvide(key, out var rpcDescriptor);

            var exception = await Record.ExceptionAsync(() => rpcDescriptor.HubRpc(
                hubContext: new HubContext(0,hubId,rpcId,"foo"), 
                serializer: new FakeSerializer(),
                peerTracker: new FakePeerTracker(),
                eventProducer: new BlockingAsyncQueue<OutputUdpPacket>(
                    boundedCapacity: Int32.MaxValue), 
                ctorArguments: ctorArgs,
                methodArguments: methodArgs));
            
            Assert.Null(exception);
        }

        public static IEnumerable<object[]> Cases()
        {
            yield return new object[] 
            { 
                1, 
                0, 
                new object [] 
                { 
                    new TestService(),
                },
                new object[0]
            };
            
            yield return new object[] 
            { 
                1, 
                1, 
                new object [] 
                { 
                    new TestService(),
                },
                new object[]
                {
                    new Message(100, 001)
                }
            };
            
            yield return new object[] 
            { 
                1, 
                2, 
                new object [] 
                { 
                    new TestService(),
                },
                new object[]
                {
                    1,
                    1
                }
            };
            
            yield return new object[] 
            { 
                0, 
                0, 
                new object [0],
                new object[0]
            };
            
            yield return new object[] 
            { 
                0, 
                1, 
                new object[0],
                new object[]
                {
                    new Message(100, 001)
                }
            };
            
            yield return new object[] 
            { 
                0, 
                2, 
                new object [0],
                new object[]
                {
                    1,
                    1
                }
            };
        }

    }
}
