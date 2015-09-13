#define MONO_CAIRO_DEBUG_DISPOSE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using GGL;
using go;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
//using GLU = OpenTK.Graphics.Glu;
using System.Linq;

namespace Magic3D
{
	class Magic : OpenTKGameWindow, IValueChange
	{
		#region IValueChange implementation

		public event EventHandler<ValueChangeEventArgs> ValueChanged;
		void notifyValueChange(string propName, object newValue)
		{
			ValueChanged.Raise(this, new ValueChangeEventArgs (propName, newValue));
		}
		#endregion

		#region FPS
		int _fps = 0;

		public int fps {
			get { return _fps; }
			set {
				if (_fps == value)
					return;

				_fps = value;

				if (_fps > fpsMax) {
					fpsMax = _fps;
					ValueChanged.Raise(this, new ValueChangeEventArgs ("fpsMax", fpsMax));
				} else if (_fps < fpsMin) {
					fpsMin = _fps;
					ValueChanged.Raise(this, new ValueChangeEventArgs ("fpsMin", fpsMin));
				}

				ValueChanged.Raise(this, new ValueChangeEventArgs ("fps", _fps));
				ValueChanged.Raise (this, new ValueChangeEventArgs ("update",
					this.updateTime.ElapsedMilliseconds.ToString () + " ms"));
			}
		}

		public int fpsMin = int.MaxValue;
		public int fpsMax = 0;

		void resetFps ()
		{
			fpsMin = int.MaxValue;
			fpsMax = 0;
			_fps = 0;
		}
		public string update = "";
		#endregion


		public Magic ()
			: base(1024, 800)
		{}

		public static Magic CurrentGameWin;

		#region  scene matrix and vectors
		public static Matrix4 modelview;
		public static Matrix4 projection;
		public static int[] viewport = new int[4];

		//public static Vector3 vEye = new Vector3(150.0f, 50.0f, 1.5f);    // Camera Position
		public static Vector3 vEye = new Vector3(0.0f, -13.0f, 7.5f);    // Camera Position
		public static Vector3 vEyeTarget;// = new Vector3(40f, 50f, 0.1f);
		public static Vector3 vLook = new Vector3(0f, 1f, -0.7f);  // Camera vLook Vector
		public static Vector4 vLight = new Vector4 (0.0f, -15.0f, 25.0f, 0.0f);
		public static Vector3 vMouse = Vector3.Zero;

		float _zFar = 1280.0f;

		public float zFar {
			get { return _zFar; }
			set {
				_zFar = value;
			}
		}

		public float zNear = 0.001f;
		public float fovY = (float)Math.PI / 4;

		float MoveSpeed = 1.0f;
		float RotationSpeed = 0.02f;

		public static Vector3 vFocusedPoint
		{
			get { return vEye + 2 * vLook; }
		}
		public static Vector3 vGroupedFocusedPoint
		{
			get { return vEye + 5f * vLook; }
		}
		public static float FocusAngle
		{
			get { return  MathHelper.Pi - Vector3.CalculateAngle (Magic.vLook, Vector3.UnitZ); }
		}
		#endregion

		public static string dataPath = "/mnt/data2/downloads/forge-gui-desktop-1.5.31/res/";
		public static string deckPath = dataPath + "quest/precons/";

		public Deck[] deckList;

		public Deck[] DeckList {
			get {
				return deckList;
			}
			set {
				deckList = value;
				notifyValueChange ("DeckList", deckList);
			}
		}
		string[] cardList;
		public string[] CardList {
			get { return cardList; }
			set {
				cardList = value;
				notifyValueChange ("CardList", deckList);
			}			
		}
		
		public static GameLib.SingleLightSimpleShader texturedShader;
		public static GameLib.GlowShader glowShader;
		public static GameLib.EffectShader wirlpoolShader;
		public static GameLib.EffectShader arrowShader;

		public static Color activeColor = Color.White;

		public static int abstractTex;

		public static List<IRenderable> Renderables = new List<IRenderable> ();
		static List<IAnimatable> Animatables = new List<IAnimatable> ();
		public static void AddAnimation(IAnimatable a){
			a.AnimationFinished += onAnimFinished;
			a.Clone ();
			Animatables.Add (a);
		}
		static void onAnimFinished(object sender, EventArgs e)
		{
			Animatables.Remove (sender as IAnimatable);
		}
		Coin coin;	//coin object for the toss

