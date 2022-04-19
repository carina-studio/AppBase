using System;

namespace CarinaStudio
{
    /// <summary>
    /// Action on reference to variable.
    /// </summary>
    /// <param name="arg">Reference to variable.</param>
    /// <typeparam name="T">Type of variable.</typeparam>
    public delegate void RefAction<T>(ref T arg);


    /// <summary>
    /// Function to return a reference to variable.
    /// </summary>
    /// <typeparam name="R">Type of returned type of reference.</typeparam>
    /// <returns>Reference to variable.</returns>
    public delegate ref R RefFunc<R>();


    /// <summary>
    /// Function to return a reference to variable.
    /// </summary>
    /// <param name="arg1">1st argument.</param>
    /// <typeparam name="T1">Type of 1st argument.</typeparam>
    /// <typeparam name="R">Type of returned type of reference.</typeparam>
    /// <returns>Reference to variable.</returns>
    public delegate ref R RefFunc<T1, R>(T1 arg1);


    /// <summary>
    /// Function to return a reference to variable.
    /// </summary>
    /// <param name="arg1">1st argument.</param>
    /// <param name="arg2">2nd argument.</param>
    /// <typeparam name="T1">Type of 1st argument.</typeparam>
    /// <typeparam name="T2">Type of 2nd argument.</typeparam>
    /// <typeparam name="R">Type of returned type of reference.</typeparam>
    /// <returns>Reference to variable.</returns>
    public delegate ref R RefFunc<T1, T2, R>(T1 arg1, T2 arg2);


    /// <summary>
    /// Function to return a reference to variable.
    /// </summary>
    /// <param name="arg1">1st argument.</param>
    /// <param name="arg2">2nd argument.</param>
    /// <param name="arg3">3rd argument.</param>
    /// <typeparam name="T1">Type of 1st argument.</typeparam>
    /// <typeparam name="T2">Type of 2nd argument.</typeparam>
    /// <typeparam name="T3">Type of 3rd argument.</typeparam>
    /// <typeparam name="R">Type of returned type of reference.</typeparam>
    /// <returns>Reference to variable.</returns>
    public delegate ref R RefFunc<T1, T2, T3, R>(T1 arg1, T2 arg2, T3 arg3);
}