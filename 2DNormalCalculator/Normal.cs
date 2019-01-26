using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Drawing;

namespace _2DNormalCalculator
{
    abstract class Normal
    {
        public enum DrawOrCapture
        {
            Draw,
            Capture,
            ArrayFill,
            Undefined
        };

        public static float Clamp(float value, float min, float max)
        {
            if (value > max)
                value = max;
            if (value < min)
                value = min;
            return value;
        }

        public static Vector3 GetVectorFromCoords(float pX, float pY)
        {
            Vector2 xyVector = new Vector2((pX - 0.5f) * 2, (pY - 0.5f) * 2);
            xyVector.X = Clamp(xyVector.X, -1, 1);
            xyVector.Y = Clamp(xyVector.Y, -1, 1);

            if (xyVector.Length() > 1)
            {
                xyVector.Normalize();
            }

            Vector3 vector = new Vector3(xyVector, (float)Math.Sqrt(Math.Pow(1 - xyVector.Length(), 2)));

            return vector;
        }

        public static Color ColorFromNormalVector(Vector3 normal)
        {
            return Color.FromArgb((int)((normal.X / 2 + 0.5) * 255), (int)((normal.Y / 2 + 0.5) * 255), (int)((normal.Z / 2 + 0.5) * 255));
        }

        public static Vector3 NormalFromColor(Color color)
        {
            return new Vector3(((color.R / 255.0f) - 0.5f) * 2.0f, ((color.G / 255.0f) - 0.5f) * 2.0f, ((color.B / 255.0f) - 0.5f) * 2.0f);
        }
    }
}
