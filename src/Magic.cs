#define MONO_CAIRO_DEBUG_DISPOSE


using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using System.Diagnostics;

using go;
using System.Threading;
using GGL;
using System.Collections.Generic;
//using GameLib;
using GLU = OpenTK.Graphics.Glu;

namespace Magic3D
{
	class Magic : OpenTKGameWindow
	{
		#region FPS
		static int _fps = 0;

		public static int fps {
			get { return _fps; }
			set {
				_fps = value;
				if (_fps > fpsMax)
					fpsMax = _fps;
				else if (_fps < fpsMin)
					fpsMin = _fps;
			}

		}

		public static int fpsMin = int.MaxValue;
		public static int fpsMax = 0;

		static void resetFps ()
		{
			fpsMin = int.MaxValue;
			fpsMax = 0;
			_fps = 0;
		}
		#endregion

		public Magic ()
			: base(1024, 800)
		{}

		#region  scene matrix and vectors
		public static Matrix4 modelview;
		public static Matrix4 projection;

		//public static Vector3 vEye = new Vector3(150.0f, 50.0f, 1.5f);    // Camera Position
		public static Vector3 vEye = new Vector3(0.0f, -10.0f, 10.0f);    // Camera Position
		public static Vector3 vEyeTarget;// = new Vector3(40f, 50f, 0.1f);
		public static Vector3 vLook = new Vector3(0f, 1f, -1.3f);  // Camera vLook Vector
		public static Vector4 vLight = new Vector4 (0.0f, -20.0f, 10.0f, 0.0f);

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
			get { return  Vector3.CalculateAngle(vLook, Vector3.UnitX); }
		}
		#endregion

		public static GameLib.SingleLightSimpleShader texturedShader;


		void drawScene()
		{
			texturedShader.Enable ();
			//shader.LineWidth = lineWidth;
			texturedShader.Color = Color.White;

			texturedShader.LightPos = vLight;
			texturedShader.ProjectionMatrix = projection;
			texturedShader.ModelViewMatrix = modelview;
			texturedShader.ModelMatrix = Matrix4.Identity;

			drawTable ();

//			CardInstance c = deck.Cards [0];
//			c.x = 0;
//			c.y = -4;

			Players[0].Render ();
			//Players[1].Render ();
			//deck.Cards [0].render ();

			//MagicCard.cardDatabase ["Abandon Hope"].Render ();

		}
			
