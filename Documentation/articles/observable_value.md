# *ObservableValue&lt;T&gt;*
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

# *MutableObservableValue&lt;T&gt;*
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