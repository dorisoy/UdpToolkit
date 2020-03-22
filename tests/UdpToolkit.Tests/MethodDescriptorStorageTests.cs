namespace UdpToolkit.Tests
{
    using System.Linq;
    using UdpToolkit.Framework.Rpcs;
    using Xunit;

    public class MethodDescriptorStorageTests
    {
        [Fact]
        public void FrameworkExtensions_FindAllHubMethods_SixHubsFounded()
        {
            var methods = MethodDescriptorStorage.HubMethods;

            Assert.Equal(6, methods.Count);
        }

        [Fact]
        public void FrameworkExtensions_FindAllHubMethods_TwoHubsFounded()
        {
            var methods = MethodDescriptorStorage.HubMethods;

            var hubs = methods
                .ToLookup(method => method.HubType, method => method)
                .ToDictionary(group => group.Key, group => group.ToArray());

            Assert.Equal(2, hubs.Keys.Count);
        }
    }
}