		void drawScene()
		{
			texturedShader.Enable ();
			//shader.LineWidth = lineWidth;
			texturedShader.Color = activeColor;

			texturedShader.LightPos = vLight;
			texturedShader.ProjectionMatrix = projection;
			texturedShader.ModelViewMatrix = modelview;
			texturedShader.ModelMatrix = Matrix4.Identity;

			drawTable ();


			if (engine != null)
				engine.processRendering ();

			//renderDice ();
			int i = 0;
			while (i < Renderables.Count) {
				Renderables [i].Render ();
				i++;
			}


		}
			
		#region table
		vaoMesh table;
		public static int tableTexture;
		public void initTableModel()
		{
			tableTexture = new Texture(@"images/marble1.jpg");

			const float _width = 16f;
			const float _height = 14f;
			const float texTileX = 1f;
			const float texTileY = 2f;
			const float z = -0.1f;

			table = new vaoMesh (0, 0, z, _width, _height, texTileX, texTileY);
		}
		void drawTable()
		{
			GL.BindTexture(TextureTarget.Texture2D, tableTexture);
			table.Render(PrimitiveType.TriangleStrip);
		}
		#endregion
		#region dice
		vaoMesh dice;
		public static Matrix4 diceMat = Matrix4.Identity;
		int diceTex;
		void initDice()
		{
			dice = vaoMesh.Load (@"meshes/d20-s2.obj");
			diceTex = new Texture (@"images/d20.png");
		}
		void renderDice()
		{
			Magic.texturedShader.ModelMatrix = diceMat;

			GL.BindTexture (TextureTarget.Texture2D, diceTex);
			dice.Render (PrimitiveType.Triangles);
			GL.BindTexture (TextureTarget.Texture2D, 0);
		}
		#endregion

		void initArrow()
		{
			arrowShader = new GameLib.EffectShader ("GGL.Shaders.GameLib.red");

		}
			


		#region interface
		public Player[] Players;
		MagicEngine engine;
		go.Container g;
		GraphicObject uiPhases, wCardText, uiMainMenu;
		MessageBoxYesNo msgBox;
		go.Label txtCard;

		void initInterface(){
			uiMainMenu = LoadInterface("#Magic3D.ui.mainMenu.goml");
			InitLogPanel ();
			//LoadInterface("#Magic3D.ui.fps.goml");
			uiPhases = LoadInterface("#Magic3D.ui.phases.goml");
			wCardText = LoadInterface ("ui/text.goml");
			txtCard = wCardText.FindByName ("txtCard") as Label;
			wCardText.Visible = false;

			//special event handlers fired only if mouse not in interface objects
			//for scene mouse handling
			this.MouseWheelChanged += new EventHandler<MouseWheelEventArgs>(Mouse_WheelChanged);
			this.MouseMove += new EventHandler<MouseMoveEventArgs>(Mouse_Move);
			this.MouseButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_Down);;
		}

		void onButExit_MouseClick (object sender, MouseButtonEventArgs e)
		{
			Exit ();			
		}
		Group hsDeck;

		void onStartNewGame(Object sender, MouseButtonEventArgs e)
		{
			uiMainMenu.Visible = false;
			Players = new Player[] 
			{ 
				new Player("player 1","Mystical Might.dck"), //"Kor Armory.dck"
				new AiPlayer("player 2","Lightforce.dck")
			};
			Players [0].playerPanel.HorizontalAlignment = HorizontalAlignment.Left;
			Players [1].playerPanel.HorizontalAlignment = HorizontalAlignment.Right;
			Players[1].zAngle = MathHelper.Pi;

			engine = new MagicEngine (Players);
			MagicEngine.MagicEvent += new Magic3D.MagicEngine.MagicEventHandler(MagicEngine_MagicEvent);

			#if DEBUG
			engine.currentPlayerIndex = engine.interfacePlayer;
			engine.ip.CurrentState = Player.PlayerStates.InitialDraw;
			engine.ip.Opponent.CurrentState = Player.PlayerStates.InitialDraw;
			#else
			coin = new Coin ();
			Renderables.Add (coin);
			AddAnimation (coin);
			coin.AnimationFinished += onTossResult;
			#endif

			btOk.Visible = false;
		}
		void onShowDecks (object sender, MouseButtonEventArgs e)
		{						
			hsDeck = LoadInterface ("#Magic3D.ui.decks.goml").FindByName("hsDeck") as Group;
		}
		void onShowCards (object sender, MouseButtonEventArgs e)
		{						
			LoadInterface ("#Magic3D.ui.Cards.goml");
		}
		void onDeckListValueChange (object sender, ValueChangeEventArgs e)
		{			
			if (e.MemberName != "SelectedItem")
				return;
			hsDeck.Children.Clear ();
			hsDeck.addChild(Interface.Load ("#Magic3D.ui.DeckDetails.goml", e.NewValue));
		}
		void onCardListValueChange (object sender, ValueChangeEventArgs e)
		{
			MagicCard c = null;
			MagicData.TryGetCardFromZip ((e.NewValue as MainLine).name, ref c);
			if (hsDeck.Children.Count > 1)
				hsDeck.Children.RemoveAt (hsDeck.Children.Count - 1);
			hsDeck.addChild(Interface.Load ("#Magic3D.ui.CardDetails.goml",new CardVisitor(c)));
		}

