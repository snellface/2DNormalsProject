using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace _2DNormalCalculator
{
    [Serializable]
    class Light
    {
        public Light()
        {
            Location = new Vector3(0, 0, 10);
            Color = new Vector3(1, 1, 1);
            Intensity = 100;
            distanceCoefficient = 0.1f;
            UseDistanceCoefficientInsteadOfRealisticLight = false;
        }

        public Light(Vector3 location, Vector3 color, float intesity)
        {
            Location = location;
            Color = color;
            Intensity = intesity;
        }

        public Light(Vector3 location, System.Drawing.Color color, float intesity, bool active)
        {
            Location = location;
            Color = new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
            Intensity = intesity;
            Active = active;
        }

        Vector3 location;
        public Vector3 Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
            }
        }

        float distanceCoefficient;
        public float DistanceCoefficient
        {
            get
            {
                return distanceCoefficient;
            }
            set
            {
                distanceCoefficient = value;
            }
        }

        public bool UseDistanceCoefficientInsteadOfRealisticLight
        {
            get;
            set;
        }

        public float CalculateDistance(Vector3 vectorFromSurfaceToLight)
        {
            if (UseDistanceCoefficientInsteadOfRealisticLight)
                return vectorFromSurfaceToLight.Length() * distanceCoefficient;
            else
                return (float)Math.Pow(vectorFromSurfaceToLight.Length(), 2);
        }

        public static Vector3 Reflect(Vector3 vector, Vector3 normal)
        {
            Vector3 reflection = normal * Vector3.Dot(vector, normal);
            return vector - (reflection * 2);
        }

        public float VisibleFrom(Vector3 pixel, BitmapSprite sprite)
        {
            float visible = 1;
            int lightX = (int)X;
            int lightY = (int)Y;

            LinkedList<Vector3> points = Utility.BresenhamLine3D(pixel, location);

            int count = points.Count;
            Vector3 v;
            Color pixelOnHeightMap;
            int x; int y; int z;
            //return points.Count;
            while (count > 0)
            {
                v = points.First();
                points.RemoveFirst();
                x = (int)v.X;
                y = (int)v.Y;
                z = (int)v.Z;
                if (x >= 0 && x < sprite.Width && y >= 0 && y < sprite.Height)
                {
                    pixelOnHeightMap = sprite.SpecialChannelsMap.GetPixel(x, y);
                    // if point is over pixel.R (low) and below pixel.G (high) its obscured
                    if (pixelOnHeightMap.R < z && z < pixelOnHeightMap.G)
                    {
                        
                        visible = 0;
                        return visible;
                    }
                }
                --count;
            }


            return visible;
        }

        public float R
        {
            get
            {
                return color.X;
            }
            set
            {
                color.X = value;
            }
        }

        public float G
        {
            get
            {
                return color.Y;
            }
            set
            {
                color.Y = value;
            }
        }

        public float B
        {
            get
            {
                return color.Z;
            }
            set
            {
                color.Z = value;
            }
        }

        public float X
        {
            get
            {
                return location.X;
            }
            set
            {
                location.X = value;
            }
        }

        public float Y
        {
            get
            {
                return location.Y;
            }
            set
            {
                location.Y = value;
            }
        }

        public float Z
        {
            get
            {
                return location.Z;
            }
            set
            {
                location.Z = value;
            }
        }

        Vector3 color;
        public Vector3 Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        public bool Active
        {
            get;
            set;
        }

        public float Intensity
        {
            get;
            set;
        }
    }
}
