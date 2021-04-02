namespace UdpToolkit.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Xunit.Abstractions;

    public class GuidHashTests
    {
        private ITestOutputHelper _testOutputHelper;

        public GuidHashTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void HashTest()
        {
            var hashes = Enumerable.Range(0, 10_000)
                .Select(_ => Guid.NewGuid().GetHashCode())
                .ToArray();

            var uniqueHashes = new HashSet<int>(hashes);
            Assert.Equal(10_000, uniqueHashes.Count);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(16)]
        public void GroupTest(int workers)
        {
            var hashes = Enumerable.Range(0, 10_000)
                .Select(_ => Guid.NewGuid().GetHashCode() % workers)
                .ToLookup(queueId => queueId);

            for (int i = 0; i < workers; i++)
            {
                _testOutputHelper.WriteLine($"Worker - {i} {hashes[i].Count()}");
            }
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(16)]
        public void GroupTest2(int workers)
        {
            var hashes = Enumerable.Range(0, 10_000)
                .Select(_ => MurMurHash.Hash3_x86_32(Guid.NewGuid()) % workers)
                .ToLookup(queueId => queueId);

            for (int i = 0; i < workers; i++)
            {
                _testOutputHelper.WriteLine($"Worker - {i} {hashes[i].Count()}");
            }
        }
    }
}