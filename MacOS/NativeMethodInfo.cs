using System;

namespace CarinaStudio.MacOS;

class NativeMethodInfo
{
    // Fields.
    int hashCode;


    // Constructor.
    public NativeMethodInfo(Type[] paramTypes, Type? returnType)
    {
        this.ParameterTypes = paramTypes;
        this.ReturnType = returnType;
    }


    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not NativeMethodInfo info)
            return false;
        var paramTypes = this.ParameterTypes;
        var otherParamTypes = info.ParameterTypes;
        if (paramTypes.Length != otherParamTypes.Length || this.ReturnType != info.ReturnType)
            return false;
        for (var i = paramTypes.Length - 1; i >= 0; --i)
        {
            if (paramTypes[i] != otherParamTypes[i])
                return false;
        }
        return true;
    }


    // ReSharper disable NonReadonlyMemberInGetHashCode
    /// <inheritdoc/>
    public override int GetHashCode()
    {
        
        if (this.hashCode != 0)
            return this.hashCode;
        var paramTypes = this.ParameterTypes;
        var returnType = this.ReturnType;
        var chars = new char[returnType != null ? paramTypes.Length + 2 : paramTypes.Length];
        for (var i = paramTypes.Length - 1; i >= 0; --i)
        {
            var t = paramTypes[i];
            if (t == typeof(nint))
                chars[i] = 'I';
            else if (t == typeof(double))
                chars[i] = 'D';
            else
                chars[i] = 'S';
        }
        if (returnType != null)
        {
            chars[^2] = '-';
            if (returnType == typeof(nint))
                chars[^1] = 'I';
            else if (returnType == typeof(double))
                chars[^1] = 'D';
            else
                chars[^1] = 'S';
        }
        this.hashCode = new string(chars).GetHashCode();
        return this.hashCode;
    }
    // ReSharper restore NonReadonlyMemberInGetHashCode


    // Types of parameters.
    public Type[] ParameterTypes { get; }


    // Type of return value.
    public Type? ReturnType { get; }
}