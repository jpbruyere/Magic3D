using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using Cairo;
using GGL;
using go;
using OpenTK;
using OpenTK.Graphics.OpenGL;

using GLU = OpenTK.Graphics.Glu;

namespace Magic3D
{
    [Serializable]
	public class CardInstance : IDamagable, IRenderable
    {
		#region CardAnimEvent
		public class CardAnimEventArg : EventArgs
		{
			public CardInstance card;
			public CardAnimEventArg(CardInstance _card){
				card = _card;
			}
		}
		#endregion

		#region CTOR
		public CardInstance(MagicCard mc)
		{
			Model = mc;

			if (Model.Types != CardTypes.Creature)
				return;


		}
		#endregion

        #region selection
		/// <summary>
		/// The focused card zoomed in the middle of screen
		/// </summary>
		public static CardInstance focusedCard = null;
        public static CardInstance selectedCard;

		public static go.Color notSelectedColor = new go.Color(0.9f, 0.9f, 0.9f, 1f);
		public static go.Color SelectedColor = new go.Color(1f, 1f, 1f, 1f);
		public static go.Color AttackingColor = new go.Color(1.0f, 0.8f, 0.8f, 1f);
        #endregion


		bool _isTapped = false;
		bool _combating;
		Player _controler;

        public MagicCard Model;
        public CardGroup CurrentGroup;
		public bool HasFocus = false;
		public bool HasSummoningSickness = false;

        public EffectGroup Effects = new EffectGroup();
		public List<CardInstance> BlockingCreatures = new List<CardInstance>();
		public List<Damage> Damages = new List<Damage>();
		public CardInstance BlockedCreature = null;

		#region Attachment
        public List<CardInstance> AttachedCards = new List<CardInstance>();
		public void AttachCard(CardInstance c)
		{			
			c.AttachedTo = this;
			AttachedCards.Add (c);

			updateArrows ();
			UpdateOverlay ();

			Controler.InPlay.UpdateLayout ();
		}
		public void DetacheCard(CardInstance c)
		{
			c.AttachedTo = null;
			AttachedCards.Remove (c);

			updateArrows ();
			UpdateOverlay ();

			if (!c.HasType (CardTypes.Equipment))
				c.PutIntoGraveyard ();			
		}
		public CardInstance AttachedTo = null;
		public bool IsAttached {
			get { return AttachedTo == null ? false : true; }
		}
		public bool IsAttachedToACardInTheSameCamp {
			get { 
				if (!IsAttached)
					return false;
				return (AttachedTo.Controler == this.Controler);
			}
		}
		#endregion
			
        public void AddDamages(Damage d)
        {
            Damages.Add(d);
            
            MagicEngine.CurrentEngine.RaiseMagicEvent(new DamageEventArg(d));
            
            int sum = 0;
            foreach (Damage dm in Damages)
                sum += dm.Amount;
            if (sum > Toughness)
                PutIntoGraveyard();
            else
                UpdateOverlay();
        }
		public void ChangeZone(CardGroupEnum _newZone){			
			CardGroupEnum _oldZone = CurrentGroup.GroupName;

			CurrentGroup.RemoveCard (this);
			Controler.GetGroup(_newZone).AddCard (this);

			MagicEngine.CurrentEngine.RaiseMagicEvent (
				new ChangeZoneEventArg (this, _oldZone, _newZone));

			UpdateOverlay ();
		}
        public void PutIntoGraveyard()
        {
            Reset();
			ChangeZone (CardGroupEnum.Graveyard);			
        }

        public void Reset()
        {
            ResetPositionAndRotation();
            ResetOverlay();

			Damages.Clear();
            Combating = false;
            if (BlockedCreature != null)
            {
                BlockedCreature.BlockingCreatures.Remove(this);
                BlockedCreature = null;
            }
            tappedWithoutEvent = false;
			HasSummoningSickness = false;
			if (!IsAttached)
				return;
			AttachedTo.DetacheCard (this);
        }

		public Ability[] getAbilitiesByType(AbilityEnum ae)
        {
			return Model.Abilities.Where (a => a.AbilityType == ae).ToArray();
        }
			        
