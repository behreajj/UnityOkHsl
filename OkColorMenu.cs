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
using UnityEditor;
using UnityEngine;

public class OkColorMenu : EditorWindow
{
    public enum Mode : int
    {
        OkHsl = 0,
        OkHsv = 1,
        OkLab = 2
    }

    public enum HueEasing : int
    {
        Near = 0,
        Clockwise = 1,
        CounterClockwise = 2
    }

    static Mode mode = Mode.OkHsl;
    static HueEasing hueEasing = HueEasing.Near;

    static Color aColor = Color.red;
    static Color bColor = new Color (0.0f, 0.6039216f, 0.6745098f, 1.0f);

    static float hOkHsl = 29.23391f;
    static float sOkHsl = 100.0f;
    static float lOkHsl = 56.80846f;

    static float hOkHsv = 29.23391f;
    static float sOkHsv = 100.0f;
    static float vOkHsv = 100.0f;

    static float lOkLab = 62.79554f;
    static float aOkLab = 22.4863f;
    static float bOkLab = 12.58463f;

    static Func<double, double, double, double, double> hueFunc = (a, b, c, d) => OkUnityBridge.LerpAngleNear (a, b, c, d);

    static Gradient gradient = new Gradient ( );

    [MenuItem ("Window/OkColor")]
    static void Init ( )
    {
        OkColorMenu window = (OkColorMenu) EditorWindow.GetWindow (
            t: typeof (OkColorMenu),
            utility: false,
            title: "OkColor",
            focus : true);
        window.Show ( );
    }

