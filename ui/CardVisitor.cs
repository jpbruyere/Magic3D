//
//  CardVisitor.cs
//
//  Author:
//       Jean-Philippe Bruyère <jp.bruyere@hotmail.com>
//
//  Copyright (c) 2015 jp
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Magic3D
{
	public class CardVisitor
	{
		public MagicCard card;

		public CardVisitor (MagicCard _card)
		{
			card = _card;
		}
		public string Name
		{ get { return card.Name; }}
		public MultiformAttribut<CardTypes> Types
		{ get { return card.Types; }}
		public int Power { get { return card.Power; }}
		public int Toughness { get { return card.Toughness; }}
		public List<Ability> Abilities { get { return card.Abilities; }}
		public List<String> StaticAbilities { get { return card.Abilities.Where(a=>a.AbilityType != AbilityEnum.Unset).
				Select(ab => ab.AbilityType.ToString()).ToList(); }}
		public List<Ability> ActivatedAbilities { get { return card.Abilities.Where(a=>a.IsActivatedAbility).ToList(); }}
		public List<Ability> TriggeredAbilities { get { return card.Abilities.Where(a=>a.IsTriggeredAbility).ToList(); }}
		public string RawCardData { get { return card.RawCardData; }}



		public string GetImagePath
		{
			get {
				return 
					Directory.GetFiles (System.IO.Path.Combine (MagicData.cardsArtPath, "cards"),
						Name + "*.full.jpg", SearchOption.AllDirectories).FirstOrDefault ();
			}
		}
		public bool IsCreature {
			get { return Types == CardTypes.Creature; }
		}
		//		public GraphicObject GoCosts
		//		{
		//			get{ 
		//				HorizontalStack hs = new HorizontalStack ();
		//				hs.addChild (new go.Image ("") { Width = 16, Height = 16 , SvgSub = "w"});
		//			}
		//		}
		public String[] CostElements
		{
			get{
				if (card.Cost == null)
					return null;
				string tmp = card.Cost.ToString ();
				return tmp.Split(' ').Where(cc => cc.Length < 3).ToArray();
			}
		}
	}
}

