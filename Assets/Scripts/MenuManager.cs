// Decompile from assembly: Assembly-CSharp.dll
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class MenuManager : MonoBehaviour
{
	public static MenuManager instance;

	public bool isOpen;

	public bool isPaused;

	public bool isCounting;

	public CanvasGroup menuCanvas;

	public BlurOptimized blur;

	public CanvasGroup settingsCanvas;

	public CanvasGroup statsCanvas;

	//public CanvasGroup creditsCanvas;

	private int currentPanelIndex;

	public MenuItemPanel[] panels;


	public Text pauseCountdownText;

	public GameObject continueButton;

	public CanvasGroup pauseMenuCanvasGroup;

	public CanvasGroup pauseButtonCanvasGroup;

	private float pauseEndTime;

	protected void Awake()
	{
		this.isCounting = false;
		MenuManager.instance = this;
	}

	private void Start()
	{
		this.currentPanelIndex = 0;
		this.panels[0].isOpen = true;
	}

	private void Update()
	{
		Helpers.UpdateCanvasVisibilty(this.menuCanvas, 2f, this.isOpen, true);
		Helpers.UpdateCanvasVisibilty(this.pauseButtonCanvasGroup, 2f, SceneManager.instance.gameStarted && (SceneManager.instance.isEndless || StoryManager.instance.introCompleted), false);
		Helpers.UpdateCanvasVisibilty(this.pauseMenuCanvasGroup, 2f, this.isPaused, true);
		if (this.isOpen)
		{
			MenuItemPanel[] array = this.panels;
			for (int i = 0; i < array.Length; i++)
			{
				MenuItemPanel menuItemPanel = array[i];
				Helpers.UpdateCanvasVisibilty(menuItemPanel.canvasGroup, 2f, menuItemPanel.isOpen, true);
				menuItemPanel.SetColor();
			}
		}
		if (this.isOpen)
		{
			if (!this.blur.enabled)
			{
				this.blur.blurSize = 0f;
				this.blur.enabled = true;
			}
			else if (this.blur.blurSize < 10f)
			{
				this.blur.blurSize += Time.unscaledDeltaTime * 20f;
			}
			else if (this.blur.blurSize != 10f)
			{
				this.blur.blurSize = 10f;
			}
		}
        else if ( this.blur.enabled)
        {
            if (this.blur.blurSize > 0f)
            {
                this.blur.blurSize -= Time.unscaledDeltaTime * 20f;
            }
            else
            {
                this.blur.blurSize = 0f;
                this.blur.enabled = false;
            }
        }
        if (!this.isPaused && this.isCounting)
		{
			int num = 3 - (int)(Time.unscaledTime - this.pauseEndTime);
			if (num <= 0)
			{
				this.isCounting = false;
				Time.timeScale = 1f;
				this.pauseCountdownText.gameObject.SetActive(false);
			}
			else
			{
				this.pauseCountdownText.text = num.ToString();
			}
		}
	}

	public void Pause()
	{
		this.isPaused = true;
		this.isCounting = false;
		this.continueButton.SetActive(true);
		this.pauseCountdownText.gameObject.SetActive(false);
		MissionManager.instance.missionFadeTime = 0.0;
		MissionManager.instance.SetColors(Color.white, -1);
		Time.timeScale = 0f;
	}

	public void Unpause()
	{
		this.isPaused = false;
		this.isCounting = true;
		MissionManager.instance.missionFadeTime = (double)(Time.time + 3f);
		MissionManager.instance.SetColors(Color.black, -1);
		this.pauseEndTime = Time.unscaledTime;
		this.continueButton.SetActive(false);
		this.pauseCountdownText.gameObject.SetActive(true);
	}

	public void Open()
	{
		if (this.currentPanelIndex == 2)
		{
			AnalyticsManager.instance.ShopPressed();
		}
		this.isOpen = true;
        //AdsController.instance.HideBanner();
	}

	public void Close()
	{
		this.isOpen = false;
        //AdsController.instance.VisibleBanner();
    }

	public void ChangePanel(int panelId)
	{
		this.panels[this.currentPanelIndex].isOpen = false;
		this.currentPanelIndex = panelId;
		this.panels[this.currentPanelIndex].isOpen = true;
	}

	protected void OnApplicationFocus(bool focus)
	{
		//if (this.isPaused)
		//{
		//	return;
		//}
		//if (!focus && SceneManager.instance.gameStarted && (SceneManager.instance.isEndless || StoryManager.instance.introCompleted))
		//{
		//	this.Pause();
		//}
	}

	protected void OnApplicationPause(bool pause)
	{
		//if (this.isPaused)
		//{
		//	return;
		//}
		//if (pause && SceneManager.instance.gameStarted && (SceneManager.instance.isEndless || StoryManager.instance.introCompleted))
		//{
		//	this.Pause();
		//}
	}
}
