using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{

    public int SceneReference = 0;
    public List<string> sceneStrings;

    // Start is called before the first frame update
    void Start()
    {
        if (sceneStrings.Count == 0)
        {
            Debug.Log("No Scene Names added");
            Debug.Break();
        }
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            SceneManager.LoadScene(sceneStrings[(SceneReference++)]);
            if (SceneReference == sceneStrings.Count) SceneReference = 0;
        }
    }
}
