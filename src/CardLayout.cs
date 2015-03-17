using System;
using OpenTK;
using System.Collections.Generic;

namespace Magic3D
{

	public class CardLayout
	{
		public List<CardInstance> Cards = new List<CardInstance>();

		public float x = 0.0f;
		public float y = 0.0f;
		public float z = 0.0f;
		public float xAngle = 0.0f;
		public float yAngle = 0.0f;
		public float zAngle = 0.0f;

		public float HorizontalSpacing = 0f;
		public float VerticalSpacing = 0f;
		public float MaxHorizontalSpace = 4.5f;

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

		public void Render()
		{
			foreach (CardInstance c in Cards)
			{
				c.render();
			}
		}
		public void ShuffleAndLayoutZ()
		{
			Cards.Shuffle();
			float currentZ = this.z;
			foreach (CardInstance c in Cards)
			{
				Animation a = null;
				if (Animation.GetAnimation(c, "z", ref a))
					a.CancelAnimation();

				if (c.z != currentZ)
					Animation.StartAnimation(new FloatAnimation(c, "z", currentZ, 0.1f));

				currentZ += VerticalSpacing;
			}
		}
		public virtual  void UpdateLayout()
		{
			//vertical layouting            

			float hSpace = HorizontalSpacing;

			if (HorizontalSpacing * Cards.Count > MaxHorizontalSpace)
				hSpace = MaxHorizontalSpace / Cards.Count;


			float halfWidth = hSpace * (Cards.Count) / 2;

			float cX = this.x - halfWidth;
			float cY = this.y;
			float cZ = this.z;

			foreach (CardInstance c in Cards)
			{
				Animation.StartAnimation(new FloatAnimation(c, "x", cX, 0.2f));
				Animation.StartAnimation(new FloatAnimation(c, "y", cY, 0.2f));
				Animation.StartAnimation(new FloatAnimation(c, "z", cZ, 0.2f));
				Animation.StartAnimation(new AngleAnimation(c, "xAngle", xAngle, MathHelper.Pi * 0.3f));
				Animation.StartAnimation(new AngleAnimation(c, "yAngle", yAngle, MathHelper.Pi * 0.3f));

				float aX = cX;
				float aY = cY;
				float aZ = cZ;
				foreach (CardInstance ac in c.AttachedCards)
				{
					aX += 0.15f;
					aY += 0.15f;
					aZ -= 0.001f;

					Animation.StartAnimation(new FloatAnimation(ac, "x", aX, 0.2f));
					Animation.StartAnimation(new FloatAnimation(ac, "y", aY, 0.2f));
					Animation.StartAnimation(new FloatAnimation(ac, "z", aZ, 0.2f));
					Animation.StartAnimation(new AngleAnimation(ac, "xAngle", xAngle, MathHelper.Pi * 0.3f));
					Animation.StartAnimation(new AngleAnimation(ac, "yAngle", yAngle, MathHelper.Pi * 0.3f));
				}


				cX += hSpace;
				cZ += VerticalSpacing;
			}
		}
		public void UpdateDefendersLayout()
		{            
			//vertical layouting            

			float hSpace = 0.25f;
			float vSpace = -0.25f;
			float zSpace = 0.001f;

			foreach (CardInstance c in Cards)
			{
				int idx = c.BlockedCreature.BlockingCreatures.IndexOf(c);

				Vector3 v = c.BlockedCreature.Position;
				v = Vector3.Transform(v, Matrix4.Invert(c.BlockedCreature.Controler.Transformations));

				float cX = v.X + hSpace * idx;
				float cY = this.y + vSpace * idx;
				float cZ = this.z + zSpace * idx;

				Animation.StartAnimation(new FloatAnimation(c, "x", cX, 0.2f));
				Animation.StartAnimation(new FloatAnimation(c, "y", cY, 0.2f));
				Animation.StartAnimation(new FloatAnimation(c, "z", cZ, 0.2f));
				Animation.StartAnimation(new AngleAnimation(c, "xAngle", xAngle, MathHelper.Pi * 0.3f));
				Animation.StartAnimation(new AngleAnimation(c, "yAngle", yAngle, MathHelper.Pi * 0.3f));

				float aX = cX;
				float aY = cY;
				float aZ = cZ;
				foreach (CardInstance ac in c.AttachedCards)
				{
					aX += 0.15f;
					aY += 0.15f;
					aZ -= 0.001f;

					Animation.StartAnimation(new FloatAnimation(ac, "x", aX, 0.2f));
					Animation.StartAnimation(new FloatAnimation(ac, "y", aY, 0.2f));
					Animation.StartAnimation(new FloatAnimation(ac, "z", aZ, 0.2f));
					Animation.StartAnimation(new AngleAnimation(ac, "xAngle", xAngle, MathHelper.Pi * 0.3f));
					Animation.StartAnimation(new AngleAnimation(ac, "yAngle", yAngle, MathHelper.Pi * 0.3f));
				}
			}
		}

		bool isExpanded = false;
		public void showAll()
		{
			isExpanded = !isExpanded;
			if (isExpanded)
			{
				Vector3 v = Magic.vGroupedFocusedPoint;
				float aCam = Magic.FocusAngle;

				float horizontalLimit = 5f;

				float cX = v.X;
				float cZ = v.Z;

				float hSpace = horizontalLimit / Cards.Count;
				float vSpace = -0.001f;

				if (hSpace > 0.9f)
					hSpace = 0.9f;

				cX -= hSpace * Cards.Count / 2;

				int delay = 0;

				foreach (CardInstance c in Cards)
				{
					Animation.StartAnimation(new FloatAnimation(c, "x", cX, 0.2f));
					Animation.StartAnimation(new FloatAnimation(c, "y", v.Y, 0.1f));
					Animation.StartAnimation(new FloatAnimation(c, "z", cZ, 0.1f));
					Animation.StartAnimation(new AngleAnimation(c, "xAngle", aCam, MathHelper.Pi * 0.1f), 100);
					Animation.StartAnimation(new AngleAnimation(c, "yAngle", 0, MathHelper.Pi * 0.1f), 100);

					cX += hSpace;
					cZ += vSpace;

					delay += 10;
				}
			}
			else
				UpdateLayout();
		}
	}
}

