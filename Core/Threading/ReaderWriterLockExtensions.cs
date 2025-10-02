using System;
using System.Threading;

namespace CarinaStudio.Threading;

/// <summary>
/// Represents a scope with reader lock of <see cref="ReaderWriterLock"/> acquired.
/// </summary>
public ref struct ReaderLockScope: IDisposable
{
    // Fields.
    bool isDisposed;
    readonly ReaderWriterLock @lock;

    // Constructor.
    internal ReaderLockScope(ReaderWriterLock @lock, int timeout)
    {
        this.@lock = @lock;
        try
        {
            @lock.AcquireReaderLock(timeout);
        }
        catch (Exception ex)
        {
            throw new TimeoutException("Timeout before reader lock acquired.", ex);
        }
    }
    
    /// <interitdoc/>
    public void Dispose()
    {
        if (!this.isDisposed)
        {
            this.isDisposed = true;
            this.@lock.ReleaseReaderLock();
        }
    }
}


/// <summary>
/// Represents a scope with reader lock of <see cref="ReaderWriterLockSlim"/> acquired.
/// </summary>
public ref struct ReaderLockScopeSlim: IDisposable
{
    // Fields.
    bool isDisposed;
    readonly ReaderWriterLockSlim @lock;

    // Constructor.
    internal ReaderLockScopeSlim(ReaderWriterLockSlim @lock, int timeout)
    {
        this.@lock = @lock;
        if (!@lock.TryEnterReadLock(timeout))
            throw new TimeoutException("Timeout before reader lock acquired.");
    }
    
    /// <interitdoc/>
    public void Dispose()
    {
        if (!this.isDisposed)
        {
            this.isDisposed = true;
            this.@lock.ExitReadLock();
        }
    }
}


/// <summary>
/// Represents a scope with upgradeable reader lock of <see cref="ReaderWriterLockSlim"/> acquired.
/// </summary>
public ref struct UpgradeableReaderLockScopeSlim: IDisposable
{
    // Fields.
    bool isDisposed;
    readonly ReaderWriterLockSlim @lock;

    // Constructor.
    internal UpgradeableReaderLockScopeSlim(ReaderWriterLockSlim @lock, int timeout)
    {
        this.@lock = @lock;
        if (!@lock.TryEnterUpgradeableReadLock(timeout))
            throw new TimeoutException("Timeout before upgradeable reader lock acquired.");
    }
    
    /// <interitdoc/>
    public void Dispose()
    {
        if (!this.isDisposed)
        {
            this.isDisposed = true;
            this.@lock.ExitUpgradeableReadLock();
        }
    }
}


/// <summary>
/// Represents a scope with writer lock of <see cref="ReaderWriterLock"/> acquired.
/// </summary>
public ref struct WriterLockScope: IDisposable
{
    // Fields.
    bool isDisposed;
    readonly ReaderWriterLock @lock;

    // Constructor.
    internal WriterLockScope(ReaderWriterLock @lock, int timeout)
    {
        this.@lock = @lock;
        try
        {
            @lock.AcquireWriterLock(timeout);
        }
        catch (ApplicationException ex)
        {
            throw new TimeoutException("Timeout before writer lock acquired.", ex);
        }
    }
    
    /// <interitdoc/>
    public void Dispose()
    {
        if (!this.isDisposed)
        {
            this.isDisposed = true;
            this.@lock.ReleaseWriterLock();
        }
    }
}


/// <summary>
/// Represents a scope with writer lock of <see cref="ReaderWriterLockSlim"/> acquired.
/// </summary>
public ref struct WriterLockScopeSlim: IDisposable
{
    // Fields.
    bool isDisposed;
    readonly ReaderWriterLockSlim @lock;

    // Constructor.
    internal WriterLockScopeSlim(ReaderWriterLockSlim @lock, int timeout)
    {
        this.@lock = @lock;
        if (!@lock.TryEnterWriteLock(timeout))
            throw new TimeoutException("Timeout before writer lock acquired.");
    }
    
    /// <interitdoc/>
    public void Dispose()
    {
        if (!this.isDisposed)
        {
            this.isDisposed = true;
            this.@lock.ExitWriteLock();
        }
    }
}


