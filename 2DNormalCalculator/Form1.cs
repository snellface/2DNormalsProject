using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework.Graphics;

namespace _2DNormalCalculator
{
    public partial class Form1 : Form
    {
        Vector3? normal;
        BitmapSprite sprite;
        int spriteAltitude = 0;

        List<Light> lights;

        bool drawingInImage = false;
        bool mouseStillInDrawImage = false;
        Normal.DrawOrCapture drawOrCapture = Normal.DrawOrCapture.Draw;
        Vector3 eyeLoc = new Vector3(50, 50, 50);
        Vector3 eyeDir = new Vector3(0, 0, -1);

        RenderForm renderForm;

        bool movingLightSource = false;

        public Form1()
        {
            InitializeComponent();
            normal = null;
            sprite = null;

            int i;
            lights = new List<Light>();
            lights.Add(new Light(new Vector3(0, 0, 50), System.Drawing.Color.FromArgb(255, 255, 255), 100, true));
            for (i = 0; i < 9; i++)
            {
                lights.Add(new Light(new Vector3(0, 0, 50), System.Drawing.Color.FromArgb(255, 255, 255), 100, false));
            }
        }

        public int Magnification 
        {
            get
            {
                return (int)magnificationNumUpDown.Value;
            }
            set
            {
                magnificationNumUpDown.Value = value;
            }
        }

        private void RedrawPreviews()
        {
            directionPickerImage.Refresh();
            currentNormalPreview.Refresh();
            heightMapHeightPicker.Refresh();
        }

        private void directionPickerImage_Paint(object sender, PaintEventArgs e)
        {
            bool pointerOverControl = false;
            System.Drawing.Point location = directionPickerImage.PointToClient(Cursor.Position);
            System.Drawing.Point center = new System.Drawing.Point(directionPickerImage.Width / 2, directionPickerImage.Height / 2);

            if (normal != null)
            {
                if(normal.Value.X > 0)
                    e.Graphics.DrawLine(Pens.Green, center.X, center.Y, center.X + normal.Value.X * ((float)255 / 2), center.Y - normal.Value.Y * ((float)255 / 2));
                else
                    e.Graphics.DrawLine(Pens.Red, center.X, center.Y, center.X + normal.Value.X * ((float)255 / 2)+1, center.Y - normal.Value.Y * ((float)255 / 2)+1);
            }

            if (location.X < 0 || location.Y < 0 || location.X > directionPickerImage.Width || location.Y > directionPickerImage.Height)
                pointerOverControl = false;
            else
                pointerOverControl = true;

            if (pointerOverControl)
            {
                // Convert "in control coords" to floats from -1 to 1
                float x = (location.X - 5) / (float)(directionPickerImage.Width - 10);
                float y = (location.Y - 5) / (float)(directionPickerImage.Height - 10);

                // Get normal vector
                Vector3 vector = Normal.GetVectorFromCoords(x, 1-y);

                // Draw direction line
                e.Graphics.DrawLine(Pens.Black, center.X, center.Y, center.X + vector.X * ((float)255 / 2), center.Y - vector.Y * ((float)255 / 2));

                // Draw height line on right side of circle
                e.Graphics.DrawLine(Pens.White, directionPickerImage.Width - 3, center.Y, directionPickerImage.Width - 3, center.Y - vector.Z * ((float)255 / 2));

                // Draw pixel color preview
                SolidBrush normalColorBrush = new SolidBrush(Normal.ColorFromNormalVector(vector));
                e.Graphics.FillRectangle(normalColorBrush, 10, 10, 10, 10);
                normalColorBrush.Dispose();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            renderForm = new RenderForm();
            renderForm.Lights = lights;
            renderForm.LightToEditLink = lightToEdit;
            renderForm.Show();

 
        }

        private void directionPickerImage_MouseMove(object sender, MouseEventArgs e)
        {
            RedrawPreviews();
        }

        private void directionPickerImage_MouseLeave(object sender, EventArgs e)
        {
            RedrawPreviews();
        }

        private void currentNormalPreview_Paint(object sender, PaintEventArgs e)
        {
            if (normal == null)
                return;

            SolidBrush brush = new SolidBrush(Normal.ColorFromNormalVector(normal.Value));
            e.Graphics.FillRectangle(brush, 0, 0, currentNormalPreview.Width, currentNormalPreview.Height);
            brush.Dispose();

            int x = (int)(((normal.Value.X / 2) + 0.5f) * currentNormalPreview.Width);
            int y = currentNormalPreview.Height - (int)(((normal.Value.Y / 2) + 0.5f) * currentNormalPreview.Height);
            e.Graphics.DrawLine(Pens.Red, currentNormalPreview.Width / 2, currentNormalPreview.Height / 2, x, y);
            e.Graphics.DrawRectangle(Pens.Black, x - 1, y - 1, 2, 2);
        }

        private void directionPickerImage_MouseUp(object sender, MouseEventArgs e)
        {
            float x = (e.X - 5) / (float)(directionPickerImage.Width - 10);
            float y = ((directionPickerImage.Height - e.Y) - 5) / (float)(directionPickerImage.Height - 10);

            if (e.Button == MouseButtons.Left)
            {
                normal = Normal.GetVectorFromCoords(x, y);
            }
            else if (e.Button == MouseButtons.Right)
            {
                normal = null;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                if (normal == null)
                    normal = Vector3.UnitZ;
                else
                {
                    Vector3 newNormal = normal.Value + Normal.GetVectorFromCoords(x, y);
                    newNormal.Normalize();
                    normal = newNormal;
                }
            }
            else if (e.Button == MouseButtons.XButton2)
            {
                if (normal == null)
                    normal = Vector3.UnitZ;
                Vector3 newNormal = normal.Value + Normal.GetVectorFromCoords(x, y);
                newNormal.Normalize();
                normal = newNormal;
            }
            else if (e.Button == MouseButtons.XButton1)
            {
                if (normal == null)
                    normal = Vector3.UnitZ;
                Vector3 newNormal = normal.Value + Vector3.UnitZ;
                newNormal.Normalize();
                normal = newNormal;
            }
            else
            {
                normal = new Vector3(0, 0, 1);
            }
            RedrawPreviews();
        }

        private void drawPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            Draw(e.X, e.Y);

            drawingInImage = false;
            lineStarted = false;
        }

