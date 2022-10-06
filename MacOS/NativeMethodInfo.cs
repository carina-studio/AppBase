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
        this.RetuenType = returnType;
    }


    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not NativeMethodInfo info)
            return false;
        var paramTypes = this.ParameterTypes;
        var otherParamTypes = info.ParameterTypes;
        if (paramTypes.Length != otherParamTypes.Length || this.RetuenType != info.RetuenType)
            return false;
        for (var i = paramTypes.Length - 1; i >= 0; --i)
        {
            if (paramTypes[i] != otherParamTypes[i])
                return false;
        }
        return true;
    }


    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (this.hashCode != 0)
            return this.hashCode;
        var paramTypes = this.ParameterTypes;
        var returnType = this.RetuenType;
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
            chars[chars.Length - 2] = '-';
            if (returnType == typeof(nint))
                chars[chars.Length - 1] = 'I';
            else if (returnType == typeof(double))
                chars[chars.Length - 1] = 'D';
            else
                chars[chars.Length - 1] = 'S';
        }
        this.hashCode = new string(chars).GetHashCode();
        return this.hashCode;
    }


    // Types of parameters.
    public Type[] ParameterTypes { get; }


    // Type of return value.
    public Type? RetuenType { get; }
}