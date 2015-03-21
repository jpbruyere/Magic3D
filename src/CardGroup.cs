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

		public CardGroupEnum GroupName;

        public bool IsVisible = true;
		public bool IsSelected = false;



		public virtual void AddCard(CardInstance c)
        {
            if (c == null)
                return;

            c.CurrentGroup = this;
            Cards.Add(c);
			Magic.AddAnimation (UpdateLayout ());
        }
		public virtual void RemoveCard(CardInstance c)
		{
			Cards.Remove(c);

			c.CurrentGroup = null;

			MagicEngine.CurrentEngine.RaiseMagicEvent(new MagicEventArg(MagicEventType.QuitZone, c));

			Magic.AddAnimation(UpdateLayout ());		
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
			float chW = MagicCard.CardWidth / 2f;
			float chH = MagicCard.CardHeight / 2f;

			Rectangle<float> r = new Rectangle<float> (
				this.x - chW, this.y - chH, 
				MagicCard.CardWidth, MagicCard.CardHeight);

			Point<float> p = new Point<float> (_p.X, _p.Y);
			return r.ContainsOrIsEqual (p);
		}
		public override void Render ()
		{
			if (IsSelected) {

				Magic.glowShader.Enable ();
//
				Magic.glowShader.ProjectionMatrix = Magic.projection;
				Magic.glowShader.ModelViewMatrix = Magic.modelview;
				Magic.glowShader.ModelMatrix = Matrix4.CreateScale(1.2f) * Transformations;
				Magic.glowShader.Color = go.Color.Red;
				Magic.glowShader.BorderWidth = 0.05f;

				GL.BindTexture (TextureTarget.Texture2D, Magic.abstractTex);
//				GL.CullFace(CullFaceMode.Front);
//				MagicCard.cardMesh.Render (PrimitiveType.TriangleStrip);
				GL.Disable(EnableCap.CullFace);
				MagicCard.cardMesh.Render (PrimitiveType.TriangleStrip);
				GL.Enable(EnableCap.CullFace);
				Magic.texturedShader.Enable ();

			}

			base.Render ();
		}
	}
}
