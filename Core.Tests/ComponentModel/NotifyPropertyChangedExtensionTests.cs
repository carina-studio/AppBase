using NUnit.Framework;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.ComponentModel;

/// <summary>
/// Tests for <see cref="NotifyPropertyChangedExtensions"/>.
/// </summary>
[TestFixture]
public class NotifyPropertyChangedExtensionTests
{
    // Test object.
    class TestClass : INotifyPropertyChanged
    {
        // Fields.
        int bar = 0;
        int foo = 0;
        
        // Bar.
        public int Bar
        {
            get => this.bar;
            set
            {
                if (this.bar != value)
                {
                    this.bar = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Bar)));
                }
            }
        }
        
        // Foo.
        public int Foo
        {
            get => this.foo;
            set
            {
                if (this.foo != value)
                {
                    this.foo = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Foo)));
                }
            }
        }
        
        // Check whether one or more handlers are attached to PropertyChanged or not.
        public bool HasPropertyChangedHandlers => this.PropertyChanged is not null;
        
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;
    }
    
    
    // Test for using WaitForPropertyChangeAsync().
    [Test]
    public async Task WaitForPropertyChangeTest()
    {
        // prepare
        var obj = new TestClass();
        
        // wait for property change to current value
        {
            var currentBar = obj.Bar;
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);
            await obj.WaitForPropertyChangeAsync(nameof(TestClass.Bar), it => it.Bar == currentBar, cancellationTokenSource.Token);
            Assert.That(!obj.HasPropertyChangedHandlers);
        }
        
        // timeout check
        {
            var currentBar = obj.Bar;
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000);
            try
            {
                await obj.WaitForPropertyChangeAsync(nameof(TestClass.Bar), it => it.Bar == currentBar + 1, cancellationTokenSource.Token);
                Assert.Fail();
            }
            catch (TaskCanceledException _)
            { }
            Assert.That(!obj.HasPropertyChangedHandlers);
        }
        
        // check changing to desired value
        var random = new Random();
        {
            var targetFoo = (int)(random.NextDouble() * int.MaxValue);
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(3000);
            var task = obj.WaitForPropertyChangeAsync(nameof(TestClass.Foo), it => it.Foo == targetFoo, cancellationTokenSource.Token);
            await Task.Delay(1000, CancellationToken.None);
            obj.Foo = targetFoo;
            await task;
            Assert.That(obj.Foo == targetFoo);
            Assert.That(!obj.HasPropertyChangedHandlers);
        }
        
        // check changing to intermediate value
        {
            var targetFoo = (int)(random.NextDouble() * int.MaxValue);
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(3000);
            var task = obj.WaitForPropertyChangeAsync(nameof(TestClass.Foo), it => it.Foo == targetFoo, cancellationTokenSource.Token);
            await Task.Delay(1000, CancellationToken.None);
            obj.Foo = targetFoo - 1;
            Task.Delay(1000, CancellationToken.None).GetAwaiter().OnCompleted(() => obj.Foo = targetFoo);
            await task;
            Assert.That(!obj.HasPropertyChangedHandlers);
        }
    }
}