		#region table
		vaoMesh table;
		int tableTexture;
		public void initTableModel()
		{
			tableTexture = new Texture(@"images/marble1.jpg");

			const float _width = 16f;
			const float _height = 16f;
			const float texTileX = 3f;
			const float texTileY = 3f;
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
		int diceTex;
		void initDice()
		{
			dice = vaoMesh.Load (@"meshes/d20-s2.obj");
			diceTex = new Texture (@"images/d20.png");
		}
		void renderDice()
		{
			texturedShader.ModelMatrix = Matrix4.CreateScale(2) * Matrix4.CreateTranslation (0, -2, 3);
			GL.BindTexture (TextureTarget.Texture2D, diceTex);
			dice.Render (PrimitiveType.Triangles);
			GL.BindTexture (TextureTarget.Texture2D, 0);
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


		public Player[] Players;
		MagicEngine engine;

		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);

//			if (!GL.GetString(StringName.Extensions).Contains("EXT_geometry_shader4") )
//			{
//				Debug.WriteLine (
//					"Your video card does not support EXT_geometry_shader4. Please update your drivers.",
//					"EXT_geometry_shader4 not supported");
//			}



			MagicCard.LoadCardDatabase();
			Edition.LoadEditionsDatabase();


			Deck.LoadPreconstructedDecks();

			Players = new Player[] { new Player(), new Player()};
			Players[0].Name = "player 1";
			Players[1].Name = "player 2";
			Players[1].zAngle = MathHelper.Pi*0.8f;

			Players[0].Deck = Deck.PreconstructedDecks ["air razers"];
			Players[1].Deck = Deck.PreconstructedDecks ["angelic might"];
			engine = new MagicEngine (Players);

			Mouse.WheelChanged += new EventHandler<MouseWheelEventArgs>(Mouse_WheelChanged);
			Mouse.Move += new EventHandler<MouseMoveEventArgs>(Mouse_Move);

			//initLights ();

			GL.ClearColor(0.0f, 0.0f, 0.2f, 1.0f);

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			GL.Enable(EnableCap.CullFace);
			//GL.FrontFace (FrontFaceDirection.Cw);

			GL.PrimitiveRestartIndex (int.MaxValue);
			GL.Enable (EnableCap.PrimitiveRestart);

			GL.Enable (EnableCap.PointSprite);
			GL.Enable(EnableCap.VertexProgramPointSize);
			GL.PointParameter(PointSpriteCoordOriginParameter.LowerLeft);

			GL.Enable (EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			//GL.FrontFace(FrontFaceDirection.Ccw);

			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
			GL.ShadeModel(ShadingModel.Smooth);
			GL.Hint (HintTarget.LineSmoothHint, HintMode.Nicest);


			texturedShader = new GameLib.SingleLightSimpleShader ();

			initTableModel ();

			GL.ActiveTexture (TextureUnit.Texture0);
			ErrorCode err = GL.GetError ();
			Debug.Assert (err == ErrorCode.NoError, "OpenGL Error");

			engine.StartNewGame();


		}
			
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			drawScene();

//			AxesHelper.Render ();
//
			base.OnRenderFrame (e);

			SwapBuffers ();
		}
		int frameCpt = 0;
		protected override void OnUpdateFrame (FrameEventArgs e)
		{
			base.OnUpdateFrame (e);

			if (frameCpt > 20)
			{
//				labFps.Text = fps.ToString();
//				labFpsMin.Text = fpsMin.ToString();
//				labFpsMax.Text = fpsMax.ToString();
				resetFps();
				frameCpt = 0;
			}
				
			if (frameCpt % 2 == 0)
			{
				//mousePicking();

			}

			frameCpt++;
			Animation.ProcessAnimations();

			if (Keyboard[Key.ControlLeft])
			{
				//light movment
				if (Keyboard [Key.Up])
					vLight += new Vector4(MoveSpeed * vLookDirOnXYPlane,0);
				else if (Keyboard[Key.Down])
					vLight -= new Vector4(MoveSpeed * vLookDirOnXYPlane,0);
				else if (Keyboard[Key.Left])
					vLight -= new Vector4(MoveSpeed * vLookPerpendicularOnXYPlane,0);
				else if (Keyboard[Key.Right])
					vLight += new Vector4(MoveSpeed * vLookPerpendicularOnXYPlane,0);
				else if (Keyboard[Key.PageUp])
					vLight.Z += MoveSpeed * .5f;
				else if (Keyboard[Key.PageDown])
					vLight.Z -= MoveSpeed * .5f;

				texturedShader.LightPos = vLight;
			}
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
				if (e.Mouse.MiddleButton == OpenTK.Input.ButtonState.Pressed)
				{
					Matrix4 m = Matrix4.CreateRotationZ(-e.XDelta * RotationSpeed);
					m *= Matrix4.CreateFromAxisAngle(-vLookPerpendicularOnXYPlane, -e.YDelta * RotationSpeed);
					vEyeTarget = Vector3.Zero;
					vEye = Vector3.Transform(vEye, Matrix4.CreateTranslation(-vEyeTarget) * m * Matrix4.CreateTranslation(vEyeTarget));
					UpdateViewMatrix();
				}
				if (e.Mouse.RightButton == ButtonState.Pressed)
				{

					Matrix4 m = Matrix4.CreateRotationZ(-e.XDelta * RotationSpeed);
					Matrix4 m2 = Matrix4.Rotate(vLookPerpendicularOnXYPlane, -e.YDelta * RotationSpeed);

					vLook = Vector3.Transform(vLook, m * m2);

					//vLook = Vector3.Transform(vLook, m2);
					UpdateViewMatrix();

				}
			}
		}			
		void Mouse_WheelChanged(object sender, MouseWheelEventArgs e)
		{
			float speed = MoveSpeed;
			if (Keyboard[Key.ShiftLeft])
				speed *= 0.1f;
			else if (Keyboard[Key.ControlLeft])
				speed *= 20.0f;
				

			vLook.Z += e.Delta * 0.1f;
			UpdateViewMatrix();
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

		public void UpdateViewMatrix()
		{
			Rectangle r = this.ClientRectangle;
			GL.Viewport( r.X, r.Y, r.Width, r.Height);
			projection = Matrix4.CreatePerspectiveFieldOfView(fovY, r.Width / (float)r.Height, zNear, zFar);
			vEyeTarget = vEye + vLook;
			modelview = Matrix4.LookAt(vEye, vEyeTarget, Vector3.UnitZ);

			texturedShader.ProjectionMatrix = projection;
			texturedShader.ModelViewMatrix = modelview;

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