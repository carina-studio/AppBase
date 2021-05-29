# Introduction
**AppBase** is a set of libraries mainly designed for .NET Core based desktop application. Currently there are 2 modules providing the following functions:

* **CarinaStudio.AppBase.Core**
  * Extensions for *System.Object* and *System.IDisposable* to make your code more elegant and clear.
  * Extended *System.IDisposable* to make it easier to share resources across components in your application.
  * Extensions for *System.Collections.Generic.\** for collection state checking and searching.
  * Extensions for *System.Threading.SynchronizationContext* for delayed call-back support.
  * Schedulable action to execute action later and prevent duplicate execution.
  * Implementation of *System.IObservable&lt;T&gt;* to support observable value/field just like LiveData on Android.
* **CarinaStudio.AppBase.Configuration**
  * Class for key-value based application settings which supports various type of value.
  * Save application settings to given file.
  * Extensible settings serialization/deserializing. Implementation based-on JSON format is included.

# Install to your project
AppBase has been uploaded to **NuGet**, you can find it on:
* [https://www.nuget.org/packages/CarinaStudio.AppBase.Core/](https://www.nuget.org/packages/CarinaStudio.AppBase.Core/)
* [https://www.nuget.org/packages/CarinaStudio.AppBase.Configuration/](https://www.nuget.org/packages/CarinaStudio.AppBase.Configuration/)

You can also install by Package Manager command:
```
Install-Package CarinaStudio.AppBase.Core
Install-Package CarinaStudio.AppBase.Configuration
```

# Source code
You can find source code on [GitHub](https://github.com/carina-studio/AppBase).