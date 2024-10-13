using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CarinaStudio;

/// <summary>
/// Tests of <see cref="ObjectExtensions"/>.
/// </summary>
[TestFixture]
public class ObjectExtensionsTest
{
    // Test method for default implementation.
    static R DefaultTestMethod<T, R>(T obj, Func<T, R> method) =>
        method(obj);
    
    
    // Test method for inlining implementation.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static R InlineTestMethod<T, R>(T obj, Func<T, R> method) =>
        method(obj);
    
    
    /// <summary>
    /// Test for performance of inlining and no-inlining implementation.
    /// </summary>
    [Test]
    public void InliningPerformanceTest()
    {
        // prepare
        var stopwatch = new Stopwatch();
        var testCount = 1000000;
        int SimpleFunc(int x) => x * x;
        int ComplexFunc(int n)
        {
            var counter = 0;
            for (var i = 0; i < n; ++i)
            {
                for (var j = n; j > 0; --j)
                {
                    counter += (i + j);
                }
            }
            return counter;
        }
        stopwatch.Start();
        
        // default implementation to call simple function
        for (var n = 0; n < testCount; ++n)
            DefaultTestMethod(n, SimpleFunc);
        stopwatch.Restart();
        for (var n = 0; n < testCount; ++n)
            DefaultTestMethod(n, SimpleFunc);
        var defaultSimpleDuration = stopwatch.ElapsedTicks;
        
        // no-inlining implementation to call simple function
        for (var n = 0; n < testCount; ++n)
            NoInlineTestMethod(n, SimpleFunc);
        stopwatch.Restart();
        for (var n = 0; n < testCount; ++n)
            NoInlineTestMethod(n, SimpleFunc);
        var noinlineSimpleDuration = stopwatch.ElapsedTicks;
        
        // inlining implementation to call simple function
        for (var n = 0; n < testCount; ++n)
            InlineTestMethod(n, SimpleFunc);
        stopwatch.Restart();
        for (var n = 0; n < testCount; ++n)
            InlineTestMethod(n, SimpleFunc);
        var inlineSimpleDuration = stopwatch.ElapsedTicks;

        // check result
        Console.WriteLine();
        Console.WriteLine("********** Simple Function **********");
        Console.WriteLine($"DefaultTestMethod: {defaultSimpleDuration} ticks");
        Console.WriteLine($"InlineTestMethod: {inlineSimpleDuration} ticks, ({defaultSimpleDuration * 100.0 / inlineSimpleDuration:F3}%)");
        Console.WriteLine($"NoInlineTestMethod: {noinlineSimpleDuration} ticks, ({defaultSimpleDuration * 100.0 / noinlineSimpleDuration:F3}%)");
        
        // prepare
        testCount = 1000;
        
        // default implementation to call complex function
        for (var n = 0; n < testCount; ++n)
            DefaultTestMethod(n, ComplexFunc);
        stopwatch.Restart();
        for (var n = 0; n < testCount; ++n)
            DefaultTestMethod(n, ComplexFunc);
        var defaultComplexDuration = stopwatch.ElapsedTicks;
        
        // no-inlining implementation to call complex function
        for (var n = 0; n < testCount; ++n)
            NoInlineTestMethod(n, ComplexFunc);
        stopwatch.Restart();
        for (var n = 0; n < testCount; ++n)
            NoInlineTestMethod(n, ComplexFunc);
        var noinlineComplexDuration = stopwatch.ElapsedTicks;
        
        // inlining implementation to call complex function
        for (var n = 0; n < testCount; ++n)
            InlineTestMethod(n, ComplexFunc);
        stopwatch.Restart();
        for (var n = 0; n < testCount; ++n)
            InlineTestMethod(n, ComplexFunc);
        var inlineComplexDuration = stopwatch.ElapsedTicks;
        
        // check result
        Console.WriteLine();
        Console.WriteLine("********** Complex Function **********");
        Console.WriteLine($"DefaultTestMethod: {defaultComplexDuration} ticks");
        Console.WriteLine($"InlineTestMethod: {inlineComplexDuration} ticks, ({defaultComplexDuration * 100.0 / inlineComplexDuration:F3}%)");
        Console.WriteLine($"NoInlineTestMethod: {noinlineComplexDuration} ticks, ({defaultComplexDuration * 100.0 / noinlineComplexDuration:F3}%)");
    }
    
    
    // Test method for no-inlining implementation.
    [MethodImpl(MethodImplOptions.NoInlining)]
    static R NoInlineTestMethod<T, R>(T obj, Func<T, R> method) =>
        method(obj);
}