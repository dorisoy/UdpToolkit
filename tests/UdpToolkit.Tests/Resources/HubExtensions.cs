using System;
using UdpToolkit.Framework;
using UdpToolkit.Framework.Hubs;
using Xunit;

namespace UdpToolkit.Tests.Resources
{
    public static class HubExtensions
    {
        public static void AssertAllPropertiesInitialized(this HubBase hubBase)
        {
            if (hubBase == null) throw new ArgumentNullException(nameof(hubBase));
            
            Assert.NotNull(hubBase.Serializer);
            Assert.NotNull(hubBase.PeerTracker);
            Assert.NotNull(hubBase.EventProducer);
            Assert.NotNull(hubBase.HubContext);
        }
    }
}