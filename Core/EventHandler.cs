namespace CarinaStudio
{
    /// <summary>
    /// Generic event handler.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">Event data.</param>
    /// <typeparam name="TSender">Type of sender.</typeparam>
    /// <typeparam name="TArgs">Type of event data.</typeparam>
    public delegate void EventHandler<in TSender, in TArgs>(TSender? sender, TArgs e);
}