using Avalonia;
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
        static FieldInfo? dialogResultField;
        static readonly HashSet<Avalonia.Controls.Window> dialogWindows = new();


        // Get predefined height of title bar of window.
        internal static int GetTitleBarHeightInPixels()
        {
            if (Platform.IsLinux)
                return 75; // Not an accurate value
            return 0; // unused
        }


        // Check whether given window is shown as dialog or not.
        internal static bool IsDialogWindow(Avalonia.Controls.Window window) =>
            dialogWindows.Contains(window);
        

        /// <summary>
		/// Move the window to center of its owner.
		/// </summary>
		/// <param name="window"><see cref="Avalonia.Controls.Window"/>.</param>
		public static void MoveToCenterOfOwner(this Avalonia.Controls.Window window)
		{
			if (window.Owner is not Avalonia.Controls.Window owner)
                return;
            var screenScale = Platform.IsMacOS ? 1.0 : (owner.Screens.ScreenFromVisual(owner)?.Scaling ?? 1.0);
            var bounds = window.Bounds;  // px
            var width = window.Width; // dip
            var height = window.Height; // dip
            if (!double.IsFinite(width))
                width = bounds.Width / screenScale;
            if (!double.IsFinite(height))
                height = bounds.Height / screenScale;
            var ownerBounds = owner.Bounds; // px
            var ownerWidth = owner.Width; // dip
            var ownerHeight = owner.Height; // dip
            if (!double.IsFinite(ownerWidth))
                ownerWidth = ownerBounds.Width / screenScale;
            if (!double.IsFinite(ownerHeight))
                ownerHeight = ownerBounds.Height / screenScale;
            var titleBarHeight = GetTitleBarHeightInPixels() / screenScale;
            var heightWithTitleBar = height + titleBarHeight;
            var ownerPosition = owner.Position;
            var offsetX = (int)((ownerWidth - width) / 2 * screenScale + 0.5);
            var offsetY = (int)((ownerHeight + titleBarHeight - heightWithTitleBar) / 2 * screenScale + 0.5);
            window.Position = new PixelPoint(ownerPosition.X + offsetX, ownerPosition.Y + offsetY - (int)(titleBarHeight * screenScale + 0.5));
		}


        /// <summary>
		/// Move the window to center of the screen.
		/// </summary>
		/// <param name="window"><see cref="Avalonia.Controls.Window"/>.</param>    
		public static void MoveToCenterOfScreen(this Avalonia.Controls.Window window)
        {
            var screen = window.Screens.ScreenFromVisual(window) ?? window.Screens.Primary;
            if (screen != null)
                MoveToCenterOfScreen(window, screen);
        }


        /// <summary>
		/// Move the window to center of given screen.
		/// </summary>
		/// <param name="window"><see cref="Avalonia.Controls.Window"/>.</param>
        /// <param name="screen">Screen.</param>      
		public static void MoveToCenterOfScreen(this Avalonia.Controls.Window window, Avalonia.Platform.Screen screen)
		{
            var screenScale = Platform.IsMacOS ? 1.0 : screen.Scaling;
            var bounds = window.Bounds;
            var width = window.Width;
            var height = window.Height;
            if (!double.IsFinite(width))
                width = bounds.Width / screenScale;
            if (!double.IsFinite(height))
                height = bounds.Height / screenScale;
            var workingArea = screen.WorkingArea;
            var titleBarHeight = GetTitleBarHeightInPixels() / screenScale;
            var heightWithTitleBar = height + titleBarHeight;
            var offsetX = (int)((workingArea.Width - width * screenScale) / 2 + 0.5);
            var offsetY = (int)((workingArea.Height + (titleBarHeight - heightWithTitleBar) * screenScale) / 2 + 0.5);
            window.Position = new PixelPoint(workingArea.X + offsetX, workingArea.Y + offsetY - (int)(titleBarHeight * screenScale + 0.5));
		}


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
            closedHandler = (_, _) =>
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