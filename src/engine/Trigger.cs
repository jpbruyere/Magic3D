using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace Magic3D
{    
	public enum TrigExec
	{
		TrigGainLife,
		TrigToken,
		TrigChangeZone,
		TrigExile,
		TrigPump,
		TrigKicker,
		TrigChange,
		TrigTap
	}

    public struct Trigger
    {        
        public MagicEventType Type;
        public GamePhases Phase;
        public CardInstance Card;
		public TrigExec Exec;
		public string Description;

		static List<string> list = new List<string> ();

		public static Trigger Parse(string str)
		{
			Trigger t = new Trigger ();
			string[] tmp = str.Trim ().Split (new char[] { '|' });



			using (Stream s = new FileStream ("trigVars.txt",FileMode.Append)) {				
				using (TextWriter tw = new StreamWriter (s)) {
					foreach (string i in tmp) {
						string[] f = i.Trim ().Split (new char[] { '$' });
						string data = f [1].Trim();
						switch (f[0]) {
						case "Mode":
							switch (data) {
							case "ChangesZone":
								break;
							case "Phase":
								break;
							case "Attacks":
								break;
							case "SpellCast":
								break;
							default:
								Debug.WriteLine ("Unknown trigger " + f[0] + " value:" + data);
								break;
							}


							break;
						case "Origin":
							switch (data) {
							case "Any":
								break;
							default:
								Debug.WriteLine ("Unknown trigger " + f[0] + " value:" + data);
								break;
							}							
							break;
						case "Destination":
							switch (data) {
							case "Battlefield":
								break;
							default:
								Debug.WriteLine ("Unknown trigger " + f[0] + " value:" + data);
								break;
							}							
							break;
						case "ValidCard":
							switch (data) {
							case "Card.Self":
								break;
							case "Card.Self+kicked":
								break;
							case "Creature.Self":
								break;
							case "Card.White":
								break;
							case "Plains.YouCtrl":
								break;
							default:
								Debug.WriteLine ("Unknown trigger " + f[0] + " value:" + data);
								break;
							}
							break;
						case "Execute":
							t.Exec = (TrigExec)Enum.Parse (typeof(TrigExec), data);
							break;
						case "TriggerDescription":
							t.Description = data;
							break;
						case "Phase":
							if (list.Contains (data))
								break;
							list.Add (data);
							tw.WriteLine ("case \"" + data + "\":\n\tbreak;");
							//tw.WriteLine (data + ",");
							break;
						case "TriggerZones":
							break;
						case "CheckSVar":
							break;
						case "SVarCompare":
							break;
						case "OptionalDecider":
							break;
						case "ValidActivatingPlayer":
							break;
						case "Secondary":
							break;
						default:
							Debug.WriteLine ("Unknown trigger var:" + f [0]);
							break;
						}

					}
				}
			}
			return t;
		}
    }
}
