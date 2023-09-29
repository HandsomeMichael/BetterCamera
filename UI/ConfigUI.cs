using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;

namespace BetterCamera.UI
{
    // TO DO : MAKE A FANCY UI
	public class MenuBar : UIState
    {
        public UIVerticalSlider playButton;

        public override void OnInitialize()
        {
            
            Append(playButton);
        }
    }
    // public class MySlider : UIVerticalSlider
    // {
    //     public MySlider(Func<float> getStatus, Action<float> setStatusKeyboard, Action setStatusGamepad, Color color) : base(getStatus, setStatusKeyboard, setStatusGamepad, color)
    //     {
    //     }
    // }
}