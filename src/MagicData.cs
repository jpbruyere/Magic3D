using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using GGL;
using Cairo;
using System.IO;
using System.Linq;

namespace Magic3D
{
	public static class MagicData
	{
		public const float CardWidth = 1f;
		public const float CardHeight = 1.425f;

		public static Rectangle<float> CardBounds = new Rectangle<float> (-0.5f,-0.7125f, 1f, 1.425f);
		public static vaoMesh CardMesh;
		public static vaoMesh AbilityMesh;
		public static vaoMesh PointsMesh;

		public static int CardBack = 0;

		public static ImageSurface imgManaW;
		public static ImageSurface imgManaG;
		public static ImageSurface imgManaR;
		public static ImageSurface imgManaB;
		public static ImageSurface imgManaU;

		public static void InitCardModel()
		{
			CardBack = new Texture(@"images/card_back.jpg");

			MagicData.CardMesh = new vaoMesh(0, 0, 0, MagicData.CardWidth, MagicData.CardHeight);

			MagicData.AbilityMesh = new vaoMesh(0, 0, 0.001f, 0.1f, 0.1f);
			MagicData.PointsMesh = new vaoMesh(0, 0, 0.002f, 0.50f, 0.2f);

			//			imgManaW = new ImageSurface(@"images/manaw.png");
			//			imgManaG = new ImageSurface(@"images/manag.png");
			//			imgManaR = new ImageSurface(@"images/manar.png");
			//			imgManaB = new ImageSurface(@"images/manab.png");
			//			imgManaU = new ImageSurface(@"images/manau.png");
		}

		public static List<MagicCard> MissingPicToDownload = new List<MagicCard>();

		static void PictureDownloader()
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

		const string rootDir = @"/mnt/data/";
		public const string cardsArtPath = @"/home/jp/.cache/forge/pics/cards/";
		const string cardsDBPath = rootDir + @"MagicCardDataBase/";

		const int maxCard = 1000;

		static Dictionary<string, MagicCard> cardDatabase = new Dictionary<string, MagicCard>(StringComparer.OrdinalIgnoreCase);

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
//							Ability aa = c.Abilities.FirstOrDefault ();
//							if (aa == null) {
//								aa = new Ability ();
//								c.Abilities.Add (aa);
//							} else
//								Debugger.Break;

							c.SpellEffects.Add(EffectGroup.Parse(tmp[1]));
							break;
						case "t":
							c.Triggers.Add(Trigger.Parse(tmp[1]));
							break;
						case "svar":

							if (SVarToResolve.TryToParseAndSetValue (tmp [1], tmp [2]))
								break;
							
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
							Ability a = Ability.ParseKeyword(tmp[1],c);
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

	}
}

