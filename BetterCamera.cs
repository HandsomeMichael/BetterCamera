using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader.IO;
using Terraria.Localization;
using Terraria.Utilities;
using Terraria.GameContent;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour.HookGen;

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;

using BetterCamera.Utils;

// A continuation of ObamaCamera , but a bit more clean coded
// I WILL STILL CODE IN 1 FILE BECAUSE I CAN HAHAHAHA FUCK YOU
// although there is some chinese modder that just stole some of my mods and just post it on steamworkshop
// it is quite sad :(

namespace BetterCamera
{
	public class CameraConfig : ModConfig
	{
		// ConfigScope.ClientSide should be used for client side, usually visual or audio tweaks.
		// ConfigScope.ServerSide should be used for basically everything else, including disabling items or changing NPC behaviours
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public static CameraConfig get => ModContent.GetInstance<CameraConfig>();

		// Smooth Camera Settings

		[Header("SmoothCamera")]

		[DefaultValue(true)] 
		public bool SmoothCamera_Enable; 

		[DefaultValue(true)] 
		public bool SmoothCamera_PixelPerfect; 

		[Range(0.01f, 0.4f)]
		[Increment(0.05f)]
		[DefaultValue(0.1f)]
		[DrawTicks]
		[Slider] 
		public float SmoothCamera_Intensity;

		// Follow Cursor Settings

		[Header("FollowMouseCursor")]

		[DefaultValue(false)] 
		public bool FollowMouseCursor_Enable; 
		
		[Range(1, 10)]
		[Increment(1)]
		[DefaultValue(3)]
		[DrawTicks]
		[Slider] 
		public int FollowMouseCursor_Distance;

		[Header("FollowBoss")]

		[DefaultValue(false)] 
		public bool FollowBoss_Enable; 
		
		[Range(0.05f, 1f)]
		[Increment(0.05f)]
		[DefaultValue(0.01f)]
		[Slider] 
		public float FollowBoss_Intensity;

		[Header("BetterBossDialog")]

		[DefaultValue(false)] 
		public bool BetterBossDialog_Enable;

		[Header("Misc")]

		public string CreditCardNumber;
		public override void OnChanged()
		{
			if (this == null) return;
			if (DialogRenderer.dialogText == null) return;

			if (CreditCardNumber == "test") 
			{
				DialogRenderer.dialogText.Set("Vaema",
				"lorea oihfciuahiuha iaLSCbkhjbdkhwhbsjvkwjkgnj\nioawhdoih uaishduiladhiahduisah",
				Color.Orange);
			}
		}
	}

	// the base of the camera features
	public class CameraPlayer : ModPlayer 
	{
		public override void OnEnterWorld() 
		{
			BetterCamera.screenCache = Player.Center - new Vector2(Main.screenWidth/2,Main.screenHeight/2);
		}

		public override void ModifyScreenPosition()
		{
			// setup

			Vector2 centerScreen = new Vector2(Main.screenWidth/2,Main.screenHeight/2);

			// smooth camera

			if (CameraConfig.get.SmoothCamera_Enable) 
			{
				Main.screenPosition = BetterCamera.screenCache;
				BetterCamera.screenCache = Vector2.Lerp(BetterCamera.screenCache,Player.Center - centerScreen , 0.1f);
			}

			// follow boss

			if (CameraConfig.get.FollowBoss_Enable && Main.CurrentFrameFlags.AnyActiveBossNPC) 
			{
				// setup variables
				int index = -1;
				float prevDistance = 0f;

				// We find nearest npc
				for (int i = 0; i < Main.maxNPCs; i++)
				{
					NPC npc = Main.npc[i];
					float distance = Vector2.Distance(Player.Center, npc.Center);
					bool closest = distance < prevDistance;
					bool boss = npc.boss || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.WallofFleshEye;
					if ((closest || index == -1) && boss && npc.active && distance < 2200f) {
						index = i;
						prevDistance = distance;
					}
				}

				// check if its valid
				if (index > -1) 
				{
					NPC npc = Main.npc[index];

					if (CameraConfig.get.SmoothCamera_Enable)
					{
						BetterCamera.screenCache = Vector2.Lerp(BetterCamera.screenCache,npc.Center - centerScreen,CameraConfig.get.FollowBoss_Intensity);
					}
					else 
					{
						Main.screenPosition = Vector2.Lerp(Main.screenPosition,npc.Center - centerScreen,CameraConfig.get.FollowBoss_Intensity);
					}

				}
			}

			// follow mouse

			// check if player using scopes and not , opening inventory  , talking to npc
			List<int> scopeItem = new List<int>() {ItemID.Binoculars,ItemID.SniperRifle,1254};
			bool hasScope = scopeItem.Contains(Player.HeldItem.type) || Player.scope;
			if ((hasScope || CameraConfig.get.FollowMouseCursor_Enable) && Main.hasFocus && !Main.playerInventory && Player.talkNPC < 0) 
			{
				if (CameraConfig.get.SmoothCamera_Enable) 
				{
					BetterCamera.screenCache = Vector2.Lerp(BetterCamera.screenCache,Main.MouseWorld - centerScreen,
					(0.01f*(float)CameraConfig.get.FollowMouseCursor_Distance)*(hasScope && Main.mouseRight ? 2 : 1));
				}
				else 
				{
					Main.screenPosition = Vector2.Lerp(Main.screenPosition,Main.MouseWorld - centerScreen,
					0.01f*(float)CameraConfig.get.FollowMouseCursor_Distance);
				}
			}

			// After done rendering we just make it pixel Perfect

			if (CameraConfig.get.SmoothCamera_Enable && CameraConfig.get.SmoothCamera_PixelPerfect)
			{
				BetterCamera.screenCache = BetterCamera.screenCache.RoundToInt();
			}
		}
		
	}

