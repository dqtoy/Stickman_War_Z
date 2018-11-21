// Decompile from assembly: Assembly-CSharp.dll
using System;
using System.Collections.Generic;
using UnityEngine;

public class StatManager : MonoBehaviour
{
	public static StatManager instance;

	public List<Stat> stats;

	public StatContainer statContainerPrefab;

	protected void Awake()
	{
		StatManager.instance = this;
	}

	private void Start()
	{
		this.Initialize();
	}

	private void Initialize()
	{
		this.stats = new List<Stat>();
		this.stats.Add(new Stat(0, "最高分", "点"));
		this.stats.Add(new Stat(1, "段位", string.Empty));
		this.stats.Add(new Stat(2, "击杀敌人", string.Empty));
		this.stats.Add(new Stat(3, "击杀头目", string.Empty));
		this.stats.Add(new Stat(4, "击杀影子", string.Empty));
		this.stats.Add(new Stat(5, "最快影杀", "秒"));
		this.stats.Add(new Stat(6, "影子闪避", string.Empty));
		this.stats.Add(new Stat(7, "死亡", string.Empty));
		this.stats.Add(new Stat(8, "收集灵魂", "魂"));
		this.stats.Add(new Stat(9, "最高灵魂连击", string.Empty));
		this.stats.Add(new Stat(10, "消耗灵魂", string.Empty));
		this.stats.Add(new Stat(11, "开启礼物", string.Empty));
		this.stats.Add(new Stat(12, "赢得重复物品", string.Empty));
		bool flag = true;
		int[] defaultValue = new int[this.stats.Count];
        int[] arrayInt;
        if (PlayerPrefs.HasKey("statValues")) {

            arrayInt = PlayerPrefsX.GetIntArray("statValues");
        } else
        {
            arrayInt = defaultValue;
        }
		foreach (Stat current in this.stats)
		{
			if (current.id < 3 || current.id > 6)
			{
				StatContainer statContainer = UnityEngine.Object.Instantiate<StatContainer>(this.statContainerPrefab, this.statContainerPrefab.transform.parent);
				if (!flag)
				{
					statContainer.bg.color = new Color(1f, 1f, 1f, 0.05882353f);
				}
				statContainer.stat = current;
				current.statContainer = statContainer;
				current.UpdateStat(arrayInt[current.id]);
				statContainer.descriptionText.text = current.description;
				statContainer.stat.UpdateStat(statContainer.stat.value);
				flag = !flag;
				statContainer.gameObject.SetActive(true);
			}
		}
	}

	public void UpdateStats()
	{
		int[] array = new int[this.stats.Count];
		for (int i = 0; i < this.stats.Count; i++)
		{
			array[i] = this.stats[i].value;
		}
		PlayerPrefsX.SetIntArray("statValues", array);
	}
}
