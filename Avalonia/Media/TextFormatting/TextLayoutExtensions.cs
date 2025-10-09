using Avalonia.Media.TextFormatting;
using CarinaStudio.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CarinaStudio.Media.TextFormatting;

/// <summary>
/// Extensions for <see cref="TextLayout"/>.
/// </summary>
public static class TextLayoutExtensions
{
    // Constants.
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicFields)]
    const string WrappingTextLineBreakTypeName = "Avalonia.Media.TextFormatting.WrappingTextLineBreak";
    
    
    // Fields.
    static FieldInfo? RemainingRunsField;
    static Type? WrappingTextLineBreakType;
    
    
    /// <summary>
    /// Check whether the text of <see cref="TextLayout"/> is trimmed or not.
    /// </summary>
    /// <param name="textLayout"><see cref="TextLayout"/>.</param>
    /// <returns>True if the text of <see cref="TextLayout"/> is trimmed.</returns>
    public static bool IsTextTrimmed(this TextLayout textLayout)
    {
        var textLines = textLayout.TextLines;
        var lineCount = textLines.Count;
        for (var i = lineCount - 1; i >= 0; --i)
        {
            var textLine = textLines[i];
            if (textLine.HasCollapsed || textLine.HasOverflowed)
                return true;
            var textLineBreak = textLine.TextLineBreak;
            if (textLineBreak is not null && textLineBreak.IsSplit)
            {
                if (RemainingRunsField is null)
                {
                    var type = Type.GetType(WrappingTextLineBreakTypeName);
                    RemainingRunsField = type?.GetField("_remainingRuns", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (RemainingRunsField is not null)
                        WrappingTextLineBreakType = type;
                }
                if (WrappingTextLineBreakType?.IsInstanceOfType(textLineBreak) == true)
                {
                    var remainingRuns = (IList<TextRun>?)RemainingRunsField!.GetValue(textLineBreak);
                    if (remainingRuns?.IsNotEmpty() == true)
                        return true;
                }
            }
        }
        return false;
    }
}