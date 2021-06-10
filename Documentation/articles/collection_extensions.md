# Extensions for *System.Collections.Generic.ICollection&lt;T&gt;*
## Empty checking
There are 3 extension methods for you to check whether ```ICollection<T>``` is empty/null or not:
* ```IsEmpty()```
* ```IsNotEmpty()```
* ```IsNullOrEmpty()```

Arrays also implement ```ICollection<T>``` interface, so you can use these methods on arrays.

```c#
void Foo<T>(ICollection<T> list)
{
    if(list.IsEmpty())
        return;
    ...
}
```

# Extensions for *System.Collections.Generic.IList&lt;T&gt;*
## AsReadOnly()
Make list as read-only list.

## Binary search
Instead of calling ```BinarySearch()``` on specific type (ex, ```Arrays```, ```List<T>```), now you can use binary search on all types which implement ```IList<T>``` interface.

You can provide ```IComparer<T>``` or ```Comparison<T>``` for binary search, or search element which implements ```IComparable<T>``` without providing any additional comparison function.

```c#
void Foo<T>(IList<T> list, T target) where T : IComparable<T>
{
    var index = list.BinarySearch(target); // No need to check the actual type of 'list'.
    ...
}
```

## CopyTo() and ToArray()
Both ```CopyTo()``` defined in ```ICollection<T>``` and ```ToArray()``` provided by ```System.Linq.Enumerable``` are designed for copying all items from collection. We provide ```CopyTo(int, int)``` and ```ToArray(int, int)``` to let you be able to copy sub range of items from ```IList<T>```.

## Shuffle()
To make items in given ```IList<T>``` shuffled randomly.

# Extensions for *System.Collections.IEnumerable*
## ContentToString()
To generate string which describes the content of ```IEnumerable```, the format of string will be ```[element1, element2, ...]```. Usually it is used for debugging purpose.
