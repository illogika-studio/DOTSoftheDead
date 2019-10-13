using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BootManager : MonoBehaviour
{
    [Header("Scenes")]
    public AssetReference MenuScene;

    void Start()
    {
        var loadSceneOperation = Addressables.LoadSceneAsync(MenuScene);
    }
}
