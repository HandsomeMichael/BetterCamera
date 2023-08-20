// terraria stuff
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

// for drawing
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader.IO;
using Terraria.Localization;
using Terraria.Utilities;
using Terraria.GameContent;

// for vectors and other miscrosoft stuff
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
// detouring
using MonoMod.RuntimeDetour.HookGen;

// reflection , list , and also config component
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

using BetterCamera.Utils;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework.Input;

// A continuation of ObamaCamera , but a bit more clean coded
// I WILL STILL CODE IN 1 FILE BECAUSE I CAN HAHAHAHA FUCK YOU
// although there is some chinese modder that just stole some of my mods and just post it on steamworkshop
// it is quite sad :(

namespace BetterCamera
{
	public class CameraConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public static CameraConfig get => ModContent.GetInstance<CameraConfig>();

		// save the config , this requires reflection though.
		public static void SaveConfig(){
			typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[1] { get });
		}

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

		// Follow Boss

		[Header("FollowBoss")]

		[DefaultValue(false)] 
		public bool FollowBoss_Enable; 
		
		[Range(0.05f, 1f)]
		[Increment(0.05f)]
		[DefaultValue(0.01f)]
		[Slider] 
		public float FollowBoss_Intensity;
		
		// Dialog 

		[Header("BetterBossDialog")]

		[DefaultValue(false)] 
		public bool BetterBossDialog_Enable;

		[Range(0.5f, 5f)]
		[Increment(0.5f)]
		[DefaultValue(3f)]
		[Slider] 
		public float BetterBossDialog_Time;

		[Range(0.1f, 2f)]
		[Increment(0.1f)]
		[DefaultValue(1f)]
		[Slider] 
		public float BetterBossDialog_Scale;

		[DefaultValue(false)] 
		public bool BetterBossDialog_UnlockOffset;

		[JsonIgnore]
		public Vector2 BetterBossDialog_Offset = Vector2.Zero;

		public List<string> BetterBossDialog_BlackList = new List<string>() {
			"has awoken","was slain"
		};

		[Header("Misc")]

		public string CreditCardNumber;

		public override void OnChanged()
		{

			// reset camera cache
			BetterCamera.ResetCameraCache();

			// dialog test
			if (DialogRenderer.dialogText == null) return;

			if (CreditCardNumber == "test") 
			{
				DialogRenderer.dialogText.Set("Vaema",
				"ambatukam ambatukam ambatukam\nambatukamambatukamambatukamambatukam",
				Color.Orange);
			}
		}
	}

	// the base of the camera features
	public class CameraPlayer : ModPlayer 
	{
		// a quick reset
		public override void OnEnterWorld() 
		{
			BetterCamera.ResetCameraCache();
			//BetterCamera.screenCache = Player.Center - new Vector2(Main.screenWidth/2,Main.screenHeight/2);
		}

		public override void ModifyScreenPosition()
		{
			// setup

			Vector2 centerScreen = new Vector2(Main.screenWidth/2,Main.screenHeight/2);

			// smooth camera

			if (CameraConfig.get.SmoothCamera_Enable && !BetterCamera.QuickLookAtNPC.Current) 
			{
				Main.screenPosition = BetterCamera.screenCache;
				BetterCamera.screenCache = Vector2.Lerp(BetterCamera.screenCache,Player.Center - centerScreen , 0.1f);
			}

			// follow boss

			if (BetterCamera.QuickLookAtNPC.Current || (CameraConfig.get.FollowBoss_Enable && Main.CurrentFrameFlags.AnyActiveBossNPC)) 
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
					bool boss = BetterCamera.QuickLookAtNPC.Current || npc.boss || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.WallofFleshEye;
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
						Main.screenPosition = Vector2.Lerp(Main.screenPosition,npc.Center - centerScreen,
						BetterCamera.QuickLookAtNPC.Current ? 1 : CameraConfig.get.FollowBoss_Intensity);
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

	// used to store specific variables and reloading
	public class BetterCamera : Mod
	{
		// global variables
		public static Vector2 screenCache;
		public static int currentRunnedNPC = -1;

		// keybinds
		public static ModKeybind QuickLookAtNPC { get; private set; }

		// reset camera cache
		public static void ResetCameraCache() 
		{
			BetterCamera.screenCache = Main.screenPosition;
		}

		// load detours
		public override void Load() {
			Hacc.Add();
			QuickLookAtNPC = KeybindLoader.RegisterKeybind(this, "Quick Look At Enemy", "V");
		}
		// unload detours , very crucial
		public override void Unload() {
			Hacc.Remove();
			QuickLookAtNPC = null;
		}

		// mod calls , yahoo
		public override object Call(params object[] args) 
		{
			// resize arguments
			int argsLength = args.Length;
			Array.Resize(ref args, 5);

			// do a really safe code
			try {

				string call = args[0] as string;

				// new dialog
				if (call == "NewDialog") 
				{
					// setup variables 
					string name = args[1] as string;
					string text = args[2] as string;
					Color? color = args[3] as Color?;

					// render the text
					DialogRenderer.dialogText.Set(name,text,color);

					// log it if necessary ?
					Logger.InfoFormat("rendered text from mod calls");

					return true;

				}
				// if mod calls is unknown we just do funny
				Logger.Error($"Unknown mod calls '{call}'");
			}
			// skill issue
			catch (Exception e) 
			{
				Logger.Error($"Call Error: You are screwed, {e.StackTrace} {e.Message}");
			}
			// return nothing if nothing
			return null;
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
			float scale = CameraConfig.get.BetterBossDialog_Scale;
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
				pos += CameraConfig.get.BetterBossDialog_Offset;

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

				pos += CameraConfig.get.BetterBossDialog_Offset;

				// if (MyConfig.get.betterDialogSmoll) {
				pos.Y -= 15;

				string text = dialogText.name;
				snippets = ChatManager.ParseMessage(text, (dialogNameColor*alpha)).ToArray();
				messageSize = ChatManager.GetStringSize(font, snippets, Vector2.One);
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, snippets, pos, 0f, messageSize/2f, Vector2.One*scale, out int hover);

			}
		}

		// update ui
		public override void UpdateUI(GameTime gameTime)
		{
			// Allow the user the offset the boss dialog a bit
			if (CameraConfig.get.BetterBossDialog_UnlockOffset) {

                dialogText.Set("Test", "Use Arrow Keys to move this gui \n press ENTER if you done", Color.Orange);
				dialogText.text = dialogText.originalText;
				
				if (Main.keyState.IsKeyDown(Keys.Right)) {
					CameraConfig.get.BetterBossDialog_Offset.X += 4;
					CameraConfig.SaveConfig();
				}
				if (Main.keyState.IsKeyDown(Keys.Left)) {
					CameraConfig.get.BetterBossDialog_Offset.X -= 4;
					CameraConfig.SaveConfig();
				}
				if (Main.keyState.IsKeyDown(Keys.Up)) {
					CameraConfig.get.BetterBossDialog_Offset.Y -= 4;
					CameraConfig.SaveConfig();
				}
				if (Main.keyState.IsKeyDown(Keys.Down)) {
					CameraConfig.get.BetterBossDialog_Offset.Y += 4;
					CameraConfig.SaveConfig();
				}

				if (Main.keyState.IsKeyDown(Keys.Enter)) {
					CameraConfig.get.BetterBossDialog_UnlockOffset = false;
					CameraConfig.SaveConfig();
				}
			}

			// Update dialog text
			if (dialogText.originalText == "") return;
			
			if (dialogText.Done()) 
			{
				dialogText.timeElapsed++;

				if (dialogText.timeElapsed >= 60 * CameraConfig.get.BetterBossDialog_Time) 
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

	// base data for better dialog text effect
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
			// ModContent.GetInstance<BetterCamera>().Logger.Info("main new text runned "+BetterCamera.currentRunnedNPC);
			if (BetterCamera.currentRunnedNPC != -1) 
			{
				// check if the text is blacklisted
				bool blackListed = false;
				foreach (var item in CameraConfig.get.BetterBossDialog_BlackList)
				{
					if (o.ToString().Contains(item))
					{
						blackListed = true;
						break;
					}
				}

				// if its not black listed we do the funny
				if (!blackListed) 
				{
					// ModContent.GetInstance<BetterCamera>().Logger.Info("gak boleh gitu");
					NPC npc = Main.npc[BetterCamera.currentRunnedNPC];
					DialogRenderer.dialogText.Set(npc.FullName,o.ToString(),color);
					return;
				}
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