        public bool HasEffect(EffectType et)
        {
			foreach (CardInstance ca in AttachedCards) {
				foreach (Ability a in ca.Model.Abilities) {
					foreach (Effect e in a.Effects)
					{
						if (e.TypeOfEffect == et)
							return true;
					}					
				}
			}
            return false;
        }
        public bool HasAbility(AbilityEnum ab)
        {
			if (getAbilitiesByType(ab).Count()==0 || HasEffect(EffectType.LooseAllAbilities))
                return false;

            foreach (Effect e in Effects)
            {
                if (e.TypeOfEffect == EffectType.Loose)
                {
                    AbilityEffect ae = e as AbilityEffect;
                    if (ae != null)
                        if (ae.Ability.AbilityType == ab)
                            return false;
                }
            }
            return true;
        }
		public bool HasColor(ManaTypes color)
		{
			if (!Model.Colors.Contains (color))
				return false;

			//TODO:test with color gain or loose effects

			return true;
		}
		public bool HasType(CardTypes t)
		{
			return Model.Types == t;
		}

        public bool Combating
        {
            get { return _combating; }
            set 
            { 
                _combating = value;                 
            }
        }
        public bool CanAttack
        {
            get
            {
				if (_isTapped || HasSummoningSickness ||!HasType(CardTypes.Creature))
                    return false;

                if (HasAbility (AbilityEnum.Defender))
                    return false;

                if (HasEffect(EffectType.CantAttack))
                    return false;
                
                return true;
            }
        }
        public bool CanBlock(CardInstance blockedCard)
        { 
            if (_isTapped)
                return false;

            if ( blockedCard.HasAbility(AbilityEnum.Flying) && 
                ! (this.HasAbility(AbilityEnum.Flying) || this.HasAbility(AbilityEnum.Reach)))
                return false;

			if (HasEffect(EffectType.CantBlock))
				return false;
			
            return true;
        }
       
		//TODO: remove redundant function
        public bool IsTapped
        {
            get { return _isTapped; }
        }
        public bool tappedWithoutEvent
        {
            get { return _isTapped; }
            set
            {
                if (value == _isTapped)
                    return;

                _isTapped = value;

 				if (_isTapped)
                {
                    Animation.StartAnimation(new FloatAnimation(this, "zAngle", -MathHelper.PiOver2, MathHelper.Pi * 0.1f));
                }
                else
                    Animation.StartAnimation(new FloatAnimation(this, "zAngle", 0f, MathHelper.Pi * 0.1f));
            }
        }
        public void Tap()
        {
            tappedWithoutEvent = true;
            MagicEngine.CurrentEngine.RaiseMagicEvent(new MagicEventArg(MagicEventType.TapCard, this));
        }
        public void TryToUntap()
        {
            Effect e = Effects.Where(ef => ef.TypeOfEffect == EffectType.DoesNotUntap).LastOrDefault();

            if (e == null)
                tappedWithoutEvent = false;
        }

		public Player Controler
		{
			get
			{
				ControlEffect ce = Effects.OfType<ControlEffect>().LastOrDefault();
				return ce == null ? _controler : ce.Controler;
			}
			set { _controler = value; }
		}

		public int Power
        {
            get
            {
                int tmp = Model.Power;

                foreach (NumericEffect e in Effects.OfType<NumericEffect>())
                {
                    switch (e.TypeOfEffect)
                    {
                        case EffectType.AddPower:
                            tmp += e.NumericValue;
                            break;
                        case EffectType.SetPower:
                            tmp = e.NumericValue;
                            break;
                    }
                }
				foreach (CardInstance c in AttachedCards) {
					foreach (Ability a in c.getAbilitiesByType(AbilityEnum.Attach)) {
						foreach (NumericEffect e in a.Effects.OfType<NumericEffect>().
							Where(ef=>ef.TypeOfEffect == EffectType.AddPower)) {
							tmp += e.NumericValue;
						}
					}
				}
                return tmp;
            }
        }
        public int Toughness
        {
            get
            {
                int tmp = Model.Toughness;
                foreach (NumericEffect e in Effects.OfType<NumericEffect>())
                {
                    switch (e.TypeOfEffect)
                    {
                        case EffectType.AddTouchness:
                            tmp += e.NumericValue;
                            break;
                        case EffectType.SetTouchness:
                            tmp = e.NumericValue;
                            break;
                    }
                }
				foreach (CardInstance c in AttachedCards) {
					foreach (Ability a in c.getAbilitiesByType(AbilityEnum.Attach)) {
						foreach (NumericEffect e in a.Effects.OfType<NumericEffect>().
							Where(ef=>ef.TypeOfEffect == EffectType.AddTouchness)) {
							tmp += e.NumericValue;
						}
					}
				}
                foreach (Damage d in Damages)
                    tmp -= d.Amount;

                return tmp;
            }
        }

		#region layouting

        float _x = 0.0f;
        float _y = 0.0f;
        float _z = 0.0f;
        float _xAngle = 0.0f;
        float _yAngle = 0.0f;
        float _zAngle = 0.0f;

