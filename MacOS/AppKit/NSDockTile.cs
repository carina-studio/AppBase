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
        static readonly Property? ShowBadgeProperty;
        static readonly Property? SizeProperty;


        // Static initializer.
        static NSDockTile()
        {
            if (Platform.IsNotMacOS)
                return;
            NSDockTileClass = Class.GetClass("NSDockTile");
            if (NSDockTileClass != null)
            {
                NSDockTileClass.TryGetProperty("badgeLabel", out BadgeLabelProperty);
                NSDockTileClass.TryGetProperty("contentView", out ContentViewProperty);
                DisplaySelector = Selector.FromName("display");
                NSDockTileClass.TryGetProperty("showsApplicationBadge", out ShowBadgeProperty);
                NSDockTileClass.TryGetProperty("size", out SizeProperty);
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
        /// Get or set whether application badge is visible or not.
        /// </summary>
        public bool ShowsApplicationBadge
        {
            get => this.GetProperty<bool>(ShowBadgeProperty!);
            set => this.SetProperty(ShowBadgeProperty!, value);
        }


        /// <summary>
        /// Get size of tile.
        /// </summary>
        public NSSize Size { get => this.GetProperty<NSSize>(SizeProperty!); }
    }
}