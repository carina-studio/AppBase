# TypeConvertedList&lt;S, D&gt;
```TypeConvertedList<S, D>``` is an ```IList<D>``` to help you to build a list based-on another ```IList<S>``` with different type of element.
```TypeConvertedList<S, D>``` is a read-only list, the elements in list are built completely based-on source list.
Items in ```TypeConvertedList<S, D>``` will update automatically if source list implements ```INotifyCollectionChanged``` interface.
Otherwise, items will be built only when creating the ```TypeConvertedList<S, D>``` instance.

```TypeConvertedList<S, D>``` is an abstract class, you need to extend it and implement conversion methods for items creation and releasing.

## Built item D based on item S from source list
You need to implement ```ConvertElement()``` to build item in ```TypeConvertedList<S, D>```.

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

class DisplayedPersonList: TypeConvertedList<Person, DisplayedPerson>
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
Sometimes items in ```TypeConvertedList<S, D>``` you built may have deep connection with items in source list (ex, adding event handler to source item). In this case, you can override ```ReleaseElement()``` to destroy connection or release related resources.

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

class DisplayedPersonList: TypeConvertedList<Person, DisplayedPerson>
{
    protected override void ReleaseElement(DisplayedPerson element)
    {
        element.Person = null;
        base.ReleaseElement(element);
    }
}
```