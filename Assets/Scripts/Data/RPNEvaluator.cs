// File: Assets/Scripts/RPNEvaluator.cs

using System;
using System.Collections.Generic;
using UnityEngine;

public static class RPNEvaluator
{
<<<<<<< HEAD
    public static int Evaluate(string expression, Dictionary<string,int> variables)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression empty", nameof(expression));

        var stack = new Stack<int>();
        foreach (var tok in expression.Split(' '))
        {
            if (variables != null && variables.TryGetValue(tok, out int v))
            {
                stack.Push(v);
            }
            else if (int.TryParse(tok, out int i))
            {
                stack.Push(i);
            }
            else
            {
                int b = stack.Pop(), a = stack.Pop(), r;
                switch (tok)
                {
                    case "+": r = a + b; break;
                    case "-": r = a - b; break;
                    case "*": r = a * b; break;
                    case "/": r = a / b; break;
                    case "%": r = a % b; break;
                    default:  throw new InvalidOperationException($"Unknown op {tok}");
                }
                stack.Push(r);
            }
        }

        return stack.Pop();
    }

    public static int SafeEvaluate(string expression, Dictionary<string,int> variables, int fallback)
    {
        try
        {
            return Evaluate(expression, variables);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"RPN SafeEvaluate failed for '{expression}': {ex.Message}");
            return fallback;
        }
    }

    public static float EvaluateFloat(string expression, Dictionary<string,float> variables)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression empty", nameof(expression));

        var stack = new Stack<float>();
        foreach (var tok in expression.Split(' '))
        {
            if (variables != null && variables.TryGetValue(tok, out float v))
            {
                stack.Push(v);
            }
            else if (float.TryParse(tok, out float f))
            {
                stack.Push(f);
            }
            else
            {
                float b = stack.Pop(), a = stack.Pop(), r;
                switch (tok)
                {
                    case "+": r = a + b; break;
                    case "-": r = a - b; break;
                    case "*": r = a * b; break;
                    case "/": r = a / b; break;
                    default:  throw new InvalidOperationException($"Unknown op {tok}");
                }
                stack.Push(r);
            }
        }

        return stack.Pop();
    }

    public static float SafeEvaluateFloat(string expression, Dictionary<string, float> variables, float fallback = 0f)
    {
        try
        {
            return EvaluateFloat(expression, variables);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"RPN SafeEvaluateFloat failed for '{expression}': {ex.Message}");
            return fallback;
        }
    }
=======
    public static float Eval(string expr, Dictionary<string,float> vars)
    {
        var st=new Stack<float>();
        foreach(var tok in expr.Split(' '))
        {
            if(vars!=null && vars.ContainsKey(tok)) st.Push(vars[tok]);
            else if(float.TryParse(tok,out var num)) st.Push(num);
            else switch(tok){
                case "+":{float b=st.Pop(),a=st.Pop(); st.Push(a+b);break;}
                case "-":{float d=st.Pop(),c=st.Pop(); st.Push(c-d);break;}
                case "*":{float f=st.Pop(),e=st.Pop(); st.Push(e*f);break;}
                case "/":{float h=st.Pop(),g=st.Pop(); st.Push(g/h);break;}
                default: throw new Exception($"Bad token '{tok}'");
            }
        }
        return st.Pop();
    }

    public static int EvalInt(string s, Dictionary<string,float> v) => Mathf.RoundToInt(Eval(s,v));
>>>>>>> 22ff77c (getting there)
}
