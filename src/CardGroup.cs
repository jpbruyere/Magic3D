using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Magic3D
{
    public enum CardGroups
    {
        Library,
        Hand,
        InPlay,
        Permanent,
        Graveyard,
        Exhiled,
        Attackers,
        Blockers
    }
		
    public class CardGroup : CardLayout
    {                
        public CardGroups GroupName;

        public bool IsVisible = true;

        public CardGroup(CardGroups groupName)         
        {
            GroupName = groupName;
        }
        public virtual void AddCard(CardInstance c)
        {
            if (c == null)
                return;

            c.CurrentGroup = this;

            float hSpace = HorizontalSpacing;

            if (HorizontalSpacing * (Cards.Count + 1) > MaxHorizontalSpace)
                hSpace = MaxHorizontalSpace / (Cards.Count + 1);


			Animation.StartAnimation(new FloatAnimation(c, "x", this.x + hSpace / 2 * Cards.Count, 0.3f));

            float halfWidth = hSpace * (Cards.Count) / 2;

			foreach (CardInstance i in Cards)
				Animation.StartAnimation (new FloatAnimation (i, "x", this.x - halfWidth + hSpace * Cards.IndexOf(i),0.1f));

            Animation.StartAnimation(new FloatAnimation(c, "y", this.y, 0.2f));
            Animation.StartAnimation(new FloatAnimation(c, "z", this.z + VerticalSpacing * Cards.Count, 0.2f));

            Cards.Add(c);
        }
        public CardInstance TakeTopOfStack
        {
            get
            {
                if (Cards.Count == 0)
                    return null;

                return takeCard(Cards.Count - 1);
            }
        }
        public CardInstance takeCard(int index)
        {
            CardInstance c = Cards[index];
            //c.Position = Vector3.Transform(this.Position, c.Deck.Player.Transformations);
            RemoveCard(c);
            return c;
        }
        public virtual void RemoveCard(CardInstance c)
        {
            Cards.Remove(c);

            c.CurrentGroup = null;

            MagicEngine.CurrentEngine.RaiseMagicEvent(new MagicEventArg(MagicEventType.QuitZone, c));

            float hSpace = HorizontalSpacing;

            if (HorizontalSpacing * (Cards.Count + 1) > MaxHorizontalSpace)
                hSpace = MaxHorizontalSpace / (Cards.Count + 1);

            float halfWidth = hSpace * (Cards.Count) / 2;

            foreach (CardInstance i in Cards)
            {
                Animation.StartAnimation(new FloatAnimation(i, "x", this.x - halfWidth + hSpace * Cards.IndexOf(i)));
                Animation.StartAnimation(new FloatAnimation(c, "z", this.z + VerticalSpacing * Cards.IndexOf(i)));
            }

            //Animation.StartAnimation(new FloatAnimation(c, "y", this.y, 0.2f));

        }
    }
}
