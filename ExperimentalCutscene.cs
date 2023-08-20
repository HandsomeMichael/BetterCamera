using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;

namespace BetterCamera.Experimental
{
	// A WORK IN PROGRESS , HOWEVER I WILL NOT CONTINUE THIS LOL
	
	// // easily make cutscene
	// public abstract class BaseKeyFrame 
	// {
	// 	/// <summary>
	// 	/// The time for cutscene in tick
	// 	/// </summary>
	// 	public int time;

	// 	/// <summary>
	// 	/// Apply scene effect
	// 	/// </summary>
	// 	/// <param name="manager"> the current cutscene manager </param>
	// 	/// <returns> returns true if it should go to the next keyframe </returns>
	// 	public virtual bool Apply(CutsceneManager manager) 
	// 	{
	// 		return false;
	// 	}

	// 	/// <summary>
	// 	/// Initialize key frame variables
	// 	/// </summary>
	// 	/// <param name="manager"> the current cutscene manager </param>
	// 	public virtual void Initialize(CutsceneManager manager)) 
	// 	{

	// 	}
	// }

	// public class WaitKeyFrame : BaseKeyFrame 
	// {
    //     public override bool Apply(CutsceneManager manager)
    //     {
	// 		manager.frame++;
	// 		return manager.frame => time;
    //     }
    // }
	// public class ScreenShakeKeyFrame : BaseKeyFrame 
	// {
	// 	public int intensity;

    //     public override bool Apply(CutsceneManager manager)
    //     {
	// 		manager.frame++;

	// 		Main.screenPosition += new Vector2(Main.rand.Next(-intensity, intensity + 1), Main.rand.Next(-intensity, intensity + 1));

	// 		if (manager.frame => time) 
	// 		{
	// 			return true;
	// 		}

    //         return false;
    //     }
    // }
	// public class PosKeyFrame : BaseKeyFrame
	// {
	// 	public Vector2 position;
	// 	public Vector2 oldPosition;
    //     public override bool Apply(CutsceneManager manager)
    //     {
	// 		manager.frame++;

	// 		Main.screenPosition = Vector2.Lerp(oldPosition,position,(float)manager.frame/(float)time);

	// 		if (manager.frame => time) 
	// 		{
	// 			return true;
	// 		}

	// 		return false;
    //     }

    //     public override void Initialize(CutsceneManager manager)
    //     {
    //         oldPosition = Main.screenPosition;
    //     }

    // }

	// //
	// public class CutsceneManager
	// {
	// 	public int frame;

	// 	public bool currentlyRunning;

	// 	public BaseKeyFrame[] keyFrames;
	// 	public int currentKeyFrame;

	// 	public void Run() 
	// 	{
	// 		currentKeyFrame = 0;
	// 		currentlyRunning = true;
	// 	}

	// 	public void Update() 
	// 	{
	// 		if (keyFrames[currentKeyFrame].Apply()) 
	// 		{
	// 			frame = 0;

	// 			if (currentKeyFrame > keyFrames.Length) return;
				
	// 			currentKeyFrame++;
	// 			keyFrames[currentKeyFrame].Initialize();
	// 		}
	// 	}
	// }

	// //
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
}