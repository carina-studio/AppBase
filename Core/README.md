# 📦CarinaStudio.AppBase.Core
This is the core assembly of AppBase providing and extending base functions for .NET Core application and other AppBase assemblies.

## 👉Extensions for *System.Object*
Inspired by Kotlin, we provide some extension methods for ```System.Object``` to make your code more elegant and clear.

### Object.AsNonNull()
Lots of methods generate nullable result. If you can assume that result should not be null, then you can use this method to eliminate extra nullibility checking.

```c#
// The type 'button' will be Button instead of Button?. 
// You don't need to take care of whether 'button' is null or not.
var button = this.FindControl<Button>("button").AsNonNull();
```

### Object.Also()
To perform action on given object and return it. Unlike C# object initializer, you can do anything in this block before you get back the object reference.

```c#
// Create instance and add to list before getting the reference.
var person = new Person().Also((it) =>
{
   it.Name = "John";
   it.Age = 26;
   this.people.Add(it);
});
```

### Object.Let()
To perform action on given object just like ```Also()```, but you can return any type of value as you want.

```c#
// Replace 'John' by 'Steven' in list and get the instance of 'Steven'.
var replacedPerson = this.people.Let((people) =>
{
    for(var i = people.Count - 1 ; i >= 0 ; --i)
    {
        if(people[i].Name == "John")
        {
            return new Person().Also((it) =>
            {
                it.Name = "Steven";
                it.Age = 30;
                people[i] = it;
            });
        }
    }
    return null;
});
```

### Object.Lock()
To hold the lock of given object, perform action and release the lock. It does almost same as ```lock``` block of C# but you can generate a value from ```Lock()``` as you want.

```c#
// Replace value in map and get the previous value.
var previousValue = this.map.Lock((it) =>
{
    it.TryGetValue(key, out var previousValue);
    it[key] = newValue;
    return previousValue;
});

// Which is same as:
object? previousValue = null;
lock(this.map)
{
    this.map.TryGetValue(key, out previousValue);
    this.map[key] = newValue;
}
```

## 👉Extensions for *System.IDisposable*

### IDisposable?.DisposeAndReturnNull()
To call ```Dispose()``` if reference is not null, then return null. Usually be used for resource releasing.

```c#
IDisposable? allocatedResource; // This is a field.
...
this.allocatedResource = this.allocatedResource.DisposeAndReturnNull();
```

### IDisposable.Exchange()
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

### IDisposable.Use()
To perform action on given ```IDisposable``` and dispose it after using it. It does almost same as C# ```using``` block but you can return a value from ```Use()``` as you want.

```c#
// Read string from file.
var str = new StreamReader(filePath, Encoding.UTF8).Use((it) => it.ReadToEnd());

// Which is same as:
string? str = null;
using(var reader = new StreamReader(filePath, Encoding.UTF8))
    str = reader.ReadToEnd();
```

## 👉*ObservableValue&lt;T&gt;*
Combination of ```IObservable<T>``` and a value just like ```LiveData<T>``` in Android. You may use it with ```ReactiveCommand``` provided by [ReactiveUI](https://github.com/reactiveui/ReactiveUI).

```c#
ObservableValue<bool> canOpenFile = new ObservableValue<bool>(); // This is a field.
...
// Create an ICommand and bind ICommand.CanExecute() with 'canOpenFile'.
this.OpenFileCommand = ReactiveCommand.Create(this.OpenFile, this.canOpenFile);
...
// This is the actual action of command 'OpenFileCommand'.
void OpenFile()
{
    ...
}
```

## 👉*MutableObservableValue&lt;T&gt;*
```ObservableValue<T>``` is an abstract class, so we provide ```MutableObservableValue<T>``` to let you easy to update value of ```ObservableValue<T>``` in your code. You can still create your own class extends from ```ObservableValue<T>``` and update value by itself. Further more, we also provide some classes extends from ```MutableObservableValue<T>``` specialized for some type of values:

* ```MutableObservableBoolean```
* ```MutableObservableInt32```
* ```MutableObservableInt64```
* ```MutableObservableString```

```c#
MutableObservableBoolean canOpenFile = new MutableObservableBoolean(); // This is a field.
...
// Create an ICommand and bind ICommand.CanExecute() with 'canOpenFile'.
this.OpenFileCommand = ReactiveCommand.Create(this.OpenFile, this.canOpenFile);
...
// This is the actual action of command 'OpenFileCommand'.
void OpenFile()
{
    if(this.canOpenFile)
        ...
}
...
// set 'canOpenFile' to true when matching some conditions.
this.canOpenFile.Update(true);
```
