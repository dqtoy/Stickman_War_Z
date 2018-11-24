// Decompile from assembly: Assembly-CSharp.dll
using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
	public static StoryManager instance;

	public ObscuredInt level;

	public Queue<LevelStep> levelSteps;

	public LevelStep currentStep;

	private int stepCount;

	public bool introCompleted;

	public bool levelCompleted;

	public Npc miniboss;

	public int hitsTaken;

	protected void Awake()
	{
		StoryManager.instance = this;
		//EpicPrefs.Initialize();
	}

	protected void Start()
	{

        this.level = PlayerPrefs.GetInt("storyLevel", 0);

        if (this.level == 0)
		{
			Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -10f);
		}
	}

	protected void Update()
	{
		this.UpdateCurrentStep();
		this.UpdateSpawns();
	}

	public void StartLevel()
	{
		this.hitsTaken = 0;
		if (SceneManager.instance.isBeltExamReady)
		{
			SceneManager.instance.isBeltExamReady = false;
            PlayerPrefsX.SetBool("BeltExam", false);
		}
		SceneManager.instance.inputStarted = false;
		this.introCompleted = false;
		this.levelCompleted = false;
		this.InstantiateSteps();
		this.StartNextStep();
		//AnalyticsManager.instance.PlayedBeltExam();
	}

	public void StartNextStep()
	{
		if (this.levelSteps.Count == 0)
		{
			if (!this.introCompleted)
			{
				this.IntroCompleted();
			}
		}
		else
		{
			this.stepCount++;
			this.currentStep = this.levelSteps.Dequeue();
			this.currentStep.Start();
		}
	}

	private void IntroCompleted()
	{
		SceneManager.instance.targetCameraZ = -7f;
		this.introCompleted = true;
		NpcManager.instance.killCount = 0;
		NpcManager.instance.lieutenantKillCount = 0;
		SceneManager.instance.inputStarted = true;
	}

	public void LevelCompleted()
	{
		this.levelCompleted = true;
		this.introCompleted = false;
		this.currentStep = null;
		this.level = ++this.level;
		if (this.level > 15)
		{
			this.level = this.hitsTaken;
		}
		StatManager.instance.stats[1].UpdateStat(this.level);
		PlayerPrefs.SetInt("storyLevel", this.level);
		ItemManager.instance.UpdateBelt();
		//if (this.level > 15)
		//{
		//	LeaderboardsManager.instance.PostShadowData();
		//}
		ScreenshotManager.instance.TakeScreenShot(true);
		SceneManager.instance.GameOver(true);
	}

	private void UpdateCurrentStep()
	{
		if (this.introCompleted || this.currentStep == null)
		{
			return;
		}
		if (this.currentStep.Update())
		{
			this.StartNextStep();
		}
	}

	public void StartFatality()
	{
		AudioManager.instance.StartFatalitySound();
		CharacterManager.instance.runTargetPos = this.miniboss.transform.position - new Vector3((float)this.miniboss.side, 0f, 0f);
		if (CharacterManager.instance.transform.position.x * (float)this.miniboss.side > CharacterManager.instance.runTargetPos.x * (float)this.miniboss.side)
		{
			CharacterManager.instance.transform.position = new Vector3(CharacterManager.instance.runTargetPos.x, CharacterManager.instance.runTargetPos.y, CharacterManager.instance.transform.position.z);
		}
		this.miniboss.weapon1Sprite.sprite = null;
		this.miniboss.weapon2Sprite.sprite = null;
		this.miniboss.weapon2hSprite.sprite = null;
		SceneManager.instance.gameStarted = false;
		SceneManager.instance.chaserObject.transform.localPosition = new Vector3((float)CharacterManager.instance.side, -1.5f, 0f);
		SceneManager.instance.targetCameraZ = -7.5f;
		CharacterManager.instance.spine.state.SetAnimation(0, ItemManager.instance.currentWeapon.category.id + "Fatality", false);
		this.miniboss.spine.state.SetAnimation(0,"FatalityGotHit", false);
	}

	public void EndFatality()
	{
		AudioManager.instance.EndFatalitySound();
		ItemManager.instance.ToggleTrails(false);
		this.miniboss.StartJustBlood();
	}

	public void AfterFatality()
	{
		SceneManager.instance.targetCameraZ = -9f;
		SceneManager.instance.chaserObject.transform.localPosition = Vector3.zero;
		this.miniboss.ChangeState(Npc.State.Dead);
		ItemManager.instance.GotItem(0, string.Empty, true, false);
	}

	private void UpdateSpawns()
	{
		if (!this.introCompleted || this.levelCompleted)
		{
			return;
		}
	}

	public void GameOver()
	{
		if (this.level > 15)
		{
			//if (this.level == int.Parse(LeaderboardsManager.instance.shadowWarriorData[4]))
			//{
			//	this.level = --this.level;
			//}
			if (this.hitsTaken > this.level)
			{
				this.level = this.hitsTaken;
				StatManager.instance.stats[1].UpdateStat(this.level);
				PlayerPrefs.SetInt("storyLevel", this.level);
				//LeaderboardsManager.instance.PostShadowData();
			}
		}
		//AnalyticsManager.instance.KilledByExaminer();
		this.introCompleted = false;
		this.currentStep = null;
	}

	private void InstantiateSteps()
	{
		this.stepCount = 0;
		this.levelSteps = new Queue<LevelStep>();
		this.levelSteps.Enqueue(new LevelStep_Wait(1f));
		this.levelSteps.Enqueue(new LevelStep_Animation(CharacterManager.instance.spine, string.Empty, true, 0f, false));
		this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, Vector3.zero));
		switch (this.level + 1)
		{
		case 1:
			this.levelSteps.Enqueue(new LevelStep_Tip("#班赞", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "fist2", "fullmetaljacket"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "FistRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "FistIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("让游戏开始吧！！", this.miniboss.head, 2.5f, false));
            this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "FistRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
		case 2:
			this.levelSteps.Enqueue(new LevelStep_Tip("#乔卡曼", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "dagger4", "pennywise"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "DaggerRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "DaggerIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("你想知道我为什么用刀吗？ 枪太快了。 ", this.miniboss.head, 0.5f, false));
            this.levelSteps.Enqueue(new LevelStep_Speech(" 枪确实太快。", this.miniboss.head, 0.5f, false));
            this.levelSteps.Enqueue(new LevelStep_Speech("你无法品味所有的…微小的情感。", this.miniboss.head, 0.5f, false));
            this.levelSteps.Enqueue(new LevelStep_Speech(" 在…你看，在他们生命的最后时刻，人们向你展示了他们真实的一面。", this.miniboss.head, 1f, false));
            this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "DaggerRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
		case 3:
			this.levelSteps.Enqueue(new LevelStep_Tip("#啤酒佬", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "dagger2", "beertime"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "DaggerRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "DaggerIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("想要……嗝……一些啤酒吗?嗝…", this.miniboss.head, 2.5f, false));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "DaggerRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
		case 4:
			this.levelSteps.Enqueue(new LevelStep_Tip("#屠夫", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "axe5", "spikey"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "AxeRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "AxeIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("靠近点新鲜的肉！", this.miniboss.head, 2.5f, false));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "AxeRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
		case 5:
			this.levelSteps.Enqueue(new LevelStep_Tip("#Jako Tonokome", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "sword1h2", "maskofthedead"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword1hRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword1hIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("亲爱的？我生命中的光。我不会伤害你的。", this.miniboss.head, 1f, false));
            this.levelSteps.Enqueue(new LevelStep_Speech(" 我不会伤害你的。", this.miniboss.head, 0.5f, false));
            this.levelSteps.Enqueue(new LevelStep_Speech(" 我只是想打烂你的脑瓜！", this.miniboss.head, 1f, false));
            this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword1hRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
		case 6:
			this.levelSteps.Enqueue(new LevelStep_Tip("#海森伯", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "fist4", "troll"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "FistRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "FistIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("我是那个敲门的人。", this.miniboss.head, 2.5f, false));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "FistRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
                break;
		case 7:
			this.levelSteps.Enqueue(new LevelStep_Tip("#哈诺班雷托", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "spear5", "vietnamese"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "SpearRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "SpearIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("曾经有一个人口调查员试图测试我。", this.miniboss.head, 1f, false));
            this.levelSteps.Enqueue(new LevelStep_Speech(" 我就着蚕豆和葡萄酒，把他的肝脏吃掉了。", this.miniboss.head, 1.5f, false));
            this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "SpearRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
		case 8:
			this.levelSteps.Enqueue(new LevelStep_Tip("#埃托钦雷", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "sword2h2", "viking"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword2hRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword2hIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("进来，小牧师。右边，小牧师。", this.miniboss.head, 1.5f, false));
                this.levelSteps.Enqueue(new LevelStep_Speech("走新娘专用道。 这是卡尔克萨。", this.miniboss.head, 1f, false));
                this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword2hRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
		case 9:
			this.levelSteps.Enqueue(new LevelStep_Tip("#雷托罗马库巴杜", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "sword1h6", "spanish"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword1hRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword1hIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("众神再次眷顾了该死的混蛋！", this.miniboss.head, 2.5f, false));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword1hRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
		case 10:
			this.levelSteps.Enqueue(new LevelStep_Tip("#国王", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "spear3", "theking"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "SpearRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "SpearIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("智者说只有傻瓜才会冲进来。", this.miniboss.head, 2.5f, false));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "SpearRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
		case 11:
			this.levelSteps.Enqueue(new LevelStep_Tip("#梅葛", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "axe7", "ottoman"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "AxeRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "AxeIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("杀女人和杀男人不一样。", this.miniboss.head, 1.5f, false));
            this.levelSteps.Enqueue(new LevelStep_Speech(" 你必须换个方式扣动扳机。", this.miniboss.head, 1f, false));
                this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "AxeRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
		case 12:
			this.levelSteps.Enqueue(new LevelStep_Tip("#乔纳林德", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "axe8", "dumbo"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "AxeRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "AxeIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("你想知道眼球被刺破后会发生什么吗？ ", this.miniboss.head, 1f, false));
            this.levelSteps.Enqueue(new LevelStep_Speech(" 你知道喉咙被割裂后脖子会流多少血吗？", this.miniboss.head, 1.5f, false));
            this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "AxeRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
		case 13:
			this.levelSteps.Enqueue(new LevelStep_Tip("#凯撒", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "dagger6", "caesar"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "DaggerRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "DaggerIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("我来我见我征服", this.miniboss.head, 2.5f, false));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "DaggerRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
		case 14:
			this.levelSteps.Enqueue(new LevelStep_Tip("#拉格纳", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "axe1", "ragnar"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "AxeRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "AxeIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("我不会因为累了就停下来，只有完成的时候我才会停下。", this.miniboss.head, 2.5f, false));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "AxeRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
		case 15:
			this.levelSteps.Enqueue(new LevelStep_Tip("#皇帝", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "sword2h6", "emperor"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword2hRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword2hIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("你很接近成为终极战士了。", this.miniboss.head, 1f, false));
            this.levelSteps.Enqueue(new LevelStep_Speech(" 但我很乐意杀了你！", this.miniboss.head, 1.5f, false));
            this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword2hRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
		case 16:
			this.levelSteps.Enqueue(new LevelStep_Tip("#白眉道长", true, 0f, false));
			this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "sword2h5", "paimei"));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword2hRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword2hIdle", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_Speech("做得好啊，战士。\n 你比我所有的学生都优秀。", this.miniboss.head, 2.5f, false));
			this.levelSteps.Enqueue(new LevelStep_Speech("影子大师？", CharacterManager.instance.head, 1f, false));
			this.levelSteps.Enqueue(new LevelStep_Speech("是的，我是白眉道长，影子大师。\n 你要是能打败我这个头衔就是你的了！ ", this.miniboss.head, 2.5f, false));
			this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword2hRun", true, 0f, true));
			this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
			this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			break;
            case 17:
                this.levelSteps.Enqueue(new LevelStep_Tip("#影子武士", true, 0f, false));
                this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, "sword2h2", "thor"));
                this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
                this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword2hRun", true, 0f, true));
                this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0.5f, false));
                this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
                this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword2hIdle", true, 0f, true));
                this.levelSteps.Enqueue(new LevelStep_Speech("所以你觉得你能拿到我的头衔？哈！", this.miniboss.head, 2.5f, false));
                this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, "Sword2hRun", true, 0f, true));
                this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
                this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
                break;
            default:
				this.levelSteps.Enqueue(new LevelStep_Tip("#自己", true, 0f, false));
				this.levelSteps.Enqueue(new LevelStep_SummonNpc(Color.black, -1, 0f, 1f, true, ItemManager.instance.currentWeapon.id, ItemManager.instance.currentHat.id));
				this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, 0f));
				this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, ItemManager.instance.currentWeapon.category.id + "Run", true, 0f, true));
				this.levelSteps.Enqueue(new LevelStep_ObjectMovement(this.miniboss.gameObject, 0f, false));
				this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, this.miniboss.gameObject, Vector3.zero, Vector3.zero));
				this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, ItemManager.instance.currentWeapon.category.id + "Idle", true, 0f, true));
				this.levelSteps.Enqueue(new LevelStep_Speech("当只剩下你自己时，你需要面对最强大的敌人。", this.miniboss.head, 2.5f, false));
				this.levelSteps.Enqueue(new LevelStep_Animation(this.miniboss.spine, ItemManager.instance.currentWeapon.category.id + "Run", true, 0f, true));
				this.levelSteps.Enqueue(new LevelStep_NpcEdit(this.miniboss, this.level, -1f));
				this.levelSteps.Enqueue(new LevelStep_CameraFocus(CharacterManager.instance.gameObject, null, Vector3.zero, new Vector3(0f, -1.5f, 0f)));
			
			break;
		}
		this.levelSteps.Enqueue(new LevelStep_Tip(string.Empty, false, 0f, false));
	}

	public int GetCoinStageCount(int l)
	{
		int num = 3;
		num += this.level / 150 * 7;
		if (num > 10)
		{
			num = 10;
		}
		return num;
	}

	public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition)
	{
		Ray ray = Camera.main.ScreenPointToRay(screenPosition);
		Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, CharacterManager.instance.transform.position.z));
		float distance;
		plane.Raycast(ray, out distance);
		return ray.GetPoint(distance);
	}
}
