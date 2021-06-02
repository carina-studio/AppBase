# Extensions for *System.IDisposable*

## DisposeAndReturnNull()
To call ```Dispose()``` if reference is not null, then return null. Usually be used for resource releasing.

```c#
IDisposable? allocatedResource; // This is a field.
...
this.allocatedResource = this.allocatedResource.DisposeAndReturnNull();
```

## Exchange()
To generate another ```IDisposable``` and dispose original one if new ```IDisposable``` instance is different from original one.

```c#
// Load and scale bitmap if needed.
var scaledBitmap = Bitmap.Load(filePath).Exchange((bitmap) =>
{
    if(this.NeedToScale(bitmap))
        return bitmap.Scale(...);
    return bitmap;
});

// Which is same as:
Bitmap? scaledBitmap = null;
Bitmap bitmap = Bitmap.Load(filePath);
try
{
    if(this.NeedToScale(bitmap))
        scaledBitmap = this.NeedToScale(bitmap);
    else
        scaledBitmap = bitmap;
}
finally
{
    if(scaledBitmap != bitmap)
        bitmap.Dispose();
}
```

## Use()
To perform action on given ```IDisposable``` and dispose it after using it. It does almost same as C# ```using``` block but you can return a value from ```Use()``` as you want.

```c#
// Read string from file.
var str = new StreamReader(filePath, Encoding.UTF8).Use((it) => it.ReadToEnd());

// Which is same as:
string? str = null;
using(var reader = new StreamReader(filePath, Encoding.UTF8))
    str = reader.ReadToEnd();
```