using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace _2DNormalCalculator
{
    class BitmapSprite
    {
        Bitmap colorMap;
        public Bitmap ColorMap
        {
            get
            {
                return colorMap;
            }
        }

        Bitmap normalMap;
        public Bitmap NormalMap
        {
            get
            {
                return normalMap;
            }
        }

        Bitmap specialChannelsMap;
        public Bitmap SpecialChannelsMap
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

        public BitmapSprite()
        {
            colorMap = null;
            normalMap = null;
            specialChannelsMap = null;
            currentFile = "";
        }

        public void OpenAll(string file)
        {
            if (System.IO.File.Exists(file))
            {
                currentFile = file;

                LoadColorMap();

                LoadNormalMap();

                LoadSpecialMap();
            }
        }

        public void LoadSpecialMap()
        {
            int x; int y;
            specialChannelsMap = null;
            string currentSpecialFile = currentFile.Insert(currentFile.Length - 4, "_specials");
            if (System.IO.File.Exists(currentSpecialFile))
            {
                specialChannelsMap = (Bitmap)LoadImageNoLock(currentSpecialFile);
                if (specialChannelsMap.Width != colorMap.Width || specialChannelsMap.Height != colorMap.Height)
                {
                    MessageBox.Show("Specials map was not the same size as original image, file was not loaded");
                    specialChannelsMap = null;
                }
            }

            if (specialChannelsMap == null)
            {
                specialChannelsMap = new Bitmap(colorMap.Width, colorMap.Height);
                for (y = 0; y < colorMap.Height; y++)
                {
                    for (x = 0; x < colorMap.Width; x++)
                    {
                        specialChannelsMap.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0));
                    }
                }
            }
        }

        public void LoadNormalMap()
        {
            int x; 
            int y;
            string currentNormalFile = currentFile.Insert(currentFile.Length - 4, "_normals");
            normalMap = null;

            if (System.IO.File.Exists(currentNormalFile))
            {
                normalMap = (Bitmap)LoadImageNoLock(currentNormalFile);
                if (normalMap.Width != colorMap.Width || normalMap.Height != colorMap.Height)
                {
                    MessageBox.Show("Normal map was not the same size as original image, file was not loaded");
                    normalMap = null;
                }
            }

            if (normalMap == null)
            {
                normalMap = new Bitmap(colorMap.Width, colorMap.Height);
                for (y = 0; y < colorMap.Height; y++)
                {
                    for (x = 0; x < colorMap.Width; x++)
                    {
                        normalMap.SetPixel(x, y, Color.Transparent);
                    }
                }
            }
        }

        public void LoadColorMap()
        {
            if (colorMap == null)
                colorMap = (Bitmap)LoadImageNoLock(currentFile);
            else
            {
                Bitmap newColorMap = (Bitmap)LoadImageNoLock(currentFile);
                if (newColorMap.Width != colorMap.Width || newColorMap.Height != colorMap.Height)
                {
                    MessageBox.Show("New image was not the same size as orignal, file was not loaded");
                    return;
                }
                colorMap = newColorMap;
            }
        }

        public void SaveNormalMap()
        {
            string currentNormalFile = currentFile.Insert(currentFile.Length - 4, "_normals");

            if (File.Exists(currentNormalFile))
            {
                if (File.Exists(currentNormalFile + ".BAK"))
                    File.Delete(currentNormalFile + ".BAK");
                File.Move(currentNormalFile, currentNormalFile + ".BAK");
            }

            normalMap.Save(currentNormalFile);
        }

        public static Image LoadImageNoLock(string path)
        {
            using (var ms = new MemoryStream(File.ReadAllBytes(path)))
            {
                return Image.FromStream(ms);
            }
        }

        internal void SaveSpecialChannelsMap()
        {
            string currentSpecialsFile = currentFile.Insert(currentFile.Length - 4, "_specials");

            if (File.Exists(currentSpecialsFile))
            {
                if (File.Exists(currentSpecialsFile + ".BAK"))
                    File.Delete(currentSpecialsFile + ".BAK");
                File.Move(currentSpecialsFile, currentSpecialsFile + ".BAK");
            }

            specialChannelsMap.Save(currentSpecialsFile);
        }

        private Microsoft.Xna.Framework.Graphics.Texture2D TextureFromBitmap(Bitmap img, Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            Microsoft.Xna.Framework.Graphics.Texture2D texture = null;
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin); //must do this, or error is thrown in next line
                texture = Microsoft.Xna.Framework.Graphics.Texture2D.FromFile(graphicsDevice, stream);
            }
            return texture;
        }

        public Microsoft.Xna.Framework.Graphics.Texture2D ColorMapToTexture(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            return TextureFromBitmap(colorMap, graphicsDevice);
        }

        public Microsoft.Xna.Framework.Graphics.Texture2D NormalMapToTexture(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            return TextureFromBitmap(normalMap, graphicsDevice);
        }

        public Microsoft.Xna.Framework.Graphics.Texture2D SpecialChannelsMapToTexture(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            return TextureFromBitmap(specialChannelsMap, graphicsDevice);
        }

        static public Bitmap BitmapFromTexture2D(Microsoft.Xna.Framework.Graphics.Texture2D texture)
        {
            int[] data = new int[texture.Width * texture.Height];
            texture.GetData<int>(0, null, data, 0, texture.Width * texture.Height);
            Bitmap bitmap = new Bitmap(texture.Width, texture.Height);

            for (int x = 0; x < texture.Width; ++x)
            {
                for (int y = 0; y < texture.Height; ++y)
                {
                    Color bitmapColor =
                        Color.FromArgb(data[y * texture.Width + x]);

                    bitmap.SetPixel(x, y, bitmapColor);
                }
            }
            return bitmap;
        }
    }
}
