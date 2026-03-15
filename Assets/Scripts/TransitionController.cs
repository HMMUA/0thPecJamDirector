using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionController : MonoBehaviour
{
    public Animator transitionAnimator; // 过渡动画控制器引用
    public DirectorSystem directorSystem; // 导演系统引用


    public SpriteRenderer[] image;//曲绘

    public SpriteRenderer[] titleImage;//标题
    public SpriteRenderer[] titleImageOutline;//标题描边
    public SpriteMask[] titleImageMask;//标题遮罩

    public SpriteRenderer[] artistImage;//曲师
    public SpriteRenderer[] artistImageOutline;//曲师描边
    public SpriteMask[] artistImageMask;//曲师遮罩

    public SpriteRenderer[] designerImage;//谱师
    public SpriteRenderer[] designerImageOutline;//谱师描边
    public SpriteMask[] designerImageMask;//谱师遮罩



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTransition(VideoData videoData)
    {
        transitionAnimator.Play("idle", 0, 0); // 重置动画状态
        transitionAnimator.SetTrigger("StartTransition");
        ResetInformation(videoData);
    }

    public void ResetInformation(VideoData videoData)
    {
        string id = videoData.id;

        
        for (int i = 0; i < image.Length; i++)
        {
            image[i].sprite = Resources.Load<Sprite>($"Texture/{id}");//曲绘
        }
        
        for (int i = 0; i < titleImage.Length; i++)
        {
            titleImage[i].sprite = Resources.Load<Sprite>($"Texture/{id}_T");//标题
        }
        for (int i = 0; i < titleImageOutline.Length; i++)
        {
            titleImageOutline[i].sprite = Resources.Load<Sprite>($"Texture/{id}_T_O");//标题描边
        }
        for (int i = 0; i < titleImageMask.Length; i++)
        {
            titleImageMask[i].sprite = Resources.Load<Sprite>($"Texture/{id}_T");//标题遮罩
        }



        for (int i = 0; i < designerImage.Length; i++)
        {
            designerImage[i].sprite = Resources.Load<Sprite>($"Texture/{id}_D");//谱师
        }
        for (int i = 0; i < designerImageOutline.Length; i++)
        {
            designerImageOutline[i].sprite = Resources.Load<Sprite>($"Texture/{id}_D_O");//谱师描边
        }
        for (int i = 0; i < designerImageMask.Length; i++)
        {
            designerImageMask[i].sprite = Resources.Load<Sprite>($"Texture/{id}_D");//谱师遮罩
        }



        for (int i = 0; i < artistImage.Length; i++)
        {
            artistImage[i].sprite = Resources.Load<Sprite>($"Texture/{id}_A");//曲师
        }        
        for (int i = 0; i < artistImageOutline.Length; i++)
        {
            artistImageOutline[i].sprite = Resources.Load<Sprite>($"Texture/{id}_A_O");//曲师描边
        }
        for (int i = 0; i < artistImageMask.Length; i++)
        {
            artistImageMask[i].sprite = Resources.Load<Sprite>($"Texture/{id}_A");//曲师遮罩
        }


// image.sprite = Resources.Load<Sprite>($"Texture/{id}");//曲绘

// laserImage.sprite = Resources.Load<Sprite>($"Texture/{id}"); //镭射曲绘

// // titleImageResources.Load<Sprite>($"Texture/{id}_T");//标题
// // titleImageOutlineResources.Load<Sprite>($"Texture/{id}_T_O");//标题描边

// artistImage.sprite = Resources.Load<Sprite>($"Texture/{id}_A");//曲师
// artistImageOutline.sprite = Resources.Load<Sprite>($"Texture/{id}_A_O");//曲师描边
// artistImageDark.sprite = Resources.Load<Sprite>($"Texture/{id}_A");//曲师暗图
// artistImageMask.sprite = Resources.Load<Sprite>($"Texture/{id}_A");//曲师遮罩

// designerImage.sprite = Resources.Load<Sprite>($"Texture/{id}_D");//谱师
// designerImageOutline.sprite = Resources.Load<Sprite>($"Texture/{id}_D_O");//谱师描边
// designerImageDark.sprite = Resources.Load<Sprite>($"Texture/{id}_D");//谱师暗图
// designerImageMask.sprite = Resources.Load<Sprite>($"Texture/{id}_D");//谱师遮罩
    }

    public void EndTransition()
    {
        directorSystem.PlayNextVideo();
    }
}
