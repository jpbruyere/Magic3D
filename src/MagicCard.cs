using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using GGL;
using OpenTK;
using go;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Drawing.Imaging;
using System.Drawing;

using GLU = OpenTK.Graphics.Glu;
using Cairo;

namespace Magic3D
{
    [Serializable]
    public class MagicCard
    {
		const int maxCard = 1000;

		public static Rectangle<float> CardBounds = new Rectangle<float> (-0.5f,-0.7125f, 1f, 1.425f);
		public const float CardWidth = 1f;
		public const float CardHeight = 1.425f;

        public static Dictionary<string, MagicCard> cardDatabase = new Dictionary<string, MagicCard>(StringComparer.OrdinalIgnoreCase);

		public const string rootDir = @"/mnt/data/";
		static string cardsArtPath = @"/home/jp/.cache/forge/pics/cards/";
		static string cardsDBPath = rootDir + @"MagicCardDataBase/";
        static Random rnd = new Random();
        
		public static vaoMesh CardMesh;
		static vaoMesh abilityMesh;
		public static vaoMesh pointsMesh;

        public static int cardBack = 0;

		public string FilePath;
        public string Name;
        public Cost Cost;
        public MultiformAttribut<CardTypes> Types = new MultiformAttribut<CardTypes>();
        public List<Ability> Abilities = new List<Ability>();
        public List<Trigger> Triggers = new List<Trigger>();
        public List<string> Konstrains = new List<string>();
        public List<string> R = new List<string>();
        public List<string> DeckNeeds = new List<string>();
        public List<string> TextField = new List<string>();
        public List<string> Comments = new List<string>();
        public static List<string> Svars = new List<string>();

        public string Oracle = "";

        public int Power = 0;
        public int Toughness = 0;        
        
        public string S = "";
        public string AlternateMode = "";
        public MultiformAttribut<ManaTypes> Colors = new MultiformAttribut<ManaTypes>();
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
        int[] texture;
        int _selectedTexture = 0;
        int nbrImg = 1;

        public int Texture
        {
            get { return texture == null ? 0 : texture[_selectedTexture]; }
        }
			
        //public bool HasType(CardTypes ct)
        //{
        //    return Types.Contains(ct) ? true : false;
        //}

        public static ImageSurface imgManaW;
        public static ImageSurface imgManaG;
        public static ImageSurface imgManaR;
        public static ImageSurface imgManaB;
        public static ImageSurface imgManaU;

        public static void InitCardMesh()
        {
            initCardModel();

            imgManaW = new ImageSurface(@"images/manaw.png");
            imgManaG = new ImageSurface(@"images/manag.png");
            imgManaR = new ImageSurface(@"images/manar.png");
            imgManaB = new ImageSurface(@"images/manab.png");
            imgManaU = new ImageSurface(@"images/manau.png");

        }

