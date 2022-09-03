using Avalonia.Input;
using Avalonia.Input.Platform;
using System.Threading.Tasks;

namespace CarinaStudio.Input.Platform
{
    /// <summary>
    /// Extensions for <see cref="IClipboard"/>.
    /// </summary>
    public static class ClipboardExtensions
    {
        /// <summary>
        /// Get data with specified format from clipboard. Or get text from clipboard if data is unavailable.
        /// </summary>
        /// <param name="clipboard"><see cref="IClipboard"/>.</param>
        /// <param name="dataFormat">Data format.</param>
        /// <returns>Task of getting data or text.</returns>
        public static async Task<(object?, string?)> GetDataOrTextAsync(this IClipboard clipboard, string dataFormat)
        {
            var data = await clipboard.GetDataAsync(dataFormat);
            if (data != null)
                return (data, null);
            return (null, await clipboard.GetTextAsync());
        }


        /// <summary>
        /// Get text from clipboard. Or get data with specified format from clipboard if text is unavailable.
        /// </summary>
        /// <param name="clipboard"><see cref="IClipboard"/>.</param>
        /// <param name="dataFormat">Data format.</param>
        /// <returns>Task of getting text or data.</returns>
        public static async Task<(string?, object?)> GetTextOrDataAsync(this IClipboard clipboard, string dataFormat)
        {
            var text = await clipboard.GetTextAsync();
            if (text != null)
                return (text, null);
            return (null, await clipboard.GetDataAsync(dataFormat));
        }


        /// <summary>
        /// Set text and data to clipboard at same time.
        /// </summary>
        /// <param name="clipboard"><see cref="IClipboard"/>.</param>
        /// <param name="text">Text.</param>
        /// <param name="dataFormat">Data format.</param>
        /// <param name="data">Data.</param>
        /// <returns>Task of setting text and data.</returns>
        public static Task SetTextAndDataAsync(this IClipboard clipboard, string text, string dataFormat, object data) =>
            SetTextAndDataObjectAsync(clipboard, text, new DataObject().Also(it => it.Set(dataFormat, data)));


        /// <summary>
        /// Set text and data to clipboard at same time.
        /// </summary>
        /// <param name="clipboard"><see cref="IClipboard"/>.</param>
        /// <param name="text">Text.</param>
        /// <param name="dataObject">Data.</param>
        /// <returns>Task of setting text and data.</returns>
        public static async Task SetTextAndDataObjectAsync(this IClipboard clipboard, string text, IDataObject dataObject)
        {
            await clipboard.SetTextAsync(text);
            var newDataObject = (dataObject as DataObject) ?? dataObject.Clone();
            foreach (var format in await clipboard.GetFormatsAsync())
            {
                var data = await clipboard.GetDataAsync(format);
                if (data != null)
                    newDataObject.Set(format, data);
            }
            await clipboard.SetDataObjectAsync(newDataObject);
        }
    }
}