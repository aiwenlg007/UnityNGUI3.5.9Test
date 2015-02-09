using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemInfo
{
    public string mStrName = "Drag";
    public int mIndex = 0;
}
public class EqptItemInfo : ItemInfo
{
    public int Exp = 3;

}

public class ResItemInfo : ItemInfo
{
    public int iCount = 3;

}

public class ArrayRefTest : MonoBehaviour
{
    private ArrayList mTestDragList = new ArrayList();

    private ArrayList mTmpList = new ArrayList();

	// Use this for initialization
	void Start () 
    {
        for (int i = 0; i < 10; ++i)
        {
            ResItemInfo resItem = new ResItemInfo();
            resItem.iCount = i + 10000;
            resItem.mStrName = "Res_" + i.ToString();
            mTestDragList.Add(resItem);

            EqptItemInfo eqptItem = new EqptItemInfo();
            eqptItem.Exp = i + 20000;
            eqptItem.mStrName = "Eqpt_" + i.ToString();
            mTestDragList.Add(eqptItem);
        }
	}
    private void PrintArr(ArrayList arrList)
    {
        for (int i = 0; i < arrList.Count; ++i)
        {
            if (arrList[i] is ResItemInfo)
            {
                Debug.LogWarning(string.Format("{0}____{1}" , (arrList[i] as ResItemInfo).mStrName, (arrList[i] as ResItemInfo).iCount));
            }
            else
            {
                Debug.LogWarning(string.Format("{0}____{1}", (arrList[i] as EqptItemInfo).mStrName, (arrList[i] as EqptItemInfo).Exp));
            }
        }
    }
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 80, 50), "Change1"))
        {
            PrintArr(mTestDragList);
            for (int i = 0; i < 10; ++i)
            {

                mTmpList.Add(mTestDragList[i]);
                if (mTmpList[i] is ResItemInfo)
                {
                    (mTmpList[i] as ResItemInfo).iCount = i + 11000;
                }
                else
                {
                    (mTmpList[i] as EqptItemInfo).Exp = i + 22000;
                }
            }
            Debug.LogWarning("=========================");
            PrintArr(mTmpList);
            Debug.LogWarning("=========================");
            PrintArr(mTestDragList);

        }


        if (GUI.Button(new Rect(100, 10, 80, 50), "Change2"))
        {
            PrintArr(mTestDragList);
            for (int i = 0; i < 10; ++i)
            {
                if (mTestDragList[i] is ResItemInfo)
                {
                    ResItemInfo resItem = new ResItemInfo();
                    resItem = mTestDragList[i] as ResItemInfo;
                    mTmpList.Add(resItem); 
                }
                else
                {
                    EqptItemInfo eqptItem = new EqptItemInfo();
                    eqptItem = mTestDragList[i] as EqptItemInfo;
                    mTmpList.Add(eqptItem);
                } 
                if (mTmpList[i] is ResItemInfo)
                {
                    (mTmpList[i] as ResItemInfo).iCount = i + 11000;
                }
                else
                {
                    (mTmpList[i] as EqptItemInfo).Exp = i + 22000;
                }
            }
            Debug.LogWarning("=========================");
            PrintArr(mTmpList);
            Debug.LogWarning("=========================");
            PrintArr(mTestDragList);

        }
    }
}
