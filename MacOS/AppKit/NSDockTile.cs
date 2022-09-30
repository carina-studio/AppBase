using CarinaStudio.MacOS.ObjectiveC;

namespace CarinaStudio.MacOS.AppKit
{
    /// <summary>
    /// NSDockTile.
    /// </summary>
    public class NSDockTile : NSObject
    {
        // Static fields.
        static readonly Property? BadgeLabelProperty;
        static readonly Property? ContentViewProperty;
        static readonly Selector? DisplaySelector;
        static readonly Class? NSDockTileClass;
        static readonly Property? SizeProperty;


        // Static initializer.
        static NSDockTile()
        {
            if (Platform.IsNotMacOS)
                return;
            NSDockTileClass = Class.GetClass("NSDockTile");
            if (NSDockTileClass != null)
            {
                BadgeLabelProperty = NSDockTileClass.GetProperty("badgeLabel");
                ContentViewProperty = NSDockTileClass.GetProperty("contentView");
                DisplaySelector = Selector.FromName("display");
                SizeProperty = NSDockTileClass.GetProperty("size");
            }
        }


        // Constructor.
        internal NSDockTile(InstanceHolder instance) : base(instance, false) =>
            this.VerifyClass(NSDockTileClass!);
        

        /// <summary>
        /// Get or set label shown on application badge.
        /// </summary>
        public string? BadgeLabel
        {
            get
            {
                using var label = this.GetProperty<NSString>(BadgeLabelProperty!);
                return label?.ToString();
            }
            set
            {
                if (value == null)
                    this.SetProperty<NSString?>(BadgeLabelProperty!, null);
                else
                {
                    using var label = new NSString(value);
                    this.SetProperty(BadgeLabelProperty!, label);
                }
            }
        }


        /// <summary>
        /// Get or set view to draw content of tile.
        /// </summary>
        public NSObject? ContentView
        {
            get => this.GetProperty<NSObject>(ContentViewProperty!);
            set => this.SetProperty(ContentViewProperty!, value);
        }


        /// <summary>
        /// Redraw content of tile.
        /// </summary>
        public void Display() =>
            this.SendMessage(DisplaySelector!);


        /// <summary>
        /// Get size of tile.
        /// </summary>
        public NSSize Size { get => this.GetProperty<NSSize>(SizeProperty!); }
    }
}