        public void Render()
        {
            if (Texture == 0)
                loadTexture();
				
			GL.CullFace(CullFaceMode.Front);
            GL.BindTexture(TextureTarget.Texture2D, cardBack);
			CardMesh.Render (PrimitiveType.TriangleStrip);

			//Magic.texturedShader.ModelMatrix = Matrix4.CreateRotationY (MathHelper.Pi) * Magic.texturedShader.ModelMatrix;

			GL.CullFace(CullFaceMode.Back);
            GL.BindTexture(TextureTarget.Texture2D, Texture);
			CardMesh.Render (PrimitiveType.TriangleStrip);

			GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public bool DownloadingTextureInProgress = false;
        public int DownloadingTryCount = 0;
        void loadTexture()
        {
            if (DownloadingTextureInProgress || DownloadingTryCount > 3)
                return;

            texture = new int[nbrImg];

            string[] subPath = { "", "4E/", "5E/", "10E/" };
            foreach (string sb in subPath)
            {
                for (int i = 0; i < nbrImg; i++)
                {
                    string f = "";
                    if (nbrImg == 1)
                        f = Directory.GetFiles(cardsArtPath + sb, Name + ".full.jpg").FirstOrDefault();
                    else
                        f = Directory.GetFiles(cardsArtPath + sb, Name + (i + 1) + ".full.jpg").FirstOrDefault();

                    if (File.Exists(f))
                    {
                        texture[i] = CreateTexture(f);                        
                    }
                }                
            }

            if (nbrImg > 1)
                _selectedTexture = rnd.Next(nbrImg);

            if (texture[_selectedTexture] > 0)
                return;

            if (!MissingPicToDownload.Contains(this))
                MissingPicToDownload.Add(this);
        }
        public void UnloadTexture()
        {
            if (texture == null)
                return;

            for (int i = 0; i < nbrImg; i++)
            {
                int tex = texture[i];
                if (tex > 0)
                    GL.DeleteTextures(1, ref tex);
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
                    gr.Color = go.Color.Black;

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
			
        public static List<MagicCard> MissingPicToDownload = new List<MagicCard>();

        public static void PictureDownloader()
        {
            while (true)
            {
                int count = 0;
                lock (MissingPicToDownload)
                {
                    count = MissingPicToDownload.Count;
                }
                if (count == 0)
                    Thread.Sleep(30000);
                else
                {
                    MagicCard mc = null;
                    string path = "";
                    string name = "";
                    lock (MissingPicToDownload)
                    {
                        mc = MissingPicToDownload[0];
                        MissingPicToDownload.RemoveAt(0);
                    }

                    lock (mc)
                    {
                        path = mc.picturePath;
                        name = mc.Name;
                        mc.DownloadingTextureInProgress = true;
                        mc.DownloadingTryCount++;
                    }

                    Debug.WriteLine("downloading:" + "http:" + path);
                    WebClient webClient = new WebClient();
                    try
                    {
                        webClient.DownloadFile("http:" + path, cardsArtPath + name + ".full.jpg");
                        Debug.WriteLine("->ok");
                    }
                    catch (Exception)
                    {
                        if (File.Exists(cardsArtPath + name + ".full.jpg"))
                            File.Delete(cardsArtPath + name + ".full.jpg");
                    }
                    lock (mc)
                    {
                        mc.DownloadingTextureInProgress = false;
                    }

                }
            }
        }

        public static void initCardModel()
        {
			cardBack = new Texture(@"images/card_back.jpg");

			CardMesh = new vaoMesh(0, 0, 0, CardWidth, CardHeight);
            
			abilityMesh = new vaoMesh(0, 0, 0.001f, 0.1f, 0.1f);
			pointsMesh = new vaoMesh(0, 0, 0.002f, 0.50f, 0.2f);            
        }
        
        public static void LoadCardDatabase()
        {

            string[] dirs = Directory.GetDirectories(cardsDBPath);

            foreach (string dir in dirs)
            {
                string[] files = Directory.GetFiles(dir, "*.txt");

                foreach (string f in files)
                {
					LoadCardData (f);

                    if (cardDatabase.Count > maxCard)
                        return;

                }

            }


            //string[] fi = Directory.GetFiles(cardsArtPath, "*.jpg");


            //for (int i = 0; i < 200; i++)
            //{
            //    MagicCard c = new MagicCard();
            //    c.Name = fi[i];
            //    c.Texture = new Texture(fi[i]);
            //    cardDatabase.Add(c);
            //}
            using (Stream s = new FileStream("enum.cs", FileMode.Create))
            {
                using (StreamWriter sr = new StreamWriter(s))
                {
                    foreach (string st in Ability.strings)
                    {
                        sr.WriteLine("case \"" + st + "\":");
                        sr.WriteLine("break;");
                    }
                }
            }
        }
		public static bool TryGetCard(string name, ref MagicCard c){
			if (cardDatabase.ContainsKey (name)) {
				c = cardDatabase [name];
				return true;
			}
			string cfn = name.Trim ().Replace (' ', '_').ToLower () + ".txt";
			foreach (string file in Directory.EnumerateFiles(
				cardsDBPath, cfn, SearchOption.AllDirectories))
			{
				LoadCardData(file);
				c = cardDatabase [name];
				return true;
			}
			return false;
		}
		public static void LoadCardData(string path)
		{
			MagicCard c = new MagicCard();
			c.FilePath = path;

			using (Stream s = new FileStream(path, FileMode.Open))
			{
				using (StreamReader sr = new StreamReader(s))
				{
					while (!sr.EndOfStream)
					{
						string line = sr.ReadLine();

						string[] tmp = line.Split(new char[] { ':' });

						switch (tmp[0].ToLower())
						{
						case "name":
							c.Name = tmp[1];
							break;
						case "manacost":
							c.Cost = Mana.Parse(tmp[1]);
							break;
						case "types":
							string[] types = tmp[1].Split(new char[] { ' ' });
							foreach (string t in types)
							{
								if (string.IsNullOrEmpty(t))
									continue;
								string tt = t.Replace('\'', '_');
								tt = tt.Replace('-', '_');
								c.Types.Value = (CardTypes)Enum.Parse(typeof(CardTypes), tt, true);
							}
							break;
						case "a":
							c.Abilities.Add(Ability.Parse(tmp[1]));
							break;
						case "oracle":
							c.Oracle = tmp[1];
							break;
						case "pt":
							string[] pt = tmp[1].Split(new char[] { '/' });

							int intPT;

							if (int.TryParse(pt[0], out intPT))
								c.Power= intPT;
							else
								Debug.WriteLine("power:" + pt[0]);

							if (int.TryParse(pt[1], out intPT))
								c.Toughness = intPT;
							else
								Debug.WriteLine("toughness:" + pt[1]);
							break;
						case "s":
							Ability aa = c.Abilities.FirstOrDefault();
							if (aa == null)
								aa = new Ability();
							aa.Effects = Effect.Parse(tmp[1]).ToList();
							break;
						case "t":
							c.Triggers.Add(Trigger.Parse(tmp[1]));
							break;
						case "svar":
							#region TRIGGERS
							if (tmp [1].ToLower ().StartsWith ("trig")) {
								TrigExec te;
								if (!Enum.TryParse (tmp [1], true, out te)) {
									Debug.WriteLine ("unknow trigger: " + tmp [1]);
									break;
								}
								IEnumerable<Trigger> trigs = c.Triggers.Where (tt => tt.Exec == te);
								if (trigs.Count () == 0) {
									Debug.WriteLine ("cant find trigGainLife trigger");
									break;
								}else if(trigs.Count () > 1)
									Debug.WriteLine ("Multiple corresponding triggers.");
								c.Abilities.Add (Ability.Parse (tmp [2], trigs.FirstOrDefault ()));

								break;
							}
							#endregion
							switch (tmp[1].ToLower())
							{
							case "darkeffect":
								break;
							case "darkpower":
								break;
							case "darkmana":
								break;
							case "picture":
								c.picturePath = tmp[3];
								break;
							case "X":
								break;
							case "remaideck":
								break;
							case "piccount":
								c.nbrImg = int.Parse(tmp[2]);
								break;
							default:
								Debug.WriteLine ("Unknow SVAR: " + tmp[1]);
								break;
							}
							break;
						case "k":
							Ability a = Ability.SpecialK(tmp[1]);
							if (a != null)
								c.Abilities.Add(a);
							break;
						case "r":
							c.R.Add(tmp[1]);
							break;
						case "deckneeds":
							c.DeckNeeds.Add(tmp[1]);
							break;
						case "text":
							c.TextField.Add(tmp[1]);
							break;
						case "alternatemode":
							c.AlternateMode = tmp[1];
							break;
						case "alternate":
							c.Alternate = true;
							break;
						case "colors":
							c.Colors.Value = (ManaTypes)Enum.Parse(typeof(ManaTypes), tmp[1], true);
							break;
						case "loyalty":
							c.Loyalty = tmp[1];
							break;
						case "handlifemodifier":
							c.HandLifeModifier = tmp[1];
							break;
						case "deckhints":
							c.DeckHints = tmp[1];
							break;
						case "var":
							Debug.WriteLine("var: {0} {1} {2}", c.Name, tmp[1], tmp[2]);
							break;
						default:
							if (tmp[0].StartsWith("#"))
								c.Comments.Add(tmp[0]);
							else if (!string.IsNullOrEmpty(tmp[0]))
							{
								string txt = "";
								for (int i = 0; i < tmp.Length; i++)
									txt += ":" + tmp[i];

								Debug.WriteLine("? => {0} {1}", c.Name, txt);
							}
							break;
						}
					}

				}
			}

			#region add mana ability to basic lands
			if (c.Types == CardTypes.Land)
			{
				if (c.Abilities.Count == 0)
				{
					Mana m = null;
					switch (c.Name.ToLower())
					{
					case "plains":
					case "snow-covered plains":
						m = ManaTypes.White;
						break;
					case "mountain":
					case "snow-covered mountain":
						m = ManaTypes.Red;
						break;
					case "swamp":
					case "snow-covered swamp":
						m = ManaTypes.Black;
						break;
					case "forest":
					case "snow-covered forest":
						m = ManaTypes.Green;
						break;
					case "island":
					case "snow-covered island":
						m = ManaTypes.Blue;
						break;
					}
					if (m != null)
						c.Abilities.Add(new ManaAbility { ProducedMana = m, ActivationCost = CostTypes.Tap });
				}
			}
			#endregion

			try
			{
				if (c.Name == "Circle of Protection" || c.Name == "Rune of Protection")
				{
					cardDatabase.Add(c.Name + ": " + c.colorComponentInPicName, c);
				}
				else
					cardDatabase.Add(c.Name, c);
			}
			catch (Exception e)
			{
				Debug.WriteLine("failed to add: {0} to db\n{1}", c.Name, e.Message);
			}
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
