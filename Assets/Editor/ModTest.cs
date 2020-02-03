﻿using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.IO;
using System;

public class ModTest
{
    public class TestContext : Context
    {
        public TestContext() : base("testContext")
        {
        }
    }

    public class TestBooleanEntityAttribute : BooleanEntityAttribute
    {
        private bool _value;

        public TestBooleanEntityAttribute(bool value)
        {
            _value = value;
        }

        public override bool GetValue()
        {
            return _value;
        }
    }

    public class TestNumericFunctionEntityAttribute : NumericEntityAttribute
    {
        private BooleanExpression _argument;

        public TestNumericFunctionEntityAttribute(Expression[] arguments)
        {
            if ((arguments == null) || (arguments.Length < 1))
            {
                throw new System.ArgumentException("Number of arguments less than 1");
            }

            _argument = BooleanExpression.ValidateExpression(arguments[0]);
        }

        public override float GetValue()
        {
            return (_argument.GetValue()) ? 10 : 2;
        }
    }

    public class TestEntity : Entity
    {
        private class InternalEntity : Entity
        {
            private TestBooleanEntityAttribute _boolAttribute = new TestBooleanEntityAttribute(true);

            public override EntityAttribute GetAttribute(string attributeId, Expression[] arguments = null)
            {
                switch (attributeId)
                {
                    case "testBoolAttribute":
                        return _boolAttribute;
                }

                return null;
            }
        }

        private InternalEntity _internalEntity = new InternalEntity();

        private TestBooleanEntityAttribute _boolAttribute = new TestBooleanEntityAttribute(false);

        private FixedEntityEntityAttribute _entityAttribute;

        public TestEntity()
        {
            _entityAttribute = new FixedEntityEntityAttribute(_internalEntity);
        }

        public override EntityAttribute GetAttribute(string attributeId, Expression[] arguments = null)
        {
            switch (attributeId)
            {
                case "testBoolAttribute":
                    return _boolAttribute;

                case "testEntityAttribute":
                    return _entityAttribute;

                case "testNumericFunctionAttribute":
                    return new TestNumericFunctionEntityAttribute(arguments);
            }

            return null;
        }
    }

    [Test]
    public void ExpressionParseTest()
    {
        int expCounter = 1;

        TestContext testContext = new TestContext();

        testContext.Expressions.Add(
            "testContextNumericExpression",
            Expression.BuildExpression(testContext, "-15"));
        testContext.Expressions.Add(
            "testContextBooleanExpression",
            Expression.BuildExpression(testContext, "!true"));

        testContext.Entities.Add("testEntity", new TestEntity());

        Expression expression = Expression.BuildExpression(testContext, "-5");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual((expression as NumericExpression).GetValue(), -5);

        expression = Expression.BuildExpression(testContext, "!false");
        Assert.AreEqual((expression as BooleanExpression).GetValue(), true);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "1 + 1");
        Assert.AreEqual((expression as NumericExpression).GetValue(), 2);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "1 + -1 + 2");
        Assert.AreEqual((expression as NumericExpression).GetValue(), 2);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "-1 + 2 + 2");
        Assert.AreEqual((expression as NumericExpression).GetValue(), 3);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "2 +2+3");
        Assert.AreEqual((expression as NumericExpression).GetValue(), 7);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "testContextNumericExpression");
        Assert.AreEqual((expression as NumericExpression).GetValue(), -15);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "testContextBooleanExpression");
        Assert.AreEqual((expression as BooleanExpression).GetValue(), false);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "testEntity.testBoolAttribute");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual((expression as BooleanExpression).GetValue(), false);

        expression = Expression.BuildExpression(
            testContext, "testEntity.testEntityAttribute.testBoolAttribute");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual((expression as BooleanExpression).GetValue(), true);

        expression = Expression.BuildExpression(
            testContext, "lerp(3, -1, 0.5)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual((expression as NumericExpression).GetValue(), 1);

        expression = Expression.BuildExpression(
            testContext, "lerp(4, (1 - 2), 0.1)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual((expression as NumericExpression).GetValue(), 3.5f);

        expression = Expression.BuildExpression(
            testContext, "2 + (1 + lerp(3, -1, 0.5))");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual((expression as NumericExpression).GetValue(), 4);

        expression = Expression.BuildExpression(
            testContext, "2 + lerp(0.5 + 0.5 + 2, -1, 0.5) + 1");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual((expression as NumericExpression).GetValue(), 4);

        expression =
            Expression.BuildExpression(testContext, "testEntity.testNumericFunctionAttribute(true)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual((expression as NumericExpression).GetValue(), 10);

        expression =
            Expression.BuildExpression(testContext, "testEntity.testNumericFunctionAttribute(false)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual((expression as NumericExpression).GetValue(), 2);

        //expression = Expression.BuildExpression(
        //    testContext, "testFunction1()");

        //Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        //Assert.AreEqual((expression as BooleanExpression).GetValue(), true);

        //expression = Expression.BuildExpression(
        //    testContext, "testFunction2(true)");

        //Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        //Assert.AreEqual((expression as BooleanExpression).GetValue(), true);

        //expression = Expression.BuildExpression(
        //    testContext, "testFunction3(false ,3 +3, -5)");

        //Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        //Assert.AreEqual((expression as BooleanExpression).GetValue(), true);
    }
}
