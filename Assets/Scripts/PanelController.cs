using UnityEngine;
using System.Collections;

public class PanelController : MonoBehaviour {

    public bool isDestroying;

    // Use this for initialization
    void Start()
    {
        isDestroying = false;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Delete()
    {
        Destroy(gameObject);
    }
}