    void OnGUI ( )
    {
        aColor = EditorGUILayout.ColorField (
            showEyedropper: true,
            value: aColor,
            showAlpha: true,
            hdr: false,
            label: new GUIContent ("Fore"));

        using (var horizontalScope = new GUILayout.HorizontalScope ("box"))
        {
            if (GUILayout.Button ("To OkColor")) { GetColor (aColor); }
            if (GUILayout.Button ("From OkColor")) { aColor = SetColor (mode, aColor.a); }
        }

        bColor = EditorGUILayout.ColorField (
            showEyedropper: true,
            value: bColor,
            showAlpha: true,
            hdr: false,
            label: new GUIContent ("Back"));

        using (var horizontalScope = new GUILayout.HorizontalScope ("box"))
        {
            if (GUILayout.Button ("To OkColor")) { GetColor (bColor); }
            if (GUILayout.Button ("From OkColor")) { bColor = SetColor (mode, bColor.a); }
        }

        if (GUILayout.Button ("Swap"))
        {
            Color temp = aColor;
            aColor = bColor;
            bColor = temp;
        }

        EditorGUILayout.Space ( );

        mode = (Mode) EditorGUILayout.EnumPopup (
            label: new GUIContent ("Mode"),
            selected: mode,
            checkEnabled: OnModeSelect,
            includeObsolete: false);

        switch (mode)
        {
            case Mode.OkLab:
                {
                    lOkLab = EditorGUILayout.Slider (
                        value: lOkLab,
                        leftValue: 0.0f,
                        rightValue: 100.0f,
                        label: "Luminance");

                    aOkLab = EditorGUILayout.Slider (
                        value: aOkLab,
                        leftValue: -32.0f,
                        rightValue : 32.0f,
                        label: "A");

                    bOkLab = EditorGUILayout.Slider (
                        value: bOkLab,
                        leftValue: -32.0f,
                        rightValue : 32.0f,
                        label: "B");
                }
                break;

            case Mode.OkHsv:
                {
                    hOkHsv = EditorGUILayout.Slider (
                        value: hOkHsv,
                        leftValue: 0.0f,
                        rightValue: 360.0f,
                        label: "Hue");

                    sOkHsv = EditorGUILayout.Slider (
                        value: sOkHsv,
                        leftValue: 0.0f,
                        rightValue: 100.0f,
                        label: "Saturation");

                    vOkHsv = EditorGUILayout.Slider (
                        value: vOkHsv,
                        leftValue: 0.0f,
                        rightValue: 100.0f,
                        label: "Value");
                }
                break;

            case Mode.OkHsl:
            default:
                {
                    hOkHsl = EditorGUILayout.Slider (
                        value: hOkHsl,
                        leftValue: 0.0f,
                        rightValue: 360.0f,
                        label: "Hue");

                    sOkHsl = EditorGUILayout.Slider (
                        value: sOkHsl,
                        leftValue: 0.0f,
                        rightValue: 100.0f,
                        label: "Saturation");

                    lOkHsl = EditorGUILayout.Slider (
                        value: lOkHsl,
                        leftValue: 0.0f,
                        rightValue: 100.0f,
                        label: "Lightness");
                }
                break;
        }

        EditorGUILayout.Space ( );
        gradient = EditorGUILayout.GradientField (
            label: new GUIContent ("Gradient"),
            value: gradient,
            hdr: false);
        if (mode == Mode.OkHsl || mode == Mode.OkHsv)
        {
            hueEasing = (HueEasing) EditorGUILayout.EnumPopup (
                label: new GUIContent ("Easing"),
                selected: hueEasing,
                checkEnabled: OnEasingSelect,
                includeObsolete: false);

            switch (hueEasing)
            {
                case HueEasing.Clockwise:
                    {
                        hueFunc = (a, b, c, d) => OkUnityBridge.LerpAngleCw (a, b, c, d);
                    }
                    break;
                case HueEasing.CounterClockwise:
                    {
                        hueFunc = (a, b, c, d) => OkUnityBridge.LerpAngleCcw (a, b, c, d);
                    }
                    break;
                case HueEasing.Near:
                default:
                    {
                        hueFunc = (a, b, c, d) => OkUnityBridge.LerpAngleNear (a, b, c, d);
                    }
                    break;
            }
        }

        if (GUILayout.Button ("Generate"))
        {
            // Max No. of keys allowed by Unity gradients is 8.
            int count = 8;
            float toFac = 1.0f / (count - 1.0f);

            GradientColorKey[ ] uckeys = new GradientColorKey[count];
            (double r, double g, double b) aRgb = (r: aColor.r, g: aColor.g, b: aColor.b);
            (double r, double g, double b) bRgb = (r: bColor.r, g: bColor.g, b: bColor.b);
            (double L, double a, double b) aLab = OkColor.SrgbToOkLab (aRgb);
            (double L, double a, double b) bLab = OkColor.SrgbToOkLab (bRgb);

            // Gradients with gray colors are not eligible for HSL or HSV easing.
            Mode valMode = mode;
            double acsq = aLab.a * aLab.a + aLab.b * aLab.b;
            double bcsq = bLab.a * bLab.a + bLab.b * bLab.b;
            if (acsq < 0.0001d || bcsq < 0.0001d)
            {
                valMode = Mode.OkLab;
            }

            switch (valMode)
            {
                case Mode.OkHsl:
                    {
                        (double h, double s, double l) aHsl = OkColor.OkLabToOkHsl (aLab);
                        (double h, double s, double l) bHsl = OkColor.OkLabToOkHsl (bLab);
                        for (int i = 0; i < count; ++i)
                        {
                            float fac = i * toFac;
                            double ch = hueFunc (aHsl.h, bHsl.h, fac, 1.0d);
                            double cs = OkUnityBridge.Lerp (aHsl.s, bHsl.s, fac);
                            double cl = OkUnityBridge.Lerp (aHsl.l, bHsl.l, fac);
                            (double h, double s, double l) cHsl = (h: ch, s: cs, l: cl);
                            (double L, double a, double b) cLab = OkColor.OkHslToOkLab (cHsl);
                            (double r, double g, double b) cRgb = OkColor.OkLabToSrgb (cLab);
                            Color cColor = OkUnityBridge.RgbToColor (cRgb);
                            uckeys[i] = new GradientColorKey (cColor, fac);
                        }
                    }
                    break;
                case Mode.OkHsv:
                    {
                        (double h, double s, double v) aHsv = OkColor.OkLabToOkHsv (aLab);
                        (double h, double s, double v) bHsv = OkColor.OkLabToOkHsv (bLab);
                        for (int i = 0; i < count; ++i)
                        {
                            float fac = i * toFac;
                            double ch = hueFunc (aHsv.h, bHsv.h, fac, 1.0d);
                            double cs = OkUnityBridge.Lerp (aHsv.s, bHsv.s, fac);
                            double cv = OkUnityBridge.Lerp (aHsv.v, bHsv.v, fac);
                            (double h, double s, double v) cHsv = (h: ch, s: cs, v: cv);
                            (double r, double g, double b) cRgb = OkColor.OkHsvToSrgb (cHsv);
                            Color cColor = OkUnityBridge.RgbToColor (cRgb);
                            uckeys[i] = new GradientColorKey (cColor, fac);
                        }
                    }
                    break;
                case Mode.OkLab:
                default:
                    {
                        for (int i = 0; i < count; ++i)
                        {
                            float fac = i * toFac;
                            (double L, double a, double b) cLab = OkUnityBridge.LerpLab (aLab, bLab, fac);
                            (double r, double g, double b) cRgb = OkColor.OkLabToSrgb (cLab);
                            Color cColor = OkUnityBridge.RgbToColor (cRgb);
                            uckeys[i] = new GradientColorKey (cColor, fac);
                        }
                    }
                    break;
            }

            GradientAlphaKey[ ] uakeys = new GradientAlphaKey[ ]
            {
                new GradientAlphaKey (aColor.a, 0.0f),
                new GradientAlphaKey (bColor.a, 1.0f)
            };

            gradient.SetKeys (uckeys, uakeys);
            gradient.mode = GradientMode.Blend;
        }
    }

