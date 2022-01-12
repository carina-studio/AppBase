using CarinaStudio.Threading;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Animation
{
    /// <summary>
    /// Tests of <see cref="Animator"/>.
    /// </summary>
    [TestFixture]
    class AnimatorTests
    {
        // Fields.
        private volatile SingleThreadSynchronizationContext? syncContext;


        /// <summary>
        /// Testing for animation.
        /// </summary>
        [Test]
        public void AnimationTest()
        {
            this.TestOnSyncContextThread(async () =>
            {
                // prepare
                var stopwatch = new Stopwatch().Also(it => it.Start());
                var prevProgress = 0.0;
                var progressUpdateCount = 0;

                // check interval and progress change
                var startTime = stopwatch.ElapsedMilliseconds;
                await Animator.StartAndWaitForCompletionAsync(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(16), (progress) =>
                {
                    Assert.GreaterOrEqual(progress, prevProgress);
                    prevProgress = progress;
                    ++progressUpdateCount;
                });
                var actualDuration = (stopwatch.ElapsedMilliseconds - startTime);
                Assert.GreaterOrEqual(prevProgress, 0.9);
                Assert.GreaterOrEqual(actualDuration, 1000);
                Assert.Less(actualDuration, 1100);
                Assert.GreaterOrEqual(progressUpdateCount, 54);
                Assert.LessOrEqual(progressUpdateCount, 66);
                prevProgress = 0;
                progressUpdateCount = 0;
                startTime = stopwatch.ElapsedMilliseconds;
                await Animator.StartAndWaitForCompletionAsync(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(33), (progress) =>
                {
                    Assert.GreaterOrEqual(progress, prevProgress);
                    prevProgress = progress;
                    ++progressUpdateCount;
                });
                actualDuration = (stopwatch.ElapsedMilliseconds - startTime);
                Assert.GreaterOrEqual(prevProgress, 0.9);
                Assert.GreaterOrEqual(actualDuration, 1000);
                Assert.Less(actualDuration, 1100);
                Assert.GreaterOrEqual(progressUpdateCount, 27);
                Assert.LessOrEqual(progressUpdateCount, 33);

                // check interpolator
                prevProgress = 1;
                progressUpdateCount = 0;
                startTime = stopwatch.ElapsedMilliseconds;
                await Animator.StartAndWaitForCompletionAsync(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(16), Interpolators.Inverted, (progress) =>
                {
                    Assert.LessOrEqual(progress, prevProgress);
                    prevProgress = progress;
                    ++progressUpdateCount;
                }, new CancellationToken());
                actualDuration = (stopwatch.ElapsedMilliseconds - startTime);
                Assert.LessOrEqual(prevProgress, 0.1);
                Assert.GreaterOrEqual(actualDuration, 1000);
                Assert.Less(actualDuration, 1100);
                Assert.GreaterOrEqual(progressUpdateCount, 54);
                Assert.LessOrEqual(progressUpdateCount, 66);
            });
        }


        /// <summary>
        /// Tests for cancellation.
        /// </summary>
        [Test]
        public void CancellationTest()
        {
            this.TestOnSyncContextThread(async () =>
            {
                // prepare
                var latestProgress = 0.0;
                var isCancelledCalled = false;
                var isCompletedCalled = false;
                var animator = new Animator();
                animator.Cancelled += (_, e) =>
                {
                    isCancelledCalled = true;
                };
                animator.Completed += (_, e) =>
                {
                    isCompletedCalled = true;
                };
                animator.Duration = TimeSpan.FromSeconds(1);
                animator.ProgressChanged += (_, e) =>
                {
                    latestProgress = animator.Progress;
                };

                // cancel before animating
                animator.Cancel();
                Assert.IsFalse(animator.IsStarted);

                // cancel when animating
                animator.Start();
                Assert.IsTrue(animator.IsStarted);
                await Task.Delay(500);
                Assert.IsTrue(animator.IsStarted);
                animator.Cancel();
                Assert.IsFalse(animator.IsStarted);
                Assert.IsTrue(isCancelledCalled);
                await Task.Delay(1500);
                Assert.Less(latestProgress, 1);
                Assert.Greater(latestProgress, 0);
                Assert.IsFalse(isCompletedCalled);

                // cancel after animating
                latestProgress = 0;
                isCancelledCalled = false;
                isCompletedCalled = false;
                animator.Start();
                Assert.IsTrue(animator.IsStarted);
                await Task.Delay(1500);
                Assert.IsFalse(animator.IsStarted);
                Assert.IsTrue(isCompletedCalled);
                Assert.IsFalse(isCancelledCalled);
                Assert.GreaterOrEqual(latestProgress, 0.9);
                animator.Cancel();
                Assert.IsFalse(animator.IsStarted);
            });
        }


        /// <summary>
        /// Complete tests.
        /// </summary>
        [OneTimeTearDown]
        public void Complete()
        {
            this.syncContext?.Dispose();
        }


        /// <summary>
        /// Testing for changing parameters.
        /// </summary>
        [Test]
        public void ParametersChangingTest()
        {
            this.TestOnSyncContextThread(async () =>
            {
                // change duration in animation
                var stopwatch = new Stopwatch().Also(it => it.Start());
                var startTime = stopwatch.ElapsedMilliseconds;
                var animator = Animator.Start(TimeSpan.FromSeconds(1), p => { });
                await Task.Delay(500);
                animator.Duration = TimeSpan.FromSeconds(2);
                await animator.WaitForCompletionAsync();
                var actualDuration = (stopwatch.ElapsedMilliseconds - startTime);
                Assert.IsFalse(animator.IsStarted);
                Assert.GreaterOrEqual(actualDuration, 2000);
                Assert.LessOrEqual(actualDuration, 2100);

                // change interval in animation
                var progressUpdateCount = 0;
                animator.Duration = TimeSpan.FromSeconds(3);
                animator.ProgressChanged += (_, e) =>
                {
                    ++progressUpdateCount;
                };
                startTime = stopwatch.ElapsedMilliseconds;
                animator.Start();
                await Task.Delay(1000);
                var firstActualDuration = (stopwatch.ElapsedMilliseconds - startTime);
                var firstProgressUpdateCount = progressUpdateCount;
                animator.Interval = TimeSpan.FromMilliseconds(100);
                await animator.WaitForCompletionAsync();
                var secondActualDuration = (stopwatch.ElapsedMilliseconds - startTime) - firstActualDuration;
                var expectedProgressCount = firstProgressUpdateCount + (secondActualDuration / 100);
                Assert.IsFalse(animator.IsStarted);
                Assert.GreaterOrEqual(progressUpdateCount, expectedProgressCount * 0.9);
                Assert.LessOrEqual(progressUpdateCount, expectedProgressCount * 1.1);
            });
        }


        /// <summary>
        /// Setup tests.
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            this.syncContext = new SingleThreadSynchronizationContext();
        }


        // Run test on the thread of test synchronization context.
		void TestOnSyncContextThread(Func<Task> asyncTest)
        {
            if (this.syncContext?.ExecutionThread == Thread.CurrentThread)
                asyncTest();
            else
            {
                var syncLock = new object();
                var awaiter = new TaskAwaiter();
                lock (syncLock)
                {
                    this.syncContext.AsNonNull().Post(() =>
                    {
                        awaiter = asyncTest().GetAwaiter();
                        awaiter.OnCompleted(() =>
                        {
                            lock (syncLock)
                            {
                                Monitor.Pulse(syncLock);
                            }
                        });
                    });
                    Monitor.Wait(syncLock);
                    awaiter.GetResult();
                }
            }
        }
    }
}