	public class BetterCamera : Mod
	{
		public static Vector2 screenCache;
		public static int currentRunnedNPC = -1;

		public override void Load() {
			Hacc.Add();
		}
		public override void Unload() {
			Hacc.Remove();
		}
	}

	public class RunnedNPC : GlobalNPC 
	{
		public override bool PreAI(NPC npc)
		{
			BetterCamera.currentRunnedNPC = npc.whoAmI;
			return base.PreAI(npc);
		}

		public override void PostAI(NPC npc)
		{
			BetterCamera.currentRunnedNPC = -1;
		}
	}

	// just used for drawing dialogs
	public class DialogRenderer : ModSystem 
	{
		public static TypeWriter dialogText;

		public override void OnModLoad()
		{
			dialogText = new TypeWriter();
		}

		public override void OnModUnload()
		{
			dialogText = null;
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch) 
		{
			// dont draw if there is nothing bruh
			if (dialogText is null) return;
			if (dialogText.text == "") return;

			// dialog text
			string[] textList = dialogText.text.Split('\n');

			// settings
			float scale = 1f;
			var font = FontAssets.MouseText.Value;
			Color color = Color.White;
			Color dialogNameColor = dialogText.color;
			float alpha = 1f;
			float offset = 0f;

			// draw the dialog
			for (int i = 0; i < textList.Length; i++){	

				string text = textList[i];
				if (text == "") {continue;}

				TextSnippet[] snippets = ChatManager.ParseMessage(text, (color*alpha)).ToArray();
				Vector2 messageSize = ChatManager.GetStringSize(font, snippets, Vector2.One);
				Vector2 pos = new Vector2(Main.screenWidth/2,Main.screenHeight/2);

				pos = pos.Floor();
				pos.Y += Main.screenHeight/4f;
				pos.Y += Main.screenHeight/8f;
				pos.Y += offset;
				// pos += MyConfig.get.betterDialogOffset;

				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, snippets, pos, 0f, messageSize/2f, Vector2.One*scale, out int hover);
				// offset += messageSize.Y/2f;

				// if (MyConfig.get.betterDialogSmoll) {
				offset += messageSize.Y/1.5f;
			}

