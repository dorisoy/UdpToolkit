namespace UdpToolkit.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class TaskUtilsTests
    {
        public static IEnumerable<object[]> TestCases()
        {
            yield return new object[] { new TestCase(job: null, logger: (ex) => { }) };
            yield return new object[] { new TestCase(job: () => Task.CompletedTask, logger: null) };
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task ArgumentNullExceptionThrown(
            TestCase testCase)
        {
            var exception = await Record
                .ExceptionAsync(
                    testCode: () => TaskUtils.RestartOnFail(
                        job: testCase.Job,
                        logger: testCase.Logger,
                        token: default))
                .ConfigureAwait(false);

            Assert.True(exception is ArgumentNullException);
        }

        [Fact]
        public async Task JobRestarted()
        {
            var cts = new CancellationTokenSource();
            var restarts = 0;
            Func<Task> job = () =>
            {
                if (restarts == 0)
                {
                    restarts++;
                    throw new Exception();
                }

                cts.Cancel();

                return Task.CompletedTask;
            };

            var exception = await Record
                .ExceptionAsync(
                    testCode: () =>
                        TaskUtils.RestartOnFail(
                            job: job,
                            logger: (ex) => { },
                            token: cts.Token))
                .ConfigureAwait(false);

            Assert.True(exception is TaskCanceledException);
            Assert.True(restarts == 1);
        }

        [Fact]
        public async Task NotThrown()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var exception = await Record
                .ExceptionAsync(() => TaskUtils
                    .RestartOnFail(
                        job: () => Task.CompletedTask,
                        logger: (ex) => { },
                        token: cts.Token))
                .ConfigureAwait(false);

            Assert.Null(exception);
        }

        public class TestCase
        {
            public TestCase(
                Func<Task> job,
                Action<Exception> logger)
            {
                Job = job;
                Logger = logger;
            }

            public Func<Task> Job { get; }

            public Action<Exception> Logger { get; }
        }
    }
}