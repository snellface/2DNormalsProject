using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace _2DNormalCalculator
{
    class Sprite
    {
        public enum MapType
        {
            Color,
            Normals,
            Special
        }

        Texture2D colorMap;
        Texture2D normalMap;
        Texture2D specialChannelsMap;

        public Texture2D ColorMap
        {
            get
            {
                return colorMap;
            }
        }
        public Texture2D NormalMap
        {
            get
            {
                return normalMap;
            }
        }
        public Texture2D SpecialChannelsMap
        {
            get
            {
                return specialChannelsMap;
            }
        }

        public int Width
        {
            get
            {
                return colorMap.Width;
            }
        }

        public int Height
        {
            get
            {
                return colorMap.Height;
            }
        }

        string currentFile;

        public Sprite()
        {
            colorMap = null;
            normalMap = null;
            specialChannelsMap = null;
            currentFile = "";
        }

        public void OpenAll(GraphicsDevice device, string file)
        {
            if (System.IO.File.Exists(file))
            {
                currentFile = file;

                LoadColorMap(device);

                LoadNormalMap(device);

                LoadSpecialMap(device);
            }
        }

        public void LoadSpecialMap(GraphicsDevice device)
        {
            int x; int y;
            specialChannelsMap = null;
            string currentSpecialFile = currentFile.Insert(currentFile.Length - 4, "_specials");
            if (System.IO.File.Exists(currentSpecialFile))
            {
                specialChannelsMap = Texture2D.FromFile(device, currentSpecialFile);
                if (specialChannelsMap.Width != colorMap.Width || specialChannelsMap.Height != colorMap.Height)
                {
                    specialChannelsMap = null;
                }
            }

            if (specialChannelsMap == null)
            {
                Color[] map = new Color[colorMap.Height * colorMap.Width];
                for (y = 0; y < colorMap.Height; y++)
                {
                    for (x = 0; x < colorMap.Width; x++)
                    {
                        map[x + y * colorMap.Width] = Color.Black;
                    }
                }

                specialChannelsMap = new Texture2D(device, colorMap.Width, colorMap.Height);
                specialChannelsMap.SetData(map);
            }
        }

        public void LoadNormalMap(GraphicsDevice device)
        {
            int x; int y;
            string currentNormalFile = currentFile.Insert(currentFile.Length - 4, "_normals");
            normalMap = null;

            if (System.IO.File.Exists(currentNormalFile))
            {
                normalMap = Texture2D.FromFile(device, currentNormalFile);
                if (normalMap.Width != colorMap.Width || normalMap.Height != colorMap.Height)
                {
                    normalMap = null;
                }
            }

            if (normalMap == null)
            {
                Color[] map = new Color[colorMap.Height * colorMap.Width];
                for (y = 0; y < colorMap.Height; y++)
                {
                    for (x = 0; x < colorMap.Width; x++)
                    {
                        map[x + y * colorMap.Width] = Color.TransparentBlack;
                    }
                }

                normalMap = new Texture2D(device, colorMap.Width, colorMap.Height);
                normalMap.SetData(map);
            }
        }

        public void LoadColorMap(GraphicsDevice device)
        {
            if (colorMap == null)
                colorMap = Texture2D.FromFile(device, currentFile);
            else
            {
                Texture2D newColorMap = Texture2D.FromFile(device, currentFile);
                if (newColorMap.Width != colorMap.Width || newColorMap.Height != colorMap.Height)
                {
                    return;
                }
                colorMap = newColorMap;
            }
        }

        public void SaveNormalMap()
        {
            string currentNormalFile = currentFile.Insert(currentFile.Length - 4, "_normals");
            normalMap.Save(currentNormalFile, ImageFileFormat.Png);
        }

        internal void SaveSpecialChannelsMap()
        {
            string currentSpecialsFile = currentFile.Insert(currentFile.Length - 4, "_specials");
            specialChannelsMap.Save(currentSpecialsFile, ImageFileFormat.Png);
        }
    }
}
