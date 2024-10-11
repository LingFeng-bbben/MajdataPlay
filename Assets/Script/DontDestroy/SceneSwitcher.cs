using Cysharp.Threading.Tasks;
using MajdataPlay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#nullable enable
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

    public void SwitchScene(int sceneIndex)
    {
        SwitchSceneInternal(sceneIndex).Forget();
    }
    public void SwitchScene(string sceneName)
    {
        SwitchSceneInternal(sceneName).Forget();
    }
    async UniTaskVoid SwitchSceneInternal(int sceneIndex)
    {
        SubImage.sprite = SkinManager.Instance.SelectedSkin.SubDisplay;
        MainImage.sprite = SkinManager.Instance.SelectedSkin.LoadingSplash;
        animator.SetBool("In", true);
        await UniTask.Delay(250);
        await SceneManager.LoadSceneAsync(sceneIndex);
        animator.SetBool("In", false);
    }
    async UniTaskVoid SwitchSceneInternal(string sceneName)
    {
        SubImage.sprite = SkinManager.Instance.SelectedSkin.SubDisplay;
        MainImage.sprite = SkinManager.Instance.SelectedSkin.LoadingSplash;
        animator.SetBool("In", true);
        await UniTask.Delay(250);
        await SceneManager.LoadSceneAsync(sceneName);
        animator.SetBool("In", false);
    }
}
