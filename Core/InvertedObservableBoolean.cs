using System;

namespace CarinaStudio
{
    class InvertedObservableBoolean : ObservableValue<bool>
    {
        // Constructor.
        public InvertedObservableBoolean(IObservable<bool> source)
        {
            source.Subscribe(value => this.Value = !value);
        }
    }
}