		#region LogPanel
		static GraphicObject uiLogs;
		public static go.Button btOk;

		static Label[] llogs = new Label[4];
		static List<String> logBuffer = new List<string> ();
		static int logBuffPtr = 0;

		public void InitLogPanel()
		{
			uiLogs = LoadInterface("ui/log.xml");
			uiLogs.MouseWheelChanged += onLogsWheel ;
			for (int i = 0; i < 4; i++) {
				llogs [i] = uiLogs.FindByName ("line" + (i + 1)) as Label;
			}
			btOk = uiLogs.FindByName ("btOk") as Button;
			//btOk.Visible = false;
		}

		void BtOk_MouseClick (object sender, MouseButtonEventArgs e)
		{			
			if (engine.NextActionOnStack != null) {
				engine.NextActionOnStack.Validate ();
			}
				
			//MagicEngine.CurrentEngine.CancelLastActionOnStack ();
			btOk.Visible = false;			
		}
		public static void AddLog(string msg)
		{
			foreach (string s in msg.Split('\n')) {
				logBuffer.Add (s);
			}
			logBuffPtr = 0;
			syncLogUi ();
		}
		static void syncLogUi()
		{
			for (int i = 0; i < 4; i++) {
				int ptr = logBuffer.Count - (i + 1 + logBuffPtr);
				if (ptr < 0)
					break;
				llogs[3-i].Text = logBuffer[ptr];
			}
		}
		void onLogsWheel(object sender, MouseWheelEventArgs e){
			logBuffPtr += e.Delta;
			int limUp = logBuffer.Count - 4;
			if (logBuffPtr > limUp)
				logBuffPtr = limUp;
			if (logBuffPtr < 0)
				logBuffPtr = 0;
			syncLogUi ();
		}
		#endregion

		#endregion
		void CloseCurrentGame(){
			engine = null;
			foreach (Player p in Players) {
				this.DeleteWidget (p.playerPanel);
			}
			Players = null;
			uiMainMenu.Visible = true;
		}

		void onTossResult(object sender, EventArgs e)
		{
			Coin.TossEventArg tea = e as Coin.TossEventArg;

			AddLog ("Toss result: " + tea.Result.ToString ());

			if (tea.Result == Coin.TossResultEnum.Head) {
				engine.ip.CurrentState = Player.PlayerStates.PlayDrawChoice;
				engine.ip.Opponent.CurrentState = Player.PlayerStates.InitialDraw;
				msgBox = new MessageBoxYesNo ("You won the toss, what will you do ?");
				msgBox.btOk.MouseClick += OnPlayFirst;
				msgBox.btOk.Text = "Play First";
				msgBox.btOk.Fit = true;
				msgBox.btCancel.MouseClick += onDrawFirst;
				msgBox.btCancel.Text = "Draw First";
				msgBox.btCancel.Fit = true;

				this.AddWidget (msgBox);
			} else {
				Renderables.Remove (coin);
				engine.ip.CurrentState = Player.PlayerStates.InitialDraw;
				engine.ip.Opponent.CurrentState = Player.PlayerStates.PlayDrawChoice;
			}
		}
		void OnPlayFirst(Object sender, MouseButtonEventArgs e)
		{
			Renderables.Remove (coin);
			DeleteWidget (msgBox);
			engine.currentPlayerIndex = engine.interfacePlayer;
			engine.ip.CurrentState = Player.PlayerStates.InitialDraw;
			//engine.State = EngineStates.PlayDrawChoiceDone;
			//engine.StartGame();
		}
		void onDrawFirst(Object sender, MouseButtonEventArgs e)
		{
			Renderables.Remove (coin);
			//Interface.UnloadPanel(sender.panel);
			DeleteWidget (msgBox);
			engine.currentPlayerIndex = engine.getPlayerIndex(engine.ip.Opponent);
			engine.ip.CurrentState = Player.PlayerStates.InitialDraw;
			//engine.State = EngineStates.PlayDrawChoiceDone;
			//engine.StartGame();
		}
			
