﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using Cairo;
using GGL;
using Crow;
using OpenTK;
using OpenTK.Graphics.OpenGL;

//using GLU = OpenTK.Graphics.Glu;

namespace Magic3D
{
    [Serializable]
	public class CardInstance : RenderedCardModel, IDamagable
    {
		static Random rnd = new Random();

		#region CardAnimEvent
		public class CardAnimEventArg : EventArgs
		{
			public CardInstance card;
			public CardAnimEventArg(CardInstance _card){
				card = _card;
			}
		}
		#endregion

		public string Edition;
		int _selectedTexture = -1;
		public MagicAction BindedAction = null;

		#region CTOR
		public CardInstance(MagicCard mc = null)
		{
			Model = mc;
		}
		public CardInstance(MagicAction _bindedAction = null)
		{
			BindedAction = _bindedAction;
			Model = _bindedAction.CardSource.Model;
		}
		#endregion

		public String Name {
			get { return Model.Name; }
		}

        #region selection
		/// <summary>
		/// The focused card zoomed in the middle of screen
		/// </summary>
		public static CardInstance focusedCard = null;
        public static CardInstance selectedCard;

		public static Crow.Color notSelectedColor = new Crow.Color(1.0f, 1.0f, 1.0f, 1f);
		public static Vector4 SelectedColor = new Vector4(1.2f, 1.2f, 1.2f, 1.2f);
		public static Crow.Color AttackingColor = new Crow.Color(1.0f, 0.8f, 0.8f, 1f);
        #endregion

		const float attachedCardsSpacing = 0.03f;


		bool _isTapped = false;
		bool _combating;
		Player _controler;

		Player _originalControler;
		        
        public CardGroup CurrentGroup;
		public bool HasFocus = false;
		public bool HasSummoningSickness = false;
		public bool IsToken = false;
		public bool Kicked = false;

		public IList<EffectGroup> PumpEffect = new List<EffectGroup> ();
		public IList<EffectGroup> Effects{
			get { return Model.SpellEffects.Concat(PumpEffect).ToList(); }
		}
//		public IList<Effect> ActiveEffects{
//			get { }
//		}
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

			Controler.InPlay.UpdateLayout ();
		}
		public void DetacheCard(CardInstance c)
		{
			c.AttachedTo = null;
			AttachedCards.Remove (c);

			updateArrows ();

			Controler.InPlay.UpdateLayout ();

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
            
            if (Toughness < 1)
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
			while (AttachedCards.Count > 0)
				this.DetacheCard (AttachedCards.First());			

            Reset();
			ChangeZone (CardGroupEnum.Graveyard);			
        }

		public void Reset(bool _positionReset = false)
        {
			if (_positionReset)
            	ResetPositionAndRotation();
            ResetOverlay();

			Damages.Clear();
            Combating = false;
			Kicked = false;
            if (BlockedCreature != null)
            {
                BlockedCreature.BlockingCreatures.Remove(this);
                BlockedCreature = null;
            }
            tappedWithoutEvent = false;
			HasSummoningSickness = false;

			if (IsAttached)				
				AttachedTo.DetacheCard (this);
        }

		public IEnumerable<Ability> getAllAbilities()
		{
			List<Ability> abs = new List<Ability>();

			foreach (CardInstance ci in MagicEngine.CurrentEngine.CardsInPlayHavingEffects) {
				bool valid = false;
				foreach (EffectGroup eg in ci.Effects) {
					foreach (CardTarget ct in eg.Affected.Values.OfType<CardTarget>()) {
						if (!ct.Accept (this, ci)) {
							valid = false;
							break;
						} else
							valid = true;
					}
					if (!valid)
						continue;

					foreach (AbilityEffect e in  eg.OfType<AbilityEffect>()) {
						switch (e.TypeOfEffect) {
						case EffectType.Gain:
							abs.Add (e.Ability);
							break;
						case EffectType.Loose:
							abs.RemoveAll (a => a.AbilityType == e.Ability.AbilityType);
							break;
						}
					}
				}
			}
			return Model.Abilities.Concat(abs);
		}
		public Ability[] getAbilitiesByType(AbilityEnum ae)
        {
			return getAllAbilities().Where (a => a.AbilityType == ae).ToArray();
        }
			        
