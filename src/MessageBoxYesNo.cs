﻿using System;
using Crow;

namespace Magic3D
{
	public class MessageBoxYesNo : Border
	{
		public Button btOk;
		public Button btCancel;

		public MessageBoxYesNo (string text) : base() 
		{
			Margin = 5;
			BorderWidth = 1;
			Width = 300;
			Height = 100;
			//BorderColor = Color.BlueCrayola;
			Background = new Color (0.6, 0.6, 0.6, 0.6);
			Group vs = this.SetChild (new Group ());
			Label label = vs.addChild (new Label (text));
			label.VerticalAlignment = VerticalAlignment.Center;
			label.HorizontalAlignment = HorizontalAlignment.Center;
			HorizontalStack hs = vs.addChild(new HorizontalStack());
			hs.Fit = true;
			hs.HorizontalAlignment = HorizontalAlignment.Right;
			hs.VerticalAlignment = VerticalAlignment.Bottom;
			hs.Margin = 0;
			hs.Spacing = 10;
			btOk = hs.addChild (new Button ());
			btOk.Caption = "Ok";
			btOk.Width = 60;
			btOk.Height = 30;
			btCancel = hs.addChild (new Button ());
			btCancel.Caption = "Cancel";
			btCancel.Width = 60;
			btCancel.Height = 30;

//			btCancel.MouseClick += onClick;
//			btOk.MouseClick += onClick;
		}
		void onClick(object sender, OpenTK.Input.MouseButtonEventArgs e)
		{
			this.HostContainer.DeleteWidget (this);
		}
	}
}

