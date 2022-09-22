namespace CarinaStudio.MacOS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var number1 = new CoreFoundation.CFNumber(123.456);
            var b = number1.ToByte();
            var s = number1.ToInt16();
            var i = number1.ToInt32();
            var u = number1.ToUInt32();
        }
    }
}