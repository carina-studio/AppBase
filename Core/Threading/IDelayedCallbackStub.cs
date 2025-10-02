namespace CarinaStudio.Threading;

/// <summary>
/// Stub of delayed call-back.
/// </summary>
internal interface IDelayedCallbackStub
{
    /// <summary>
    /// Perform call-back.
    /// </summary>
    void Callback();

    
    /// <summary>
    /// Cancel call-back.
    /// </summary>
    /// <returns>True if call-back has been cancelled.</returns>
    bool Cancel();
}