[![NuGet](https://img.shields.io/nuget/v/CarinaStudio.AppBase.Core.svg)](https://www.nuget.org/packages/CarinaStudio.AppBase.Core) [![License](https://img.shields.io/github/license/carina-studio/AppBase)](https://github.com/carina-studio/AppBase/blob/master/LICENSE) [![Releases](https://img.shields.io/github/release-date-pre/carina-studio/AppBase)](https://github.com/carina-studio/AppBase/releases) 

# 👋Introduction of AppBase 
**AppBase** is a set of libraries designed for .NET based application. Currently there are 2 modules providing the following functions:

📦[CarinaStudio.AppBase.Core](https://github.com/carina-studio/AppBase/tree/master/Core#carinastudioappbasecore)
 * Extensions for ```System.Object``` and ```System.IDisposable``` to make your code more elegant and clear. ([Learn more](https://github.com/carina-studio/AppBase/tree/master/Core#extensions-for-systemobject))
 * Extensions for arrays and ```System.Memory<T>``` to get memory address.
* Extended ```System.IDisposable``` to make it easier to share resources across components in your application.
* Extensions for ```System.Collections.Generic.*``` for collection state checking and searching. ([Learn more](https://github.com/carina-studio/AppBase/tree/master/Core/Collections#extensions-for-systemcollectionsgenericicollectiont))
* Extensions for ```System.Threading.SynchronizationContext``` for delayed call-back support. ([Learn mode](https://github.com/carina-studio/AppBase/tree/master/Core/Threading#extensions-for-systemthreadingsynchronizationcontext))
* Implementations of ```System.Threading.SynchronizationContext``` and ```System.Threading.Tasks.TaskScheduler``` to let you schedule and run tasks on dedicated threads. ([Learn more](https://github.com/carina-studio/AppBase/tree/master/Core/Threading#singlethreadsynchronizationcontext))
* Schedulable action to execute action later and prevent duplicate execution. ([Learn more](https://github.com/carina-studio/AppBase/tree/master/Core/Threading#scheduledaction))
* Implementation of ```System.IObservable<T>``` to support observable value/field just like ```LiveData<T>``` on Android. ([Learn more](https://github.com/carina-studio/AppBase/tree/master/Core#observablevaluet))

📦CarinaStudio.AppBase.Configuration
* Class for key-value based application settings which supports various type of value.
* Save application settings to given file.
* Extensible settings serialization/deserializing. Implementations based-on JSON and XML format are included.

# 📥Install to your project
AppBase has been uploaded to **NuGet**, you can find it on:
* [https://www.nuget.org/packages/CarinaStudio.AppBase.Core/](https://www.nuget.org/packages/CarinaStudio.AppBase.Core/)
* [https://www.nuget.org/packages/CarinaStudio.AppBase.Configuration/](https://www.nuget.org/packages/CarinaStudio.AppBase.Configuration/)

You can also install by Package Manager command:
```
Install-Package CarinaStudio.AppBase.Core
Install-Package CarinaStudio.AppBase.Configuration
```

# 📃API Documentation
You can find API documentation [HERE](https://carina-studio.github.io/AppBase/Documentation/api/).
