using System;
using Avalonia;
using Avalonia.Threading;

namespace CarinaStudio
{
    /// <summary>
    /// Object to generate formatted string.
    /// </summary>
    public class FormattedString : AvaloniaObject, IObservable<string>
    {
        /// <summary>
        /// Property of <see cref="Arg1"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg1Property = AvaloniaProperty.Register<FormattedString, object?>(nameof(Arg1));
        /// <summary>
        /// Property of <see cref="Arg2"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg2Property = AvaloniaProperty.Register<FormattedString, object?>(nameof(Arg2));
        /// <summary>
        /// Property of <see cref="Arg3"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg3Property = AvaloniaProperty.Register<FormattedString, object?>(nameof(Arg3));
        /// <summary>
        /// Property of <see cref="Arg4"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg4Property = AvaloniaProperty.Register<FormattedString, object?>(nameof(Arg4));
        /// <summary>
        /// Property of <see cref="Arg5"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg5Property = AvaloniaProperty.Register<FormattedString, object?>(nameof(Arg5));
        /// <summary>
        /// Property of <see cref="Arg6"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg6Property = AvaloniaProperty.Register<FormattedString, object?>(nameof(Arg6));
        /// <summary>
        /// Property of <see cref="Arg7"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg7Property = AvaloniaProperty.Register<FormattedString, object?>(nameof(Arg7));
        /// <summary>
        /// Property of <see cref="Arg8"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg8Property = AvaloniaProperty.Register<FormattedString, object?>(nameof(Arg8));
        /// <summary>
        /// Property of <see cref="Arg9"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg9Property = AvaloniaProperty.Register<FormattedString, object?>(nameof(Arg9));
        /// <summary>
        /// Property of <see cref="Format"/>.
        /// </summary>
        public static readonly AvaloniaProperty<string?> FormatProperty = AvaloniaProperty.Register<FormattedString, string?>(nameof(Format));
        /// <summary>
        /// Property of <see cref="String"/>.
        /// </summary>
        public static readonly AvaloniaProperty<string> StringProperty = AvaloniaProperty.RegisterDirect<FormattedString, string>(nameof(String), fs => fs.String);


        // Control block of subscribed observers.
        class SubscribedObserver : IDisposable
        {
            // Fields.
            public bool IsDisposed;
            public readonly IObserver<string> Observer;
            public SubscribedObserver? Previous;
            public SubscribedObserver? Next;
            public readonly FormattedString Owner;

            // Constructor.
            public SubscribedObserver(FormattedString owner, IObserver<string> observer)
            {
                this.Observer = observer;
                this.Owner = owner;
            }

            // Dispose.
            public void Dispose() =>
                this.Owner.Unsubscribe(this);
        }


        // Fields.
        readonly IObserver<object?> argObserver;
        string formattedString = "";
        bool hasPendingDisposedObservers;
        bool isUpdatingStringScheduled;
        int notifyingCounter;
        SubscribedObserver? observerListHead;
        readonly Action updateStringAction;


        /// <summary>
        /// Initialize new <see cref="FormattedString"/> instance.
        /// </summary>
        public FormattedString()
        {
            Dispatcher.UIThread.VerifyAccess();
            this.argObserver = new Observer<object?>(_ => this.ScheduleUpdatingString());
            this.updateStringAction = () =>
            {
                if (this.isUpdatingStringScheduled)
                {
                    this.isUpdatingStringScheduled = false;
                    this.UpdateString();
                }
            };
            this.GetObservable(Arg1Property).Subscribe(this.argObserver);
            this.GetObservable(Arg2Property).Subscribe(this.argObserver);
            this.GetObservable(Arg3Property).Subscribe(this.argObserver);
            this.GetObservable(Arg4Property).Subscribe(this.argObserver);
            this.GetObservable(Arg5Property).Subscribe(this.argObserver);
            this.GetObservable(Arg6Property).Subscribe(this.argObserver);
            this.GetObservable(Arg7Property).Subscribe(this.argObserver);
            this.GetObservable(Arg8Property).Subscribe(this.argObserver);
            this.GetObservable(Arg9Property).Subscribe(this.argObserver);
            this.GetObservable(FormatProperty).Subscribe(this.argObserver);
        }


        /// <summary>
        /// Get or set 1st argument to generate formatted string.
        /// </summary>
        public object? Arg1
        {
            get => this.GetValue<object?>(Arg1Property);
            set => this.SetValue(Arg1Property, value);
        }


        /// <summary>
        /// Get or set 2nd argument to generate formatted string.
        /// </summary>
        public object? Arg2
        {
            get => this.GetValue<object?>(Arg2Property);
            set => this.SetValue(Arg2Property, value);
        }


        /// <summary>
        /// Get or set 3rd argument to generate formatted string.
        /// </summary>
        public object? Arg3
        {
            get => this.GetValue<object?>(Arg3Property);
            set => this.SetValue(Arg3Property, value);
        }


        /// <summary>
        /// Get or set 4th argument to generate formatted string.
        /// </summary>
        public object? Arg4
        {
            get => this.GetValue<object?>(Arg4Property);
            set => this.SetValue(Arg4Property, value);
        }


