using CarinaStudio.Threading;
using NUnit.Framework;
using System;
using System.Threading;

namespace CarinaStudio
{
    /// <summary>
    /// Tests of <see cref="CachedObservableValue{T}"/>.
    /// </summary>
    [TestFixture]
    public class CachedObservableValueTests
    {
        /// <summary>
        /// Test for updating cached value by given function.
        /// </summary>
        [Test]
        public void UpdatingByFunctionTest()
        {
            // prepare
            var random = new Random();
            var latestUpdatedValue = 0;
            var updateFunc = new Func<int>(() => 
            {
                latestUpdatedValue = random.Next();
                return latestUpdatedValue;
            });

            // create
            var cachedValue = new CachedObservableValue<int>(updateFunc);
            Assert.That(latestUpdatedValue == cachedValue.Value);

            // update value
            latestUpdatedValue = -1;
            cachedValue.Invalidate();
            Assert.That(latestUpdatedValue >= 0);
            Assert.That(latestUpdatedValue == cachedValue.Value);
        }


        /// <summary>
        /// Test for updating cached value by another observable value.
        /// </summary>
        [Test]
        public void UpdatingByObservableTest()
        {
            // prepare
            var random = new Random();
            var sourceValue = new MutableObservableInt32(random.Next());
            using var syncContext = new SingleThreadSynchronizationContext();

            // update value in current thread
            var cachedValueRef = default(WeakReference<CachedObservableValue<int>>);
            var testAction = new Action(() => 
            {
                // create
                var cachedValue = new CachedObservableValue<int>(sourceValue);
                Assert.That(sourceValue.Value == cachedValue.Value);

                // update value
                for (var i = 0; i < 10; ++i)
                {
                    sourceValue.Update(random.Next());
                    Assert.That(sourceValue.Value == cachedValue.Value);
                }
                cachedValueRef = new(cachedValue);
            });
            testAction();
            for (var i = 9; i >= 0; --i)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();
                if (cachedValueRef?.TryGetTarget(out var _) != true)
                    break;
                if (i == 0)
                    throw new AssertionException("Unable to finalize cached value.");
                Thread.Sleep(500);
            }
            Assert.That(!sourceValue.HasObservers);

            // create in specific thread
            syncContext.Send(testAction);
            for (var i = 9; i >= 0; --i)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();
                if (cachedValueRef?.TryGetTarget(out var _) != true)
                    break;
                if (i == 0)
                    throw new AssertionException("Unable to finalize cached value.");
                Thread.Sleep(500);
            }
            syncContext.Send(() => Assert.That(!sourceValue.HasObservers));
        }
    }
}