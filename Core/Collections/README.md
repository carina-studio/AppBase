## 👉Extensions for *System.Collections.Generic.ICollection&lt;T&gt;*
### Empty checking
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

## 👉Extensions for *System.Collections.Generic.IList&lt;T&gt;*
### Binary search
Instead of calling ```BinarySearch()``` on specific type (ex, ```Arrays```, ```List<T>```), now you can use binary search on all types which implement ```IList<T>``` interface.

You can provide ```IComparer<T>``` or ```Comparison<T>``` for binary search, or search element which implements ```IComparable<T>``` without providing any additional comparison function.

```c#
void Foo<T>(IList<T> list, T target) where T : IComparable<T>
{
    var index = list.BinarySearch(target); // No need to check the actual type of 'list'.
    ...
}
```

## 👉Extensions for *System.Collections.IEnumerable*
### IEnumerable.ContentToString()
To generate string which describes the content of ```IEnumerable```, the format of string will be ```[element1, element2, ...]```. Usually it is used for debugging purpose.
