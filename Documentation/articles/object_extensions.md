# Extensions for *System.Object*
Inspired by Kotlin, we provide some extension methods for ```System.Object``` to make your code more elegant and clear.

## AsNonNull()
Lots of methods generate nullable result. If you can assume that result should not be null, then you can use this method to eliminate extra nullibility checking.

```c#
// The type 'button' will be Button instead of Button?. 
// You don't need to take care of whether 'button' is null or not.
var button = this.FindControl<Button>("button").AsNonNull();
```

## Also()
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

## Let()
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

## Lock()
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