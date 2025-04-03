using CarinaStudio.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarinaStudio;

/// <summary>
/// <see cref="ObservableValue{Boolean}"/> which forwards values from other <see cref="IObservable{Boolean}"/>.
/// </summary>
public class ForwardedObservableBoolean : ObservableValue<bool>
{
    /// <summary>
    /// Mode of combining values of <see cref="IObservable{Boolean}"/>.
    /// </summary>
    public enum CombinationMode
    {
        /// <summary>
        /// And.
        /// </summary>
        And,
        /// <summary>
        /// Or.
        /// </summary>
        Or,
        /// <summary>
        /// Inversion of <see cref="And"/>.
        /// </summary>
        Nand,
        /// <summary>
        /// Inversion of <see cref="Or"/>.
        /// </summary>
        Nor,
    }


    // Fields.
    readonly CombinationMode combinationMode;
    bool isAttaching;
    IDisposable?[] observerDisposables = [];
    IObservable<bool>[] sources = [];
    bool[] sourceValues = [];


    /// <summary>
    /// Initialize new <see cref="ForwardedObservableBoolean"/> instance.
    /// </summary>
    /// <param name="mode">Mode of combining values of sources.</param>
    /// <param name="defaultValue">Default value if instance doesn't attach to any source.</param>
    public ForwardedObservableBoolean(CombinationMode mode, bool defaultValue = default)
    { 
        this.combinationMode = mode;
        this.DefaultValue = defaultValue;
        this.Value = defaultValue;
    }


    /// <summary>
    /// Initialize new <see cref="ForwardedObservableBoolean"/> instance.
    /// </summary>
    /// <param name="mode">Mode of combining values of <paramref name="sources"/>.</param>
    /// <param name="defaultValue">Default value if instance doesn't attach to any source.</param>
    /// <param name="sources">Source <see cref="IObservable{Boolean}"/> to attach.</param>
    public ForwardedObservableBoolean(CombinationMode mode, bool defaultValue = default, params IObservable<bool>[] sources) :
        this(mode, defaultValue, (IEnumerable<IObservable<bool>>)sources)
    { }


    /// <summary>
    /// Initialize new <see cref="ForwardedObservableBoolean"/> instance.
    /// </summary>
    /// <param name="mode">Mode of combining values of <paramref name="sources"/>.</param>
    /// <param name="defaultValue">Default value if instance doesn't attach to any source.</param>
    /// <param name="sources">Source <see cref="IObservable{Boolean}"/> to attach.</param>
    public ForwardedObservableBoolean(CombinationMode mode, bool defaultValue, IEnumerable<IObservable<bool>> sources)
    {
        this.combinationMode = mode;
        this.DefaultValue = defaultValue;
        if (!(sources is ICollection<IObservable<bool>> collection) || collection.IsNotEmpty())
            this.Attach(sources);
        else
            this.Value = defaultValue;
    }


    /// <summary>
    /// Detach from current sources and attach to given sources.
    /// </summary>
    /// <param name="sources"><see cref="IObservable{Boolean}"/> to attach to.</param>
    public void Attach(params IObservable<bool>[] sources) =>
        this.Attach((IEnumerable<IObservable<bool>>)sources);


    /// <summary>
    /// Detach from current sources and attach to given sources.
    /// </summary>
    /// <param name="sources"><see cref="IObservable{Boolean}"/> to attach to.</param>
    public void Attach(IEnumerable<IObservable<bool>> sources)
    {
        if (this.isAttaching)
            throw new InvalidOperationException();
        this.isAttaching = true;
        try
        {
            // detach
            foreach (var disposable in this.observerDisposables)
                disposable?.Dispose();

            // attach
            this.sources = sources.ToArray();
            var count = this.sources.Length;
            this.sourceValues = new bool[count];
            this.observerDisposables = new IDisposable?[count];
            for (var i = count - 1; i >= 0; --i)
            {
                var index = i;
                this.observerDisposables[i] = this.sources[i].Subscribe(new Observer<bool>(value =>
                {
                    this.sourceValues[index] = value;
                    if (!this.isAttaching)
                        this.UpdateValue(value);
                }));
            }

            // update value
            this.UpdateValue(null);
        }
        finally
        {
            this.isAttaching = false;
        }
    }


    /// <summary>
    /// Default value if instance doesn't attach to any source.
    /// </summary>
    public bool DefaultValue { get; }


    /// <summary>
    /// Detach from current sources.
    /// </summary>
    public void Detach()
    {
        if (this.isAttaching)
            throw new InvalidOperationException();
        if (this.sources.Length == 0)
            return;
        foreach (var disposable in this.observerDisposables)
            disposable?.Dispose();
        this.observerDisposables = [];
        this.sources = [];
        this.sourceValues = [];
        this.UpdateValue(null);
    }


    // Update value.
    void UpdateValue(bool? changedValue)
    {
        switch (this.combinationMode)
        {
            case CombinationMode.And:
                if (changedValue != null && !changedValue.Value)
                {
                    this.Value = false;
                    return;
                }
                for (var i = this.sourceValues.Length - 1; i >= 0; --i)
                {
                    if (!this.sourceValues[i])
                    {
                        this.Value = false;
                        return;
                    }
                }
                this.Value = this.sources.IsNotEmpty() || this.DefaultValue;
                return;
            case CombinationMode.Or:
                if (changedValue != null && changedValue.Value)
                {
                    this.Value = true;
                    return;
                }
                for (var i = this.sourceValues.Length - 1; i >= 0; --i)
                {
                    if (this.sourceValues[i])
                    {
                        this.Value = true;
                        return;
                    }
                }
                this.Value = !this.sources.IsNotEmpty() && this.DefaultValue;
                return;
            case CombinationMode.Nand:
                if (changedValue != null && !changedValue.Value)
                {
                    this.Value = true;
                    return;
                }
                for (var i = this.sourceValues.Length - 1; i >= 0; --i)
                {
                    if (!this.sourceValues[i])
                    {
                        this.Value = true;
                        return;
                    }
                }
                this.Value = !this.sources.IsNotEmpty() && this.DefaultValue;
                return;
            case CombinationMode.Nor:
                if (changedValue != null && changedValue.Value)
                {
                    this.Value = false;
                    return;
                }
                for (var i = this.sourceValues.Length - 1; i >= 0; --i)
                {
                    if (this.sourceValues[i])
                    {
                        this.Value = false;
                        return;
                    }
                }
                this.Value = this.sources.IsNotEmpty() || this.DefaultValue;
                return;
            default:
                throw new NotSupportedException();
        }
    }
}