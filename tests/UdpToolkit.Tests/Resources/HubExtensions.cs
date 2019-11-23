using System;
using UdpToolkit.Core;
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
            Assert.NotNull(hubBase.UdpSenderProxy);
            Assert.NotNull(hubBase.HubContext);
        }
    }
}