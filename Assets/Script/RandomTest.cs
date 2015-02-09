using UnityEngine;
using System.Collections;

public class RandomTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        if (GUI.Button(new Rect(50, 50, 100, 50), "Random"))
        {
            for (int i = 0; i < 100000; ++i)
            {
                int iRand = Random.Range(0, i);
                Random.seed = Time.frameCount;
                if (iRand == i) 
                { 
                    Debug.LogWarning(string.Format("[0,{0}]Current Random = {1}" ,i ,iRand));
                }
            }
        }
    }
}
