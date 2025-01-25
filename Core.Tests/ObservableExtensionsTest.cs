using NUnit.Framework;
using System;
using System.Threading;

namespace CarinaStudio;

/// <summary>
/// Tests of <see cref="ObservableExtensions"/>.
/// </summary>
[TestFixture]
public class ObservableExtensionsTest
{
    /// <summary>
    /// Test for using ObservableExtensions.Subscribe() methods.
    /// </summary>
    [Test]
    public void SubscriptionTest()
    {
        // prepare
        var valuePassedToOnNext = 0;
        var isOnNextCalled = false;
        var onNextWithValue = new Action<int>(value => valuePassedToOnNext = value);
        var onNextWithoutValue = new Action(() => isOnNextCalled = true);
        var observable = new MutableObservableInt32(1);
        
        // subscribe onNext with value
        using (_ = observable.Subscribe(onNextWithValue))
        {
            Assert.That(valuePassedToOnNext == 1);
            observable.Update(2);
            Assert.That(valuePassedToOnNext == 2);
        }
        observable.Update(3);
        Assert.That(valuePassedToOnNext == 2);
        
        // subscribe onNext without value
        using (_ = observable.Subscribe(onNextWithoutValue))
        {
            Assert.That(isOnNextCalled);
            isOnNextCalled = false;
            observable.Update(10);
            Assert.That(isOnNextCalled);
            isOnNextCalled = false;
        }
        observable.Update(11);
        Assert.That(!isOnNextCalled);
        
        // subscribe without calling onNext during subscription
        valuePassedToOnNext = 0;
        using (_ = observable.Subscribe(onNextWithValue, true))
        {
            Assert.That(valuePassedToOnNext == 0);
            observable.Update(1);
            Assert.That(valuePassedToOnNext == 1);
        }
    }
}