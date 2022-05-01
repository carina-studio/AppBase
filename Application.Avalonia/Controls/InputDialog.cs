using Avalonia;
using CarinaStudio.Threading;
using CarinaStudio.Windows.Input;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// <see cref="Dialog"/> which takes user input.
    /// </summary>
    public abstract class InputDialog : Dialog
    {
        /// <summary>
		/// Property of <see cref="IsValidInput"/>.
		/// </summary>
		public static readonly AvaloniaProperty<bool> IsValidInputProperty = AvaloniaProperty.RegisterDirect<InputDialog, bool>(nameof(IsValidInput), d => d.isValidInput);


        // Fields.
        bool isGeneratingResult;
        bool isValidInput;
        readonly CancellationTokenSource resultGeneratingCancellationTokenSource = new CancellationTokenSource();
        readonly ScheduledAction validateInputAction;


        /// <summary>
        /// Initialize new <see cref="InputDialog{TApp}"/> instance.
        /// </summary>
        protected InputDialog()
        {
            this.GenerateResultCommand = new Command(this.GenerateResult, this.GetObservable(IsValidInputProperty));
            this.validateInputAction = new ScheduledAction(() =>
            {
                if (this.IsClosed)
                    return;
                this.SetAndRaise<bool>(IsValidInputProperty, ref this.isValidInput, this.OnValidateInput());
            });
        }


        // Generate result and close dialog.
        async void GenerateResult()
        {
            // check state
            this.VerifyAccess();
            if (this.IsClosed || this.isGeneratingResult)
                return;
            this.validateInputAction.ExecuteIfScheduled();
            if (!this.IsValidInput)
                return;

            // generate result
            this.isGeneratingResult = true;
            var result = (object?)null;
            try
            {
                result = await this.GenerateResultAsync(this.resultGeneratingCancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            { }
            finally
            {
                this.isGeneratingResult = false;
            }

            // check result
            if (result == null || this.resultGeneratingCancellationTokenSource.IsCancellationRequested)
                return;

            // close dialog
            this.Close(result);
        }


        /// <summary>
        /// Called to generate result of dialog.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task of generating result. Dialog won't close if result is null.</returns>
        protected abstract Task<object?> GenerateResultAsync(CancellationToken cancellationToken);


        /// <summary>
        /// Command to generate result and close dialog.
        /// </summary>
        public ICommand GenerateResultCommand { get; }


        /// <summary>
		/// Invalid user input of dialog.
		/// </summary>
		protected void InvalidateInput()
        {
            if (!this.IsClosed)
                this.validateInputAction.Schedule();
        }


        /// <summary>
		/// Check whether user input of dialog is valid or not.
		/// </summary>
		public bool IsValidInput { get => this.isValidInput; }


        /// <summary>
        /// Called when window closed.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            this.validateInputAction.Cancel();
            this.resultGeneratingCancellationTokenSource.Cancel();
            base.OnClosed(e);
        }


        /// <summary>
        /// Called when window opened.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            this.validateInputAction.Schedule();
        }


        /// <summary>
        /// Called to validate input of user.
        /// </summary>
        /// <returns>True if input is valid.</returns>
        protected virtual bool OnValidateInput() => true;
    }


    /// <summary>
    /// <see cref="Dialog{TApp}"/> which takes user input.
    /// </summary>
    /// <typeparam name="TApp">Type of application.</typeparam>
    public abstract class InputDialog<TApp> : InputDialog where TApp : class, IApplication
    {
        /// <summary>
		/// Get application instance.
		/// </summary>
		public new TApp Application
        {
            get => (base.Application as TApp) ?? throw new ArgumentException($"Application doesn't implement {typeof(TApp)} interface.");
        }
    }
}
