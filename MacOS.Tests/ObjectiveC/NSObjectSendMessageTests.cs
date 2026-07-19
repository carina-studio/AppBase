using CarinaStudio.MacOS.CoreGraphics;
using NUnit.Framework;

namespace CarinaStudio.MacOS.ObjectiveC
{
    /// <summary>
    /// Tests of sending message with arbitrary signature through <see cref="NSObject"/>.
    /// </summary>
    [TestFixture]
    public class NSObjectSendMessageTests
    {
        /// <summary>
        /// Test for sending message with boolean and numeric arguments and return values.
        /// </summary>
        [Test]
        public void PrimitiveArgumentsTest()
        {
            this.VerifyPlatform();
            var numberClass = Class.GetClass("NSNumber").AsNonNull();
            using var intNumber = NSObject.SendMessage<NSObject>(numberClass.Handle, Selector.FromName("numberWithInt:"), 12345);
            using var sameIntNumber = NSObject.SendMessage<NSObject>(numberClass.Handle, Selector.FromName("numberWithInt:"), 12345);
            using var doubleNumber = NSObject.SendMessage<NSObject>(numberClass.Handle, Selector.FromName("numberWithDouble:"), 6789.5);
            using var boolNumber = NSObject.SendMessage<NSObject>(numberClass.Handle, Selector.FromName("numberWithBool:"), true);
            var isEqualToNumberSelector = Selector.FromName("isEqualToNumber:");
            Assert.That(intNumber.SendMessage<int>(Selector.FromName("intValue")), Is.EqualTo(12345));
            Assert.That(doubleNumber.SendMessage<double>(Selector.FromName("doubleValue")), Is.EqualTo(6789.5));
            Assert.That(boolNumber.SendMessage<bool>(Selector.FromName("boolValue")), Is.True);
            Assert.That(intNumber.SendMessage<bool>(isEqualToNumberSelector, sameIntNumber), Is.True);
            Assert.That(intNumber.SendMessage<bool>(isEqualToNumberSelector, doubleNumber), Is.False);
        }


        /// <summary>
        /// Test for sending message with structure argument and mixed arguments.
        /// </summary>
        [Test]
        public void StructureArgumentTest()
        {
            this.VerifyPlatform();
            using var str = new NSString("Hello, World!");
            using var substring = str.SendMessage<NSString>(Selector.FromName("substringWithRange:"), new NSRange(7, 5));
            Assert.That(substring.ToString(), Is.EqualTo("World"));
            using var replacement = new NSString("Claude");
            using var replaced = str.SendMessage<NSString>(Selector.FromName("stringByReplacingCharactersInRange:withString:"), new NSRange(7, 5), replacement);
            Assert.That(replaced.ToString(), Is.EqualTo("Hello, Claude!"));
        }


        /// <summary>
        /// Test for sending message which returns structure.
        /// </summary>
        [Test]
        public void StructureReturningTest()
        {
            this.VerifyPlatform();

            // structure fitting in registers
            using var str = new NSString("Hello, World!");
            using var substring = new NSString("World");
            var range = str.SendMessage<NSRange>(Selector.FromName("rangeOfString:"), substring);
            Assert.That((int)range.Location, Is.EqualTo(7));
            Assert.That((int)range.Length, Is.EqualTo(5));

            // structure larger than 16 bytes which needs objc_msgSend_stret on x64
            var valueClass = Class.GetClass("NSValue").AsNonNull();
            var rect = new CGRect(10, 20, 30, 40);
            using var value = NSObject.SendMessage<NSObject>(valueClass.Handle, Selector.FromName("valueWithRect:"), rect);
            var returnedRect = value.SendMessage<CGRect>(Selector.FromName("rectValue"));
            Assert.That(returnedRect.Origin.X, Is.EqualTo(rect.Origin.X));
            Assert.That(returnedRect.Origin.Y, Is.EqualTo(rect.Origin.Y));
            Assert.That(returnedRect.Size.Width, Is.EqualTo(rect.Size.Width));
            Assert.That(returnedRect.Size.Height, Is.EqualTo(rect.Size.Height));
        }


        // Make sure that current platform is macOS.
        void VerifyPlatform()
        {
            if (Platform.IsNotMacOS)
                Assert.Ignore("Tests can run on macOS only.");
        }
    }
}
