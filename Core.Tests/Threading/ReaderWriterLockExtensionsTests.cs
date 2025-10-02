using NUnit.Framework;
using System;
using System.Threading;

namespace CarinaStudio.Threading;

/// <summary>
/// Tests for <see cref="ReaderWriterLockExtensions"/>.
/// </summary>
[TestFixture]
public class ReaderWriterLockExtensionsTests
{
    /// <summary>
    /// Test for entering read/write scope.
    /// </summary>
    [Test]
    public void ReadWriteScopeTest()
    {
        // normal reader/writer lock (ReaderWriterLockSlim)
        var value = 1;
        using var rwLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        using var event1 = new ManualResetEventSlim();
        using var event2 = new ManualResetEventSlim();
        ThreadPool.QueueUserWorkItem(_ =>
        {
            using var scope = rwLockSlim.EnterWriteScope();
            event1.Set();
            Thread.Sleep(1000);
            Interlocked.Increment(ref value);
        });
        Assert.That(event1.Wait(5000));
        event1.Reset();
        using (rwLockSlim.EnterReadScope())
        {
            using var innerScope = rwLockSlim.EnterReadScope(1);
            Assert.That(value == 2);
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Assert.That(event2.Wait(5000));
                using var scope = rwLockSlim.EnterWriteScope();
                Interlocked.Increment(ref value);
                event1.Set();
            });
            event2.Set();
            Thread.Sleep(1000);
            Assert.That(value == 2);
        }
        Assert.That(event1.Wait(5000));
        Assert.That(value == 3);
        
        // reader lock timeout (ReaderWriterLockSlim)
        event1.Reset();
        event2.Reset();
        ThreadPool.QueueUserWorkItem(_ =>
        {
            using var scope = rwLockSlim.EnterWriteScope();
            event1.Set();
            Thread.Sleep(3000);
            event1.Set();
        });
        Assert.That(event1.Wait(5000));
        event1.Reset();
        try
        {
            using var _ = rwLockSlim.EnterReadScope(1000);
            Assert.Fail();
        }
        catch (TimeoutException)
        { }
        Assert.That(event1.Wait(5000));
        
        // writer lock timeout (ReaderWriterLockSlim)
        event1.Reset();
        event2.Reset();
        ThreadPool.QueueUserWorkItem(_ =>
        {
            using var scope = rwLockSlim.EnterReadScope();
            event1.Set();
            Thread.Sleep(3000);
            event1.Set();
        });
        Assert.That(event1.Wait(5000));
        event1.Reset();
        try
        {
            using var _ = rwLockSlim.EnterWriteScope(1000);
            Assert.Fail();
        }
        catch (TimeoutException)
        { }
        Assert.That(event1.Wait(5000));
        
        // upgradeable reader lock
        value = 1;
        event1.Reset();
        event2.Reset();
        ThreadPool.QueueUserWorkItem(_ =>
        {
            using (rwLockSlim.EnterWriteScope())
            {
                event1.Set();
                Thread.Sleep(1000);
                Interlocked.Increment(ref value);
            }
            Assert.That(event2.Wait(5000));
            using (rwLockSlim.EnterReadScope())
            {
                Assert.That(value == 3);
            }
        });
        Assert.That(event1.Wait(5000));
        using (rwLockSlim.EnterUpgradeableReadScope())
        {
            Assert.That(value == 2);
            using (rwLockSlim.EnterWriteScope())
            {
                event2.Set();
                Thread.Sleep(1000);
                Interlocked.Increment(ref value);
            }
        }
        
        // normal reader/writer lock (ReaderWriterLock)
        var rwLock = new ReaderWriterLock();
        value = 1;
        event1.Reset();
        event2.Reset();
        ThreadPool.QueueUserWorkItem(_ =>
        {
            using var scope = rwLock.EnterWriteScope();
            event1.Set();
            Thread.Sleep(1000);
            Interlocked.Increment(ref value);
        });
        Assert.That(event1.Wait(5000));
        event1.Reset();
        using (rwLock.EnterReadScope())
        {
            using var innerScope = rwLock.EnterReadScope(1);
            Assert.That(value == 2);
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Assert.That(event2.Wait(5000));
                using var scope = rwLock.EnterWriteScope();
                Interlocked.Increment(ref value);
                event1.Set();
            });
            event2.Set();
            Thread.Sleep(1000);
            Assert.That(value == 2);
        }
        Assert.That(event1.Wait(5000));
        Assert.That(value == 3);
        
        // reader lock timeout (ReaderWriterLock)
        event1.Reset();
        event2.Reset();
        ThreadPool.QueueUserWorkItem(_ =>
        {
            using var scope = rwLock.EnterWriteScope();
            event1.Set();
            Thread.Sleep(3000);
            event1.Set();
        });
        Assert.That(event1.Wait(5000));
        event1.Reset();
        try
        {
            using var _ = rwLock.EnterReadScope(1000);
            Assert.Fail();
        }
        catch (TimeoutException)
        { }
        Assert.That(event1.Wait(5000));
        
        // writer lock timeout (ReaderWriterLock)
        event1.Reset();
        event2.Reset();
        ThreadPool.QueueUserWorkItem(_ =>
        {
            using var scope = rwLock.EnterReadScope();
            event1.Set();
            Thread.Sleep(3000);
            event1.Set();
        });
        Assert.That(event1.Wait(5000));
        event1.Reset();
        try
        {
            using var _ = rwLock.EnterWriteScope(1000);
            Assert.Fail();
        }
        catch (TimeoutException)
        { }
        Assert.That(event1.Wait(5000));
    }
}