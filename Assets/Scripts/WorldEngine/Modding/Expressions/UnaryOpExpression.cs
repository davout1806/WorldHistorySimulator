﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Base class for all unary operation expressions (i.e. -3, !true)
/// </summary>
public abstract class UnaryOpExpression<T> : IValueExpression<T>
{
    protected IExpression _expression;

    private readonly string _opStr;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="opStr">Operator string (i.e. '+', '-', '!', '*')</param>
    /// <param name="expression">Operand expression</param>
    public UnaryOpExpression(string opStr, IExpression expression)
    {
        _opStr = opStr;
        _expression = expression;
    }

    public override string ToString()
    {
        return "(" + _opStr + _expression + ")";
    }

    public string GetString() => Value.ToString();

    public abstract T Value { get; }
}
