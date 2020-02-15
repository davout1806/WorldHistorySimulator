﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NumericEntityAttributeExpression : INumericExpression
{
    private readonly string _expressionStr;
    private readonly string _attributeId;
    private readonly string _arguments;
    private readonly NumericEntityAttribute _attribute;

    public NumericEntityAttributeExpression(
        EntityAttribute attribute, string expStr, string attrId, string args)
    {
        _attribute = attribute as NumericEntityAttribute;
        _expressionStr = expStr;
        _attributeId = attrId;
        _arguments = args;
    }

    public float GetValue()
    {
        return _attribute.GetValue();
    }

    public override string ToString()
    {
        return _expressionStr + "." + _attributeId
            + (string.IsNullOrWhiteSpace(_arguments) ? "" : "(" + _arguments + ")");
    }
}
