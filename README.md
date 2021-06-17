[![](https://img.shields.io/nuget/v/CarinaStudio.AppBase.Core.svg)](https://www.nuget.org/packages/CarinaStudio.AppBase.Core) 
[![](https://img.shields.io/github/license/carina-studio/AppBase)](https://github.com/carina-studio/AppBase/blob/master/LICENSE) 
[![](https://img.shields.io/github/release-date-pre/carina-studio/AppBase)](https://github.com/carina-studio/AppBase/releases) 

# 👋Introduction of AppBase 
**AppBase** is a set of libraries designed for .NET based application. Currently there are 3 packages providing the following functions:

📦**CarinaStudio.AppBase.Core**
- Extensions for ```System.Object``` to make your code more elegant and clear. ([Learn more](https://carina-studio.github.io/AppBase/articles/object_extensions.html))
- Extensions for ```System.IDisposable``` to make your code more elegant and clear. ([Learn more](https://carina-studio.github.io/AppBase/articles/disposable_extensions.html))
- Extensions for arrays and ```System.Memory<T>``` to get memory address.
- Extended ```System.IDisposable``` to make it easier to share resources across components in your application.
- Extensions for ```System.Collections.Generic.*``` for collection state checking and searching. ([Learn more](https://carina-studio.github.io/AppBase/articles/collection_extensions.html))
- ```SortedList<T>``` to let you easy to build sorted list and display on UI. ([Learn more](https://carina-studio.github.io/AppBase/articles/sorted_list.html))
- ```TypeConvertedList<S, D>``` to help you to build a list based-on another list with different type of element. ([Learn more](https://carina-studio.github.io/AppBase/articles/type_converted_list.html))
- Extensions for ```System.Threading.SynchronizationContext``` for delayed call-back support. ([Learn mode](https://carina-studio.github.io/AppBase/articles/threading.html#extensions-for-systemthreadingsynchronizationcontext))
- Implementations of ```System.Threading.SynchronizationContext``` and ```System.Threading.Tasks.TaskScheduler``` to let you schedule and run tasks on dedicated threads. ([Learn more](https://carina-studio.github.io/AppBase/articles/threading.html#singlethreadsynchronizationcontext))
- Schedulable action to execute action later and prevent duplicate execution. ([Learn more](https://carina-studio.github.io/AppBase/articles/threading.html#scheduledaction))
- Implementation of ```System.IObservable<T>``` to support observable value/field just like ```LiveData<T>``` on Android. ([Learn more](https://carina-studio.github.io/AppBase/articles/observable_value.html))

📦**CarinaStudio.AppBase.Configuration**
- Class for key-value based application settings which supports various type of value.
- Save application settings to given file.
- Extensible settings serialization/deserializing. Implementations based-on JSON and XML format are included.

📦**CarinaStudio.AppBase.Application**
- ```IApplication``` to provide abstract interface to application instance no matter what UI framework you use.
- ```ViewModel``` to provide base implementation of view-model including observable properties, logger, etc.

# 📥Install to your project
AppBase has been uploaded to **NuGet**, you can find it on:
* [https://www.nuget.org/packages/CarinaStudio.AppBase.Core/](https://www.nuget.org/packages/CarinaStudio.AppBase.Core/)
* [https://www.nuget.org/packages/CarinaStudio.AppBase.Configuration/](https://www.nuget.org/packages/CarinaStudio.AppBase.Configuration/)

You can also install by Package Manager command:
```
Install-Package CarinaStudio.AppBase.Core
Install-Package CarinaStudio.AppBase.Configuration
```

# 📔API Documentation
You can find API documentation [HERE](https://carina-studio.github.io/AppBase/api/).