        public float x
        {
            get { return _x; }
            set { 
				if (_x == value)
					return;
				
				_x = value; 

				float a = _x;
				foreach (CardInstance ac in AttachedCards) {
					if (ac.Controler != Controler) {
						updateArrows ();		
						continue;
					}
					a += 0.15f;
					Animation.StartAnimation (new FloatAnimation (ac, "x", a, 0.2f));
				}
				//updateArrows ();
			}
        }
        public float y
        {
            get { return _y; }
			set { 
				if (_y == value)
					return;

				_y = value; 

				float a = _y;
				foreach (CardInstance ac in AttachedCards) {
					if (ac.Controler != Controler) {
						updateArrows ();		
						continue;
					}
					a += 0.15f;
					Animation.StartAnimation (new FloatAnimation (ac, "y", a, 0.2f));
				}


			}        
		}
		static float attachedCardsSpacing = 0.03f;
        public float z
        {
            get { return _z; }
			set { 
				if (_z == value)
					return;

				_z = value; 

				float a = _z;
				foreach (CardInstance ac in AttachedCards) {
					if (ac.Controler != Controler) {
						updateArrows ();		
						continue;
					}
					a -=  attachedCardsSpacing;
					Animation.StartAnimation (new FloatAnimation (ac, "z", a, 0.2f));
				}
				//updateArrows ();
			}        
		}
        public float xAngle
        {
            get { return _xAngle; }
            set { _xAngle = value; }
        }
        public float yAngle
        {
            get { return _yAngle; }
            set { _yAngle = value; }
        }
        public float zAngle
        {
            get { return _zAngle; }
            set { _zAngle = value; }
        }

        public float saved_x = 0.0f;
        public float saved_y = 0.0f;
        public float saved_z = 0.0f;
        public float saved_xAngle = 0.0f;
        public float saved_yAngle = 0.0f;
        public float saved_zAngle = 0.0f;

        public virtual Vector3 Position
        {
            get
            { return new Vector3(x, y, z); }
            set
            {
                x = value.X;
                y = value.Y;
                z = value.Z;
            }
        }

        public void ResetPositionAndRotation()
        {
            x = y = z = xAngle = yAngle = zAngle = 0;
        }
       	public void SwitchFocus()
        {
            HasFocus = !HasFocus;

            if (HasFocus)
            {
                Debug.WriteLine("{0} => {1}", this.Model.Name, this.Model.Oracle);
                if (focusedCard != null)
                    focusedCard.SwitchFocus();

				Vector3 v = Magic.vFocusedPoint;
                SavePosition();

                v = Vector3.Transform(v, Matrix4.Invert(Controler.Transformations));

				Animation.StartAnimation(new FloatAnimation(this, "x", v.X, 0.9f));
				Animation.StartAnimation(new FloatAnimation(this, "y", v.Y, 1.2f));
				Animation.StartAnimation(new FloatAnimation(this, "z", v.Z, 0.9f));
				float aCam = Magic.FocusAngle;
				Animation.StartAnimation(new AngleAnimation(this, "xAngle", aCam, MathHelper.Pi * 0.1f));
                //Animation.StartAnimation(new AngleAnimation(this, "yAngle", -Controler.Value.zAngle, MathHelper.Pi * 0.03f));
                Animation.StartAnimation(new AngleAnimation(this, "zAngle", -Controler.zAngle, MathHelper.Pi * 0.3f));


                focusedCard = this;
            }
            else
            {
                focusedCard = null;
                RestoreSavedPosition();
            }
        }

        public void SavePosition()
        {
            saved_x = x;
            saved_xAngle = xAngle;
            saved_y = y;
            saved_yAngle = yAngle;
            saved_z = z;
            saved_zAngle = zAngle;
        }
        public void RestoreSavedPosition()
        {
     		Animation.StartAnimation(new FloatAnimation(this, "x", saved_x, 0.5f));
            Animation.StartAnimation(new FloatAnimation(this, "y", saved_y, 0.5f));
            Animation.StartAnimation(new FloatAnimation(this, "z", saved_z, 0.5f));
            Animation.StartAnimation(new AngleAnimation(this, "xAngle", saved_xAngle, 0.5f));
            Animation.StartAnimation(new AngleAnimation(this, "yAngle", saved_yAngle, 0.5f));
            Animation.StartAnimation(new AngleAnimation(this, "zAngle", saved_zAngle, 0.5f));
        }
        
