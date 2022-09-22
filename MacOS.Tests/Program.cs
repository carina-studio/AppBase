namespace CarinaStudio.MacOS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var cfs1 = new CoreFoundation.CFString("Hello World!");
            using var cfs2 = CoreFoundation.CFObject.Wrap<CoreFoundation.CFString>(cfs1.Handle, false);
        }
    }
}