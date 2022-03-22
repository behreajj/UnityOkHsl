// Copyright(c) 2021 Bjorn Ottosson
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files(the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using UnityEngine;

public static class OkUnityBridge
{
    public static double Clamp (in double a, in double lb = 0.0d, in double ub = 1.0d)
    {
        return Math.Min (Math.Max (a, lb), ub);
    }

    public static double DistAngleUnsigned (in double origin, in double dest, in double range = 1.0d)
    {
        double halfRange = range * 0.5d;
        return halfRange - Math.Abs (Math.Abs (RemFloor (dest, range) - RemFloor (origin, range)) - halfRange);
    }

    public static double Lerp (in double a, in double b, in double t = 0.5d)
    {
        return (1.0d - t) * a + t * b;
    }

    public static double LerpAngleFar (in double origin, in double dest, in double t = 0.5d, in double range = 1.0d)
    {
        double halfRange = range * 0.5d;
        double o = RemFloor (origin, range);
        double d = RemFloor (dest, range);
        double diff = d - o;
        double u = 1.0d - t;

        if (diff == 0.0d || (o < d && diff < halfRange))
        {
            return RemFloor (u * (o + range) + t * d, range);
        }
        else if (o > d && diff > -halfRange)
        {
            return RemFloor (u * o + t * (d + range), range);
        }
        else
        {
            return u * o + t * d;
        }
    }

    public static double LerpAngleNear (in double origin, in double dest, in double t = 0.5d, in double range = 1.0d)
    {
        double halfRange = range * 0.5d;
        double o = RemFloor (origin, range);
        double d = RemFloor (dest, range);
        double diff = d - o;
        double u = 1.0d - t;
        if (diff == 0.0d)
        {
            return o;
        }
        else if (o < d && diff > halfRange)
        {
            return RemFloor (u * (o + range) + t * d, range);
        }
        else if (o > d && diff < -halfRange)
        {
            return RemFloor (u * o + t * (d + range), range);
        }
        else
        {
            return u * o + t * d;
        }
    }

    public static double LerpAngleCcw (in double origin, in double dest, in double t = 0.5d, in double range = 1.0d)
    {
        double o = RemFloor (origin, range);
        double d = RemFloor (dest, range);
        double diff = d - o;
        double u = 1.0d - t;
        if (diff == 0.0d)
        {
            return o;
        }
        else if (o > d)
        {
            return RemFloor (u * o + t * (d + range), range);
        }
        else
        {
            return u * o + t * d;
        }
    }

    public static double LerpAngleCw (in double origin, in double dest, in double t = 0.5d, in double range = 1.0d)
    {
        double o = RemFloor (origin, range);
        double d = RemFloor (dest, range);
        double diff = d - o;
        double u = 1.0d - t;
        if (diff == 0.0d)
        {
            return d;
        }
        else if (o < d)
        {
            return RemFloor (u * (o + range) + t * d, range);
        }
        else
        {
            return u * o + t * d;
        }
    }

    public static (double L, double a, double b) LerpLab (in (double L, double a, double b) a, in (double L, double a, double b) b,
        double t = 0.5d)
    {
        return (
            L: Lerp (a.L, b.L, t),
            a : Lerp (a.a, b.a, t),
            b : Lerp (a.b, b.b, t));
    }

    public static double RemFloor (in double a, in double b)
    {
        return b != 0.0d ? a - b * Math.Floor (a / b) : a;
    }

    public static Color RgbToColor (in (double r, double g, double b) rgb, in double alpha = 1.0d)
    {
        float rc01 = (float) Clamp (rgb.r, 0.0d, 1.0d);
        float gc01 = (float) Clamp (rgb.g, 0.0d, 1.0d);
        float bc01 = (float) Clamp (rgb.b, 0.0d, 1.0d);
        float ac01 = (float) Clamp (alpha, 0.0d, 1.0d);

        return new Color (
            r: rc01,
            g: gc01,
            b: bc01,
            a: ac01);
    }
}