using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

using Crow;
using System.Collections;

namespace Magic3D
{
    [Serializable]
	public class Deck : IValueChange
    {
		#region IValueChange implementation

		public event EventHandler<ValueChangeEventArgs> ValueChanged;
		void notifyValueChange(string propName, object newValue)
		{
			ValueChanged.Raise(this, new ValueChangeEventArgs (propName, newValue));
		}

		#endregion

        public static Dictionary<string, Deck> PreconstructedDecks = new Dictionary<string, Deck>(StringComparer.OrdinalIgnoreCase);
        
        //metadata
		public string Name = "unamed";
		public string DeckType = "";
		public string Description = "";
		public string Set = "";
		public string Image = "";

		public string ImgSetPath {
			get {
				return "#Magic3D.images.expansions." + Set + ".svg";
			}
		}
			
        //shop
        public int Credits = 0;
        public int MinDifficulty = 0;
        public int MaxDifficulty = 0;

        public List<CardInstance> Cards = new List<CardInstance>();
        public Player Player;

		public CardInstance AddCard(MagicCard mc, string edition = "")
        {
			CardInstance tmp = new CardInstance (mc) { Edition = edition };
            Cards.Add(tmp);
			return tmp;
        }

        public void ResetCardStates()
        {

        }
		public bool HasAnimatedCards
		{
			get {
				return 
					Animation.AnimationList.Select (al => al.AnimatedInstance).
					OfType<CardInstance> ().Where (c => c.Controler == this.Player).Count() > 0 ?
					true : false;
			}
		}
        enum parserState
        {
            init,
            shop,
            metadata,
            main,
            sideboard,
        }
		public void LoadCards()
		{
			foreach (MainLine l in CardEntries) {
				MagicCard c = null;
				if (!MagicData.TryGetCardFromZip (l.name, ref c)) {
					Debug.WriteLine ("DCK: {0} => Card not found: {1}", Name, l.name);
					return;
				}
				for (int i = 0; i < l.count; i++)
					AddCard (c, l.code);
//				lock (Player.pgBar) {
//					Player.pgBar.Value++;
//				}
			}
		}
		public void LoadNextCardsData()
		{
			MainLine l = cardLines.Dequeue();
			MagicCard c = null;
			if (!MagicData.TryGetCardFromZip (l.name, ref c)) {
				Debug.WriteLine ("DCK: {0} => Card not found: {1}", Name, l.name);
				return;
			}
			for (int i = 0; i < l.count; i++)
				AddCard(c);
		}
		public int CardCount {
			get { return cardLines == null ?
				Cards == null ? 0 : Cards.Count : cardLines.Count; }
		}
		public IList CardEntries {
			get { return cardLines.ToList ();}
		}
		Queue<MainLine> cardLines;
		public static Deck PreLoadDeck(string path)
		{
			Deck d = new Deck();
			d.cardLines = new Queue<MainLine> ();
			parserState state = parserState.init;

			using (Stream s = new FileStream(path, FileMode.Open))
			{
				using (StreamReader sr = new StreamReader(s))
				{
					while (!sr.EndOfStream)
					{
						string tmp = sr.ReadLine();

						if (tmp.StartsWith("["))
						{
							switch (tmp.ToLower())
							{
							case "[shop]":
								state = parserState.shop;
								continue;
							case "[metadata]":
								state = parserState.metadata;
								continue;
							case "[main]":
								state = parserState.main;
								continue;
							case "[sideboard]":
								state = parserState.sideboard;
								continue;
							default:
								state = parserState.init;
								continue;
							}
						}

						switch (state)
						{
						case parserState.shop:
							break;
						case parserState.metadata:
							string[] tokens = tmp.Split(new char[] { '=' });
							switch (tokens[0].ToLower())
							{
							case "name":
								d.Name = tokens[1];
								break;
							case "description":
								d.Description = tokens[1];
								break;
							case "set":
								d.Set = tokens[1];
								break;
							case "Image":
								d.Image = tokens[1];
								break;
							case "deck type":
								d.DeckType = tokens[1];
								break;
							default:
								break;
							}
							break;
						case parserState.main:
							if (string.IsNullOrEmpty (tmp))
								continue;
							MainLine l = new MainLine ();

							string strCount = tmp.Split (new char[] { ' ' }) [0];
							l.count = int.Parse (strCount);
							string strTmp = tmp.Substring (strCount.Length).Trim ();
							string[] ts = strTmp.Split (new char[] { '|' });
							l.name = ts [0];
							l.code = "";
							if (ts.Length > 1)
								l.code = ts [1];

							//List<MagicCard> lmc = MagicCard.cardDatabase.Values.ToList().Where(c => c.Name.StartsWith("Faith", StringComparison.OrdinalIgnoreCase)).ToList();
							d.cardLines.Enqueue (l);
							break;
						case parserState.sideboard:
							break;
						default:
							break;
						}
					}
				}
			}

			//PreconstructedDecks.Add(d.Name,d);
			return d;
		}
			
		public override string ToString ()
		{
			return Name;
		}
    }
	class MainLine
	{
		public int count = 0;
		public string name = "unamed";
		public string code = "";
		public string ExpansionImg {
			get { return "#Magic3D.images.expansions." + code + ".svg"; }
		}
	}
}