		void MagicEngine_MagicEvent(MagicEventArg arg)
		{
			Border b;

			AddLog (arg.ToString ());

			switch (arg.Type)
			{
			case MagicEventType.PlayerHasLost:
				CloseCurrentGame ();
				break;
			case MagicEventType.Unset:
				break;
			case MagicEventType.BeginPhase:
				b = uiPhases.FindByName 
					((arg as PhaseEventArg).Phase.ToString ()) as Border;
				if (b!=null)
					b.BorderColor = Color.White;				
				break;
			case MagicEventType.EndPhase:
				b = uiPhases.FindByName 
					((arg as PhaseEventArg).Phase.ToString ()) as Border;
				if (b!=null)
					b.BorderColor = Color.Transparent;
				break;
			case MagicEventType.PlayLand:
				break;
			case MagicEventType.CastSpell:
				break;
			case MagicEventType.TapCard:
				break;
			case MagicEventType.ChangeZone:
				break;
			default:
				break;
			}
		}
						
		int frameCpt = 0;
		public static float time = 0f;

		protected override void OnLoad (EventArgs e)
		{			
			base.OnLoad (e);

			CurrentGameWin = this;

			loadPreconstructedDecks ();
			loadCardList ();

			AddWidget (new MessageBox () );

			initInterface ();

			#region init GL
			MagicData.InitCardModel();
			//MagicCard.LoadCardDatabase();
			Edition.LoadEditionsDatabase();
			//Deck.LoadPreconstructedDecks();
			//initLights ();

			GL.ClearColor(0.0f, 0.0f, 0.2f, 1.0f);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);
			GL.Enable(EnableCap.CullFace);
			GL.PrimitiveRestartIndex (int.MaxValue);
			GL.Enable (EnableCap.PrimitiveRestart);
//			GL.Enable (EnableCap.PointSprite);
//			GL.Enable(EnableCap.VertexProgramPointSize);
//			GL.PointParameter(PointSpriteCoordOriginParameter.LowerLeft);
			GL.Enable (EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
//			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
//			GL.ShadeModel(ShadingModel.Smooth);
//			GL.Hint (HintTarget.LineSmoothHint, HintMode.Nicest);

			abstractTex = new Texture(@"images/abstract1.jpg");

			texturedShader = new GameLib.SingleLightSimpleShader ();
			glowShader = new GameLib.GlowShader ();
			wirlpoolShader = new GameLib.EffectShader ("GGL.Shaders.GameLib.wirlpool2",256,256);

			initArrow ();
			initTableModel ();
			initDice ();

			GL.ActiveTexture (TextureUnit.Texture0);
			ErrorCode err = GL.GetError ();
			Debug.Assert (err == ErrorCode.NoError, "OpenGL Error");
			#endregion

			//engine.StartNewGame();
			//this.AddWidget (new MessageBox ("Play first?"));
			//this.CursorVisible = false;
		}

