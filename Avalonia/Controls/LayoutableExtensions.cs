using Avalonia.Layout;
using System;

namespace CarinaStudio.Controls;

/// <summary>
/// Extension methods for <see cref="Layoutable"/>.
/// </summary>
public static class LayoutableExtensions
{
    // Enqueued layout call-back.
    class LayoutCallback : IDisposable
    {
        // Fields.
        readonly Action callback;
        bool isDisposed;
        readonly Layoutable layoutable;
        
        // Constructor.
        public LayoutCallback(Layoutable layoutable, Action callback, bool invalidateArrange)
        {
            this.callback = callback;
            this.layoutable = layoutable;
            layoutable.LayoutUpdated += this.EventHandler;
            if (invalidateArrange)
                layoutable.InvalidateArrange();
        }

        // Dispose.
        public void Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;
            this.layoutable.LayoutUpdated -= this.EventHandler;
        }

        // Handler for LayoutUpdated event.
        void EventHandler(object? sender, EventArgs e)
        {
            this.Dispose();
            callback();
        }
    }


    /// <summary>
    /// Enqueue a call-back to be called when next layout update.
    /// </summary>
    /// <param name="layoutable"><see cref="Layoutable"/>.</param>
    /// <param name="callback">Call-back to be called.</param>
    /// <param name="invalidateArrange">True to invalidate arrange of <see cref="Layoutable"/> as well.</param>
    /// <returns><see cref="IDisposable"/> represents enqueued call-back. The call-back can be dequeued by disposing the <see cref="IDisposable"/>.</returns>
    public static IDisposable RequestLayoutCallback(this Layoutable layoutable, Action callback, bool invalidateArrange = false) =>
        new LayoutCallback(layoutable, callback, invalidateArrange);
}