		public Rectangle<float> getProjectedBounds()
		{
			Matrix4 M = ModelMatrix * Controler.Transformations *
			            Magic.texturedShader.ModelViewMatrix *
						Magic.texturedShader.ProjectionMatrix;
			Rectangle<float> projR = Rectangle<float>.Zero;
			Point<float> topLeft, bottomRight;
			if (_isTapped) {
				topLeft = MagicCard.CardBounds.BottomLeft;
				bottomRight = MagicCard.CardBounds.TopRight;
			} else {
				topLeft = MagicCard.CardBounds.TopLeft;
				bottomRight = MagicCard.CardBounds.BottomRight;
			}
			
			Point<float> pt1 = glHelper.Project (topLeft, M, Magic.viewport [2], Magic.viewport [3]);
			Point<float> pt2 = glHelper.Project (bottomRight, M, Magic.viewport [2], Magic.viewport [3]);
			if (pt1 < pt2) {
				projR.TopLeft = pt1;
				projR.BottomRight = pt2;
			} else {
				projR.TopLeft = pt2;
				projR.BottomRight = pt1;
			}
			return projR;
		}
		public bool mouseIsIn(Point<float> m)
		{
			Rectangle<float> r = getProjectedBounds ();
				return r.ContainsOrIsEqual (m);
		}
		#endregion

		#region IRenderable implementation
		public void Render ()
		{
			Matrix4 mSave = Magic.texturedShader.ModelMatrix;            
			Magic.texturedShader.ModelMatrix = ModelMatrix * Magic.texturedShader.ModelMatrix;

			if (CardInstance.selectedCard == this)
				Magic.texturedShader.Color = SelectedColor;
			else if (Combating)
				Magic.texturedShader.Color = AttackingColor;
			else 
				Magic.texturedShader.Color = notSelectedColor;

			Model.Render();

			if (pointsTexture != 0 && CurrentGroup != null && !HasFocus)
			{				
				if (CurrentGroup.GroupName == CardGroupEnum.InPlay)
				{
					Matrix4 mO = Matrix4.Identity;
					GL.BindTexture(TextureTarget.Texture2D, pointsTexture);

						mO = Matrix4.CreateRotationX (Magic.FocusAngle) * Matrix4.CreateRotationZ (-Controler.zAngle);
					if (_isTapped)
						mO *= Matrix4.CreateRotationZ (MathHelper.PiOver2);

					mO *= Matrix4.CreateTranslation(0.25f, -0.6125f, 0.1f);

					Magic.texturedShader.ModelMatrix = mO * Magic.texturedShader.ModelMatrix;
					Magic.texturedShader.Color = go.Color.White;
					MagicCard.pointsMesh.Render(PrimitiveType.TriangleStrip);
				}
			}

			if (HasSummoningSickness) {			
				Magic.texturedShader.ModelMatrix = Matrix4.CreateTranslation (0, 0, 0.3f) * ModelMatrix * mSave ;
				GL.BindTexture (TextureTarget.Texture2D, Magic.wirlpoolShader.Texture);
				MagicCard.CardMesh.Render (PrimitiveType.TriangleStrip);
				GL.BindTexture (TextureTarget.Texture2D, 0);
			}


			Magic.texturedShader.ModelMatrix = mSave;
			Magic.texturedShader.Color = go.Color.White;

			if (arrows == null)
				return;

			renderArrow ();

		}

		#region Arrows
		vaoMesh arrows;
		public void updateArrows(){
			if (arrows!=null)
				arrows.Dispose ();
			arrows = null;

			float z = 1.0f;
			foreach (CardInstance ac in AttachedCards.Where(c=>c.Controler != this.Controler)) {
				arrows += new Arrow3d (
					Vector3.TransformPosition(ac.Position, ac.Controler.Transformations), 
					Vector3.TransformPosition(this.Position, this.Controler.Transformations),
					Vector3.UnitZ * z);
				z += 0.2f;
			}
		}
		void renderArrow(){
			Magic.arrowShader.Enable ();
			Magic.arrowShader.ProjectionMatrix = Magic.projection;
			Magic.arrowShader.ModelViewMatrix = Magic.modelview;
			Magic.arrowShader.ModelMatrix = Matrix4.Identity;
			GL.PointSize (2f);
			GL.Disable (EnableCap.CullFace);
			arrows.Render (PrimitiveType.TriangleStrip);
			GL.Enable (EnableCap.CullFace);
			Magic.arrowShader.Disable ();
		}
		#endregion

