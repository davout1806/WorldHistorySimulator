﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SubstractExpression : BinaryOpExpression<float>
{
    protected IValueExpression<float> _numExpressionA;
    protected IValueExpression<float> _numExpressionB;

    public SubstractExpression(IExpression expressionA, IExpression expressionB)
        : base("-", expressionA, expressionB)
    {
        _numExpressionA = ExpressionBuilder.ValidateValueExpression<float>(expressionA);
        _numExpressionB = ExpressionBuilder.ValidateValueExpression<float>(expressionB);
    }

    public static IExpression Build(Context context, string expressionAStr, string expressionBStr)
    {
        IExpression expressionA = ExpressionBuilder.BuildExpression(context, expressionAStr);
        IExpression expressionB = ExpressionBuilder.BuildExpression(context, expressionBStr);

        if ((expressionA is FixedNumberExpression) &&
            (expressionB is FixedNumberExpression))
        {
            FixedNumberExpression numExpA = expressionA as FixedNumberExpression;
            FixedNumberExpression numExpB = expressionB as FixedNumberExpression;

            numExpA.NumberValue -= numExpB.NumberValue;

            return numExpA;
        }

        return new SubstractExpression(expressionA, expressionB);
    }

    public override float Value => _numExpressionA.Value - _numExpressionB.Value;
}
