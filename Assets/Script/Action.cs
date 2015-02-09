using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GRID_OR_TABLE
{
    TEST_NONE,
    TEST_GRID,
    TEST_TABLE,
}
public class Action : MonoBehaviour {

    public GameObject []mItemPrefab;
    public Transform mContainerTrans;
    public UIScrollView mScrollView;
    public int mInitItemNum = 5;


    private UITable mTable = null;
    private UIGrid mGrid = null;


    private InfiniteListPopulator mInfiniteGridList = null; 

    public GRID_OR_TABLE mTestType = GRID_OR_TABLE.TEST_NONE;

    List<GameObject> mItemList = new List<GameObject>();
    
    private   ArrayList mItemArrayList = new ArrayList() ;
	// Use this for initialization
	void Start () 
    {
        switch (mTestType)
        {
            case GRID_OR_TABLE.TEST_TABLE:
                mTable = mContainerTrans.GetComponent<UITable>();
                break;
            case GRID_OR_TABLE.TEST_GRID:
                mGrid = mContainerTrans.GetComponent<UIGrid>();
                break;
            default: break;
        }
        mInfiniteGridList = mScrollView.GetComponent<InfiniteListPopulator>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    private void OnNodeShow(Transform trans, int idx)
    {
        if (idx >= 0 && idx < mItemArrayList.Count)
        {
            ItemUICtrl itmUICtrl = trans.GetComponent<ItemUICtrl>();
            ItemInfo itmInfo = (mItemArrayList[idx] as ItemInfo);
            itmUICtrl.SetItemData(itmInfo);
        }
    }
    public void BindDataSource(ArrayList arrList, int iCurNo)
    {
        if (null != arrList)
        {
            mItemArrayList = arrList;
            if (null != mInfiniteGridList)
            {
                mInfiniteGridList.m_bFirstRefresh = false;
                mInfiniteGridList.BindDataSource(mItemArrayList.Count, OnNodeShow, iCurNo);
            }
        }
        /*mInfiniteGridList.RepositionList();
        mRankItemGrid.Reposition();*/ 
    }

    void AddItems()
    {

        for (int i = 0; i < mInitItemNum; ++i)
        {
            for (int j = 0; j <  mItemPrefab.Length; ++j)
            {
                GameObject go = GameObject.Instantiate(mItemPrefab[j]) as GameObject;
                go.transform.parent = mContainerTrans;
                go.transform.localPosition = new Vector3(0, 0, 0);
                go.transform.localScale = new Vector3(1, 1, 1);
                mItemList.Add(go);
            }  
        }
        switch (mTestType)
        {
            case GRID_OR_TABLE.TEST_TABLE:
                mTable.Reposition();
                break;
            case GRID_OR_TABLE.TEST_GRID:
                mGrid.Reposition();
                break;
            default: break;
        }
        mScrollView.ResetPosition();
    }
    void DelItems()
    {
        for (int i = 0; i < mItemList.Count; ++i)
        {
            GameObject.Destroy(mItemList[i]);//
        }
        mScrollView.ResetPosition();
    }
    void OnGUI()
    {

        if (GUI.Button(new Rect(0, 50, 100, 50), "Add"))
        { 
            ArrayList ArrList = new ArrayList();
            int idx = 0;
            for (int i = 0; i < 10; ++i)
            {
                ResItemInfo resItem = new ResItemInfo();
                resItem.iCount = i + 10000;
                resItem.mIndex = ++idx;
                resItem.mStrName = "Res_" + idx.ToString();
                ArrList.Add(resItem);

                EqptItemInfo eqptItem = new EqptItemInfo();
                eqptItem.Exp = i + 20000;
                eqptItem.mIndex = ++idx;
                eqptItem.mStrName = "Eqpt_" + idx.ToString();
                ArrList.Add(eqptItem);
            }
            Debug.LogWarning("========Len = "+ArrList.Count);
            BindDataSource(ArrList, 1);

        }
        if (GUI.Button(new Rect(0, 100, 100, 50), "Del"))
        { 
        }
        if (GUI.Button(new Rect(0, 150, 100, 50), "Del&Add"))
        { 
        }
        return;

        if (GUI.Button(new Rect(0, 50, 100, 50), "Add"))
        {
            AddItems();
        }
        if (GUI.Button(new Rect(0 ,100, 100, 50), "Del"))
        {
            DelItems(); 
        }
        if (GUI.Button(new Rect(0, 150, 100, 50), "Del&Add"))
        {
            DelItems();
            AddItems();
        }
    }
}
