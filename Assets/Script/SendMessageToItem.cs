using UnityEngine;
using System.Collections;

public class SendMessageToItem : MonoBehaviour {

    public Transform ItemTrans = null;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        if (GUI.Button(new Rect(50, 50, 100, 100), "SendMessageToItem"))
        {
            ItemTrans.gameObject.SendMessage("OnClick", null, SendMessageOptions.DontRequireReceiver);
        }
        if (GUI.Button(new Rect(50, 250, 100, 100), "calcChild"))
        {
            Component[] gos = ItemTrans.GetComponentsInChildren(typeof(Transform));

            Debug.Log(gos.Length);
            foreach (Transform child in transform)
            {
                Debug.Log(child.gameObject.name);
            }
        }
        
    }
}
