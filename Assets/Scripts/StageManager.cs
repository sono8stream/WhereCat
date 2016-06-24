using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StageManager : MonoBehaviour {

    [SerializeField]
    AudioClip enter;
    AudioSource audio;

    // Use this for initialization
    void Start()
    {
        DataController.Instance.Load();
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeScene(int sceneNo)
    {
        audio.PlayOneShot(enter);
        SceneManager.LoadScene(sceneNo);
    }
}
