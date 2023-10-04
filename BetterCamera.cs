// terraria stuff
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

// for drawing
// using Terraria.UI;
using Terraria.UI.Chat;
// using Terraria.DataStructures;
// using Terraria.GameInput;
// using Terraria.ModLoader.IO;
// using Terraria.Localization;
// using Terraria.Utilities;
using Terraria.GameContent;

// for vectors and other miscrosoft stuff
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// detouring unused
// using MonoMod.RuntimeDetour.HookGen;

// reflection , list , and also config component
using System;
// using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
// using System.Text.RegularExpressions;

using BetterCamera.Utils;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework.Input;
// using Terraria.Graphics;
using Terraria.Graphics.CameraModifiers;
using Microsoft.Xna.Framework.Audio;
using Terraria.Audio;

// A continuation of ObamaCamera , but a bit more clean coded
// I WILL STILL CODE IN 1 FILE BECAUSE I CAN , HEHEHEHAW FUCK U HEHEHEHAW

// i just watched the nun 2 , bro just fight the demon with a vilgigantinious amount of beer

namespace BetterCamera
{
	
	public class CameraConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public static CameraConfig Get => ModContent.GetInstance<CameraConfig>();

        // save the config , this requires reflection though.
        public static void SaveConfig() => typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[1] { Get });

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

		[Range(1f, 3f)]
		[Increment(0.5f)]
		[DefaultValue(2f)]
		[Slider] 
		public float SmoothCamera_ResetIntensity;

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
		
		[Range(0.1f, 1f)]
		[Increment(0.05f)]
		[DefaultValue(0.1f)]
		[Slider] 
		public float FollowBoss_Intensity;

		[Range(0.1f, 1f)]
		[Increment(0.1f)]
		[DefaultValue(0.5f)]
		[Slider] 
		public float FollowBoss_Range;

		[Range(500, 4000)]
		[Increment(100)]
		[DefaultValue(2000)]
		public int FollowBoss_Distance;

		// Screen Shakes
		// if you see this maybe try listening to this funny music , idk im bored
		// https://www.youtube.com/watch?v=yModCU1OVHY

		[Header("ExperimentalScreenShakes")]

		[DefaultValue(false)] 
		public bool ScreenShake_OnRoar;
		
		[DefaultValue(false)] 
		public bool ScreenShake_OnHit;  

		[DefaultValue(false)] 
		public bool ScreenShake_OnKill;

		[DefaultValue(false)] 
		public bool ScreenShake_OnGotHit;

		[Range(5f, 50f)]
		[Increment(5f)]
		[DefaultValue(30f)]
		[Slider] 
		public float ScreenShake_Intensity;
		
		// Dialog 

		[Header("BetterBossDialog")]

		[DefaultValue(false)] 
		public bool BetterBossDialog_Enable;

		[DefaultValue(true)]
		[ReloadRequired]

		public bool BetterBossDialog_UseGlobalNPC;

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

		public List<string> BetterBossDialog_BlackList = new() {
			"has awoken","was slain"
		};

		[Header("Misc")]

		public string CreditCardNumber;

		[DefaultValue(true)] 
		public bool BetterBinoculars;

		[DefaultValue(false)]
		[ReloadRequired] 
		public bool BetterBomb;

		[DefaultValue(false)] 
		public bool SpectatePlayers;

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

		// screen shakes
		public override void OnHitAnything(float x, float y, Entity victim)
        {
			if (Player.whoAmI != Main.myPlayer) return;
			if (CameraConfig.Get.ScreenShake_OnHit) 
			{
				float strength = 1f;
				if (CameraConfig.Get.ScreenShake_OnKill && victim is NPC target) {
					if (target.life <= 0) {
						strength = 3f;
					}	
				}
            	Hacc.ShakeThatAss(new Vector2(x,y),strength);
			}
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (CameraConfig.Get.ScreenShake_OnGotHit) 
			{
            	Hacc.ShakeThatAss(Player.Center,5f);
			}
        }
		

		public static void UpdateCamera(Player Player)
		{
			if (BetterCamera.DisableThisShit) return;
			
			// ModContent.GetInstance<BetterCamera>().Logger.Info("Top singko capybara");

			// setup

			Vector2 centerScreen = new(Main.screenWidth/2,Main.screenHeight/2);

			bool useBinoculars = CameraConfig.Get.BetterBinoculars && Player.HeldItem.type == ItemID.Binoculars;
			bool centerCameraToPlayer = !BetterCamera.QuickLookAtNPC.Current && !useBinoculars && !Player.dead;

			// smooth camera
			// honestly i have no idea what im doing , but it works

			if (CameraConfig.Get.SmoothCamera_Enable) 
			{
				// smoothen out
				Vector2 smoothValue = Vector2.Lerp(BetterCamera.screenCache,Main.screenPosition,
				CameraConfig.Get.SmoothCamera_Intensity*CameraConfig.Get.SmoothCamera_ResetIntensity);

				// snap the pixels
				if (CameraConfig.Get.SmoothCamera_PixelPerfect) 
				smoothValue = smoothValue.RoundToInt();

				// apply value
				Main.screenPosition = smoothValue;

				// center the camera to player
				if (centerCameraToPlayer)
				BetterCamera.screenCache = Vector2.Lerp(BetterCamera.screenCache,Player.Center - centerScreen ,
				CameraConfig.Get.SmoothCamera_Intensity);
			}

			// spectate
			// honestly i cant test wether this worked or not also this may or may not corrupted my player
			if (CameraConfig.Get.SpectatePlayers && Player.dead) 
			{
				if (Main.mouseLeftRelease) BetterCamera.spectatedPlayer--;
				if (Main.mouseRightRelease) BetterCamera.spectatedPlayer++;

				if (BetterCamera.spectatedPlayer <= -1) 
				{
					for (int i = 255; i < 0; i--)
					{
						if (!Main.player[i].dead) 
						{
							BetterCamera.spectatedPlayer = i;
						}
					}
				}

				if (BetterCamera.spectatedPlayer > 255) 
				{
					for (int i = 0; i < Main.maxPlayers; i++)
					{
						if (!Main.player[i].dead) 
						{
							BetterCamera.spectatedPlayer = i;
						}
					}
				}

				if (BetterCamera.spectatedPlayer >= 0 && BetterCamera.spectatedPlayer <= 255) {
					if (Main.player[BetterCamera.spectatedPlayer].dead) 
					{
						for (int i = 0; i < Main.maxPlayers; i++)
						{
							if (!Main.player[i].dead) 
							{
								BetterCamera.spectatedPlayer = i;
							}
						}

					}
				}

			}

			// better binoculars
			if (useBinoculars) {
				if (Main.mouseLeft) 
				{
					BetterCamera.screenCache = Vector2.Lerp(BetterCamera.screenCache,Main.MouseWorld - centerScreen,0.05f);
				}
				else if (Main.mouseRight) {
					BetterCamera.screenCache = Vector2.Lerp(BetterCamera.screenCache,Player.Center - centerScreen,0.05f);
				}
			}

			// follow boss

			if (BetterCamera.QuickLookAtNPC.Current || 
			(CameraConfig.Get.FollowBoss_Enable && Main.CurrentFrameFlags.AnyActiveBossNPC)) 
			{
				// setup variables
				int index = -1;
				float prevDistance = 0f;
				bool isPrevBoss = false;

				// We find nearest npc
				for (int i = 0; i < Main.maxNPCs; i++)
				{
					NPC npc = Main.npc[i];
					float distance = Vector2.Distance(Player.Center, npc.Center);
					bool closest = distance < prevDistance;
					bool boss = npc.boss || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.WallofFleshEye;
					bool inRange = distance < CameraConfig.Get.FollowBoss_Distance;

					// we prioritize boss
					if (inRange && (closest || index == -1) && (boss || (BetterCamera.QuickLookAtNPC.Current && !isPrevBoss && !boss)) && npc.active && distance < 2200f) {
						index = i;
						isPrevBoss = boss;
						prevDistance = distance;
					}
				}

				// check if its valid
				if (index > -1) 
				{
					float followBossIntensity = CameraConfig.Get.FollowBoss_Intensity;
					NPC npc = Main.npc[index];

					var pos = Vector2.Lerp(Player.Center - centerScreen,npc.Center - centerScreen ,
					CameraConfig.Get.FollowBoss_Range * (BetterCamera.QuickLookAtNPC.Current ? 2f : 1f));

					if (CameraConfig.Get.SmoothCamera_Enable)
					{
						BetterCamera.screenCache = Vector2.Lerp(BetterCamera.screenCache,
						pos,followBossIntensity);
					}
					else 
					{
						Main.screenPosition = Vector2.Lerp(Main.screenPosition,npc.Center - centerScreen,followBossIntensity);
					}

				}
			}

			// Follow Cursor
			if (CameraConfig.Get.FollowMouseCursor_Enable && Main.hasFocus && !Main.playerInventory && Player.talkNPC < 0) 
			{
				if (CameraConfig.Get.SmoothCamera_Enable) 
				{
					BetterCamera.screenCache = Vector2.Lerp(BetterCamera.screenCache,Main.MouseWorld - centerScreen,
					0.01f*(float)CameraConfig.Get.FollowMouseCursor_Distance);
				}
				else 
				{
					Main.screenPosition = Vector2.Lerp(Main.screenPosition,Main.MouseWorld - centerScreen,
					0.01f*(float)CameraConfig.Get.FollowMouseCursor_Distance);
				}
			}
		}
		
	}

	// used to store specific variables and reloading
	public class BetterCamera : Mod
	{
		// global variables
		public static Vector2 screenCache;
		public static int currentRunnedNPC = -1;
		public static int spectatedPlayer = 0;
		public static bool DisableThisShit = false;

		// keybinds
		public static ModKeybind QuickLookAtNPC { get; private set; }

		// reset camera cache
		public static void ResetCameraCache() 
		{
            screenCache = Main.screenPosition;
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
					if (argsLength <= 4) 
					{
						Logger.InfoFormat("NewDialog mod calls has less arguments.");	
						return false;
					}
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
				// disable the mod
				else if (call == "DisableThisShit") 
				{
					string text = args[1] as string;
					Main.NewText("BetterCamera mod disabled for good , next time dont boot bettercamera mod with "+text);
					DisableThisShit = true;
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

	public class ModifyProjectileVisual : GlobalProjectile 
	{
		// load if needed
		public override bool IsLoadingEnabled(Mod mod) => CameraConfig.Get.BetterBomb;

		// seems like this hook initialized once , what a big poo
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return entity.aiStyle == ProjAIStyleID.Explosive;
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
			projectile.scale = 1f + (0.5f - (Math.Clamp(projectile.timeLeft,0,180) / 360f));
            return base.PreDraw(projectile, ref lightColor);
        }
        public override Color? GetAlpha(Projectile projectile, Color lightColor)
        {
			int tick = projectile.timeLeft % 30;
			if (tick == 0 || tick == 1 || tick == 3) {
				return Color.Red;
			}
            return base.GetAlpha(projectile, lightColor);
        }
        public override void PostDraw(Projectile projectile, Color lightColor)
        {
			if (projectile.timeLeft > 60 * 60) return;
			var distance = Main.MouseWorld.Distance(projectile.Center);
			if (distance < 150f) 
			{
				ChatManager.DrawColorCodedString(Main.spriteBatch, 
				FontAssets.MouseText.Value,
				$"{projectile.timeLeft/60}s",
				projectile.Center - Main.screenPosition, Color.White * (1f- (distance / 150f)),
				0f, Vector2.One, Vector2.One);
			}
        }    
	}

	// Check runned npc ( optional )
	public class RunnedNPC : GlobalNPC 
	{
        public override bool IsLoadingEnabled(Mod mod)
        {
            return CameraConfig.Get.BetterBossDialog_UseGlobalNPC;
        }
        
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
			dialogText = new TypeWriter("");
		}

		public override void OnModUnload()
		{
			dialogText = null;
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch) 
		{
			// do better binoculars
			var font = FontAssets.MouseText.Value;
			if (CameraConfig.Get.BetterBinoculars && Main.LocalPlayer.HeldItem.type == ItemID.Binoculars) 
			{
				TextSnippet[] snippets = ChatManager.ParseMessage("Click to move", Color.White).ToArray();
				Vector2 messageSize = ChatManager.GetStringSize(font, snippets, Vector2.One);
				Vector2 pos = Main.LocalPlayer.Center - Main.screenPosition + new Vector2(0,
				40 + (float)(Math.Sin(Main.GameUpdateCount / 45f)*8.0f));
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, snippets, pos, 
				0f, messageSize, Vector2.One, out int hover);
			}

			// dont draw if there is nothing bruh
			if (dialogText is null) return;
			if (dialogText.text == "") return;

			// dialog text
			string[] textList = dialogText.text.Split('\n');

			// settings
			float scale = CameraConfig.Get.BetterBossDialog_Scale;
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
				pos += CameraConfig.Get.BetterBossDialog_Offset;

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

				pos += CameraConfig.Get.BetterBossDialog_Offset;

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
			if (CameraConfig.Get.BetterBossDialog_UnlockOffset) {

				// tell the player to actually do
                dialogText.Set("Test", "Use Arrow Keys to move this gui \n press SPACE if you done", Color.Orange);
				dialogText.text = dialogText.originalText;

				// Move the dialog
				if (Main.keyState.IsKeyDown(Keys.Right)) {
					CameraConfig.Get.BetterBossDialog_Offset.X += 4;
					CameraConfig.SaveConfig();
				}
				if (Main.keyState.IsKeyDown(Keys.Left)) {
					CameraConfig.Get.BetterBossDialog_Offset.X -= 4;
					CameraConfig.SaveConfig();
				}
				if (Main.keyState.IsKeyDown(Keys.Up)) {
					CameraConfig.Get.BetterBossDialog_Offset.Y -= 4;
					CameraConfig.SaveConfig();
				}
				if (Main.keyState.IsKeyDown(Keys.Down)) {
					CameraConfig.Get.BetterBossDialog_Offset.Y += 4;
					CameraConfig.SaveConfig();
				}
				if (Main.keyState.IsKeyDown(Keys.Space)) {
					CameraConfig.Get.BetterBossDialog_UnlockOffset = false;
					CameraConfig.SaveConfig();
				}
			}

			// Update dialog text
			if (dialogText.originalText == "") return;
			
			if (dialogText.Done()) 
			{
				dialogText.timeElapsed++;
				if (dialogText.timeElapsed >= 60 * CameraConfig.Get.BetterBossDialog_Time) dialogText.Reset();
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
		public string originalText = "";
		public string text = "";
		public int index;
		public int timeElapsed;

		public TypeWriter(string text) {
			this.text = text;
		}

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

	// Provides few info about music
	// Perhaps it is time to remove music and biome info because gabe already made a better one
	// public class MusicInfo 
	// {
	// 	public string name;
	// 	public string composer;
	// 	public int musicId;
	// 	public int musicBoxId;

	// 	public MusicInfo(string name = "",string composer = "", int musicId = 0 , int musicBoxId = 0)
	// 	{
	// 		this.name = name;
	// 		this.composer = composer;
	// 		this.musicId = musicId;
	// 		this.musicBoxId = musicBoxId;
	// 	}
	// }

	// They replaced hookendpointmanager with monomodhooks, cool i guess ??

	public static class Hacc
	{
		public const BindingFlags defaultBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

		public static void Add() 
		{
			// There is a bunch of option to modify this , but i choose how overhaul did it

			// On_ModifyScreenPosition += ScreenPatch;
			// Terraria.On_NPC.AI += AIPatch;

			// MonoModHooks.Add(typeof(Mod).Assembly.GetType(
			// 	"Terraria.ModLoader.PlayerLoader").GetMethod(
			// 	"ModifyScreenPosition",defaultBindingFlags),
			// 	ScreenPatch);

			if (!CameraConfig.Get.BetterBossDialog_UseGlobalNPC) 
			{
				MonoModHooks.Add(typeof(Mod).Assembly.GetType(
					"Terraria.ModLoader.NPCLoader").GetMethod(
					"NPCAI",defaultBindingFlags), NPCAIPatch);
			}

			Terraria.On_Main.DoDraw_UpdateCameraPosition += ScreenPatch;
			Terraria.Audio.On_SoundEngine.PlaySound_int_int_int_int_float_float += AudioPatch;
			Terraria.On_Main.NewText_object_Nullable1 += NewTextPatch;
		}

        public static void Remove() {
			// On_ModifyScreenPosition -= ScreenPatch;
			// Terraria.On_NPC.AI -= AIPatch;
			Terraria.On_Main.DoDraw_UpdateCameraPosition -= ScreenPatch;
			Terraria.Audio.On_SoundEngine.PlaySound_int_int_int_int_float_float -= AudioPatch;
			Terraria.On_Main.NewText_object_Nullable1 -= NewTextPatch;
		}


        public static void NPCAIPatch(orig_NPCAI orig , NPC npc)
		{
			BetterCamera.currentRunnedNPC = npc.whoAmI;
			orig(npc);
			BetterCamera.currentRunnedNPC = -1;
		}

		public delegate void orig_NPCAI(NPC npc);

        private static SoundEffectInstance AudioPatch(On_SoundEngine.orig_PlaySound_int_int_int_int_float_float orig,
		 int type, int x, int y, int Style, float volumeScale, float pitchOffset)
        {
			if (CameraConfig.Get.ScreenShake_OnRoar) 
			{
				if (type == 36) 
				{
					ShakeThatAss(new Vector2(x,y),volumeScale * 6f);
				}
			}
            return orig(type,x,y,Style,volumeScale,pitchOffset);
        }

        private static void ScreenPatch(On_Main.orig_DoDraw_UpdateCameraPosition orig)
        {
			orig();
			if (!Main.gameMenu) 
			{
				CameraPlayer.UpdateCamera(Main.LocalPlayer);
			}
        }


		public static void ShakeThatAss(Vector2 position ,float strength = 1f, int frames = 20) 
		{
			PunchCameraModifier modifier = new(position,
			(Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), strength * CameraConfig.Get.ScreenShake_Intensity, 6f, frames, 1000f, "RoarHappens");
			Main.instance.CameraModifiers.Add(modifier);
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
				foreach (var item in CameraConfig.Get.BetterBossDialog_BlackList)
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

	}
}

// if i become popular i should delete all of my browser history