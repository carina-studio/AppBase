namespace CarinaStudio.MacOS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var cfs1 = new CoreFoundation.CFString("Hello World!");
            var cfs2 = cfs1.Retain();
            var cfs3 = CoreFoundation.CFString.Wrap(cfs1.Handle);
            cfs1.Release();

            var b = cfs1 == cfs2;
            b = cfs1 == cfs3;
            b = cfs2 == cfs3;
            b = cfs1 == null;
        }
    }
}