        public bool HasEffect(EffectType et)
        {
			foreach (CardInstance ca in AttachedCards) {
				foreach (EffectGroup eg in ca.Effects) {
					foreach (Effect e in eg) {
						if (e.TypeOfEffect == et)
							return true;
					}
				}
			}
            return false;
        }
        public bool HasAbility(AbilityEnum ab)
        {
			return getAbilitiesByType (ab).Count () > 0;
        }
		public bool HasColor(ManaTypes color)
		{
			return Model.Colors == null ?
				Model.Cost.GetDominantMana() == color : 
				Model.Colors.Contains (color);
			//TODO:test with color gain or loose effects
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
		/// <summary>
		/// Determines whether this instance can block the specified CardInstance.
		/// </summary>
		/// <param name="blockedCard">Null value can be passed to check basic conditions for defenders</param>
		public bool CanBlock(CardInstance blockedCard = null)
        { 			
			if (_isTapped || !HasType (CardTypes.Creature))
                return false;
			
			if (HasEffect(EffectType.CantBlock))
				return false;

			if (blockedCard == null)
				return true;

            if ( blockedCard.HasAbility(AbilityEnum.Flying) && 
                ! (this.HasAbility(AbilityEnum.Flying) || this.HasAbility(AbilityEnum.Reach)))
                return false;
							
            return true;
        }
       
		//TODO: remove redundant function
        public bool IsTapped
        {
            get { return _isTapped; }
        }
        /// <summary> Tap card without raising MagicEvent, usefull when card enter battelfield tapped </summary>        
		public bool tappedWithoutEvent
        {
            get { return _isTapped; }
            set
            {
                if (value == _isTapped)
                    return;

                _isTapped = value;

				if (_isTapped)
					Animation.StartAnimation(new FloatAnimation(this, "zAngle", -MathHelper.PiOver2, MathHelper.Pi * 0.1f));
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
//            Effect e = Effects.Where(ef => ef.TypeOfEffect == EffectType.DoesNotUntap).LastOrDefault();
//
//            if (e == null)
                tappedWithoutEvent = false;
        }
		public void UpdatePowerAndToughness()
		{
			_power = int.MinValue;
			_toughness = int.MinValue;

			foreach (CardInstance ci in MagicEngine.CurrentEngine.CardsInPlayHavingEffects) {
				bool valid = false;
				foreach (EffectGroup eg in ci.Effects) {						
					foreach (CardTarget ct in eg.Affected.Values.OfType<CardTarget>()) {
						if (!ct.Accept (this, ci)) {
							valid = false;
							break;
						} else
							valid = true;
					}
					if (!valid)
						continue;
					foreach (NumericEffect e in  eg.OfType<NumericEffect>()) {
						switch (e.TypeOfEffect) {
						case EffectType.AddPower:
							if (_power == int.MinValue)
								_power = Model.Power;
							_power += e.Amount.GetValue(ci) * e.Multiplier;
							break;
						case EffectType.SetPower:
							if (_power == int.MinValue)
								_power = Model.Power;
							_power = e.Amount.GetValue(ci) * e.Multiplier;
							break;
						case EffectType.AddTouchness:
							if (_toughness == int.MinValue)
								_toughness = Model.Toughness;
							_toughness += e.Amount.GetValue (ci);
							break;
						case EffectType.SetTouchness:
							if (_toughness == int.MinValue)
								_toughness = Model.Toughness;
							_toughness = e.Amount.GetValue (ci);
							break;
						}
					}						
				}
			}
			int damages = 0;
			foreach (Damage d in Damages)
				damages += d.Amount;

			if (damages == 0)
				return;

			if (_toughness == int.MinValue)
				_toughness = Model.Toughness;
			_toughness -= damages;
		}
		public bool UpdateControler()
		{
			_controler = null;
			foreach (CardInstance ci in MagicEngine.CurrentEngine.CardsInPlayHavingEffect(EffectType.GainControl)) {
				bool valid = false;
				foreach (EffectGroup eg in ci.Effects) {						
					foreach (CardTarget ct in eg.Affected.Values.OfType<CardTarget>()) {
						if (!ct.Accept (this, ci)) {
							valid = false;
							break;
						} else
							valid = true;
					}
					if (!valid)
						continue;
					foreach (Effect e in  eg) {
						switch (e.TypeOfEffect) {
						case EffectType.GainControl:
							_controler = ci.Controler;
							break;
						}
					}						
				}
			}
			return _controler != null;
		}
		public Player Controler
		{
			get
			{
				return _controler ?? _originalControler;
			}
			set { _originalControler = value; }
		}
		int _power = int.MinValue;
		int _toughness = int.MinValue;
		public int 	Power
        {
            get
            {
				return _power == int.MinValue ? Model.Power : _power;
            }
        }
        public int Toughness
        {
            get
            {
				return _toughness == int.MinValue ? Model.Toughness : _toughness;
            }
        }

		#region layouting
		//TODO: create function for attached card position update instead of
		//		copying again and again the same code
        public override float x
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
					if (Math.Abs (a - ac.x) > 1.0)
						Animation.StartAnimation (new FloatAnimation (ac, "x", a, 0.2f));
					else
						ac.x = a;
				}
			}
        }
        public override float y
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
					if (Math.Abs (a - ac.y) > 1.0)
						Animation.StartAnimation (new FloatAnimation (ac, "y", a, 0.2f));
					else
						ac.y = a;
				}					
			}        
		}
        public override float z
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
					if (Math.Abs (a - ac.z) > 1.0)
						Animation.StartAnimation (new FloatAnimation (ac, "z", a, 0.2f));
					else
						ac.z = a;
				}
				//updateArrows ();
			}        
		}
		public override float xAngle
        {
            get { return _xAngle; }
            set {
				if (_xAngle == value)
					return;
				
				_xAngle = value; 

				foreach (CardInstance ac in AttachedCards) {
					if (ac.Controler != Controler)
						continue;
				
					if (Math.Abs (_xAngle - ac.xAngle) > 1.0)
						Animation.StartAnimation (new FloatAnimation (ac, "z", _xAngle, 0.2f));
					else
						ac.xAngle = _xAngle;
				}
			}
        }
		public override float yAngle
        {
            get { return _yAngle; }
            set { _yAngle = value; }
        }
		public override float zAngle
        {
            get { return _zAngle; }
            set { _zAngle = value; }
        }
		public override float Scale {
			get {
				return _scale;
			}
			set {
				_scale = value;
			}
		}

        public float saved_x = 0.0f;
        public float saved_y = 0.0f;
        public float saved_z = 0.0f;
        public float saved_xAngle = 0.0f;
        public float saved_yAngle = 0.0f;
        public float saved_zAngle = 0.0f;
		public float saved_scale = 0.0f;


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
				Animation.StartAnimation (new FloatAnimation (this, "Scale", 1.0f, 0.05f));


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
			saved_scale = Scale;
        }
        public void RestoreSavedPosition()
        {
     		Animation.StartAnimation(new FloatAnimation(this, "x", saved_x, 0.5f));
            Animation.StartAnimation(new FloatAnimation(this, "y", saved_y, 0.5f));
            Animation.StartAnimation(new FloatAnimation(this, "z", saved_z, 0.5f));
            Animation.StartAnimation(new AngleAnimation(this, "xAngle", saved_xAngle, 0.5f));
            Animation.StartAnimation(new AngleAnimation(this, "yAngle", saved_yAngle, 0.5f));
            Animation.StartAnimation(new AngleAnimation(this, "zAngle", saved_zAngle, 0.5f));
			Animation.StartAnimation (new FloatAnimation (this, "Scale", saved_scale, 0.05f));
        }
        
		public Rectangle<float> getProjectedBounds()
		{
			Matrix4 M = ModelMatrix * Controler.Transformations *
			            Magic.texturedShader.ModelViewMatrix *
						Magic.texturedShader.ProjectionMatrix;
			Rectangle<float> projR = Rectangle<float>.Zero;
			Point<float> topLeft, bottomRight;
			if (_isTapped) {
				topLeft = MagicData.CardBounds.BottomLeft;
				bottomRight = MagicData.CardBounds.TopRight;
			} else {
				topLeft = MagicData.CardBounds.TopLeft;
				bottomRight = MagicData.CardBounds.BottomRight;
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
		public override void Render ()
		{
			Matrix4 mSave = Magic.texturedShader.ModelMatrix;            
			Magic.texturedShader.ModelMatrix = ModelMatrix * Magic.texturedShader.ModelMatrix;

			if (CardInstance.selectedCard == this)
				Magic.texturedShader.Color = SelectedColor;
			else if (Combating)
				Magic.texturedShader.Color = AttackingColor;
			else 
				Magic.texturedShader.Color = notSelectedColor;

			if (_selectedTexture < 0) {
				if (Model.nbrImg > 1)
					_selectedTexture = rnd.Next (Model.nbrImg);
				else
					_selectedTexture = 0;
			}
			Model.Render(Edition, _selectedTexture);

			if (pointsTexture != 0 && CurrentGroup != null && !HasFocus)
			{				
				if (CurrentGroup.GroupName == CardGroupEnum.InPlay)
				{
					Magic.texturedShader.Color = Crow.Color.White;

					Matrix4 mMod = Magic.texturedShader.ModelMatrix;
					Matrix4 mO = Matrix4.CreateRotationX (Magic.FocusAngle) * Matrix4.CreateRotationZ (-Controler.zAngle);

					if (_isTapped)
						mO *= Matrix4.CreateRotationZ (MathHelper.PiOver2);

					Magic.texturedShader.ModelMatrix = 
						mO * Matrix4.CreateTranslation(-0.02f, 0.55f, 0.1f) * mMod;
					GL.BindTexture(TextureTarget.Texture2D, abilitiesTexture);
					MagicData.AbilityMesh.Render(PrimitiveType.TriangleStrip);

					Magic.texturedShader.ModelMatrix = 
						mO * Matrix4.CreateTranslation(0.27f, -0.7f, 0.1f) * mMod;
					GL.BindTexture(TextureTarget.Texture2D, pointsTexture);
					MagicData.PointsMesh.Render(PrimitiveType.TriangleStrip);

				}
			}

			if (HasSummoningSickness) {			
				Magic.texturedShader.ModelMatrix = Matrix4.CreateTranslation (0, 0, 0.3f) * ModelMatrix * mSave ;
				GL.BindTexture (TextureTarget.Texture2D, Magic.wirlpoolShader.Texture);
				MagicData.CardMesh.Render (PrimitiveType.TriangleStrip);
				GL.BindTexture (TextureTarget.Texture2D, 0);
			}


			Magic.texturedShader.ModelMatrix = mSave;
			Magic.texturedShader.Color = Crow.Color.White;

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
			if (AttachedTo != null)
				AttachedTo.updateArrows ();
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

		#endregion

		#region Overlay
		int pointsTexture = 0;
		int abilitiesTexture = 0;
		public void UpdateOverlayAbilities()
		{
			int width = 200;
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
//					Crow.Rectangle r = new Crow.Rectangle(0, 0, width, height);
//					gr.Color = Crow.Color.White;
//					gr.Rectangle(r);
//					gr.Fill();


					//
					gr.Scale (0.45, 0.45);

					foreach (AbilityEnum ae in getAllAbilities ().Select (a => a.AbilityType).Distinct ()) {
						MagicData.hSVGsymbols.RenderCairoSub (gr, "#" + ae.ToString());
						gr.Translate (100, 0);
					}


					draw.Flush();
				}
				//draw.WriteToPng(@"d:/test.png");
			}
			imgHelpers.imgHelpers.flipY(data.Scan0, stride, height);

			if (abilitiesTexture == 0)
			{
				GL.GenTextures(1, out abilitiesTexture);
				GL.BindTexture(TextureTarget.Texture2D, abilitiesTexture);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
					OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
//				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
//				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
			}
			else
			{				
				GL.BindTexture(TextureTarget.Texture2D, abilitiesTexture);
				GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height,
					OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
			}
			GL.BindTexture (TextureTarget.Texture2D, 0);
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
					Crow.Rectangle r = new Crow.Rectangle(0, 0, width, height);

					if (Damages.Count == 0)
						gr.SetSourceColor(Crow.Color.White);
					else
						gr.SetSourceColor(Crow.Color.Red);

					gr.Rectangle(r);
					gr.FillPreserve();
					gr.SetSourceColor(Crow.Color.Black);
					gr.LineWidth = 1.5f;
					gr.Stroke();

					gr.SelectFontFace("Times New Roman", FontSlant.Normal, FontWeight.Bold);
					gr.SetFontSize(40);
					gr.SetSourceColor(Crow.Color.Black);

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
				GL.BindTexture(TextureTarget.Texture2D, pointsTexture);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
					OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
//				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
//				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
			}
			else
			{				
				GL.BindTexture(TextureTarget.Texture2D, pointsTexture);
				GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height,
					OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
			}
			GL.BindTexture (TextureTarget.Texture2D, 0);
			//bmp.Save(@"d:/test.png");
			bmp.UnlockBits(data);

			UpdateOverlayAbilities ();
		}
		public void ResetOverlay()
		{
			if (GL.IsTexture (pointsTexture))
				GL.DeleteTexture (pointsTexture);
			if (GL.IsTexture (abilitiesTexture))
				GL.DeleteTexture (abilitiesTexture);
			pointsTexture = abilitiesTexture = 0;
		}

		#endregion

        public override string ToString()
        {
            return string.Format("{0} | {1} | {2}", Model.Name, Model.Types, Model.Cost);
        }
    }
}
