﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using GGL;
using OpenTK;
using Crow;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Drawing.Imaging;
using System.Drawing;

//using GLU = OpenTK.Graphics.Glu;
using Cairo;

namespace Magic3D
{	
    [Serializable]
    public class MagicCard
    {
		//TODO:fix data links
        
                
		public string RawCardData;

		public string FilePath;
        public string Name;
        public Cost Cost;
        public MultiformAttribut<CardTypes> Types = new MultiformAttribut<CardTypes>();
        public List<Ability> Abilities = new List<Ability>();
        public List<Trigger> Triggers = new List<Trigger>();
		public List<EffectGroup> SpellEffects = new List<EffectGroup> ();
        public List<string> Konstrains = new List<string>();
        public List<string> R = new List<string>();
        public List<string> DeckNeeds = new List<string>();
        public List<string> TextField = new List<string>();
        public List<string> Comments = new List<string>();        

        public string Oracle = "";

        public int Power = 0;
        public int Toughness = 0;        
        
        public string S = "";
        public string AlternateMode = "";
        public MultiformAttribut<ManaTypes> Colors;
        public string Loyalty = "";
        public string HandLifeModifier = "";
        public string DeckHints = "";

        public string picturePath = "";
        public string picFileNameWithoutExtension
        {
            get
            {				
                string[] tmp = picturePath.Split(new char[] { '/' });
                string[] f = tmp[tmp.Length - 1].Split(new char[] { '.' });
                return f[0];
            }
        }

        public bool Alternate = false;

        [NonSerialized]
		Dictionary<String, int[]> textures = new Dictionary<string, int[]>();        
        public int nbrImg = 1;

		public void SetTexture(string _path)
        {
			UnloadTextures ();
			if (File.Exists(_path))
			{
				textures[""] = new int[1];
				textures[""][0] = CreateTexture(_path);  
			}
        }
			
		public void Render(string edition = "", int selectedTexIdx = 0)
        {
			if (!textures.ContainsKey (edition)) {
				loadTextures (edition);

			}
				
			GL.CullFace(CullFaceMode.Front);
            GL.BindTexture(TextureTarget.Texture2D, MagicData.CardBack);
			MagicData.CardMesh.Render (PrimitiveType.TriangleStrip);

			//Magic.texturedShader.ModelMatrix = Matrix4.CreateRotationY (MathHelper.Pi) * Magic.texturedShader.ModelMatrix;

			GL.CullFace(CullFaceMode.Back);
            GL.BindTexture(TextureTarget.Texture2D, textures[edition][selectedTexIdx]);
			MagicData.CardMesh.Render (PrimitiveType.TriangleStrip);

			GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public bool DownloadingTextureInProgress = false;
        public int DownloadingTryCount = 0;
		void loadTextures(string edition = "")
        {
            if (DownloadingTextureInProgress || DownloadingTryCount > 3)
                return;
				            
			string basePath = System.IO.Path.Combine (MagicData.cardsArtPath, "cards");
			string editionPicsPath = System.IO.Path.Combine (basePath, edition);

			if (Directory.Exists (editionPicsPath))
				basePath = editionPicsPath;
			
			textures[edition] = new int[nbrImg];

			bool texturesFound = false;
            for (int i = 0; i < nbrImg; i++)
            {
                string f = "";
                if (nbrImg == 1)
                    f = Directory.GetFiles(basePath, Name + ".full.jpg").FirstOrDefault();
                else
                    f = Directory.GetFiles(basePath, Name + (i + 1) + ".full.jpg").FirstOrDefault();

                if (File.Exists(f))
                {
					texturesFound = true;
					textures[edition][i] = CreateTexture(f);                        
                }
            }                

			if (texturesFound)
				return;
			
            if (!MagicData.MissingPicToDownload.Contains(this))
                MagicData.MissingPicToDownload.Add(this);
        }
        public void UnloadTextures()
        {
            if (textures == null)
                return;

			foreach (int[] texs in textures.Values) {
				for (int i = 0; i < nbrImg; i++)
				{
					int tex = texs[i];
					if (tex > 0)
						GL.DeleteTextures(1, ref tex);
				}
			}
        }
        public int CreateTexture(string file)
        {
            Texture t = new Texture(file);
            //updateGraphic(file);
            return t;        
        }
			
        void updateGraphic(string file)
        {
            int width = 100;
            int height = 15;
            int x = 10;
            int y = 6;

            Bitmap bmp = new Bitmap(file);
            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(x, y, width, height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int stride = 4 * width;

            using (ImageSurface draw =
                new ImageSurface(data.Scan0, Format.Argb32, width, height, stride))
            {
                using (Context gr = new Context(draw))
                {
                    //Rectangle r = new Rectangle(0, 0, renderBounds.Width, renderBounds.Height);
                    gr.SelectFontFace("MagicMedieval", FontSlant.Normal, FontWeight.Bold);
                    gr.SetFontSize(12);
					gr.SetSourceColor(Crow.Color.Black);

                    string test = "Test string";

                    FontExtents fe = gr.FontExtents;
                    TextExtents te = gr.TextExtents(test);
                    double xt = 20;// 0.5 - te.XBearing - te.Width / 2,
                    double yt = fe.Height;

                    gr.MoveTo(xt, yt);
                    gr.ShowText(test);

                    using (ImageSurface imgSurf = new ImageSurface(@"images/manaw.png"))
                    {
                        gr.SetSourceSurface(imgSurf, 0,0);

                        gr.Paint();
                    }
                    draw.Flush();
                }
                //draw.WriteToPng(directories.rootDir + @"test.png");
            }


			imgHelpers.imgHelpers.flipY(data.Scan0, stride, height);

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, bmp.Height - y - height, width, height,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);

        }
			
  
        public string colorComponentInPicName
        {
            get
            {
                string color = picFileNameWithoutExtension.Split(new char[] { '_' }).LastOrDefault();

                color = char.ToUpper(color[0]) + color.Substring(1);

                switch (color)
                {
                    case "Black":
                    case "White":
                    case "Red":
                    case "Blue":
                    case "Green":
                    case "Artifacts":
                    case "Shadow":
                    case "Lands":
                        return color;
                    default:
                        return "none";
                }
            }
        }
    }

}
