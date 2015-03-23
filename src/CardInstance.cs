using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using go;
using GGL;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Cairo;

using GLU = OpenTK.Graphics.Glu;

namespace Magic3D
{
    [Serializable]
	public class CardInstance : IDamagable, IRenderable
    {
		public class CardAnimEventArg : EventArgs
		{
			public CardInstance card;
			public CardAnimEventArg(CardInstance _card){
				card = _card;
			}
		}

		#region CTOR
		public CardInstance(MagicCard mc)
		{
			Model = mc;

			if (Model.Types != CardTypes.Creature)
				return;

			UpdateOverlay();
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
		public static go.Color SicknessColor = new go.Color(0.8f, 0.7f, 1f, 1f);
        #endregion


		bool _isTapped = false;
		bool _combating;
		Player _controler;

        public MagicCard Model;
        public CardGroup CurrentGroup;
        public bool HasFocus = false;

        public EffectList Effects = new EffectList();
        public List<CardInstance> AttachedCards = new List<CardInstance>();
        public List<CardInstance> BlockingCreatures = new List<CardInstance>();
        public List<Damage> Damages = new List<Damage>();
        public CardInstance BlockedCreature = null;
        public bool IsAttached = false;
		public bool HasSummoningSickness = false;

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

        public void PutIntoGraveyard()
        {
            Reset();
            CurrentGroup.RemoveCard(this);
            Controler.Graveyard.AddCard(this);
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
            tapped = false;        
        }

        public Ability AttachAb
        {
            get { return Model.Abilities.Where(a => a.AbilityType == AbilityEnum.Attach).FirstOrDefault(); }
        }

		public Ability[] getAbilitiesByType(AbilityEnum ae)
        {
			return Model.Abilities.Where (a => a.AbilityType == ae).ToArray();
        }
			        

        public bool HasEffect(EffectType et)
        {
            foreach (Effect e in Effects)
            {
                if (e.TypeOfEffect == et)
                    return true;
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
				if (_isTapped||HasSummoningSickness)
                    return false;

                if (getAbilitiesByType(AbilityEnum.Defender) != null)
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


            return true;
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

		//TODO: remove redundant function
        public bool IsTapped
        {
            get { return _isTapped; }
        }
        bool tapped
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
            tapped = true;
            MagicEngine.CurrentEngine.RaiseMagicEvent(new MagicEventArg(MagicEventType.TapCard, this));
        }
        public void Untap()
        {
            Effect e = Effects.Where(ef => ef.TypeOfEffect == EffectType.DoesNotUntap).LastOrDefault();

            if (e == null)
                tapped = false;
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
            set { _x = value; }
        }
        public float y
        {
            get { return _y; }
            set { _y = value; }
        }
        public float z
        {
            get { return _z; }
            set { _z = value; }
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
			Point<float> pt1 = glHelper.Project (MagicCard.CardBounds.TopLeft, M, Magic.viewport [2], Magic.viewport [3]);
			Point<float> pt2 = glHelper.Project (MagicCard.CardBounds.BottomRight, M, Magic.viewport [2], Magic.viewport [3]);
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
			if (CardInstance.selectedCard == this)
				Magic.texturedShader.Color = SelectedColor;
			else if (Combating)
				Magic.texturedShader.Color = go.Color.Red;
			else if (HasSummoningSickness)
				Magic.texturedShader.Color = SicknessColor;
			else
				Magic.texturedShader.Color = notSelectedColor;

			Matrix4 mSave = Magic.texturedShader.ModelMatrix;            
			Magic.texturedShader.ModelMatrix = ModelMatrix * Magic.texturedShader.ModelMatrix;

			Model.Render();

			if (pointsTexture != 0 && CurrentGroup != null)
			{
				if (CurrentGroup.GroupName == CardGroupEnum.InPlay)
				{
					Matrix4 mO = Matrix4.Identity;
					GL.BindTexture(TextureTarget.Texture2D, pointsTexture);
					if (!HasFocus)
						mO = Matrix4.CreateRotationZ (-Controler.zAngle);

					mO *= Matrix4.CreateTranslation(0.25f, -0.6125f, 0f);
					Magic.texturedShader.ModelMatrix = mO * Magic.texturedShader.ModelMatrix;

					MagicCard.pointsMesh.Render(PrimitiveType.TriangleStrip);
				}
			}
			Magic.texturedShader.ModelMatrix = mSave;
			Magic.texturedShader.Color = go.Color.White;

		}

		public Matrix4 ModelMatrix {
			get
			{
				Matrix4 transformation;


				Matrix4 Rot = Matrix4.CreateRotationX(xAngle);
				Rot *= Matrix4.CreateRotationY(yAngle);
				Rot *= Matrix4.CreateRotationZ(zAngle);
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

        public bool IsInLibrary
        {
            get { return CurrentGroup.GroupName == CardGroupEnum.Library ? true : false; }
        }
			

        public override string ToString()
        {
            return string.Format("{0} | {1} | {2}", Model.Name, Model.Types, Model.Cost);
        }
    }
}