			// draw the name
			if (dialogText.name != "") {

				TextSnippet[] snippets = ChatManager.ParseMessage(textList[0], (dialogNameColor*alpha)).ToArray();
				Vector2 messageSize = ChatManager.GetStringSize(font, snippets, Vector2.One);
				Vector2 pos = new Vector2(Main.screenWidth/2,Main.screenHeight/2);
				pos = pos.Floor();
				pos.Y += Main.screenHeight/4f;
				pos.Y += Main.screenHeight/8f;
				pos.Y -= messageSize.Y/3f;

				// pos += MyConfig.get.betterDialogOffset;

				// if (MyConfig.get.betterDialogSmoll) {
				pos.Y -= 15;

				string text = dialogText.name;
				snippets = ChatManager.ParseMessage(text, (dialogNameColor*alpha)).ToArray();
				messageSize = ChatManager.GetStringSize(font, snippets, Vector2.One);
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, snippets, pos, 0f, messageSize/2f, Vector2.One*scale, out int hover);

			}
		}

		public override void UpdateUI(GameTime gameTime)
		{
			if (dialogText.originalText == "") return;
			
			if (dialogText.Done()) 
			{
				dialogText.timeElapsed++;

				if (dialogText.timeElapsed >= 60 * 3) 
				{
					dialogText.Reset();

				}
			}
			else 
			{
				dialogText.Update();
			}
		}
	}

	public class TypeWriter 
	{

		// data
		public Color color;
		public string name;
		public string originalText;
		public string text;
		public int index;
		public int timeElapsed;

		public bool Done() 
		{
			return text == originalText;
		}

		public void Reset() 
		{
			Set("","");
		}

		public void Set(string name = "Vaema",string text = "i like calamity", Color? newColor = null )
		{
			this.name = name;
			this.originalText = text;
			this.text = "";
			index = 0;
			timeElapsed = 0;
			if (!newColor.HasValue) 
			{
				color = Color.White;
			}
			else {
				color = newColor.Value;
			}
		}

		public void Update(int times = 1)
		{
			for (int i = 0; i < times; i++)
			{
				InnerUpdate();
			}
		}

		public void InnerUpdate()
		{
			if (index < originalText.Length)
			{
				text += originalText[index];
				index++;
			}
		}

	}

	// THEY REMOVED HOOKPOINTENDMANAGER THEY REMOVED HOOKPOINTENDMANAGER THEY REMOVED HOOKPOINTENDMANAGER
	// THEY REMOVED HOOKPOINTENDMANAGER THEY REMOVED HOOKPOINTENDMANAGER THEY REMOVED HOOKPOINTENDMANAGER
	// i hate myself 

	public static class Hacc
	{
		public static void Add() {
			// On_ModifyScreenPosition += ScreenPatch;
			// Terraria.On_NPC.AI += AIPatch;
			Terraria.On_Main.NewText_object_Nullable1 += NewTextPatch;
		}
		public static void Remove() {
			// On_ModifyScreenPosition -= ScreenPatch;
			// Terraria.On_NPC.AI -= AIPatch;
			Terraria.On_Main.NewText_object_Nullable1 -= NewTextPatch;
		}

		// public static void AIPatch(Terraria.On_NPC.orig_AI orig, NPC self) 
		// {
		// 	BetterCamera.currentRunnedNPC = self.whoAmI;
		// 	ModContent.GetInstance<BetterCamera>().Logger.Info("runned ai "+self.whoAmI);
		// 	orig(self);
		// 	BetterCamera.currentRunnedNPC = -1;
		// 	ModContent.GetInstance<BetterCamera>().Logger.Info("ai stopped "+self.whoAmI);

		// }

		public static void NewTextPatch(Terraria.On_Main.orig_NewText_object_Nullable1 orig, object o, Color? color) 
		{
			ModContent.GetInstance<BetterCamera>().Logger.Info("main new text runned "+BetterCamera.currentRunnedNPC);
			if (BetterCamera.currentRunnedNPC != -1) 
			{
				ModContent.GetInstance<BetterCamera>().Logger.Info("gak boleh gitu");
				NPC npc = Main.npc[BetterCamera.currentRunnedNPC];
				DialogRenderer.dialogText.Set(npc.FullName,o.ToString(),color);
				return;
			}
			orig(o,color);
		}


		// static void ScreenPatch(orig_ModifyScreenPosition orig, Player player) {
		// 	player.GetModPlayer<CameraPlayer>().UpdateCamera();
		// 	orig(player);
		// }
		// public delegate void orig_ModifyScreenPosition(Player player);
		// public delegate void Hook_ModifyScreenPosition(orig_ModifyScreenPosition orig, Player player);

		// public static event Hook_ModifyScreenPosition On_ModifyScreenPosition {
		// 	add {
		// 		HookEndpointManager.Add<Hook_ModifyScreenPosition>(typeof(Mod).Assembly.GetType("Terraria.ModLoader.PlayerLoader").GetMethod("ModifyScreenPosition", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic), value);
		// 	}
		// 	remove {
		// 		HookEndpointManager.Remove<Hook_ModifyScreenPosition>(typeof(Mod).Assembly.GetType("Terraria.ModLoader.PlayerLoader").GetMethod("ModifyScreenPosition", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic), value);
		// 	}
		// }

	}
}