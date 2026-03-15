using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class CanvasinfoController : MonoBehaviour
{   
    public TMP_Text stageText;
    public TMP_Text numberText;

    public DirectorSystem directorSystem; // 导演系统引用
    

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
        stageText.DOFade(0, 1f);
        numberText.DOFade(0, 1f);
    }

    public void ShowInfo()
    {
        stageText.text = directorSystem.playlist[directorSystem.currentIndex+1].stage;
        numberText.text = directorSystem.playlist[directorSystem.currentIndex+1].number;
        stageText.DOFade(1, 1f);
        numberText.DOFade(1, 1f);
    }
}
