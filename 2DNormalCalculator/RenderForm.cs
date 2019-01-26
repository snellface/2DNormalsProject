using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.IO;

namespace _2DNormalCalculator
{
    public partial class RenderForm : Form
    {
        public RenderForm()
        {
            InitializeComponent();
        }


        private void xnaFrame1_Load(object sender, EventArgs e)
        {

        }

        BitmapSprite sprite = null;
        internal BitmapSprite Sprite
        {
            set
            {
                sprite = value;
                objectColorMap = null;
                RenderPreviewImage();
            }
        }

        List<Light> lights;
        internal List<Light> Lights
        {
            get
            {
                return lights;
            }
            set
            {
                lights = value;
            }
        }

        Vector3 eyeLoc = new Vector3(50, 50, 50);
        Vector3 eyeDir = new Vector3(0, 0, -1);

        RenderTarget2D fullscreenColorMap = null;
        RenderTarget2D fullscreenNormalMap = null;
        RenderTarget2D fullscreenSpecialMap = null;
        RenderTarget2D fullscreenLightMap = null;
        RenderTarget2D fullscreenSingleLightRender = null;
        RenderTarget2D fullscreenFinalImageUnscaled = null;
        Texture2D fullscreenLightMapHolder = null;

        bool effectCompiled = false;
        public void ReloadEffects()
        {
            effectCompiled = false;
        }
        Effect pointLightEffect;
        Effect clearRenderTargetEffect;
        Effect textureCombineEffect;
        Effect derpShader;
        Effect renderByChannelEffect;

        NumericUpDown lightToEditLink = null;
        public NumericUpDown LightToEditLink
        {
            set
            {
                lightToEditLink = value;
            }
        }

        bool objectChangedSinceLastRender = true;
        public bool ObjectChangedSinceLastRender
        {
            set
            {
                objectChangedSinceLastRender = value;
            }
        }

        Texture2D objectColorMap = null;
        Texture2D objectNormalMap = null;
        Texture2D objectSpecialMap = null;

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
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                effectCompiled = true;
            }
        }

        public void RenderPreviewImage()
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

            if (objectChangedSinceLastRender || objectColorMap == null)
            {
                if (sprite == null)
                    return;

                objectColorMap = sprite.ColorMapToTexture(xnaFrame.Device);
                objectNormalMap = sprite.NormalMapToTexture(xnaFrame.Device);
                objectSpecialMap = sprite.SpecialChannelsMapToTexture(xnaFrame.Device);

                objectChangedSinceLastRender = false;
            }

            CompileEffectsIfNeeded();
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
                        #region SetCombineEffectsParameters
                        textureCombineEffect.Parameters["ViewportSize"].SetValue(new Vector2(xnaFrame.Device.Viewport.Width, xnaFrame.Device.Viewport.Height));
                        textureCombineEffect.Parameters["TextureSize"].SetValue(new Vector2(fullscreenColorMap.Width, fullscreenColorMap.Height));
                        textureCombineEffect.Parameters["MatrixTransform"].SetValue(Microsoft.Xna.Framework.Matrix.Identity);
                        textureCombineEffect.Parameters["Additive"].SetValue(true);
                        #endregion

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
        }

        private void xnaFrame1_Click(object sender, EventArgs e)
        {
            Texture2D objectColorMap = Texture2D.FromFile(xnaFrame.Device, @"H:\Normal map projects\derp.png");

            xnaFrame.Begin();

            SpriteBatch sb = new SpriteBatch(xnaFrame.Device);
            sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            {
                sb.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Point;
                sb.Draw(objectColorMap, new Vector2(0, 0), Microsoft.Xna.Framework.Graphics.Color.White);
            }
            sb.End();

            xnaFrame.Present();
        }

        private void TestForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void displayColorsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RenderPreviewImage();
        }

        bool movingLightSource = false;

        private void PreviewPictureBoxMouseInteraction(MouseButtons button, int x, int y)
        {
            if (button == MouseButtons.Left)
            {
                lights[(int)lightToEditLink.Value].X = x;
                lights[(int)lightToEditLink.Value].Y = y;
            }
            else if (button == MouseButtons.Right)
            {
                eyeLoc.X = x;
                eyeLoc.Y = y;
            }
        }

        private void xnaFrame_MouseDown(object sender, MouseEventArgs e)
        {
            movingLightSource = true;

            PreviewPictureBoxMouseInteraction(e.Button, (int)((e.X / (float)xnaFrame.Width) * 320), (int)((e.Y / (float)xnaFrame.Height) * 200));
            RenderPreviewImage();
        }

        private void xnaFrame_MouseMove(object sender, MouseEventArgs e)
        {
            if (movingLightSource)
            {
                PreviewPictureBoxMouseInteraction(e.Button, (int)((e.X / (float)xnaFrame.Width) * 320), (int)((e.Y / (float)xnaFrame.Height) * 200));
                RenderPreviewImage();
            }
        }

        private void xnaFrame_MouseUp(object sender, MouseEventArgs e)
        {
            movingLightSource = false;

            PreviewPictureBoxMouseInteraction(e.Button, (int)((e.X / (float)xnaFrame.Width) * 320), (int)((e.Y / (float)xnaFrame.Height) * 200));

            RenderPreviewImage();
        }

        private void RenderForm_Load(object sender, EventArgs e)
        {
            if (lightToEditLink == null)
                throw new NullReferenceException("lightToEditLink was not set");

            formSize = Size;

            RenderPreviewImage();
        }
        Size formSize;
        private void RenderForm_Resize(object sender, EventArgs e)
        {
            float ratio = 320f / 200f;
            Size newSize = new Size(320, 200) + (Size - formSize);

            if ((float)newSize.Width / (float)newSize.Height > ratio) 
            {
                newSize.Width = (int)(newSize.Height * ratio);
            } 
            else 
            {
                newSize.Height = (int)(newSize.Width / ratio);
            }

            xnaFrame.Size = newSize;
            RenderPreviewImage();
        }

        private void setZoomUpDown_ValueChanged(object sender, EventArgs e)
        {
            Size newSize = new Size(320 * ((int)setZoomUpDown.Value - 1), 200 * ((int)setZoomUpDown.Value -1));

            Size = newSize + formSize;
            RenderPreviewImage();
        }

        private void singleReRenderButton_Click(object sender, EventArgs e)
        {
            RenderPreviewImage();
        }
    }
}
