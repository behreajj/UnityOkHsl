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
        CounterClockwise = 2,
        Far = 3
    }

    public const int MaxKeyCount = 8;
    public const double HueYellow = 0.304914533545892d;
    public const double HueBlue = HueYellow + 0.5d;
    public const double HueGreen = 146.0d / 360.0d;
    public const double HueShadow = 291.0d / 360.0d;
    public const double HueDay = 96.0d / 360.0d;

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
    static Gradient shades = new Gradient ( );

    [MenuItem ("Window/OkColor")]
    static void Init ( )
    {
        gradient.SetKeys (new GradientColorKey[ ]
        {
            new GradientColorKey (new Color (1.0f, 0.0f, 0.0f, 1.0f), 0.0f),
                new GradientColorKey (new Color (0.9665875f, 0.0f, 0.471451f, 1.0f), 0.1428571f),
                new GradientColorKey (new Color (0.9013672f, 0.0001537915f, 0.7556401f, 1.0f), 0.2857143f),
                new GradientColorKey (new Color (0.7519007f, 0.1942044f, 0.9999999f, 1.0f), 0.4285715f),
                new GradientColorKey (new Color (0.51731f, 0.4186569f, 1.0f, 1.0f), 0.5714286f),
                new GradientColorKey (new Color (0.227677f, 0.5083964f, 1.0f, 1.0f), 0.7142857f),
                new GradientColorKey (new Color (0.0f, 0.5792553f, 0.8038571f, 1.0f), 0.8571429f),
                new GradientColorKey (new Color (0.0f, 0.6039216f, 0.6745098f, 1.0f), 1.0f)
        }, new GradientAlphaKey[ ]
        {
            new GradientAlphaKey (1.0f, 0.0f),
                new GradientAlphaKey (1.0f, 1.0f)
        });

        shades.SetKeys (new GradientColorKey[ ]
        {
            new GradientColorKey (new Color (0.2264238f, 0.0f, 0.127922f, 1.0f), 0.1428571f),
                new GradientColorKey (new Color (0.4338861f, 0.0f, 0.1902658f, 1.0f), 0.2857143f),
                new GradientColorKey (new Color (0.6498392f, 0.0f, 0.190087f, 1.0f), 0.4285714f),
                new GradientColorKey (new Color (0.8775765f, 0.0f, 0.0f, 1.0f), 0.5714286f),
                new GradientColorKey (new Color (1.0f, 0.3361318f, 0.0861003f, 1.0f), 0.7142857f),
                new GradientColorKey (new Color (1.0f, 0.6122944f, 0.4257437f, 1.0f), 0.8571429f),
                new GradientColorKey (new Color (1.0f, 0.8188902f, 0.7030936f, 1.0f), 1.0f)
        }, new GradientAlphaKey[ ]
        {
            new GradientAlphaKey (1.0f, 0.0f),
                new GradientAlphaKey (1.0f, 1.0f)
        });
        shades.mode = GradientMode.Fixed;

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
                case HueEasing.Far:
                    {
                        hueFunc = (a, b, c, d) => OkUnityBridge.LerpAngleFar (a, b, c, d);
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
            float toFac = 1.0f / (MaxKeyCount - 1.0f);

            GradientColorKey[ ] uckeys = new GradientColorKey[MaxKeyCount];
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
                        for (int i = 0; i < MaxKeyCount; ++i)
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
                        for (int i = 0; i < MaxKeyCount; ++i)
                        {
                            float fac = i * toFac;
                            double ch = hueFunc (aHsv.h, bHsv.h, fac, 1.0d);
                            double cs = OkUnityBridge.Lerp (aHsv.s, bHsv.s, fac);
                            double cv = OkUnityBridge.Lerp (aHsv.v, bHsv.v, fac);
                            (double h, double s, double v) cHsv = (h: ch, s: cs, v: cv);
                            (double L, double a, double b) cLab = OkColor.OkHsvToOkLab (cHsv);
                            (double r, double g, double b) cRgb = OkColor.OkLabToSrgb (cLab);
                            Color cColor = OkUnityBridge.RgbToColor (cRgb);
                            uckeys[i] = new GradientColorKey (cColor, fac);
                        }
                    }
                    break;
                case Mode.OkLab:
                default:
                    {
                        for (int i = 0; i < MaxKeyCount; ++i)
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

        EditorGUILayout.Space ( );
        shades = EditorGUILayout.GradientField (
            label: new GUIContent ("Shades"),
            value: shades,
            hdr: false);
        if (GUILayout.Button ("Generate"))
        {
            (double r, double g, double b) aRgb = (r: aColor.r, g: aColor.g, b: aColor.b);
            (double L, double a, double b) aLab = OkColor.SrgbToOkLab (aRgb);
            (double h, double s, double l) aHsl = OkColor.OkLabToOkHsl (aLab);

            double sat = aHsl.s;

            double hue = aHsl.h;
            double hueIncr = 0.125d;
            double[ ] hues = new double[ ]
            {
                OkUnityBridge.LerpAngleNear (hue, HueShadow, hueIncr * 3, 1.0d),
                OkUnityBridge.LerpAngleNear (hue, HueShadow, hueIncr * 2, 1.0d),
                OkUnityBridge.LerpAngleNear (hue, HueShadow, hueIncr, 1.0d),
                hue,
                OkUnityBridge.LerpAngleNear (hue, HueDay, hueIncr, 1.0d),
                OkUnityBridge.LerpAngleNear (hue, HueDay, hueIncr * 2, 1.0d),
                OkUnityBridge.LerpAngleNear (hue, HueDay, hueIncr * 3, 1.0d),
            };

            double lightIncr = 0.125d;
            double[ ] lights = new double[ ]
            {
                0.5f - lightIncr * 3,
                0.5f - lightIncr * 2,
                0.5f - lightIncr,
                0.5f,
                0.5f + lightIncr,
                0.5f + lightIncr * 2,
                0.5f + lightIncr * 3,
            };

            GradientColorKey[ ] uckeys = new GradientColorKey[7];
            for (int i = 0; i < 7; ++i)
            {
                (double h, double s, double l) cHsl = (h: hues[i], s: sat, l: lights[i]);
                (double L, double a, double b) cLab = OkColor.OkHslToOkLab (cHsl);
                (double r, double g, double b) cRgb = OkColor.OkLabToSrgb (cLab);
                Color cColor = OkUnityBridge.RgbToColor (cRgb);
                uckeys[i] = new GradientColorKey (cColor, (i + 1.0f) / 7.0f);
            }

            GradientAlphaKey[ ] uakeys = new GradientAlphaKey[ ]
            {
                new GradientAlphaKey (aColor.a, 0.0f),
                new GradientAlphaKey (aColor.a, 1.0f)
            };

            shades.SetKeys (uckeys, uakeys);
            shades.mode = GradientMode.Fixed;
        }
    }

    static void GetColor (in Color c)
    {
        (double r, double g, double b) rgb = (r: c.r, g: c.g, b: c.b);
        (double L, double a, double b) lab = OkColor.SrgbToOkLab (rgb);
        lOkLab = (float) (lab.L * 100.0d);
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
                    rgb = OkColor.OkLabToSrgb ((
                        L: lOkLab * 0.01d,
                        a: aOkLab * 0.01d,
                        b: bOkLab * 0.01d));
                }
                break;
            case Mode.OkHsv:
                {
                    rgb = OkColor.OkHsvToSrgb ((
                        h: hOkHsv / 360.0d,
                        s: sOkHsv * 0.01d,
                        v: vOkHsv * 0.01d));
                }
                break;
            case Mode.OkHsl:
            default:
                {
                    rgb = OkColor.OkHslToSrgb ((
                        h: hOkHsl / 360.0d,
                        s: sOkHsl * 0.01d,
                        l: lOkHsl * 0.01d));
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