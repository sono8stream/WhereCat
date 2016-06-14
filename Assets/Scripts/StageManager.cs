using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StageManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ChangeScene(int sceneNo)
    {
        SceneManager.LoadScene(sceneNo);
    }
}
