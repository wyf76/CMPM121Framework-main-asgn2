using System;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public static class RPNEvaluator
{
    public static int Evaluate(string expression, Dictionary<string, int> variables)
    {
        Stack<int> stack = new Stack<int>();
        string[] tokens = expression.Split(' ');

        foreach (var token in tokens)
        {
            if (int.TryParse(token, out int number))
            {
                stack.Push(number);
            }
            else if (variables.ContainsKey(token))
            {
                stack.Push(variables[token]);
            }
            else
            {
                if (stack.Count < 2)
                {
                    Debug.LogError($"Malformed RPN expression: {expression}");
                    return 0;
                }

                int b = stack.Pop();
                int a = stack.Pop();
                int result = token switch
                {
                    "+" => a + b,
                    "-" => a - b,
                    "*" => a * b,
                    "/" => (b != 0) ? a / b : 0,
                    "%" => (b != 0) ? a % b : 0,
                    _ => throw new Exception($"Unknown operator '{token}'")
                };

                stack.Push(result);
            }
        }

        if (stack.Count != 1)
        {
            Debug.LogError($"Malformed RPN expression (stack left with {stack.Count} values): {expression}");
            return 0;
        }

        return stack.Pop();
    }
}

public static class RPNEvaluatorInt
{
    public static int Evaluate(string expression, int wave, int power)
    {
        var vars = new Dictionary<string, float>
        {
            { "wave", wave },
            { "power", power }
        };
        return Mathf.RoundToInt(RPNEvaluatorFloat.EvaluateInternal(expression, vars));
    }
}

public static class RPNEvaluatorFloat
{
    public static float Evaluate(string expression, int wave, int power)
    {
        var vars = new Dictionary<string, float>
        {
            { "wave", wave },
            { "power", power }
        };
        return EvaluateInternal(expression, vars);
    }

    public static float EvaluateInternal(string expression, Dictionary<string, float> variables)
    {
        if (string.IsNullOrWhiteSpace(expression)) return 0f;

        Stack<float> stack = new Stack<float>();
        string[] tokens = expression.Split(' ');

        foreach (var token in tokens)
        {
            if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
            {
                stack.Push(number);
            }
            else if (variables.ContainsKey(token))
            {
                stack.Push(variables[token]);
            }
            else
            {
                if (stack.Count < 2) throw new Exception($"Malformed RPN expression: '{expression}'");

                float b = stack.Pop();
                float a = stack.Pop();

                switch (token)
                {
                    case "+": stack.Push(a + b); break;
                    case "-": stack.Push(a - b); break;
                    case "*": stack.Push(a * b); break;
                    case "/": stack.Push(b != 0 ? a / b : 0); break;
                    case "%": stack.Push(a % b); break;
                    default: throw new Exception($"Unknown operator '{token}'");
                }
            }
        }

        return stack.Count == 1 ? stack.Pop() : throw new Exception($"Invalid RPN expression: '{expression}'");
    }
}

