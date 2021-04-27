using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace month_6_Project_and_Portfolio_I {
    public static class UFloat {
        // https://en.wikipedia.org/wiki/Modulo_operation
        // `mod1` should be same as `a % b`
        public static float mod1(float a, float b) => a - b * (a / b);
        public static float mod2(float a, float b) => a - b * (float)Math.Floor(a / b);
        public static float mod3(float a, float b) => a - (float)Math.Abs(b) * (float)Math.Floor(a / Math.Abs(b));
    }

    public static class UVector2 {
        public static Vector2 Left  => new Vector2(-1,  0);
        public static Vector2 Right => new Vector2( 1,  0);
        public static Vector2 Up    => new Vector2( 0,  1);
        public static Vector2 Down  => new Vector2( 0, -1);

        public static Vector2 Normalized(Vector2 v) => v == Vector2.Zero ? v : Vector2.Normalize(v);

        public static Vector2 Floor(Vector2 v) => new Vector2((float)Math.Floor(v.X), (float)Math.Floor(v.Y));

        public static Vector2 mod1(Vector2 a, Vector2 b) => a - b * (a / b);
        public static Vector2 mod2(Vector2 a, Vector2 b) => a - b * UVector2.Floor(a / b);
        public static Vector2 mod3(Vector2 a, Vector2 b) => a - Vector2.Abs(b) * UVector2.Floor(a / Vector2.Abs(b));

        public static Vector2 mod1(Vector2 a, float b) => a - b * (a / b);
        public static Vector2 mod2(Vector2 a, float b) => a - b * UVector2.Floor(a / b);
        public static Vector2 mod3(Vector2 a, float b) => a - new Vector2(Math.Abs(b)) * UVector2.Floor(a / Math.Abs(b));

        public static PointF ToPointF(this Vector2 vector2) => new PointF(vector2.X, vector2.Y);
        public static SizeF ToSizeF(this Vector2 vector2) => new SizeF(vector2.X, vector2.Y);
    }

    public static class UIEnumerable {
        public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> enumerable) =>
            enumerable.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public static class UHashSet {
        public static void ForEach<T>(this HashSet<T> hashset, Action<T> callback) =>
            hashset.ToList().ForEach((item) => callback(item));
    }

    public static class UDictionary {
        public static void ForEach<K, V>(this Dictionary<K, V> dictionary, Action<K> callback) =>
            dictionary.Keys.ToList().ForEach(key => callback(key));

        public static void ForEach<K, V>(this Dictionary<K, V> dictionary, Action<K, V> callback) =>
            dictionary.ToList().ForEach(pair => callback(pair.Key, pair.Value));
    }

    public static class UPoint {
        public static Vector2 ToVector2(this Point point) => new Vector2(point.X, point.Y);
    }

    public static class USize {
        public static Vector2 ToVector2(this Size size) => new Vector2(size.Width, size.Height);
    }

    public static class UMath {
        public static float Clamp(float n, float min, float max) => n < min ? min : n > max ? max : n;
    }

    public static class UMatrix {
        public delegate void ForEachXCallback(int x);
        public delegate void ForEachYCallback(int y);
        public delegate void ForEachXYCallback(int x, int y);

        public static void ForEach(Vector2 size, ForEachXCallback x_callback, ForEachXYCallback xy_callback) {
            for (int x = 0; x < size.X; x += 1) {
                x_callback(x);

                for (int y = 0; y < size.Y; y += 1) {
                    xy_callback(x, y);
                }
            }
        }

        public static void ForEach(Vector2 size, ForEachXYCallback xy_callback) =>
            UMatrix.ForEach(size, (x) => { }, xy_callback);

        public static void ForEachRotated(Vector2 size, ForEachYCallback y_callback, ForEachXYCallback xy_callback) =>
            UMatrix.ForEach(new Vector2(size.Y, size.X), (y) => y_callback(y), (x, y) => xy_callback(y, x));

        public static void ForEachRotated(Vector2 size, ForEachXYCallback xy_callback) =>
            UMatrix.ForEachRotated(size, (y) => { }, xy_callback);







        public delegate void ForEach3x3Callback(Vector2 curr_cell);

        public static void ForEach3x3(Vector2 curr_cell, ForEach3x3Callback callback) {
            // TODO: rewrite this in terms of ForEach
            for (int x_offset = -1; x_offset <= 1; x_offset += 1) {
                for (int y_offset = -1; y_offset <= 1; y_offset += 1) {
                    callback(curr_cell + new Vector2(x_offset, y_offset));
                }
            }
        }



        public delegate T Reduce3x3Callback<T>(T prev_cell, Vector2 curr_cell);

        public static T Reduce3x3<T>(Vector2 curr_cell, Reduce3x3Callback<T> callback, T initial) {
            T result = initial;

            UMatrix.ForEach3x3(curr_cell, curr_neighbour => {
                result = callback(result, curr_neighbour);
            });

            return result;
        }

        public static Vector2 Reduce3x3(Vector2 curr_cell, Reduce3x3Callback<Vector2> callback) {
            return UMatrix.Reduce3x3<Vector2>(
                curr_cell,
                (prev, curr) => curr_cell == curr ? prev : callback(prev, curr),
                curr_cell
            );
        }
    }
}