    static void GetColor (in Color c)
    {
        (double r, double g, double b) rgb = (r: c.r, g: c.g, b: c.b);
        (double l, double a, double b) lab = OkColor.SrgbToOkLab (rgb);
        lOkLab = (float) (lab.l * 100.0d);
        aOkLab = (float) (lab.a * 100.0d);
        bOkLab = (float) (lab.b * 100.0d);

        (double h, double s, double v) hsv = OkColor.OkLabToOkHsv (lab);
        hOkHsv = (float) (hsv.h * 360.0d);
        sOkHsv = (float) (hsv.s * 100.0d);
        vOkHsv = (float) (hsv.v * 100.0d);

        (double h, double s, double l) hsl = OkColor.OkLabToOkHsl (lab);
        hOkHsl = (float) (hsl.h * 360.0d);
        sOkHsl = (float) (hsl.s * 100.0d);
        lOkHsl = (float) (hsl.l * 100.0d);
    }

    static Color SetColor (in Mode mode, in float alpha = 1.0f)
    {
        (double r, double g, double b) rgb = (r: 0.0d, g: 0.0d, b: 0.0d);
        switch (mode)
        {
            case Mode.OkLab:
                {
                    (double l, double a, double b) lab = (
                        l: lOkLab * 0.01d,
                        a: aOkLab * 0.01d,
                        b: bOkLab * 0.01d);
                    rgb = OkColor.OkLabToSrgb (lab);
                }
                break;
            case Mode.OkHsv:
                {
                    (double h, double s, double v) hsv = (
                        h: hOkHsv / 360.0d,
                        s: sOkHsv * 0.01d,
                        v: vOkHsv * 0.01d);
                    rgb = OkColor.OkHsvToSrgb (hsv);
                }
                break;
            case Mode.OkHsl:
            default:
                {
                    (double h, double s, double l) hsl = (
                        h: hOkHsl / 360.0d,
                        s: sOkHsl * 0.01d,
                        l: lOkHsl * 0.01d);
                    rgb = OkColor.OkHslToSrgb (hsl);
                }
                break;
        }
        return OkUnityBridge.RgbToColor (rgb, alpha);
    }

    static bool OnEasingSelect (Enum mode)
    {
        return true;
    }

    static bool OnModeSelect (Enum mode)
    {
        return true;
    }
}