		public Matrix4 ModelMatrix {
			get
			{
				Matrix4 transformation;


				Matrix4 Rot = 
					Matrix4.CreateRotationX (xAngle) *
					Matrix4.CreateRotationY (yAngle) *
					Matrix4.CreateRotationZ (zAngle);

				//Matrix4 Rot = Matrix4.CreateRotationZ(zAngle);
				transformation = Rot * Matrix4.CreateTranslation(x, y, z);

				return transformation;
			}
			set {
				throw new NotImplementedException ();
			}
		}
		#endregion

		#region Overlay
		int pointsTexture = 0;
		public void UpdateOverlay2()
		{
			int width = 100;
			int height = 40;
			int x = 0;
			int y = 0;

			Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(x, y, width, height),
				ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			int stride = 4 * width;

			using (ImageSurface draw =
				new ImageSurface(data.Scan0, Format.Argb32, width, height, stride))
			{
				using (Context gr = new Context(draw))
				{
					go.Rectangle r = new go.Rectangle(0, 0, width, height);
					gr.Color = go.Color.White;
					gr.Rectangle(r);
					gr.FillPreserve();
					gr.Color = go.Color.Black;
					gr.LineWidth = 1.5f;
					gr.Stroke();

					gr.SelectFontFace("Times New Roman", FontSlant.Normal, FontWeight.Bold);
					gr.SetFontSize(40);
					gr.Color = go.Color.Black;

					string text = "Test";

					FontExtents fe = gr.FontExtents;
					TextExtents te = gr.TextExtents(text);
					double xt = width / 2 - te.Width / 2;
					double yt = height / 2 + te.Height / 2;

					gr.MoveTo(xt, yt);
					gr.ShowText(text);

					draw.Flush();
				}
				//draw.WriteToPng(@"d:/test.png");
			}

			imgHelpers.imgHelpers.flipY(data.Scan0, stride, height);

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, pointsTexture);
			GL.Enable(EnableCap.Texture2D);
			GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

			//bmp.Save(@"d:/test.png");
			bmp.UnlockBits(data);
		}
		public void UpdateOverlay()
		{
			if (!(this.HasType (CardTypes.Creature) && CurrentGroup.GroupName == CardGroupEnum.InPlay))
				return;
			
			int width = 100;
			int height = 40;
			int x = 0;
			int y = 0;

			Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(x, y, width, height),
				ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			int stride = 4 * width;

			using (ImageSurface draw =
				new ImageSurface(data.Scan0, Format.Argb32, width, height, stride))
			{
				using (Context gr = new Context(draw))
				{
					go.Rectangle r = new go.Rectangle(0, 0, width, height);

					if (Damages.Count == 0)
						gr.Color = go.Color.White;
					else
						gr.Color = go.Color.Red;

					gr.Rectangle(r);
					gr.FillPreserve();
					gr.Color = go.Color.Black;
					gr.LineWidth = 1.5f;
					gr.Stroke();

					gr.SelectFontFace("Times New Roman", FontSlant.Normal, FontWeight.Bold);
					gr.SetFontSize(40);
					gr.Color = go.Color.Black;

					string text = Power.ToString() + " / " + Toughness.ToString();

					FontExtents fe = gr.FontExtents;
					TextExtents te = gr.TextExtents(text);
					double xt = width / 2 - te.Width / 2;
					double yt = height / 2 + te.Height / 2;

					gr.MoveTo(xt, yt);
					gr.ShowText(text);

					draw.Flush();
				}
				//draw.WriteToPng(@"d:/test.png");
			}


			imgHelpers.imgHelpers.flipY(data.Scan0, stride, height);

			if (pointsTexture == 0)
			{
				GL.GenTextures(1, out pointsTexture);
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, pointsTexture);
				GL.Enable(EnableCap.Texture2D);
				//GLU.Build2DMipmap(OpenTK.Graphics.TextureTarget.Texture2D,
				//            (int)PixelInternalFormat.Rgba,
				//            width, height,
				//            OpenTK.Graphics.PixelFormat.Bgra,
				//            OpenTK.Graphics.PixelType.UnsignedByte, data.Scan0);

				//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
				//    (int)TextureMinFilter.NearestMipmapLinear);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
					OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
			}
			else
			{
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, pointsTexture);
				GL.Enable(EnableCap.Texture2D);
				GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height,
					OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
			}
			//bmp.Save(@"d:/test.png");
			bmp.UnlockBits(data);
		}
		public void ResetOverlay()
		{
			GL.DeleteTextures(1, ref pointsTexture);
		}

		#endregion

        public override string ToString()
        {
            return string.Format("{0} | {1} | {2}", Model.Name, Model.Types, Model.Cost);
        }
    }
}
