using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AssetManager : MonoBehaviour
{
    public static AssetManager Instance { get; private set; }

    [SerializeField] private List<AssetReference> gOassetReferences;
    [SerializeField] private List<AssetReference> materialReferences;
    [SerializeField] private List<AssetReference> textureReferences;

    [SerializeField] private List<AssetReference> levelGroup0;
    [SerializeField] private List<AssetReference> levelGroup1;

    public int lastLoadedGroup { get; private set; }

    public Dictionary<string, GameObject> GOLoadedAssets { get; private set; }
    public Dictionary<string, Material> MatLoadedAssets { get; private set; }
    public Dictionary<string, Texture> TexLoadedAssets { get; private set; }

    public event Action OnLoadComplete;
    public event Action OnLevelLoadComplete;

    public bool MainAssetsLoaded { get; private set; }

    [SerializeField] private bool useRemoteAssets = true;

    private string localURL = "http://localhost:3000/";
    private string cloudURL = "https://myserver.com/";
    public string LocalURL => localURL;
    public string CloudURL => cloudURL;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        MainAssetsLoaded = false;
        lastLoadedGroup = -1;

        if (useRemoteAssets) Addressables.ResourceManager.InternalIdTransformFunc += ChangeAssetUrlToPrivateServer;

        GOLoadedAssets = new Dictionary<string, GameObject>();
        MatLoadedAssets = new Dictionary<string, Material>();
        TexLoadedAssets = new Dictionary<string, Texture>();
    }

    protected string ChangeAssetUrlToPrivateServer(IResourceLocation location)
    {
        string assetURL = location.InternalId;

        if (location.InternalId.IndexOf(localURL) != -1) assetURL = location.InternalId.Replace(localURL, cloudURL);

        return location.InternalId;
    }

    public void LoadAssets()
    {
        if (MainAssetsLoaded)
        {
            OnLoadComplete?.Invoke();
            return;
        }

        StartCoroutine(LoadAssetsCoroutine());
    }

    private IEnumerator LoadAssetsCoroutine()
    {
        int assetsToLoad = gOassetReferences.Count + materialReferences.Count + textureReferences.Count;
        int assetsLoaded = 0;

        foreach (AssetReference assetReference in gOassetReferences)
        {
            AsyncOperationHandle<GameObject> handle =
           assetReference.LoadAssetAsync<GameObject>();
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                String assetName = handle.Result.name.Split(" ")[0];
                GOLoadedAssets.Add(assetName, handle.Result);
                assetsLoaded++;
            }
        }

        foreach (AssetReference assetReference in materialReferences)
        {
            AsyncOperationHandle<Material> handle =
           assetReference.LoadAssetAsync<Material>();
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                String assetName = handle.Result.name.Split(" ")[0];
                MatLoadedAssets.Add(assetName, handle.Result);
                assetsLoaded++;
            }
        }

        foreach (AssetReference assetReference in textureReferences)
        {
            AsyncOperationHandle<Texture> handle =
           assetReference.LoadAssetAsync<Texture>();
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                String assetName = handle.Result.name.Split(" ")[0];
                TexLoadedAssets.Add(assetName, handle.Result);
                assetsLoaded++;
            }
        }

        if (assetsLoaded == assetsToLoad)
        {
            OnLoadComplete?.Invoke();
            MainAssetsLoaded = true;
        }
    }

    public void LoadLevelGroup(int index)
    {
        if (index == lastLoadedGroup)
        { 
            OnLevelLoadComplete?.Invoke();
            return;
        }

        StartCoroutine(LoadLevelAssets(index));
    }

    private IEnumerator LoadLevelAssets(int index)
    {
        int assetsToLoad = 4;
        int assetsLoaded = 0;

        if(GOLoadedAssets.ContainsKey("Bottom"))
        {
            Addressables.ReleaseInstance(GOLoadedAssets["Bottom"]);
        }
        if (GOLoadedAssets.ContainsKey("Left"))
        {
            Addressables.ReleaseInstance(GOLoadedAssets["Left"]);
        }
        if (GOLoadedAssets.ContainsKey("Right"))
        {
            Addressables.ReleaseInstance(GOLoadedAssets["Right"]);
        }
        if (GOLoadedAssets.ContainsKey("Top"))
        {
            Addressables.ReleaseInstance(GOLoadedAssets["Top"]);
        }

        List<AssetReference> assets = new List<AssetReference>();
        switch(index)
        {
            case 0:
                assets = levelGroup0;
                break;
            case 1:
                assets = levelGroup1;
                break;
            default:
                StopAllCoroutines();
                break;
        }

        foreach (AssetReference assetReference in assets)
        {
            AsyncOperationHandle<GameObject> handle =
           assetReference.LoadAssetAsync<GameObject>();
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                String assetName = handle.Result.name.Split(" ")[0];
                GOLoadedAssets.Add(assetName, handle.Result);
                assetsLoaded++;
            }
        }

        if (assetsLoaded == assetsToLoad)
        {
            OnLevelLoadComplete?.Invoke();
            lastLoadedGroup = index;
        }
    }

    public GameObject GetGameObjectInstance(string assetName)
    {
        if (GOLoadedAssets.ContainsKey(assetName))
        {
            return Instantiate(GOLoadedAssets[assetName]);
        }
        Debug.LogError($"Asset '{assetName}' not found.");
        return null;
    }

    public Material GetMaterialWithTextureAsset(string assetName)
    {
        if (MatLoadedAssets.ContainsKey(assetName) && TexLoadedAssets.ContainsKey(assetName))
        {
            MatLoadedAssets[assetName].SetTexture("_MainTex", TexLoadedAssets[assetName]);
            return MatLoadedAssets[assetName];
        }
        Debug.LogError($"Asset '{assetName}' not found.");
        return null;
    }

    public Material GetMaterialAsset(string assetName)
    {
        if (MatLoadedAssets.ContainsKey(assetName))
        {
            return MatLoadedAssets[assetName];
        }
        Debug.LogError($"Asset '{assetName}' not found.");
        return null;
    }
}
