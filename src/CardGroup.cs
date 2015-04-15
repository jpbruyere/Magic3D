using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace Magic3D
{
    public enum CardGroupEnum
    {
		Any,
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
		#region CTOR
		public CardGroup(CardGroupEnum groupName)         
		{
			GroupName = groupName;
		}
		#endregion

		public static CardGroupEnum ParseZoneName(string zone)
		{
			switch (zone) {
			case "Any":
				return CardGroupEnum.Any;
			case "Battlefield":
				return CardGroupEnum.InPlay;
			case "Library":
				return CardGroupEnum.Library;
			case "Hand":
				return CardGroupEnum.Hand;
			default:
				Debug.WriteLine ("Unknow zone: " + zone);
				return CardGroupEnum.Any;					
			}
		}
		public CardGroupEnum GroupName;

        public bool IsVisible = true;
		public bool IsSelected = false;

		public virtual void AddCard(CardInstance c)
        {
            if (c == null)
                return;

            c.CurrentGroup = this;
//            float hSpace = HorizontalSpacing;
//
//            if (HorizontalSpacing * (Cards.Count + 1) > MaxHorizontalSpace)
//                hSpace = MaxHorizontalSpace / (Cards.Count + 1);
//
//
//			Animation.StartAnimation(new FloatAnimation(c, "x", this.x + hSpace / 2 * Cards.Count, 0.2f));
//
//            float halfWidth = hSpace * (Cards.Count) / 2;
//
//			foreach (CardInstance i in Cards) {
//				Animation.StartAnimation (new FloatAnimation (i, "x", this.x - halfWidth + hSpace * Cards.IndexOf (i), 0.2f));
//				Animation.StartAnimation(new FloatAnimation(i, "z", this.z + VerticalSpacing * Cards.IndexOf(i), 0.2f));
//			}
//
//            Animation.StartAnimation(new FloatAnimation(c, "y", this.y, 0.2f));
//			Animation.StartAnimation(new FloatAnimation(c, "z", this.z + VerticalSpacing * (Cards.Count+1), 0.2f));
//			Animation.StartAnimation(new AngleAnimation(c, "yAngle", this.yAngle, MathHelper.Pi * 0.1f));
//			Animation.StartAnimation(new AngleAnimation(c, "xAngle", this.xAngle, MathHelper.Pi * 0.03f));
            
			Cards.Add(c);
			UpdateLayout ();
		}
        public virtual void RemoveCard(CardInstance c)
        {
            Cards.Remove(c);

            //c.CurrentGroup = null;

//            float hSpace = HorizontalSpacing;
//
//            if (HorizontalSpacing * (Cards.Count + 1) > MaxHorizontalSpace)
//                hSpace = MaxHorizontalSpace / (Cards.Count + 1);
//
//            float halfWidth = hSpace * (Cards.Count) / 2;
//
//            foreach (CardInstance i in Cards)
//            {
//                Animation.StartAnimation(new FloatAnimation(i, "x", this.x - halfWidth + hSpace * Cards.IndexOf(i)));
//                Animation.StartAnimation(new FloatAnimation(c, "z", this.z + VerticalSpacing * Cards.IndexOf(i)));
//            }

            //Animation.StartAnimation(new FloatAnimation(c, "y", this.y, 0.2f));
			UpdateLayout();
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
        
		public bool PointIsIn(Vector3 _p)
		{
			float chW = MagicData.CardWidth / 2f;
			float chH = MagicData.CardHeight / 2f;

			Rectangle<float> r = new Rectangle<float> (
				this.x - chW, this.y - chH, 
				MagicData.CardWidth, MagicData.CardHeight);

			Point<float> p = new Point<float> (_p.X, _p.Y);
			return r.ContainsOrIsEqual (p);
		}
		public override void Render ()
		{
			if (IsSelected) {

				Magic.glowShader.Enable ();

				Magic.glowShader.ProjectionMatrix = Magic.projection;
				Magic.glowShader.ModelViewMatrix = Magic.modelview;
				Magic.glowShader.ModelMatrix = Matrix4.CreateScale(1.2f) * Transformations;
				Magic.glowShader.Color = go.Color.Red;
				Magic.glowShader.BorderWidth = 0.05f;

				GL.BindTexture (TextureTarget.Texture2D, Magic.abstractTex);

				GL.Disable(EnableCap.CullFace);

				MagicData.CardMesh.Render (PrimitiveType.TriangleStrip);

				GL.Enable(EnableCap.CullFace);

				Magic.texturedShader.Enable ();
			}

			base.Render ();
		}
	}
}