        /// <summary>
        /// Get or set 5th argument to generate formatted string.
        /// </summary>
        public object? Arg5
        {
            get => this.GetValue<object?>(Arg5Property);
            set => this.SetValue(Arg5Property, value);
        }


        /// <summary>
        /// Get or set 6th argument to generate formatted string.
        /// </summary>
        public object? Arg6
        {
            get => this.GetValue<object?>(Arg6Property);
            set => this.SetValue(Arg6Property, value);
        }


        /// <summary>
        /// Get or set 7th argument to generate formatted string.
        /// </summary>
        public object? Arg7
        {
            get => this.GetValue<object?>(Arg7Property);
            set => this.SetValue(Arg7Property, value);
        }


        /// <summary>
        /// Get or set 8th argument to generate formatted string.
        /// </summary>
        public object? Arg8
        {
            get => this.GetValue<object?>(Arg8Property);
            set => this.SetValue(Arg8Property, value);
        }


        /// <summary>
        /// Get or set 9th argument to generate formatted string.
        /// </summary>
        public object? Arg9
        {
            get => this.GetValue<object?>(Arg9Property);
            set => this.SetValue(Arg9Property, value);
        }


        /// <summary>
        /// Get or set string format.
        /// </summary>
        public string? Format
        {
            get => this.GetValue<string?>(FormatProperty);
            set => this.SetValue(FormatProperty, value);
        }


        // Notify change.
        void Notify()
        {
            if (this.observerListHead == null)
                return;
            var formattedString = this.formattedString;
            var observer = this.observerListHead;
            ++this.notifyingCounter;
            try
            {
                while (observer != null)
                {
                    var nextObserver = observer.Next;
                    if (!observer.IsDisposed)
                        observer.Observer.OnNext(formattedString);
                    observer = nextObserver;
                }
            }
            finally
            {
                --this.notifyingCounter;
                if (this.notifyingCounter <= 0 && this.hasPendingDisposedObservers)
                {
                    this.hasPendingDisposedObservers = false;
                    observer = this.observerListHead;
                    while (observer != null)
                    {
                        var nextObserver = observer.Next;
                        if (observer.IsDisposed)
                        {
                            if (this.observerListHead == observer)
                                this.observerListHead = observer.Next;
                            if (observer.Previous != null)
                                observer.Previous.Next = observer.Next;
                            if (observer.Next != null)
                                observer.Next.Previous = observer.Previous;
                            observer.Previous = null;
                            observer.Next = null;
                        }
                        observer = nextObserver;
                    }
                }
            }
        }


        // Schedule updating formatted string.
        void ScheduleUpdatingString()
        {
            if (!this.isUpdatingStringScheduled)
            {
                this.isUpdatingStringScheduled = true;
                Dispatcher.UIThread.Post(this.updateStringAction, DispatcherPriority.Normal);
            }
        }


        /// <summary>
        /// Get formatted string.
        /// </summary>
        public string String 
        { 
            get
            {
                if (this.isUpdatingStringScheduled)
                {
                    this.isUpdatingStringScheduled = false;
                    this.UpdateString();
                }
                return this.formattedString;
            }
        }


        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<string> observer)
        {
            // update formatted string if needed
            if (this.isUpdatingStringScheduled)
            {
                this.isUpdatingStringScheduled = false;
                this.UpdateString();
            }

            // add observer to list
            var subscribedObserver = new SubscribedObserver(this, observer);
            subscribedObserver.Next = this.observerListHead;
            if (this.observerListHead != null)
                this.observerListHead.Previous = subscribedObserver;
            this.observerListHead = subscribedObserver;

            // complete
             observer.OnNext(this.formattedString);
            return subscribedObserver;
        }


        /// <inheritdoc/>
        public override string ToString() =>
            this.formattedString;


        // Unsubdcribe observer.
        void Unsubscribe(SubscribedObserver observer)
        {
            if (observer.IsDisposed)
                return;
            observer.IsDisposed = true;
            if (notifyingCounter <= 0)
            {
                if (this.observerListHead == observer)
                    this.observerListHead = observer.Next;
                if (observer.Previous != null)
                    observer.Previous.Next = observer.Next;
                if (observer.Next != null)
                    observer.Next.Previous = observer.Previous;
                observer.Previous = null;
                observer.Next = null;
            }
            else
                this.hasPendingDisposedObservers = true;
        }


        // Update formatted string.
        void UpdateString()
        {
            var format = this.GetValue<string?>(FormatProperty);
            var formattedString = string.IsNullOrEmpty(format)
                ? ""
                : string.Format(format, new object?[] {
                    this.GetValue<object?>(Arg1Property),
                    this.GetValue<object?>(Arg2Property),
                    this.GetValue<object?>(Arg3Property),
                    this.GetValue<object?>(Arg4Property),
                    this.GetValue<object?>(Arg5Property),
                    this.GetValue<object?>(Arg6Property),
                    this.GetValue<object?>(Arg7Property),
                    this.GetValue<object?>(Arg8Property),
                    this.GetValue<object?>(Arg9Property),
                });
            if (this.formattedString.Length != formattedString.Length
                || formattedString.Length > 1024
                || this.formattedString != formattedString)
            {
                this.SetAndRaise<string>(StringProperty, ref this.formattedString, formattedString);
                this.Notify();
            }
        }
    }
}