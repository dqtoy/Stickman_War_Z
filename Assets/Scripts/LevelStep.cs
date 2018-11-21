﻿// Decompile from assembly: Assembly-CSharp.dll
using System;
using UnityEngine;

public class LevelStep
{
	public float startTime;

	public virtual void Start()
	{
		this.startTime = Time.unscaledTime;
	}

	public virtual bool Update()
	{
		return true;
	}
}