		void loadPreconstructedDecks()
		{
			string[] editions = Directory.GetFiles(Magic.deckPath, "*.dck");
			            
			List<Deck> tmpList = new List<Deck> ();
			int i = 0;
            foreach (string f in editions)
            {
				tmpList.Add(Deck.PreLoadDeck (f));
				i++;
				if (i > 50)
					break;
            }
			deckList = tmpList.ToArray ();
		}
		void loadCardList()
		{
			CardList = MagicData.GetCardDataFileNames ();
		}
		protected override void OnUpdateFrame (FrameEventArgs e)
		{
			time += (float)e.Time;

			base.OnUpdateFrame (e);

			fps = (int)RenderFrequency;
			if (frameCpt > 200) {
				resetFps ();
				frameCpt = 0;
			}
			frameCpt++;

			int i = 0;
			while(i < Animatables.Count)
			{
				Animatables[i].Animate((float)e.Time);
				i++;
			}
				
			if (Keyboard [Key.ControlLeft]) {
				//light movment
				if (Keyboard [Key.Up])
					vLight += new Vector4 (MoveSpeed * vLookDirOnXYPlane, 0);
				else if (Keyboard [Key.Down])
					vLight -= new Vector4 (MoveSpeed * vLookDirOnXYPlane, 0);
				else if (Keyboard [Key.Left])
					vLight -= new Vector4 (MoveSpeed * vLookPerpendicularOnXYPlane, 0);
				else if (Keyboard [Key.Right])
					vLight += new Vector4 (MoveSpeed * vLookPerpendicularOnXYPlane, 0);
				else if (Keyboard [Key.PageUp])
					vLight.Z += MoveSpeed * .5f;
				else if (Keyboard [Key.PageDown])
					vLight.Z -= MoveSpeed * .5f;

				texturedShader.LightPos = vLight;
			} else if (Keyboard [Key.ShiftLeft]) {
				if (Keyboard [Key.Up])
					vEye.Y += MoveSpeed * .5f;
				else if (Keyboard [Key.Down])
					vEye.Y -= MoveSpeed * .5f;
				else if (Keyboard [Key.PageUp])
					vEye.Z += MoveSpeed * .5f;
				else if (Keyboard [Key.PageDown])
					vEye.Z -= MoveSpeed * .5f;
				
				UpdateViewMatrix ();
			} 
//			else if (Keyboard [Key.AltLeft]) {
//				if (Keyboard [Key.Up])
//					engine.ip.Hand.y += MoveSpeed * .5f;
//				else if (Keyboard [Key.Down])
//					engine.ip.Hand.y -= MoveSpeed * .5f;
//				else if (Keyboard [Key.PageUp])
//					engine.ip.Hand.z += MoveSpeed * .5f;
//				else if (Keyboard [Key.PageDown])
//					engine.ip.Hand.z -= MoveSpeed * .5f;
//
//				engine.ip.Hand.UpdateLayout ();
//			}


			if (engine == null)
				return;

			engine.Process ();

			//TODO:disable update if wirlpoolTexture not binded
			wirlpoolShader.Update (time);

			Rectangle r = this.ClientRectangle;
			GL.Viewport( r.X, r.Y, r.Width, r.Height);
		}
		public override void OnRender (FrameEventArgs e)
		{
			drawScene();
		}
		public override void GLClear ()
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
			UpdateViewMatrix();
		}
		protected override void OnKeyDown (KeyboardKeyEventArgs e)
		{
			switch (e.Key) {
			case Key.Space:
				Players [0].DrawOneCard ();
				break;
			case Key.KeypadEnter:
				engine.ip.PhaseDone = true;
				if (engine.pp == engine.ip && engine.cp != engine.pp)
					engine.GivePriorityToNextPlayer ();
				break;
			case Key.Escape:
				CloseCurrentGame ();
				break;
			case Key.H:				
				if (wCardText.Visible) {
					wCardText.Visible = false;
					break;
				}
				if (CardInstance.selectedCard == null)
					return;
					
				wCardText.Visible = true;
				break;
			case Key.O:
				engine.ip.Opponent.Hand.RevealToUIPlayer();
				break;
			case Key.T:
				//startTossing ();
				break;
			case Key.U:
				if (CardInstance.selectedCard == null)
					return;
				CardInstance.selectedCard.tappedWithoutEvent = false;			
				break;
			case Key.L:
				engine.ip.Library.toogleShowAll ();
				break;
			case Key.R:
				foreach (CardInstance ci in engine.Players.SelectMany(p => p.InPlay.Cards.Where(c => c.HasType(CardTypes.Creature))))
					ci.UpdateOverlay ();
				
				break;
			case Key.Delete:
				if (CardInstance.selectedCard == null)
					return;
				CardInstance.selectedCard.Reset ();
				CardInstance.selectedCard.ChangeZone(CardGroupEnum.Hand);			
				break;
			case Key.KeypadPlus:
				engine.ip.AllowedLandsToBePlayed++;
				break;
			}
		}

		#region Mouse Handling
		void Object_Mouse_Move(object sender, MouseMoveEventArgs e){
			if (activeWidget == null)
				return;

			activeWidget.registerClipRect ();
			if (activeWidget!=null) {
				activeWidget.Left += e.XDelta;
				activeWidget.Top += e.YDelta;
			}

		}
		void Mouse_Move(object sender, MouseMoveEventArgs e)
		{
			if (e.XDelta != 0 || e.YDelta != 0)
			{
				if (e.Mouse.MiddleButton == OpenTK.Input.ButtonState.Pressed) {
					Matrix4 m = Matrix4.CreateRotationX (-e.YDelta * RotationSpeed);
					vLook = Vector3.Transform (vLook, m);
					UpdateViewMatrix ();
					return;
				}
//				if (e.Mouse.RightButton == ButtonState.Pressed) {
//
//					Matrix4 m = Matrix4.CreateRotationZ (-e.XDelta * RotationSpeed);
//					Matrix4 m2 = Matrix4.Rotate (vLookPerpendicularOnXYPlane, -e.YDelta * RotationSpeed);
//
//					vLook = Vector3.Transform (vLook, m * m2);
//
//					//vLook = Vector3.Transform(vLook, m2);
//					UpdateViewMatrix ();
//					return;
//				}
					
				if (engine == null)
					return;
					
				engine.processMouseMove (new Point<float> ((float)e.X, (float)e.Y));
			}

		}			
		void Mouse_WheelChanged(object sender, MouseWheelEventArgs e)
		{
			float speed = MoveSpeed;
			if (Keyboard[Key.ShiftLeft])
				speed *= 0.1f;
			else if (Keyboard[Key.ControlLeft])
				speed *= 20.0f;
				
			vEye += vLook * e.Delta * speed;
			//vLook.Z += e.Delta * 0.1f;
			UpdateViewMatrix();
		}
		void Mouse_Down (object sender, MouseButtonEventArgs e)
		{
			if (engine == null)
				return;
			engine.processMouseDown (e);		
		}
		#endregion

		#region vLookCalculations
		Vector3 vLookDirOnXYPlane
		{
			get
			{
				return Vector3.NormalizeFast(new Vector3 (vLook.X, vLook.Y, 0));
			}
		}
		public Vector3 vLookPerpendicularOnXYPlane
		{
			get
			{
				Vector3 vHorizDir = Vector3.Cross(Vector3.NormalizeFast(new Vector3 (vLook.X, vLook.Y, 0)), Vector3.UnitZ);
				return vHorizDir;
			}
		}

		void moveCamera(Vector3 v)
		{
			vEye += v;
			vEyeTarget += v;
		}
		#endregion

		void initLights()
		{
			float[] lmKa = { 0.0f, 0.0f, 0.0f, 0.0f };

			GL.LightModel(LightModelParameter.LightModelAmbient, lmKa);
			GL.LightModel(LightModelParameter.LightModelLocalViewer, 1f);
			GL.LightModel(LightModelParameter.LightModelTwoSide, 0f);

			//GL.Light(LightName.Light0, LightParameter.SpotDirection, vSpot);
			//GL.Light(LightName.Light0, LightParameter.SpotExponent, 30);
			//GL.Light(LightName.Light0, LightParameter.SpotCutoff, 180);

			float Kc = 1.0f;
			float Kl = 0.0f;
			float Kq = 0.0f;

			GL.Light(LightName.Light0, LightParameter.ConstantAttenuation, Kc);
			GL.Light(LightName.Light0, LightParameter.LinearAttenuation, Kl);
			GL.Light(LightName.Light0, LightParameter.QuadraticAttenuation, Kq);

			float[] light_Ka = { 0.5f, 0.5f, 0.5f, 1.0f };
			float[] light_Kd = { 0.9f, 0.9f, 0.9f, 1.0f };
			float[] light_Ks = { 1.0f, 1.0f, 1.0f, 1.0f };

			GL.Light(LightName.Light0, LightParameter.Position, vLight);
			GL.Light(LightName.Light0, LightParameter.Ambient, light_Ka);
			GL.Light(LightName.Light0, LightParameter.Diffuse, light_Kd);
			GL.Light(LightName.Light0, LightParameter.Specular, light_Ks);
		}

		public void UpdateViewMatrix()
		{
			Rectangle r = this.ClientRectangle;
			GL.Viewport( r.X, r.Y, r.Width, r.Height);
			projection = Matrix4.CreatePerspectiveFieldOfView(fovY, r.Width / (float)r.Height, zNear, zFar);
			vEyeTarget = vEye + vLook;
			modelview = Matrix4.LookAt(vEye, vEyeTarget, Vector3.UnitZ);

			GL.GetInteger(GetPName.Viewport, viewport);
			try {
				texturedShader.ProjectionMatrix = projection;
				texturedShader.ModelViewMatrix = modelview;
			} catch (Exception ex) {
				
			}

		}
		[STAThread]
		static void Main ()
		{
			Console.WriteLine ("starting example");

			using (Magic win = new Magic( )) {
				win.Run (30.0);
			}
		}
	}
}