using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BundleLoader : MonoBehaviour
{
    public string path;

    // Start is called before the first frame update
    void Start()
    {
        AssetBundle bundle = AssetBundle.LoadFromFile(path);

        foreach (var scenePath in bundle.GetAllScenePaths())
        {
            SceneManager.LoadScene(scenePath, LoadSceneMode.Additive);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
