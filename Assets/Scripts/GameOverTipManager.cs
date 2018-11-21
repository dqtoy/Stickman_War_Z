// Decompile from assembly: Assembly-CSharp.dll
using System;
using UnityEngine;

public class GameOverTipManager : MonoBehaviour
{
	public static GameOverTipManager instance;

	private string[] tips;

	private int lastTipIndex;

	private void Awake()
	{
		GameOverTipManager.instance = this;
	}

	private void Start()
	{
		this.lastTipIndex = PlayerPrefs.GetInt("lastTipIndex", 0);
		this.tips = new string[9];
		this.tips[0] = "完成所有任务解锁下一次段位考校";
		this.tips[1] = "你可以消耗灵魂重新进入失败的段位考校";
		this.tips[2] = "如果你移动过程中没有击中敌人或灵魂，你会产生短暂的晕眩";
		this.tips[3] = "仅当在你没有击中敌人的情况下，收集灵魂才会使你停止向前移动";
		this.tips[4] = "在灵魂消失之前收集到可以加强你的灵魂连击";
		this.tips[5] = "获得灵魂连击不会影响你的得分";
		this.tips[6] = "当你通过段位考校后，主考人的物品就可以购买和掉落了";
		this.tips[7] = "在暂停菜单消耗灵魂可以跳过任务";
		this.tips[8] = "在设置中关闭流血和天气效果可以提高性能";
	}

	public string GetNextTip()
	{
		this.lastTipIndex++;
		if (this.lastTipIndex > this.tips.Length)
		{
			this.lastTipIndex = 1;
		}
		return this.tips[this.lastTipIndex - 1];
	}

	public void Save()
	{
		PlayerPrefs.SetInt("lastTipIndex", this.lastTipIndex);
	}
}
