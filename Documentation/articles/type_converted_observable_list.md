# TypeConvertedObservableList&lt;S, D&gt;
```TypeConvertedObservableList<S, D>``` is an ```IList<D>``` to help you to build a list based-on another ```IList<S>``` with different type of element.
```TypeConvertedObservableList<S, D>``` is a read-only list, the elements in list are built completely based-on source list.
Items in ```TypeConvertedObservableList<S, D>``` will update automatically if source list implements ```INotifyCollectionChanged``` interface.
Otherwise, items will be built only when creating the ```TypeConvertedObservableList<S, D>``` instance.

```TypeConvertedObservableList<S, D>``` is an abstract class, you need to extend it and implement conversion methods for items creation and releasing.

## Built item D based on item S from source list
You need to implement ```ConvertElement()``` to build item in ```TypeConvertedObservableList<S, D>```.

```c#
class Person 
{
    string FirstName { get; }
    string LastName { get; }
}
class DisplayedPerson 
{
    string DisplayedName { get; set; }
    Person Person { get; set; }
}

class DisplayedPersonList: TypeConvertedObservableList<Person, DisplayedPerson>
{
    protected override DisplayedPerson ConvertElement(Person source)
    {
        return new DisplayedPerson()
        {
            Person = source,
            DisplayedName = source.FirstName + " " + source.LastName,
        };
    }
}
```

## Release item
Sometimes items in ```TypeConvertedObservableList<S, D>``` you built may have deep connection with items in source list (ex, adding event handler to source item). In this case, you can override ```ReleaseElement()``` to destroy connection or release related resources.

```c#
class Person 
{
    string FirstName { get; }
    string LastName { get; }
}
class DisplayedPerson 
{
    string DisplayedName { get; set; }
    Person Person { get; set; }
}

class DisplayedPersonList: TypeConvertedObservableList<Person, DisplayedPerson>
{
    protected override void ReleaseElement(DisplayedPerson element)
    {
        element.Person = null;
        base.ReleaseElement(element);
    }
}
```