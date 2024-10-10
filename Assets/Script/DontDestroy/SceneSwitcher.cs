using MajdataPlay;
using MajdataPlay.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{
    Animator animator;
    public Image SubImage;
    public Image MainImage;
    public static SceneSwitcher Instance;
    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        DontDestroyOnLoad(this);
    }

    public void SwitchScene(int index)
    {
        StartCoroutine(SwitchSceneInternal(index));
    }

    IEnumerator SwitchSceneInternal(int index)
    {
        SubImage.sprite = SkinManager.Instance.SelectedSkin.SubDisplay;
        MainImage.sprite = SkinManager.Instance.SelectedSkin.LoadingSplash;
        animator.SetBool("In", true);
        yield return new WaitForSeconds(0.25f);
        yield return SceneManager.LoadSceneAsync(index);
        animator.SetBool("In", false);
        
    }
}