        private void drawPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawingInImage && mouseStillInDrawImage)
            {
                Draw(e.X, e.Y);
            }
        }

        private void drawPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            drawingInImage = true;
            if (e.Button == MouseButtons.Left)
                drawOrCapture = Normal.DrawOrCapture.Draw;
            else if (e.Button == MouseButtons.Right)
                drawOrCapture = Normal.DrawOrCapture.Capture;
            else if (e.Button == MouseButtons.Middle)
                drawOrCapture = Normal.DrawOrCapture.ArrayFill;
            else
                drawOrCapture = Normal.DrawOrCapture.Undefined;

            Draw(e.X, e.Y);
        }

        bool lineStarted = false;
        int lastDrawLocationX = -1;
        int lastDrawLocationY = -1;
        private void Draw(int x, int y)
        {
            if (sprite == null)
                return;

            x = (int)((x / (float)xnaFrame.Width) * 320);
            y = (int)((y / (float)xnaFrame.Height) * 200);
            
            if (x < 0 || y < 0 || x >= sprite.ColorMap.Width || y >= sprite.ColorMap.Height)
                return;

            if (lineStarted)
            {
                LinkedList<System.Drawing.Point> points = null;
                points = Utility.BresenhamLine2D(x, y, lastDrawLocationX, lastDrawLocationY);
                foreach (System.Drawing.Point p in points)
                {
                    if (toolsTabControl.SelectedTab.Name == "normalsTabPage")
                        DrawOnNormalMap(p.X, p.Y);
                    else if (toolsTabControl.SelectedTab.Name == "heightTabPage")
                        DrawOnHeightMap(p.X, p.Y);
                    else if (toolsTabControl.SelectedTab.Name == "specularTabPage")
                        DrawOnSpecularMap(p.X, p.Y);
                }
            }
            else
            {
                if (toolsTabControl.SelectedTab.Name == "normalsTabPage")
                    DrawOnNormalMap(x, y);
                else if (toolsTabControl.SelectedTab.Name == "heightTabPage")
                    DrawOnHeightMap(x, y);
                else if (toolsTabControl.SelectedTab.Name == "specularTabPage")
                    DrawOnSpecularMap(x, y);
            }

            lineStarted = true;
            lastDrawLocationX = x;
            lastDrawLocationY = y;
            RenderDrawImage();
            objectChangedSinceLastRender = true;
            renderForm.ObjectChangedSinceLastRender = true;
            renderForm.RenderPreviewImage();
            RedrawPreviews();
        }

        private void DrawOnSpecularMap(int x, int y)
        {
            int specularLevel = (int)specularUpDown.Value;
            System.Drawing.Color newPixel = sprite.SpecialChannelsMap.GetPixel(x, y);

            if (drawOrCapture == Normal.DrawOrCapture.Draw)
            {
                newPixel = System.Drawing.Color.FromArgb(newPixel.A, newPixel.R, newPixel.G, specularLevel);
                sprite.SpecialChannelsMap.SetPixel(x, y, newPixel);
            }
            else if (drawOrCapture == Normal.DrawOrCapture.Capture)
            {
                specularUpDown.Value = newPixel.B;
            }
            else if (drawOrCapture == Normal.DrawOrCapture.ArrayFill)
            {
                newPixel = System.Drawing.Color.FromArgb(newPixel.A, newPixel.R, newPixel.G, specularLevel);

                FloodFill(sprite.SpecialChannelsMap, x, y, newPixel);
            }
        }

        private void DrawOnHeightMap(int x, int y)
        {
            int lowValue = (int)lowHeightUpDown.Value;
            int highValue = (int)highHeightUpDown.Value;
            bool drawLow = drawOnBothRadioButton.Checked || drawOnLowRadioButton.Checked;
            bool drawHigh = drawOnBothRadioButton.Checked || drawOnHighRadioButton.Checked;
            System.Drawing.Color newPixel = sprite.SpecialChannelsMap.GetPixel(x, y);
            int a = newPixel.A;
            int r = newPixel.R;
            int g = newPixel.G;
            int b = newPixel.B;

            if (drawOrCapture == Normal.DrawOrCapture.Draw)
            {
                if (drawLow)
                    r = lowValue;
                if (drawHigh)
                    g = highValue;

                if (removeHeightDataCheckBox.Checked)
                {
                    r = 255;
                    g = 0;
                }

                newPixel = System.Drawing.Color.FromArgb(a, r, g, b);
                sprite.SpecialChannelsMap.SetPixel(x, y, newPixel);
            }
            else if (drawOrCapture == Normal.DrawOrCapture.Capture)
            {
                if (r > g)
                {
                    removeHeightDataCheckBox.Checked = true;
                }
                else
                    removeHeightDataCheckBox.Checked = false;
                lowHeightUpDown.Value = r;
                highHeightUpDown.Value = g;
            }
            else if (drawOrCapture == Normal.DrawOrCapture.ArrayFill)
            {
                if (drawLow)
                    r = lowValue;
                if (drawHigh)
                    g = highValue;

                if (removeHeightDataCheckBox.Checked)
                {
                    r = 255;
                    g = 0;
                }

                newPixel = System.Drawing.Color.FromArgb(a, r, g, b);

                FloodFill(sprite.SpecialChannelsMap, x, y, newPixel);
            }
        }

        private void drawOnTarget_CheckedChanged(object sender, EventArgs e)
        {
            RenderDrawImage();
        }

        private void DrawOnNormalMap(int x, int y)
        {
            if (drawOrCapture == Normal.DrawOrCapture.Draw)
            {
                if (normal.HasValue)
                    sprite.NormalMap.SetPixel(x, y, Normal.ColorFromNormalVector(normal.Value));
                else
                    sprite.NormalMap.SetPixel(x, y, System.Drawing.Color.Transparent);
            }
            else if (drawOrCapture == Normal.DrawOrCapture.Capture)
            {
                System.Drawing.Color pixel = sprite.NormalMap.GetPixel(x, y);
                if (pixel.A == 0 || (pixel.R == 255 && pixel.G == 0 && pixel.B == 255))
                    normal = null;
                else
                    normal = Normal.NormalFromColor(pixel);
            }
            else if (drawOrCapture == Normal.DrawOrCapture.ArrayFill)
            {
                System.Drawing.Color pixel = sprite.NormalMap.GetPixel(x, y);
                if (normal.HasValue)
                    FloodFill(sprite.NormalMap, x, y, Normal.ColorFromNormalVector(normal.Value));
                else
                    FloodFill(sprite.NormalMap, x, y, System.Drawing.Color.Transparent);
            }

        }

        private void FloodFill(Bitmap bitmap, int x, int y, System.Drawing.Color color)
        {
            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int[] bits = new int[data.Stride / 4 * data.Height];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bits, 0, bits.Length);

            LinkedList<System.Drawing.Point> check = new LinkedList<System.Drawing.Point>();
            int floodTo = color.ToArgb();
            int floodFrom = bits[x + y * data.Stride / 4];
            bits[x + y * data.Stride / 4] = floodTo;

            if (floodFrom != floodTo)
            {
                check.AddLast(new System.Drawing.Point(x, y));
                while (check.Count > 0)
                {
                    System.Drawing.Point cur = check.First.Value;
                    check.RemoveFirst();

                    foreach (System.Drawing.Point off in new System.Drawing.Point[]
                                                    {
                                                    new System.Drawing.Point(0, -1), new System.Drawing.Point(0, 1), 
                                                    new System.Drawing.Point(-1, 0), new System.Drawing.Point(1, 0)
                                                    })
                    {
                        System.Drawing.Point next = new System.Drawing.Point(cur.X + off.X, cur.Y + off.Y);
                        if (next.X >= 0 && next.Y >= 0 && next.X < data.Width && next.Y < data.Height)
                        {
                            if (bits[next.X + next.Y * data.Stride / 4] == floodFrom)
                            {
                                check.AddLast(next);
                                bits[next.X + next.Y * data.Stride / 4] = floodTo;
                            }
                        }
                    }
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(bits, 0, data.Scan0, bits.Length);
            bitmap.UnlockBits(data);
        }

        private void drawPictureBox_MouseLeave(object sender, EventArgs e)
        {
            mouseStillInDrawImage = false;
            lineStarted = false;
        }

        private void drawPictureBox_MouseEnter(object sender, EventArgs e)
        {
            mouseStillInDrawImage = true;
        }

        private void browseFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {

                if (System.IO.File.Exists(ofd.FileName))
                {
                    sprite = new BitmapSprite();
                    sprite.OpenAll(ofd.FileName);

                    renderForm.Sprite = sprite;
                    objectColorMap = null;
                    uneditedPreviewPictureBox.Image = sprite.ColorMap;

                    xnaFrame.Size = new Size(320 * Magnification, 200 * Magnification);
                    xnaFrame.Visible = true;
                }
                else
                {
                    xnaFrame.Visible = false;
                }
                RenderDrawImage();
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (sprite.NormalMap == null)
                return;

            if (MessageBox.Show("Are you sure you want to save?", "Confirm save", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            sprite.SaveNormalMap();
            sprite.SaveSpecialChannelsMap();
        }

        private void drawPictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (sprite.ColorMap == null)
                return;

            if (toolsTabControl.SelectedTab.Name == "normalsTabPage")
                NormalMap_Paint(e);
            else if (toolsTabControl.SelectedTab.Name == "heightTabPage")
                HeighMap_Paint(e);
            else if (toolsTabControl.SelectedTab.Name == "specularTabPage")
                SpecularMap_Paint(e);
        }

        private void SpecularMap_Paint(PaintEventArgs e)
        {
            int x;
            int y;
            SolidBrush brush;
            System.Drawing.Color color;
            // For each pixel in the normal map
            for (y = 0; y < sprite.ColorMap.Height; y++)
            {
                for (x = 0; x < sprite.ColorMap.Width; x++)
                {
                    // If draw background is checked, and the normal map says this space is cleared
                    if (sprite.SpecialChannelsMap.GetPixel(x, y).B == 0 && drawImageUnderNormalMap.Checked)
                    {
                        brush = new SolidBrush(sprite.ColorMap.GetPixel(x, y));
                        e.Graphics.FillRectangle(brush, x * Magnification, y * Magnification, Magnification, Magnification);
                        brush.Dispose();
                    }
                    else // Else draw only the normal map
                    {
                        color = sprite.SpecialChannelsMap.GetPixel(x, y);
                        color = System.Drawing.Color.FromArgb(color.B, color.B, color.B);
                        brush = new SolidBrush(color);
                        e.Graphics.FillRectangle(brush, x * Magnification, y * Magnification, Magnification, Magnification);
                        brush.Dispose();
                    }

                    // Allows for a crappy version of background bleed-through, should change it to individual pixels
                    if (normalMapViewBleedThrough.Checked)
                    {
                        brush = new SolidBrush(sprite.ColorMap.GetPixel(x, y));
                        e.Graphics.FillRectangle(brush, x * Magnification, y * Magnification, Magnification / 2, Magnification / 2);
                        brush.Dispose();
                    }
                }
            }
        }

        private void HeighMap_Paint(PaintEventArgs e)
        {
            int x;
            int y;
            SolidBrush brush;
            System.Drawing.Color pixel;
            // For each pixel in the normal map
            for (y = 0; y < sprite.ColorMap.Height; y++)
            {
                for (x = 0; x < sprite.ColorMap.Width; x++)
                {
                    //continue;
                    pixel = sprite.SpecialChannelsMap.GetPixel(x, y);
                    brush = null;
                    if (drawImageUnderNormalMap.Checked && pixel.G == 0)
                    {
                        brush = new SolidBrush(sprite.ColorMap.GetPixel(x, y));
                        e.Graphics.FillRectangle(brush, x * Magnification, y * Magnification, Magnification, Magnification);

                    }
                    else // Else draw only the normal map
                    {
                        if (drawOnLowRadioButton.Checked)
                        {
                            if (pixel.R != 0)
                                brush = new SolidBrush(System.Drawing.Color.FromArgb(pixel.R, 0, 0));
                        }
                        else if (drawOnHighRadioButton.Checked)
                        {
                            if (pixel.G != 0)
                                brush = new SolidBrush(System.Drawing.Color.FromArgb(0, pixel.G, 0));
                        }
                        else
                        {
                            if (pixel.G != 0)
                                brush = new SolidBrush(System.Drawing.Color.FromArgb(pixel.R, pixel.G, 0));
                        }

                        if(brush != null)
                            e.Graphics.FillRectangle(brush, x * Magnification, y * Magnification, Magnification, Magnification);
                    }
                    if(brush != null)
                        brush.Dispose();

                    // Allows for a crappy version of background bleed-through, should change it to individual pixels
                    if (normalMapViewBleedThrough.Checked)
                    {
                        brush = new SolidBrush(sprite.ColorMap.GetPixel(x, y));
                        e.Graphics.FillRectangle(brush, x * Magnification, y * Magnification, Magnification / 2, Magnification / 2);
                        brush.Dispose();
                    }
                }
            }
        }

        private void NormalMap_Paint(PaintEventArgs e)
        {
            int x;
            int y;
            SolidBrush brush;
            // For each pixel in the normal map
            for (y = 0; y < sprite.NormalMap.Height; y++)
            {
                for (x = 0; x < sprite.NormalMap.Width; x++)
                {
                    //continue;
                    // If draw background is checked, and the normal map says this space is cleared
                    if (sprite.NormalMap.GetPixel(x, y).A == 0 && drawImageUnderNormalMap.Checked)
                    {
                        brush = new SolidBrush(sprite.ColorMap.GetPixel(x, y));
                        e.Graphics.FillRectangle(brush, x * Magnification, y * Magnification, Magnification, Magnification);
                        brush.Dispose();
                    }
                    else // Else draw only the normal map
                    {
                        brush = new SolidBrush(sprite.NormalMap.GetPixel(x, y));
                        e.Graphics.FillRectangle(brush, x * Magnification, y * Magnification, Magnification, Magnification);
                        brush.Dispose();
                    }

                    // Allows for a crappy version of background bleed-through, should change it to individual pixels
                    if (normalMapViewBleedThrough.Checked)
                    {
                        brush = new SolidBrush(sprite.ColorMap.GetPixel(x, y));
                        e.Graphics.FillRectangle(brush, x * Magnification, y * Magnification, Magnification / 2, Magnification / 2);
                        brush.Dispose();
                    }
                }
            }
        }
//raytraceish (This is not called by anything anymore, used for referance only)
        private void previewPictureBox_Paint(object sender, PaintEventArgs e)
        {
            Stopwatch st=new Stopwatch();
            st.Start();
            if (sprite.ColorMap == null)
                return;

            int x;
            int y;
            SolidBrush brush;
            System.Drawing.Color pixel;
            System.Drawing.Color normalPixel;
            System.Drawing.Color specialPixel;
            float r = 0;
            float g = 0;
            float b = 0;

            

            Vector3 pixelLocation;
            Vector3 directionFromLightToPixel;
            Vector3 surfaceNormal;
            Vector3 finalPixelColor;
            float lightDistance;
            int i;
            // For every pixel in the preview box
            for (y = 0; y < sprite.ColorMap.Height; y++)
            {
                for (x = 0; x < sprite.ColorMap.Width; x++)
                {
                    // Actiuall pixel color
                    pixel = sprite.ColorMap.GetPixel(x, y);
                    if(pixel.A == 0 || (pixel.R == 255 && pixel.G == 0 && pixel.B == 255))
                        continue;

                    // Get normal map RGB values
                    normalPixel = sprite.NormalMap.GetPixel(x, y);

                    // Get special data
                    specialPixel = sprite.SpecialChannelsMap.GetPixel(x, y);

                    r = (float)(pixel.R / 255.0f);
                    g = (float)(pixel.G / 255.0f);
                    b = (float)(pixel.B / 255.0f);

                    finalPixelColor = new Vector3(r, g, b);

                    finalPixelColor.X = r * ((float)ambientColorR.Value / 255.0f);
                    finalPixelColor.Y = g * ((float)ambientColorG.Value / 255.0f);
                    finalPixelColor.Z = b * ((float)ambientColorB.Value / 255.0f);

                    pixelLocation = new Vector3(x, y, spriteAltitude + specialPixel.G);

                    for(i = 0; i < lights.Count; i++)
                    {
                        if (lights[i].Active)
                        {
                            if(pixelLocation.X == lights[i].X && pixelLocation.Y == lights[i].Y && pixelLocation.Z == lights[i].Z)
                                continue;

                            float shadowTest = 1;

                            if(useShadows.Checked)
                                shadowTest = lights[i].VisibleFrom(pixelLocation, sprite);

                            if (shadowTest == 0)
                                continue;

                            // Possible to add partly lit pixels later
                            shadowTest = Normal.Clamp(shadowTest, 0, 1);

                            directionFromLightToPixel = pixelLocation - lights[i].Location;
                            lightDistance = lights[i].CalculateDistance(directionFromLightToPixel);
                            directionFromLightToPixel.Normalize();

                            // Get surface normal
                            if (normalPixel.A == 0)
                                surfaceNormal = new Vector3(0, 0, 1);
                            else
                                surfaceNormal = Normal.NormalFromColor(sprite.NormalMap.GetPixel(x, y));
                            surfaceNormal.Normalize();

                            // Flip for light dir calc
                            surfaceNormal.Y = -surfaceNormal.Y;

                            // Calc amount of light should be reflected by surface
                            float lambertTerm = (float)Vector3.Dot(-directionFromLightToPixel, surfaceNormal);
                            if (lambertTerm <= 0)
                            {
                                continue;
                            }
                            // Add diffuse light to pixel (this will work with more lights)
                            if (lightDistance < 0.5f)
                                lightDistance = 0.5f;

                            Vector3 lightComp = lights[i].Color * lambertTerm * lights[i].Intensity / lightDistance;
                            finalPixelColor.X += lightComp.X * r;
                            finalPixelColor.Y += lightComp.Y * g;
                            finalPixelColor.Z += lightComp.Z * b;

                            // Specular
                            float specularReflectivness = specialPixel.B / 255f;
                            if (specularReflectivness > 0)
                            {
                                Vector3 directionToPixelFromEye = pixelLocation - eyeLoc;
                                directionToPixelFromEye.Normalize();
                                Vector3 reflectionVector = Light.Reflect(-directionFromLightToPixel, surfaceNormal);

                                double dot = Vector3.Dot(directionToPixelFromEye, reflectionVector);
                                if (dot > 0)
                                {
                                    float specularLumination = (float)Math.Pow(dot, 20) * specularReflectivness;
                                    finalPixelColor.X += lights[i].Color.X * specularLumination;
                                    finalPixelColor.Y += lights[i].Color.Y * specularLumination;
                                    finalPixelColor.Z += lights[i].Color.Z * specularLumination;
                                }
                            }
                        }
                    }

                    // Clamp values between 0 and 255, otherwise windows forms wont draw this (remove this for xna version)
                    r = Normal.Clamp(finalPixelColor.X * 255, 0, 255);
                    g = Normal.Clamp(finalPixelColor.Y * 255, 0, 255);
                    b = Normal.Clamp(finalPixelColor.Z * 255, 0, 255);

                    if (float.IsNaN(r) || float.IsNaN(g) || float.IsNaN(b))
                    {
                        r = 255;
                        g = 0;
                        b = 255;
                    }

                    brush = new SolidBrush(System.Drawing.Color.FromArgb((int)r, (int)g, (int)b));
                    e.Graphics.FillRectangle(brush, x * Magnification, y * Magnification, Magnification, Magnification);
                    brush.Dispose();
                }
            }

            st.Stop();
            Text = "Time taken: " + st.ElapsedMilliseconds.ToString() + "ms";

            RenderDrawImage();
        }

        private void drawImageUnderNormalMap_CheckedChanged(object sender, EventArgs e)
        {
            RenderDrawImage();
        }

        #region PreviewPictureBoxMouseControls
        private void previewPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            movingLightSource = false;

            PreviewPictureBoxMouseInteraction(e.Button, e.X / Magnification, e.Y / Magnification);

            RenderDrawImage();
        }

        private void previewPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (movingLightSource)
            {
                PreviewPictureBoxMouseInteraction(e.Button, e.X / Magnification, e.Y / Magnification);
                RenderDrawImage();
            }
        }

        private void previewPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            movingLightSource = true;

            PreviewPictureBoxMouseInteraction(e.Button, e.X / Magnification, e.Y / Magnification);
            RenderDrawImage();
        }

        private void PreviewPictureBoxMouseInteraction(MouseButtons button, int x, int y)
        {
            if (button == MouseButtons.Left)
            {
                lights[(int)lightToEdit.Value].X = x;
                lights[(int)lightToEdit.Value].Y = y;
            }
            else if (button == MouseButtons.Right)
            {
                eyeLoc.X = x;
                eyeLoc.Y = y;
            }
        }

        #endregion

        private void forceLightClamp_CheckedChanged(object sender, EventArgs e)
        {
            RenderDrawImage();
        }

        private void lightAltitude_ValueChanged(object sender, EventArgs e)
        {
            lights[(int)lightToEdit.Value].Z = (float)lightAltitude.Value;
            RenderDrawImage();
        }

        private void magnificationNumUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (sprite == null)
                return;

            if (sprite.ColorMap != null && sprite.NormalMap != null)
            {
                xnaFrame.Size = new Size(320 * Magnification, 200 * Magnification);
                RenderDrawImage();
            }
        }

        private void normalMapViewBleedThrough_CheckedChanged(object sender, EventArgs e)
        {
            RenderDrawImage();
        }

        #region lightControls
        private void currentLightActiveCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            lights[(int)lightToEdit.Value].Active = currentLightActiveCheckBox.Checked;
            RenderDrawImage();
        }

        private void lightToEdit_ValueChanged(object sender, EventArgs e)
        {
            SetupLightSettingsDisplay();
        }

        private void SetupLightSettingsDisplay()
        {
            int index = (int)lightToEdit.Value;
            diffuseColorR.Value = (decimal)(lights[index].Color.X * 255.0f);
            diffuseColorG.Value = (decimal)(lights[index].Color.Y * 255.0f);
            diffuseColorB.Value = (decimal)(lights[index].Color.Z * 255.0f);

            lightIntensityUpDown.Value = (decimal)(lights[index].Intensity);

            lightAltitude.Value = (decimal)(lights[index].Z);
            currentLightActiveCheckBox.Checked = lights[index].Active;
        }

        private void diffuseColorR_ValueChanged(object sender, EventArgs e)
        {
            lights[(int)lightToEdit.Value].R = (float)diffuseColorR.Value / 255.0f;
            RenderDrawImage();
        }

        private void diffuseColorG_ValueChanged(object sender, EventArgs e)
        {
            lights[(int)lightToEdit.Value].G = (float)diffuseColorG.Value / 255.0f;
            RenderDrawImage();
        }

        private void diffuseColorB_ValueChanged(object sender, EventArgs e)
        {
            lights[(int)lightToEdit.Value].B = (float)diffuseColorB.Value / 255.0f;
            RenderDrawImage();
        }

        private void reloadColorImage_Click(object sender, EventArgs e)
        {
            sprite.LoadColorMap();

            renderForm.Sprite = sprite;

            RenderDrawImage();
            uneditedPreviewPictureBox.Image = sprite.ColorMap;
            uneditedPreviewPictureBox.Refresh();
        }

        private void ambientColorR_ValueChanged(object sender, EventArgs e)
        {
            RenderDrawImage();
        }

        private void ambientColorG_ValueChanged(object sender, EventArgs e)
        {
            RenderDrawImage();
        }

        private void ambientColorB_ValueChanged(object sender, EventArgs e)
        {
            RenderDrawImage();
        }

        private void lightIntensityUpDown_ValueChanged(object sender, EventArgs e)
        {
            lights[(int)lightToEdit.Value].Intensity = (float)lightIntensityUpDown.Value;
            RenderDrawImage();
        }
        #endregion

        private void heightMapHeightPicker_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Pink, 0, 0, 19, heightMapHeightPicker.Height);
            e.Graphics.FillRectangle(Brushes.LightGreen, 20, 0, 19, heightMapHeightPicker.Height);

            e.Graphics.FillRectangle(Brushes.Orange, 0, 255 - (int)highHeightUpDown.Value, 39, (int)highHeightUpDown.Value - (int)lowHeightUpDown.Value);

            e.Graphics.DrawLine(Pens.Black, 0, 255 - (int)lowHeightUpDown.Value, 19, 255 - (int)lowHeightUpDown.Value);
            e.Graphics.DrawLine(Pens.Black, 20, 255 - (int)highHeightUpDown.Value, 39, 255 - (int)highHeightUpDown.Value);

            e.Graphics.DrawLine(Pens.Black, 19, 0, 19, heightMapHeightPicker.Height);

            bool pointerOverControl = false;
            System.Drawing.Point location = heightMapHeightPicker.PointToClient(Cursor.Position);

            if (location.X < 0 || location.Y < 0 || location.X > heightMapHeightPicker.Width || location.Y > heightMapHeightPicker.Height)
                pointerOverControl = false;
            else
                pointerOverControl = true;

            if (pointerOverControl)
            {
                // left side = low, right side = high
                if (location.X < 20)
                {
                    e.Graphics.DrawLine(Pens.Blue, 0, location.Y, 19, location.Y);
                    if(255 - location.Y - 1 > (int)highHeightUpDown.Value)
                        e.Graphics.DrawLine(Pens.Red, 20, location.Y, 39, location.Y);
                }
                else
                {
                    e.Graphics.DrawLine(Pens.Blue, 20, location.Y, 39, location.Y);
                    if (255 - location.Y <= (int)lowHeightUpDown.Value)
                        e.Graphics.DrawLine(Pens.Red, 0, location.Y, 19, location.Y);
                }
            }

            if ((int)minRangeHeight.Value > 0)
            {
                e.Graphics.DrawLine(Pens.Green, 0, 255 - (int)minRangeHeight.Value, 39, 255 - (int)minRangeHeight.Value);
            }

            if ((int)maxRangeHeight.Value < 255)
            {
                e.Graphics.DrawLine(Pens.Red, 0, 255 - (int)maxRangeHeight.Value, 39, 255 - (int)maxRangeHeight.Value);
            }
        }

        private void heightMapHeightPicker_MouseDown(object sender, MouseEventArgs e)
        {
            int height = 255 - e.Y;
            if (e.Button == MouseButtons.Left)
            {
                removeHeightDataCheckBox.Checked = false;
                if (e.X < 20)
                {
                    lowHeightUpDown.Value = height;
                }
                else
                {
                    highHeightUpDown.Value = height;
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                removeHeightDataCheckBox.Checked = false;
                lowHeightUpDown.Value = height;
                highHeightUpDown.Value = height;
            }
            else
            {
                removeHeightDataCheckBox.Checked = true;
            }
        }

        private void lowHeightUpDown_ValueChanged(object sender, EventArgs e)
        {
            if ((int)lowHeightUpDown.Value > (int)highHeightUpDown.Value)
                highHeightUpDown.Value = (int)lowHeightUpDown.Value;

            UpdateHeightCalculationDisplay();
            heightMapHeightPicker.Refresh();
        }

        private void UpdateHeightCalculationDisplay()
        {
            lowHighAsMeterCalc.Text = ((int)lowHeightUpDown.Value / (float)pixelsPerMeter.Value).ToString() + "m / " + ((int)highHeightUpDown.Value / (float)pixelsPerMeter.Value).ToString() + "m";
            heightCalculatedLabel.Text = "Height: " + ((int)highHeightUpDown.Value - (int)lowHeightUpDown.Value).ToString() + "  (ex: " + (((int)highHeightUpDown.Value - (int)lowHeightUpDown.Value) / (float)pixelsPerMeter.Value).ToString() + "m)";
        }

        private void highHeightUpDown_ValueChanged(object sender, EventArgs e)
        {
            if ((int)highHeightUpDown.Value < (int)lowHeightUpDown.Value)
                lowHeightUpDown.Value = (int)highHeightUpDown.Value;

            UpdateHeightCalculationDisplay();
            heightMapHeightPicker.Refresh();
        }

        private void heightMapHeightPicker_MouseMove(object sender, MouseEventArgs e)
        {
            heightMapHeightPicker.Refresh();
        }

        private void heightMapHeightPicker_MouseLeave(object sender, EventArgs e)
        {
            heightMapHeightPicker.Refresh();
        }

        private void toolsTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            RenderDrawImage();
        }

        private void pixelsPerMeter_ValueChanged(object sender, EventArgs e)
        {
            UpdateHeightCalculationDisplay();
        }

        private void specularPicker_MouseDown(object sender, MouseEventArgs e)
        {
            int spec = 255 - e.Y;
            specularUpDown.Value = spec;
        }

        private void specularUpDown_ValueChanged(object sender, EventArgs e)
        {
            specularPicker.Refresh();
        }

        private void specularPicker_Paint(object sender, PaintEventArgs e)
        {
            LinearGradientBrush gradientBrush = new LinearGradientBrush(new System.Drawing.Point(0, 0), new System.Drawing.Point(0, 255), System.Drawing.Color.White, System.Drawing.Color.Black);
            e.Graphics.FillRectangle(gradientBrush, new System.Drawing.Rectangle(0, 0, 20, 256));
            gradientBrush.Dispose();

            e.Graphics.DrawLine(Pens.Red, 0, 255 - (int)specularUpDown.Value, 20, 255 - (int)specularUpDown.Value);
        }

        private void useShadows_CheckedChanged(object sender, EventArgs e)
        {
            RenderDrawImage();
        }

        private void saveLights_Click(object sender, EventArgs e)
        {
            int i;
            IFormatter formatter = new BinaryFormatter();
            for (i = 0; i < lights.Count; i++)
            {
                Stream stream = new FileStream("Lgt" + i + ".bin", FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, lights[i]);
                stream.Close();
            }
        }

        private void loadLights_Click(object sender, EventArgs e)
        {
            IFormatter formatter = new BinaryFormatter();
            int i;
            for (i = 0; i < lights.Count; i++)
            {
                lightToEdit.Value = 0;
                SetupLightSettingsDisplay();

                if (File.Exists("Lgt" + i + ".bin"))
                {
                    Stream stream = new FileStream("Lgt" + i + ".bin", FileMode.Open, FileAccess.Read, FileShare.Read);
                    Light tmp = (Light)formatter.Deserialize(stream);
                    stream.Close();
                    lights[i] = tmp;
                }
            }

            renderForm.RenderPreviewImage();
        }

        private void xnaFrame_Click(object sender, EventArgs e)
        {
            RenderDrawImage();
        }

        private void xnaFrame_Paint(object sender, PaintEventArgs e)
        {
            RenderDrawImage();
        }

        private void recompileEffect_Click(object sender, EventArgs e)
        {
            renderForm.ReloadEffects();
            effectCompiled = false;
            fullscreenColorMap = null;
            renderForm.RenderPreviewImage();
        }

        #region PreviewImageStuff
        RenderTarget2D fullscreenColorMap = null;
        RenderTarget2D fullscreenNormalMap = null;
        RenderTarget2D fullscreenSpecialMap = null;
        RenderTarget2D fullscreenLightMap = null;
        RenderTarget2D fullscreenSingleLightRender = null;
        RenderTarget2D fullscreenFinalImageUnscaled = null;
        Texture2D fullscreenLightMapHolder = null;

        bool effectCompiled = false;

        Effect pointLightEffect;
        Effect clearRenderTargetEffect;
        Effect textureCombineEffect;
        Effect derpShader;
        Effect renderByChannelEffect;
        Effect removePixelsEffect;
        Effect overlayTextureEffect;

        bool objectChangedSinceLastRender = true;
        Texture2D objectColorMap = null;
        Texture2D objectNormalMap = null;
        Texture2D objectSpecialMap = null;
        #endregion

        RenderTarget2D drawAreaRenderTarget = null;
        Texture2D drawAreaTempTexture = null;

        private void CompileEffectsIfNeeded()
        {
            if (!effectCompiled)
            {
                try
                {
                    pointLightEffect = new Effect(xnaFrame.Device, File.ReadAllBytes(@"PointLightRender.fxo"), CompilerOptions.None, null);
                    clearRenderTargetEffect = new Effect(xnaFrame.Device, File.ReadAllBytes(@"ClearRenderTarget.fxo"), CompilerOptions.None, null);
                    textureCombineEffect = new Effect(xnaFrame.Device, File.ReadAllBytes(@"TextureCombine.fxo"), CompilerOptions.None, null);
                    derpShader = new Effect(xnaFrame.Device, File.ReadAllBytes(@"ColorMod.fxo"), CompilerOptions.None, null);
                    renderByChannelEffect = new Effect(xnaFrame.Device, File.ReadAllBytes(@"RenderByChannel.fxo"), CompilerOptions.None, null);
                    removePixelsEffect = new Effect(xnaFrame.Device, File.ReadAllBytes(@"RemovePixels.fxo"), CompilerOptions.None, null);
                    overlayTextureEffect = new Effect(xnaFrame.Device, File.ReadAllBytes(@"TextureOverlay.fxo"), CompilerOptions.None, null);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                effectCompiled = true;
            }
        }

        private void RenderDrawImage()
        {
            if (sprite == null)
                return;

            if(objectColorMap == null)
                objectColorMap = sprite.ColorMapToTexture(xnaFrame.Device);

            if (drawAreaRenderTarget == null)
            {
                drawAreaRenderTarget = new RenderTarget2D(xnaFrame.Device, sprite.Width, sprite.Height, 1, SurfaceFormat.Color, RenderTargetUsage.DiscardContents);
                drawAreaTempTexture = new Texture2D(xnaFrame.Device, sprite.Width, sprite.Height);
            }
            else if (drawAreaRenderTarget.Width != sprite.Width || drawAreaRenderTarget.Height != sprite.Height)
            {
                drawAreaRenderTarget = new RenderTarget2D(xnaFrame.Device, sprite.Width, sprite.Height, 1, SurfaceFormat.Color, RenderTargetUsage.DiscardContents);
                drawAreaTempTexture = new Texture2D(xnaFrame.Device, sprite.Width, sprite.Height);
            }

            Microsoft.Xna.Framework.Rectangle workingOnRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, sprite.Width, sprite.Height);

            CompileEffectsIfNeeded();

            renderForm.RenderPreviewImage();

            Texture2D currentEditing = null;
            Vector4 colorChannelLowRange = new Vector4(0, 0, 0, 1);
            Vector4 colorChannelHighRange = new Vector4(1, 1, 1, 1);
            Vector4 colorChannelMod = new Vector4(1, 1, 1, 1);

            if (toolsTabControl.SelectedTab.Name == "normalsTabPage")
            {
                currentEditing = sprite.NormalMapToTexture(xnaFrame.Device);
            }
            else if (toolsTabControl.SelectedTab.Name == "heightTabPage")
            {
                currentEditing = sprite.SpecialChannelsMapToTexture(xnaFrame.Device);
                colorChannelLowRange.X = ((float)minRangeHeight.Value / 255f);
                colorChannelLowRange.Y = ((float)minRangeHeight.Value / 255f);

                colorChannelHighRange.X = ((float)maxRangeHeight.Value / 255f);
                colorChannelHighRange.Y = ((float)maxRangeHeight.Value / 255f);

                if (!(drawOnLowRadioButton.Checked || drawOnBothRadioButton.Checked))
                    colorChannelMod.X = 0;

                if (!(drawOnHighRadioButton.Checked || drawOnBothRadioButton.Checked))
                    colorChannelMod.Y = 0;

                colorChannelMod.Z = 0;
            }
            else if (toolsTabControl.SelectedTab.Name == "specularTabPage")
            {
                currentEditing = sprite.SpecialChannelsMapToTexture(xnaFrame.Device);
                colorChannelLowRange.Z = ((float)minRangeSpecular.Value / 255f);
                colorChannelHighRange.Z = ((float)maxRangeSpecular.Value / 255f);
                colorChannelMod.X = 0;
                colorChannelMod.Y = 0;
            }
            else
                return;

            xnaFrame.Begin();
            {

                SpriteBatch sb = new SpriteBatch(xnaFrame.Device);
                sb.GraphicsDevice.SetRenderTarget(0, drawAreaRenderTarget);
                sb.GraphicsDevice.Clear(Microsoft.Xna.Framework.Graphics.Color.Black);
                sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                {
                    renderByChannelEffect.Parameters["ViewportSize"].SetValue(new Vector2(xnaFrame.Device.Viewport.Width, xnaFrame.Device.Viewport.Height));
                    renderByChannelEffect.Parameters["TextureSize"].SetValue(new Vector2(currentEditing.Width, currentEditing.Height));
                    renderByChannelEffect.Parameters["MatrixTransform"].SetValue(Microsoft.Xna.Framework.Matrix.Identity);

                    renderByChannelEffect.Parameters["ChannelMod"].SetValue(colorChannelMod);
                    renderByChannelEffect.Parameters["ChannelLowRange"].SetValue(colorChannelLowRange);
                    renderByChannelEffect.Parameters["ChannelHighRange"].SetValue(colorChannelHighRange);
                    renderByChannelEffect.Begin();
                    foreach (EffectPass pass in renderByChannelEffect.CurrentTechnique.Passes)
                    {
                        pass.Begin();
                        sb.Draw(currentEditing, new Vector2(0, 0), Microsoft.Xna.Framework.Graphics.Color.White);
                        pass.End();
                    }
                    renderByChannelEffect.End();
                }
                sb.End();

                sb.GraphicsDevice.SetRenderTarget(0, null);
                drawAreaTempTexture = drawAreaRenderTarget.GetTexture();

                sb.GraphicsDevice.SetRenderTarget(0, drawAreaRenderTarget);
                sb.GraphicsDevice.Clear(Microsoft.Xna.Framework.Graphics.Color.Black);

                sb.Begin(SpriteBlendMode.Additive, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                {
                    removePixelsEffect.Parameters["ViewportSize"].SetValue(new Vector2(xnaFrame.Device.Viewport.Width, xnaFrame.Device.Viewport.Height));
                    removePixelsEffect.Parameters["TextureSize"].SetValue(new Vector2(drawAreaTempTexture.Width, drawAreaTempTexture.Height));
                    removePixelsEffect.Parameters["MatrixTransform"].SetValue(Microsoft.Xna.Framework.Matrix.Identity);
                    removePixelsEffect.Begin();
                    foreach (EffectPass pass in removePixelsEffect.CurrentTechnique.Passes)
                    {
                        pass.Begin();
                        sb.Draw(drawAreaTempTexture, workingOnRectangle, Microsoft.Xna.Framework.Graphics.Color.White);
                        pass.End();
                    }
                }
                removePixelsEffect.End();
                sb.End();

                sb.GraphicsDevice.SetRenderTarget(0, null);

                drawAreaTempTexture = drawAreaRenderTarget.GetTexture();
                sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                {
                    overlayTextureEffect.Parameters["ViewportSize"].SetValue(new Vector2(xnaFrame.Device.Viewport.Width, xnaFrame.Device.Viewport.Height));
                    overlayTextureEffect.Parameters["TextureSize"].SetValue(new Vector2(drawAreaTempTexture.Width, drawAreaTempTexture.Height));
                    overlayTextureEffect.Parameters["MatrixTransform"].SetValue(Microsoft.Xna.Framework.Matrix.Identity);
                    overlayTextureEffect.Parameters["OverlayTexture"].SetValue(drawAreaTempTexture);
                    overlayTextureEffect.Begin();
                    foreach (EffectPass pass in overlayTextureEffect.CurrentTechnique.Passes)
                    {
                        pass.Begin();
                        sb.Draw(objectColorMap, workingOnRectangle, Microsoft.Xna.Framework.Graphics.Color.White);
                        pass.End();
                    }
                }
                overlayTextureEffect.End();
                sb.End(); 
                
            }
            xnaFrame.Present();
        }

        private void RenderPreviewImage()
        {
            #region SetupStuff
            int width = 320;
            int height = 200;

            Stopwatch st = new Stopwatch();
            st.Start();

            // Load test textures
            if (fullscreenColorMap == null)
            {
                #region LoadAndCreateTextures
                fullscreenColorMap = new RenderTarget2D(xnaFrame.Device, width, height, 1, SurfaceFormat.Color, RenderTargetUsage.DiscardContents); //Texture2D.FromFile(xnaFrame.Device, @"..\..\..\..\..\mockup2.png");
                fullscreenNormalMap = new RenderTarget2D(xnaFrame.Device, width, height, 1, SurfaceFormat.Color, RenderTargetUsage.DiscardContents); //Texture2D.FromFile(xnaFrame.Device, @"..\..\..\..\..\mockup2_normals.png");
                fullscreenSpecialMap = new RenderTarget2D(xnaFrame.Device, width, height, 1, SurfaceFormat.Color, RenderTargetUsage.DiscardContents); //Texture2D.FromFile(xnaFrame.Device, @"..\..\..\..\..\mockup2_specials.png");

                fullscreenFinalImageUnscaled = new RenderTarget2D(xnaFrame.Device, width, height, 1, SurfaceFormat.Color, RenderTargetUsage.DiscardContents);
                fullscreenLightMapHolder = new Texture2D(xnaFrame.Device, 320, 200);
                fullscreenSingleLightRender = new Microsoft.Xna.Framework.Graphics.RenderTarget2D(xnaFrame.Device, width, height, 1, SurfaceFormat.Color, RenderTargetUsage.DiscardContents);
                fullscreenLightMap = new Microsoft.Xna.Framework.Graphics.RenderTarget2D(xnaFrame.Device, width, height, 1, xnaFrame.Device.PresentationParameters.BackBufferFormat, RenderTargetUsage.DiscardContents);

                #endregion
            }
            Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(0, 0, fullscreenColorMap.Width, fullscreenColorMap.Height);

            if (objectChangedSinceLastRender)
            {
                if (sprite == null)
                    return;

                objectColorMap = sprite.ColorMapToTexture(xnaFrame.Device);
                objectNormalMap = sprite.NormalMapToTexture(xnaFrame.Device);
                objectSpecialMap = sprite.SpecialChannelsMapToTexture(xnaFrame.Device);

                objectChangedSinceLastRender = false;
            }

            // Load effects
            if (!effectCompiled)
            {
                #region LoadEffects
                try
                {
                    pointLightEffect = new Effect(xnaFrame.Device, File.ReadAllBytes(@"PointLightRender.fxo"), CompilerOptions.None, null);
                    clearRenderTargetEffect = new Effect(xnaFrame.Device, File.ReadAllBytes(@"ClearRenderTarget.fxo"), CompilerOptions.None, null);
                    textureCombineEffect = new Effect(xnaFrame.Device, File.ReadAllBytes(@"TextureCombine.fxo"), CompilerOptions.None, null);
                    derpShader = new Effect(xnaFrame.Device, File.ReadAllBytes(@"ColorMod.fxo"), CompilerOptions.None, null);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                effectCompiled = true;
                #endregion
            }
            #endregion

            // XNA control
            xnaFrame.Begin();
            {
                // Create sprite batch
                SpriteBatch sb = new SpriteBatch(xnaFrame.Device);

                int renderOffsetX = 0;
                int renderOffsetY = 0;
                // Draw objects to fullscreen buffer
                if (centerImageCheckBox.Checked)
                {
                    renderOffsetX = (fullscreenColorMap.Width / 2) - (objectSpecialMap.Width / 2);
                    renderOffsetY = (fullscreenColorMap.Height / 2) - (objectSpecialMap.Height / 2);
                }


                // Color
                sb.GraphicsDevice.SetRenderTarget(0, fullscreenColorMap);
                sb.GraphicsDevice.Clear(Microsoft.Xna.Framework.Graphics.Color.Black);
                sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                {
                    sb.Draw(objectColorMap, new Vector2(renderOffsetX, renderOffsetY), null, Microsoft.Xna.Framework.Graphics.Color.White);
                }
                sb.End();
                sb.GraphicsDevice.SetRenderTarget(0, null);

                // Normals
                sb.GraphicsDevice.SetRenderTarget(0, fullscreenNormalMap);
                sb.GraphicsDevice.Clear(new Microsoft.Xna.Framework.Graphics.Color(127, 127, 255));
                sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                {
                    sb.Draw(objectNormalMap, new Vector2(renderOffsetX, renderOffsetY), null, Microsoft.Xna.Framework.Graphics.Color.White);
                }
                sb.End();
                sb.GraphicsDevice.SetRenderTarget(0, null);

                // Special
                sb.GraphicsDevice.SetRenderTarget(0, fullscreenSpecialMap);
                sb.GraphicsDevice.Clear(Microsoft.Xna.Framework.Graphics.Color.Black);
                sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                {
                    sb.Draw(objectSpecialMap, new Vector2(renderOffsetX, renderOffsetY), null, Microsoft.Xna.Framework.Graphics.Color.White);
                }
                sb.End();
                sb.GraphicsDevice.SetRenderTarget(0, null);

                #region SetPointLightEffectsParameters
                // Set generic lights data
                pointLightEffect.Parameters["ViewportSize"].SetValue(new Vector2(xnaFrame.Device.Viewport.Width, xnaFrame.Device.Viewport.Height));
                pointLightEffect.Parameters["TextureSize"].SetValue(new Vector2(fullscreenColorMap.Width, fullscreenColorMap.Height));
                pointLightEffect.Parameters["MatrixTransform"].SetValue(Microsoft.Xna.Framework.Matrix.Identity);
                pointLightEffect.Parameters["InverseViewportDimensions"].SetValue(new Vector2(1f / xnaFrame.Device.Viewport.Width, 1f / xnaFrame.Device.Viewport.Height));
                pointLightEffect.Parameters["NormalMap"].SetValue(fullscreenNormalMap.GetTexture());
                pointLightEffect.Parameters["SpecialMap"].SetValue(fullscreenSpecialMap.GetTexture());
                pointLightEffect.Parameters["EyeLocation"].SetValue(eyeLoc);
                pointLightEffect.Parameters["EyeDirection"].SetValue(eyeDir);
                #endregion

                #region SetCombineEffectsParameters
                textureCombineEffect.Parameters["ViewportSize"].SetValue(new Vector2(xnaFrame.Device.Viewport.Width, xnaFrame.Device.Viewport.Height));
                textureCombineEffect.Parameters["TextureSize"].SetValue(new Vector2(fullscreenColorMap.Width, fullscreenColorMap.Height));
                textureCombineEffect.Parameters["MatrixTransform"].SetValue(Microsoft.Xna.Framework.Matrix.Identity);
                textureCombineEffect.Parameters["Additive"].SetValue(true);
                #endregion

                // Render lights and colors from fullscreen buffer
                sb.GraphicsDevice.SetRenderTarget(0, fullscreenLightMap);
                sb.GraphicsDevice.Clear(Microsoft.Xna.Framework.Graphics.Color.Black);
                sb.GraphicsDevice.SetRenderTarget(0, null);

                int i;
                for (i = 0; i < lights.Count; i++)
                {
                    if (!lights[i].Active)
                        continue;

                    xnaFrame.Device.SetRenderTarget(0, fullscreenSingleLightRender);
                    xnaFrame.Device.Clear(Microsoft.Xna.Framework.Graphics.Color.Black);
                    // Set current light data
                    #region CurrentLightLocationAndColor
                    pointLightEffect.Parameters["LightPos"].SetValue(lights[i].Location);
                    pointLightEffect.Parameters["LightColor"].SetValue(lights[i].Color);
                    pointLightEffect.Parameters["LightIntensity"].SetValue(lights[i].Intensity);
                    #endregion

                    sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                    {
                        // Begin point light effect
                        pointLightEffect.Begin();
                        foreach (EffectPass pass in pointLightEffect.CurrentTechnique.Passes)
                        {
                            pass.Begin();
                            sb.Draw(fullscreenColorMap.GetTexture(), rect, Microsoft.Xna.Framework.Graphics.Color.White);
                            pass.End();
                        }
                        pointLightEffect.End();
                    }
                    sb.End();
                    xnaFrame.Device.SetRenderTarget(0, null);

                    // Render light to lightmap
                    fullscreenLightMapHolder = fullscreenLightMap.GetTexture();
                    xnaFrame.Device.SetRenderTarget(0, fullscreenLightMap);

                    // Clear lightmap
                    xnaFrame.Device.Clear(Microsoft.Xna.Framework.Graphics.Color.Black);

                    // Set textures and draw
                    textureCombineEffect.Parameters["Additive"].SetValue(true);
                    textureCombineEffect.Parameters["SecondTexture"].SetValue(fullscreenSingleLightRender.GetTexture());
                    sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                    {
                        textureCombineEffect.Begin();
                        foreach (EffectPass pass in textureCombineEffect.CurrentTechnique.Passes)
                        {
                            pass.Begin();
                            sb.Draw(fullscreenLightMapHolder, rect, Microsoft.Xna.Framework.Graphics.Color.White);
                            pass.End();
                        }
                        textureCombineEffect.End();
                    }
                    sb.End();
                    xnaFrame.Device.SetRenderTarget(0, null);

                }

                // Draw light map to back buffer
                xnaFrame.Device.SetRenderTarget(0, fullscreenFinalImageUnscaled);
                if (!displayColorsCheckBox.Checked)
                {
                    sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                    {
                        sb.Draw(fullscreenLightMap.GetTexture(), rect, Microsoft.Xna.Framework.Graphics.Color.White);
                    }
                    sb.End();
                }
                else
                {
                    // Select light map for second texture
                    textureCombineEffect.Parameters["Additive"].SetValue(false);
                    textureCombineEffect.Parameters["SecondTexture"].SetValue(fullscreenLightMap.GetTexture());
                    sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                    {
                        textureCombineEffect.Begin();
                        foreach (EffectPass pass in textureCombineEffect.CurrentTechnique.Passes)
                        {
                            pass.Begin();
                            // Render combination with colormap
                            sb.Draw(fullscreenColorMap.GetTexture(), rect, Microsoft.Xna.Framework.Graphics.Color.White);
                            pass.End();
                        }
                        textureCombineEffect.End();
                    }
                    sb.End();
                }

                xnaFrame.Device.SetRenderTarget(0, null);
                sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                {
                    sb.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Point;
                    sb.Draw(fullscreenFinalImageUnscaled.GetTexture(), new Vector2(0, 0), Microsoft.Xna.Framework.Graphics.Color.White);
                }
                sb.End();

            }
            xnaFrame.Present();

            st.Stop();
            Text = "Time taken: " + st.ElapsedMilliseconds.ToString() + "ms";

            renderForm.RenderPreviewImage();
        }

        private void centerImageCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RenderDrawImage();
        }

        private void displayColorsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RenderDrawImage();
        }

        private void minMaxRenderRange_max_Changed(object sender, EventArgs e)
        {
            int minHeight = (int)minRangeHeight.Value;
            int minSpec = (int)minRangeSpecular.Value;
            int maxHeight = (int)maxRangeHeight.Value;
            int maxSpec = (int)maxRangeSpecular.Value;

            if (minHeight >= maxHeight)
                minHeight = maxHeight-1;
            if (minSpec >= maxSpec)
                minSpec = maxSpec - 1;

            minRangeHeight.Value = minHeight;
            minRangeSpecular.Value = minSpec;
            maxRangeHeight.Value = maxHeight;
            maxRangeSpecular.Value = maxSpec;

            heightRangeDisplay.Text = "(" + (maxHeight - minHeight).ToString() + ")";
            specularRangeDisplay.Text = "(" + (maxSpec - minSpec).ToString() + ")";

            RenderDrawImage();
            RedrawPreviews();
        }

        private void minMaxRenderRange_min_Changed(object sender, EventArgs e)
        {
            int minHeight = (int)minRangeHeight.Value;
            int minSpec = (int)minRangeSpecular.Value;
            int maxHeight = (int)maxRangeHeight.Value;
            int maxSpec = (int)maxRangeSpecular.Value;

            if (minHeight >= maxHeight)
                maxHeight = minHeight + 1;
            if (minSpec >= maxSpec)
                maxSpec = minSpec + 1;

            minRangeHeight.Value = minHeight;
            minRangeSpecular.Value = minSpec;
            maxRangeHeight.Value = maxHeight;
            maxRangeSpecular.Value = maxSpec;

            heightRangeDisplay.Text = "(" + (maxHeight - minHeight).ToString() + ")";
            specularRangeDisplay.Text = "(" + (maxSpec - minSpec).ToString() + ")";

            RenderDrawImage();
            RedrawPreviews();
        }


    }
}


