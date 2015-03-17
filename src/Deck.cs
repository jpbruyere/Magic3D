using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Magic3D
{
    [Serializable]
    public class Deck
    {
        public static Dictionary<string, Deck> PreconstructedDecks = new Dictionary<string, Deck>(StringComparer.OrdinalIgnoreCase);
		static string preconstructedDecksPath = @"/mnt/data2/downloads/forge-gui-desktop-1.5.31/res/quest/precons/";
        
        //metadata
        public string Name = "unamed";
        public string DeckType = "";
        public string Description = "";
        public string Set = "";
        public string Image = "";
       

        //shop
        public int Credits = 0;
        public int MinDifficulty = 0;
        public int MaxDifficulty = 0;

        public List<CardInstance> Cards = new List<CardInstance>();
        public Player Player;

        public void AddCard(MagicCard mc)
        {
            Cards.Add(new CardInstance(mc));
        }

        public void ResetCardStates()
        {

        }

        enum parserState
        {
            init,
            shop,
            metadata,
            main,
            sideboard,
        }

        public static void LoadPreconstructedDecks()
        {
            string[] editions = Directory.GetFiles(preconstructedDecksPath, "*.dck");
            parserState state = parserState.init;

            foreach (string f in editions)
            {
                Deck d = new Deck();


                using (Stream s = new FileStream(f, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(s))
                    {
                        while (!sr.EndOfStream)
                        {
                            string tmp = sr.ReadLine();

                            if (tmp.StartsWith("["))
                            {
                                switch (tmp)
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
                                    if (string.IsNullOrEmpty(tmp))
                                        continue;

                                    string strCount = tmp.Split(new char[] { ' ' })[0];
                                    int count = int.Parse(strCount);
                                    string strTmp = tmp.Substring(strCount.Length).Trim();
                                    string[] ts = strTmp.Split(new char[] { '|' });
                                    string name = ts[0];
                                    string code = "";
                                    if (ts.Length > 1)
                                        code = ts[1];

                                    //List<MagicCard> lmc = MagicCard.cardDatabase.Values.ToList().Where(c => c.Name.StartsWith("Faith", StringComparison.OrdinalIgnoreCase)).ToList();

                                    try
                                    {
                                        for (int i = 0; i < count; i++)
                                            d.AddCard(MagicCard.cardDatabase[name]);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine("DCK: {0} => Card not found: {1}", d.Name, name);
                                    }

                                    break;
                                case parserState.sideboard:
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

                PreconstructedDecks.Add(d.Name,d);
            }

        }

    }
}
