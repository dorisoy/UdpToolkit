using System.Linq;
using UdpToolkit.Framework;
using Xunit;

namespace UdpToolkit.Tests
{
    public class FrameworkExtensionsTests
    {
        [Fact]
        public void FrameworkExtensions_FindAllHubMethods_SixHubsFounded()
        {
            var methods = FrameworkExtensions.FindAllHubMethods();

            Assert.Equal(6, methods.Count);
        }

        [Fact]
        public void FrameworkExtensions_FindAllHubMethods_TwoHubsFounded()
        {
            var methods = FrameworkExtensions.FindAllHubMethods();

            var hubs = methods
                .ToLookup(method => method.HubType, method => method)
                .ToDictionary(group => group.Key, group => group.ToArray());
            
            Assert.Equal(2, hubs.Keys.Count);
        }
    }
}
