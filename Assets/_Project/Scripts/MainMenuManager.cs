using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scenes")]
    public AssetReference PlayScene;

    [Header("UI")]
    public Button PlayButton;

    void Start()
    {
        PlayButton.onClick.AddListener(OnPlayButton);
    }

    public void OnPlayButton()
    {
        var loadSceneOperation = Addressables.LoadSceneAsync(PlayScene);
    }
}
