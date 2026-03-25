using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class CanvasinfoController : MonoBehaviour
{   
    public TMP_Text stageText;
    public TMP_Text numberText;

    public TMP_Text stageTextShadow;
    public TMP_Text numberTextShadow;

    public DirectorSystem directorSystem; // 导演系统引用

    public Animator animator; // 动画组件引用
    public Image stageSprite; // 用于显示背景图片的SpriteRenderer
    public Sprite neichang;
    public Sprite waichang;
    public Sprite heima;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HideInfo()
    {
        // stageText.DOFade(0, 1f);
        // numberText.DOFade(0, 1f);
        AniHideInfo();
    }

    public void ShowInfo()
    {
        // stageText.text = directorSystem.playlist[directorSystem.currentIndex+1].stage;
        // numberText.text = directorSystem.playlist[directorSystem.currentIndex+1].number;
        // stageText.DOFade(1, 1f);
        // numberText.DOFade(1, 1f);
        AniShowInfo();
    }

    public void ShowInterval()
    {
        // stageText.text = "INTERVAL";
        // numberText.text = "";
        // stageText.DOFade(1, 1f);
        // numberText.DOFade(1, 1f);
        AniShowInterval();
    }

    public void AniHideInfo()
    {
        Debug.Log("HideInfo");
        animator.Play("Hide", 0, 0);
    }

    public void AniShowInfo()
    {
        Debug.Log("ShowInfo");
        stageText.text = directorSystem.playlist[directorSystem.currentIndex+1].submissionTrack;
        numberText.text = directorSystem.playlist[directorSystem.currentIndex+1].number;
        stageTextShadow.text = stageText.text;
        numberTextShadow.text = numberText.text;


        
        if(directorSystem.playlist[directorSystem.currentIndex+1].isMainStage == 1)
        {
            //内场
            stageSprite.gameObject.SetActive(true);
            stageSprite.sprite = neichang;
        }
        else if(directorSystem.playlist[directorSystem.currentIndex+1].isDarkHorse)
        {
            //黑马
            stageSprite.gameObject.SetActive(true);
            stageSprite.sprite = heima;
        }
        else if(directorSystem.playlist[directorSystem.currentIndex+1].isMainStage == 0)
        {
            //外场
            stageSprite.gameObject.SetActive(true);
            stageSprite.sprite = waichang;
        }
        else
        {
            //无场
            stageSprite.gameObject.SetActive(true);
            stageSprite.gameObject.SetActive(false);
        }

        animator.Play("ShowNPut", 0, 0);
    }

    public void AniShowInterval()
    {
        stageSprite.gameObject.SetActive(false);
        Debug.Log("ShowInterval");
        stageText.text = "INTERVAL";
        numberText.text = "";
        stageTextShadow.text = "INTERVAL";
        numberTextShadow.text = "";
        stageSprite.sprite = null;
        animator.Play("ShowNormal", 0, 0);
    }

}