/// <summary>
/// Extension methods for <see cref="ReaderWriterLock"/> and <see cref="ReaderWriterLockSlim"/>.
/// </summary>
public static class ReaderWriterLockExtensions
{
    // Convert timeout in TimeSpan into milliseconds.
    static int ConvertTimeoutToMilliseconds(TimeSpan timeout)
    {
        var totalMills = timeout.TotalMilliseconds;
        if (totalMills < -1 || totalMills > int.MaxValue)
            throw new ArgumentException("Invalid timeout.");
        return (int)totalMills;
    }
    
    
    /// <summary>
    /// Enter the scope with reader lock acquired.
    /// </summary>
    /// <param name="lock"><see cref="ReaderWriterLock"/>.</param>
    /// <param name="timeout">Timeout before reader lock acquired.</param>
    /// <returns>Scope with reader lock acquired.</returns>
    public static ReaderLockScope EnterReadScope(this ReaderWriterLock @lock, int timeout = Timeout.Infinite) => new(@lock, timeout);
    
    
    /// <summary>
    /// Enter the scope with reader lock acquired.
    /// </summary>
    /// <param name="lock"><see cref="ReaderWriterLock"/>.</param>
    /// <param name="timeout">Timeout before reader lock acquired.</param>
    /// <returns>Scope with reader lock acquired.</returns>
    public static ReaderLockScope EnterReadScope(this ReaderWriterLock @lock, TimeSpan timeout) => new(@lock, ConvertTimeoutToMilliseconds(timeout));
    
    
    /// <summary>
    /// Enter the scope with reader lock acquired.
    /// </summary>
    /// <param name="lock"><see cref="ReaderWriterLockSlim"/>.</param>
    /// <param name="timeout">Timeout before reader lock acquired.</param>
    /// <returns>Scope with reader lock acquired.</returns>
    public static ReaderLockScopeSlim EnterReadScope(this ReaderWriterLockSlim @lock, int timeout = Timeout.Infinite) => new(@lock, timeout);
    
    
    /// <summary>
    /// Enter the scope with reader lock acquired.
    /// </summary>
    /// <param name="lock"><see cref="ReaderWriterLockSlim"/>.</param>
    /// <param name="timeout">Timeout before reader lock acquired.</param>
    /// <returns>Scope with reader lock acquired.</returns>
    public static ReaderLockScopeSlim EnterReadScope(this ReaderWriterLockSlim @lock, TimeSpan timeout) => new(@lock, ConvertTimeoutToMilliseconds(timeout));
    
    
    /// <summary>
    /// Enter the scope with upgradeable reader lock acquired.
    /// </summary>
    /// <param name="lock"><see cref="ReaderWriterLockSlim"/>.</param>
    /// <param name="timeout">Timeout before upgradeable reader lock acquired.</param>
    /// <returns>Scope with upgradeable reader lock acquired.</returns>
    public static UpgradeableReaderLockScopeSlim EnterUpgradeableReadScope(this ReaderWriterLockSlim @lock, int timeout = Timeout.Infinite) => new(@lock, timeout);
    
    
    /// <summary>
    /// Enter the scope with upgradeable reader lock acquired.
    /// </summary>
    /// <param name="lock"><see cref="ReaderWriterLockSlim"/>.</param>
    /// <param name="timeout">Timeout before upgradeable reader lock acquired.</param>
    /// <returns>Scope with upgradeable reader lock acquired.</returns>
    public static UpgradeableReaderLockScopeSlim EnterUpgradeableReadScope(this ReaderWriterLockSlim @lock, TimeSpan timeout) => new(@lock, ConvertTimeoutToMilliseconds(timeout));
    
    
    /// <summary>
    /// Enter the scope with writer lock acquired.
    /// </summary>
    /// <param name="lock"><see cref="ReaderWriterLock"/>.</param>
    /// <param name="timeout">Timeout before writer lock acquired.</param>
    /// <returns>Scope with writer lock acquired.</returns>
    public static WriterLockScope EnterWriteScope(this ReaderWriterLock @lock, int timeout = Timeout.Infinite) => new(@lock, timeout);
    
    
    /// <summary>
    /// Enter the scope with writer lock acquired.
    /// </summary>
    /// <param name="lock"><see cref="ReaderWriterLock"/>.</param>
    /// <param name="timeout">Timeout before writer lock acquired.</param>
    /// <returns>Scope with writer lock acquired.</returns>
    public static WriterLockScope EnterWriteScope(this ReaderWriterLock @lock, TimeSpan timeout) => new(@lock, ConvertTimeoutToMilliseconds(timeout));
    
    
    /// <summary>
    /// Enter the scope with writer lock acquired.
    /// </summary>
    /// <param name="lock"><see cref="ReaderWriterLockSlim"/>.</param>
    /// <param name="timeout">Timeout before writer lock acquired.</param>
    /// <returns>Scope with writer lock acquired.</returns>
    public static WriterLockScopeSlim EnterWriteScope(this ReaderWriterLockSlim @lock, int timeout = Timeout.Infinite) => new(@lock, timeout);
    
    
    /// <summary>
    /// Enter the scope with writer lock acquired.
    /// </summary>
    /// <param name="lock"><see cref="ReaderWriterLockSlim"/>.</param>
    /// <param name="timeout">Timeout before writer lock acquired.</param>
    /// <returns>Scope with writer lock acquired.</returns>
    public static WriterLockScopeSlim EnterWriteScope(this ReaderWriterLockSlim @lock, TimeSpan timeout) => new(@lock, ConvertTimeoutToMilliseconds(timeout));
}