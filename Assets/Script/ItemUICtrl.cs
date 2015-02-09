using UnityEngine;
using System.Collections;

public class ItemUICtrl : MonoBehaviour {

    public UILabel mLblItemName;
    public UILabel mLblIndex;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetItemData(ItemInfo itmInfo)
    {
        mLblIndex.text = itmInfo.mIndex.ToString();
        mLblItemName.text = itmInfo.mStrName;

        if (itmInfo is ResItemInfo)
        {
            
        }
        else
        {

        }
    }
}
