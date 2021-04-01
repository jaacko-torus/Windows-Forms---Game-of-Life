using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace month_6_Project_and_Portfolio_I {
    public static class UVector2 {
        // TODO: `Floor` should be in a helper file
        public static Vector2 Floor(Vector2 v) => new Vector2((float)Math.Floor(v.X), (float)Math.Floor(v.Y));

        // https://en.wikipedia.org/wiki/Modulo_operation
        // `mod1` should be same as `a % b`
        public static Vector2 mod1(Vector2 a, Vector2 b) => a - b * (a / b);
        public static Vector2 mod2(Vector2 a, Vector2 b) => a - b * UVector2.Floor(a / b);
        public static Vector2 mod3(Vector2 a, Vector2 b) => a - Vector2.Abs(b) * UVector2.Floor(a / Vector2.Abs(b));

        public static Vector2 mod1(Vector2 a, int b) => a - b * (a / b);
        public static Vector2 mod2(Vector2 a, int b) => a - b * UVector2.Floor(a / b);
        public static Vector2 mod3(Vector2 a, int b) => a - new Vector2(Math.Abs(b)) * UVector2.Floor(a / Math.Abs(b));
    }
}
