using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
public class LoadScene : MonoBehaviour
{
    [Scene] [SerializeField] string sceneToLoad;

    public void MyLoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
