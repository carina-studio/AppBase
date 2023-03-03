using System.Collections.Generic;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// Extensions for <see cref="Avalonia.Controls.Window"/>.
    /// </summary>
    public static class WindowExtensions
    {
        // Static fields.
        static volatile FieldInfo? dialogResultField;
        static readonly HashSet<Avalonia.Controls.Window> dialogWindows = new();


        // Check whether given window is shown as dialog or not.
        internal static bool IsDialogWindow(Avalonia.Controls.Window window) =>
            dialogWindows.Contains(window);


#pragma warning disable CS8600
#pragma warning disable CS8604
        /// <summary>
        /// Show window as dialog without parent window.
        /// </summary>
        /// <param name="window"><see cref="Avalonia.Controls.Window"/>.</param>
        /// <typeparam name="T">Type of result of dialog.</typeparam>
        /// <returns>Task of showing dialog.</returns>
        public static Task<T> ShowDialog<T>(this Avalonia.Controls.Window window)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            var closedHandler = (EventHandler?)null;
            closedHandler = (_, e) =>
            {
                window.Closed -= closedHandler;
                dialogWindows.Remove(window);
                try
                {
                    dialogResultField ??= typeof(Avalonia.Controls.Window).GetField("_dialogResult", BindingFlags.Instance | BindingFlags.NonPublic);
                    taskCompletionSource.SetResult((T)(dialogResultField?.GetValue(window) ?? default(T)));
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            };
            window.Closed += closedHandler;
            window.Show();
            dialogWindows.Add(window);
            return taskCompletionSource.Task;
        }
#pragma warning restore CS8600
#pragma warning restore CS8604
    }
}