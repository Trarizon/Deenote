#nullable enable

using Deenote.Library.Collections;
using System;
using UnityEngine;

namespace Deenote.Editing.Grids
{
    // REFACTOR: 考虑移到Core里
    public sealed class SplineCurve
    {
        private readonly double[] x;
        private readonly double[] a;
        private readonly double[] b;
        private readonly double[] c;
        private readonly double[] d;

        public float MinX => (float)x[0];
        public float MaxX => (float)x[^1];

        private SplineCurve(int pointCount)
        {
            x = new double[pointCount];
            a = new double[pointCount];
            c = new double[pointCount];
            b = new double[pointCount - 1];
            d = new double[pointCount - 1];
        }

        internal static SplineCurve Linear<T>(ReadOnlySpan<T> source, Func<T, float> xSelector, Func<T, float> ySelector)
        {
            var curve = new SplineCurve(source.Length);

            for (int i = 0; i < source.Length; i++) {
                var item = source[i];
                curve.x[i] = xSelector.Invoke(item);
                curve.a[i] = ySelector.Invoke(item);
            }
            for (int i = 0; i < source.Length - 1; i++) {
                curve.b[i] = (curve.a[i + 1] - curve.a[i]) / (curve.x[i + 1] - curve.x[i]);
            }
            Array.Clear(curve.c, 0, curve.c.Length);
            Array.Clear(curve.d, 0, curve.d.Length);

            return curve;
        }

        internal static SplineCurve Cubic<T>(ReadOnlySpan<T> source, Func<T, float> xSelector, Func<T, float> ySelector)
        {
            var curve = new SplineCurve(source.Length);

            var x = curve.x.AsSpan();
            var a = curve.a.AsSpan();
            var b = curve.b.AsSpan();
            var c = curve.c.AsSpan();
            var d = curve.d.AsSpan();

            int count = source.Length;

            for (int i = 0; i < count; i++) {
                var item = source[i];
                x[i] = xSelector.Invoke(item);
                a[i] = ySelector.Invoke(item);
            }

            var alpha = (stackalloc double[count - 1]);
            var h = (stackalloc double[count - 1]);
            var mu = (stackalloc double[count]);
            var l = (stackalloc double[count]);
            var z = (stackalloc double[count]);

            for (int i = 0; i < count - 1; i++) {
                h[i] = x[i + 1] - x[i];
            }
            for (int i = 1; i < count - 1; i++) {
                alpha[i] = 3 * (a[i + 1] - a[i]) / h[i] - 3 * (a[i] - a[i - 1]) / h[i - 1];
            }

            l[0] = 1d;
            mu[0] = 0d;
            z[0] = 0d;
            for (int i = 1; i < count - 1; i++) {
                l[i] = 2 * (x[i + 1] - x[i - 1]) - h[i - 1] * mu[i - 1];
                mu[i] = h[i] / l[i];
                z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
            }
            l[^1] = 1d;
            z[^1] = 0d;
            c[^1] = 0d;

            for (int i = count - 2; i >= 0; i--) {
                c[i] = z[i] - mu[i] * c[i + 1];
                b[i] = (a[i + 1] - a[i]) / h[i] - h[i] * (c[i + 1] + 2 * c[i]) / 3;
                d[i] = (c[i + 1] - c[i]) / (3 * h[i]);
            }

            return curve;
        }

        public float? GetValue(float time)
        {
            if (time < x[0] || time > x[^1])
                return null;

            for (int i = 0; i < x.Length - 1; i++) {
                if (!(time < x[i + 1])) continue;
                double diff = time - x[i];
                return (float)(
                    a[i] +
                    b[i] * diff +
                    c[i] * diff * diff +
                    d[i] * diff * diff * diff
                );
            }

            Debug.Assert(time == x[^1]);
            return 0;
        }
    }
}
