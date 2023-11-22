using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class FieldNameAttribute : PropertyAttribute
{
    public readonly string label;

    public readonly string htmlColor;

    public FieldNameAttribute(string label)
    {
        this.label = label;
        htmlColor = "#FFFFFF";
    }

    public FieldNameAttribute(string label, string htmlColor)
    {
        this.label = label;
        this.htmlColor = htmlColor;
    }
}