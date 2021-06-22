[![](https://img.shields.io/nuget/v/CarinaStudio.AppBase.Core.svg)](https://www.nuget.org/packages/CarinaStudio.AppBase.Core) 
[![](https://img.shields.io/github/license/carina-studio/AppBase)](https://github.com/carina-studio/AppBase/blob/master/LICENSE) 
[![](https://img.shields.io/github/release-date-pre/carina-studio/AppBase)](https://github.com/carina-studio/AppBase/releases) 

# 👋Introduction of AppBase 
**AppBase** is a set of libraries designed for .NET based application. Currently there are 4 packages providing the following functions:

📦**CarinaStudio.AppBase.Core**
- Extensions for ```System.Object``` to make your code more elegant and clear. ([Learn more](articles/object_extensions.md))
- Extensions for ```System.IDisposable``` to make your code more elegant and clear. ([Learn more](articles/disposable_extensions.md))
- Extensions for arrays and ```System.Memory<T>``` to get memory address.
- Extended ```System.IDisposable``` to make it easier to share resources across components in your application.
- Extensions for ```System.Collections.Generic.*``` for collection state checking and searching. ([Learn more](articles/collection_extensions.md))
- ```SortedList<T>``` to let you easy to build sorted list and display on UI. ([Learn more](articles/sorted_list.md))
- ```TypeConvertedList<S, D>``` to help you to build a list based-on another list with different type of element. ([Learn more](articles/type_converted_list.md))
- Extensions for ```System.Threading.SynchronizationContext``` for delayed call-back support. ([Learn mode](articles/threading.md#extensions-for-systemthreadingsynchronizationcontext))
- Implementations of ```System.Threading.SynchronizationContext``` and ```System.Threading.Tasks.TaskScheduler``` to let you schedule and run tasks on dedicated threads. ([Learn more](articles/threading.md#singlethreadsynchronizationcontext))
- Schedulable action to execute action later and prevent duplicate execution. ([Learn more](articles/threading.md#scheduledaction))
- Implementation of ```System.IObservable<T>``` to support observable value/field just like ```LiveData<T>``` on Android. ([Learn more](articles/observable_value.md))

📦**CarinaStudio.AppBase.Configuration**
- Class for key-value based application settings which supports various type of value.
- Save application settings to given file.
- Extensible settings serialization/deserializing. Implementations based-on JSON and XML format are included.

📦**CarinaStudio.AppBase.Application**
- ```IApplication``` to provide abstract interface to application instance no matter what UI framework you use.
- ```ViewModel``` to provide base implementation of view-model including observable properties, logger, etc.

📦**CarinaStudio.AppBase.Tests**
- Provide random functions like generating random string or creating file with random name.
- ```EventMonitor<T>``` to monitor and track event raising.
- ```INotifyPropertyChanged.WaitForPropertyAsync()``` to wait for specfic property to be given value.

# 📥Install to your project
AppBase has been uploaded to **NuGet**, you can find it on:
- [https://www.nuget.org/packages/CarinaStudio.AppBase.Core/](https://www.nuget.org/packages/CarinaStudio.AppBase.Core/)
- [https://www.nuget.org/packages/CarinaStudio.AppBase.Configuration/](https://www.nuget.org/packages/CarinaStudio.AppBase.Configuration/)
- [https://www.nuget.org/packages/CarinaStudio.AppBase.Application/](https://www.nuget.org/packages/CarinaStudio.AppBase.Application/)
- [https://www.nuget.org/packages/CarinaStudio.AppBase.Tests/](https://www.nuget.org/packages/CarinaStudio.AppBase.Tests/)

You can also install by Package Manager command:
```
Install-Package CarinaStudio.AppBase.Core
Install-Package CarinaStudio.AppBase.Configuration
Install-Package CarinaStudio.AppBase.Application
Install-Package CarinaStudio.AppBase.Tests
```

# 📁Source code
You can find source code on [GitHub](https://github.com/carina-studio/AppBase).