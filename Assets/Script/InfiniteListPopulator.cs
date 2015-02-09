
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 使用UIGrid 多行多列的时候只支持纵向滑动，
/// 且scrollview movement应为Vertical + UIGrid Arranement :Horizontal
/// extraBuffer的个数一定是显示列数的倍数
/// 
/// 
/// UITable的模式支持横向和纵向
///FindItemByDataIdx 通过数据Idx找到对应的itemobj
/// </summary>
public class InfiniteListPopulator : MonoBehaviour
{

    // events to listen to if needed...
    public delegate void InfiniteItemIsPressed(int itemDataIndex, bool isDown);
    public event InfiniteItemIsPressed InfiniteItemIsPressedEvent;
    public delegate void InfiniteItemIsClicked(int itemDataIndex);
    public event InfiniteItemIsClicked InfiniteItemIsClickedEvent;


    private bool enableLog = true;
    //Prefabs

    public Transform itemPrefab;

    //UIScrollView
    bool m_bGrid = false;
    public UIGrid m_Grid;
    public UITable m_Table;

    public UIScrollView m_ScrollView;
    /// <summary>
    /// 当某个tabledeactibe的情况，m_ScrollView没有执行awake，m_ScrollView.panel是null
    /// 指定好uipanel，用来确定clip的范围
    /// </summary>
    public UIPanel m_UIPanel;

    //ScrollBar
    Transform scrollIndicator;
    private int scrollCursor = 0;
    // pool
    public float cellHeight = 94f;// at the moment we support fixed height... insert here or measure it
    private int poolSize = 6;
    private List<Transform> itemsPool = new List<Transform>();
    public int extraBuffer = 5;
    public int m_nFirstCreateCnt = 0; // 第一帧创建的节点数
    private int startIndex = 0; // where to start
    private Hashtable dataTracker = new Hashtable();// hashtable to keep track of what is being displayed by the pool

    int m_nTotalDataCount = 1;
    bool m_bUseTest = false;

    [HideInInspector]
    public UIScrollView.Movement m_Movement;
    #region Start & Update for MonoBehaviour
    void Awake()
    {
        m_Movement = m_ScrollView.movement;
        //只支持Horizontal ， Vertical
        if (m_Movement > UIScrollView.Movement.Vertical)
        {
            Debug.LogError("不支持UITable scrolview movment " + m_Movement);
        }
        if (m_Grid != null && m_Table == null)
        {
            m_bGrid = true;
            if (m_Movement == UIScrollView.Movement.Horizontal)
            {
                Debug.LogError("不支持UIGrid scrolview movment " + m_Movement);
            }
        }
        else
        {
            m_bGrid = false;
        }
    }

    void Start()
    {
    }
    #endregion

    #region 只需要看此部分，对外接口绑定原始数据以及动态更新ITEM内容
    /// <summary>
    /// 绑定需要显示的数据及对应Item显示内容
    /// </summary>
    /// <param name="Count"> 数据总数</param>
    /// <param name="func"> item界面刷新显示</param>
    /// 

    public void BindDataSource(int Count, ProcessItemByIndex func,int iStartIndex)
    {
        m_Movement = m_ScrollView.movement;

        if (m_Movement > UIScrollView.Movement.Vertical)
        {
            Debug.LogError("不支持scrolview movment " + m_Movement);
        }
        if (m_Grid != null && m_Table == null)
        {
            m_bGrid = true;
            if (m_Movement == UIScrollView.Movement.Horizontal)
            {
                Debug.LogError("不支持UIGrid scrolview movment " + m_Movement);
            }
        }
        else
        {
            m_bGrid = false;
        }
        _ProcessItemByIndex = func;
        m_nTotalDataCount = Count;
        InitTableViewImp(iStartIndex);
    }
    public delegate void ProcessItemByIndex(Transform item, int dataIndex);
    public ProcessItemByIndex _ProcessItemByIndex = null;
    void PopulateListItemWithIndex(Transform item, int dataIndex)
    {
        //item.GetComponent<InfiniteItemBehavior>().label.text = m_dataList[dataIndex].mailData.iMailIndex.ToString();// casting to our class... 
        if (_ProcessItemByIndex != null)
        {
            _ProcessItemByIndex(item, dataIndex);
        }
    }
    /// <summary>
    /// 传入真实数据序号
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public Transform FindItemByDataIdx(int idx)
    {
        Transform findObj = null;
        if (dataTracker.ContainsKey(idx))
        {
            int prefabIdx = (int)dataTracker[idx];
            if (prefabIdx >= 0 && prefabIdx < poolSize)
            {
                findObj = itemsPool[prefabIdx];
            }
        }
        return findObj;
    }
    #endregion

    #region 删除Item && 增加Item

    //删除，增加数据后，刷新显示的item
    /// <summary>
    /// 刷新显示的item
    /// </summary>
    /// <param name="inStartIndex"></param>
    /// <param name="nCount"></param>
    void RefreshVisualItem(int inStartIndex, int nCount)
    {
        m_nTotalDataCount = nCount;

        startIndex = inStartIndex;
        scrollCursor = inStartIndex;
        if (dataTracker.ContainsKey(inStartIndex))
        {
            Transform tr = itemsPool[(int)dataTracker[inStartIndex]];
            Vector3 delta = tr.localPosition;


            for (int i = 0; i < itemsPool.Count; i++)
            {
                Transform item = itemsPool[i];
                if (m_Movement == UIScrollView.Movement.Vertical)
                {
                    item.localPosition = new Vector3(delta.x, delta.y - cellHeight * i, 0);
                }
                else if (m_Movement == UIScrollView.Movement.Horizontal)
                {
                    item.localPosition = new Vector3(delta.x + cellHeight * i, delta.y, 0);
                }
                item.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log("fatal error in scrollview");
        }
        dataTracker.Clear();


        int j = 0;
        for (int i = startIndex; i < m_nTotalDataCount; i++)
        {
            Transform item = GetItemFromPool(j);
            if (item != null)
            {
                InitListItemWithIndex(item, i, j);
                if (enableLog)
                {
                    Debug.Log(item.name + "::" + item.tag);
                }
                j++;


            }
            else // end of pool
            {

                break;
            }
        }
        // Invoke("RepositionList", 0.001f);
    }
    public delegate void ProcessDeleteItem(int curClickItemDataIndex);
    public ProcessDeleteItem _ProcessDeleteItem = null;

    /// <summary>
    /// 删除第几个item的数据,一次删除一个
    /// </summary>
    /// <param name="itemDataIndex"></param>
    public void DeleteItemByIdx(int itemDataIndex)
    {
        if (_ProcessDeleteItem != null && m_Table)
        {
            //get first dataindex
            int firstDataIndex = int.MaxValue; //itempool中第一个（可见）
            int itemMax = 0; ///itempool中最后一个可见的
            InfiniteItemBehavior[] behs = m_Table.GetComponentsInChildren<InfiniteItemBehavior>();
            foreach (InfiniteItemBehavior bb in behs)
            {
                if (bb.itemDataIndex < firstDataIndex)
                {
                    firstDataIndex = bb.itemDataIndex;
                }
                if (bb.itemDataIndex > itemMax)
                {
                    itemMax = bb.itemDataIndex;
                }

            }

            //删除itemdataindex后，数据会上移动一位，这样最后一个和可见的item要设为不可见
            Transform ItemMaxObj = itemsPool[(int)(dataTracker[itemMax])];
            if (ItemMaxObj)
            {
                ItemMaxObj.gameObject.SetActive(false);//后面的item
            }
            _ProcessDeleteItem(itemDataIndex);
            RefreshVisualItem(firstDataIndex, m_nTotalDataCount - 1);
        }
    }

    /// <summary>
    /// 传入更新后的数据大小
    /// </summary>
    /// <param name="nCount"></param>
    public void AddItemData(int nCount)
    {
        //找到当前显示区域的第一个Item
        int firstDataIndex = int.MaxValue;
        InfiniteItemBehavior[] behs = m_Table.GetComponentsInChildren<InfiniteItemBehavior>();
        if (behs.Length == 0)
        {
            firstDataIndex = 0;
        }
        foreach (InfiniteItemBehavior bb in behs)
        {
            if (bb.itemDataIndex < firstDataIndex)
            {
                firstDataIndex = bb.itemDataIndex;
            }
        }

        int nOldCount = m_nTotalDataCount;

        RefreshVisualItem(firstDataIndex, nCount);

        if (nOldCount == 0)
        {
            // 当之前的数量为0，全部隐藏，并不会排序，需要重新排序
            RepositionList();
        }
    }
    public bool m_bSelfCaculateItemHeight = false;
    void RefreshVisualItemByHeight(int lastItemIdex, int inStartIndex, int nCount)
    {
        if (m_bSelfCaculateItemHeight)
        {
            RefreshVisualItemByDynaicHeight(lastItemIdex, inStartIndex, nCount);
            return;
        }
        m_nTotalDataCount = nCount;

        startIndex = inStartIndex;
        scrollCursor = inStartIndex;
        Vector3 delta = new Vector3();

        for (int i = 0; i < itemsPool.Count; i++)
        {
            Transform item = itemsPool[i];
            item.name = "xxxx";

            item.gameObject.SetActive(false);
        }

        if (dataTracker.ContainsKey(lastItemIdex))
        {
            Transform tr = itemsPool[(int)dataTracker[lastItemIdex]];
            delta = tr.localPosition;
        }

        dataTracker.Clear();


        int j = m_nTotalDataCount - 1;
        if (m_nTotalDataCount > 4)
        {
            int endIndex = m_nTotalDataCount - 4;
            if (m_nTotalDataCount >= poolSize)
            {
                endIndex = m_nTotalDataCount - poolSize;
            }
            else if (m_nTotalDataCount < poolSize)
            {
                endIndex = 0;
            }
            j = m_nTotalDataCount - endIndex - 1;

            for (int i = m_nTotalDataCount - 1; i >= endIndex; i--)
            {
                Transform item = GetItemFromPool(j);

                item.localPosition = new Vector3(delta.x, delta.y + cellHeight * (m_nTotalDataCount - 1 - i), 0);
                if (item != null)
                {
                    InitListItemWithIndex(item, i, j);
                    if (enableLog)
                    {
                        Debug.Log(item.name + "::" + item.tag);
                    }
                    j--;


                }
                else // end of pool
                {

                    break;
                }
            }
        }
        else
        {

            for (int i = m_nTotalDataCount - 1; i >= 0; i--)
            {
                Transform item = GetItemFromPool(j);
                item.localPosition = new Vector3(delta.x, delta.y + cellHeight * (m_nTotalDataCount - 1 - i), 0); if (item != null)
                {
                    InitListItemWithIndex(item, i, j);


                    if (enableLog)
                    {
                        Debug.Log(item.name + "::" + item.tag);
                    }
                    j--;


                }
                else // end of pool
                {

                    break;
                }
            }
        }

    }
    //float[] m_CalcHeight = new float[3000];
    void RefreshVisualItemByDynaicHeight(int lastItemIdex, int inStartIndex, int nCount)
    {
        m_nTotalDataCount = nCount;

        startIndex = inStartIndex;
        scrollCursor = inStartIndex;
        Vector3 delta = new Vector3();

        for (int i = 0; i < itemsPool.Count; i++)
        {
            Transform item = itemsPool[i];
            item.name = "xxxx";

            item.gameObject.SetActive(false);
        }

        if (dataTracker.ContainsKey(lastItemIdex))
        {
            Transform tr = itemsPool[(int)dataTracker[lastItemIdex]];
            delta = tr.localPosition;
        }

        //找到最小面的y坐标
        float h = m_UIPanel.clipOffset.y - m_UIPanel.baseClipRegion.w * 0.5f - m_Table.transform.localPosition.y;


        delta = new Vector3(delta.x, h, delta.z);
        dataTracker.Clear();


        int j = m_nTotalDataCount - 1;
        if (m_nTotalDataCount > 4)
        {
            int endIndex = m_nTotalDataCount - 4;
            if (m_nTotalDataCount >= poolSize)
            {
                endIndex = m_nTotalDataCount - poolSize;
            }
            else if (m_nTotalDataCount < poolSize)
            {
                endIndex = 0;
            }
            j = m_nTotalDataCount - endIndex - 1;
            float calcHeight = 0;
            for (int i = m_nTotalDataCount - 1; i >= endIndex; i--)
            {
                Transform item = GetItemFromPool(j);


                if (item != null)
                {
                    InitListItemWithIndex(item, i, j);

                    if (m_bSelfCaculateItemHeight)
                    {
                        float getItemHeight = item.GetComponent<InfiniteItemBehavior>().m_ItemHeight;
                        calcHeight += getItemHeight;
                        item.localPosition = new Vector3(delta.x, delta.y + calcHeight - getItemHeight * 0.5f, 0);
                        item.GetComponent<InfiniteItemBehavior>().m_ItemWorldY = item.localPosition.y;
                        calcHeight += 2;
                        //m_CalcHeight[i] = item.localPosition.y; ;
                    }
                    else
                    {
                        item.localPosition = new Vector3(delta.x, delta.y + cellHeight * (m_nTotalDataCount - 1 - i), 0);
                    }


                    if (enableLog)
                    {
                        Debug.Log(item.name + "::" + item.tag);
                    }
                    j--;


                }
                else // end of pool
                {

                    break;
                }
            }
        }
        else
        {
            float calcHeight = 0;
            for (int i = m_nTotalDataCount - 1; i >= 0; i--)
            {
                Transform item = GetItemFromPool(j);
                if (item != null)
                {
                    InitListItemWithIndex(item, i, j);
                    if (m_bSelfCaculateItemHeight)
                    {
                        float getItemHeight = item.GetComponent<InfiniteItemBehavior>().m_ItemHeight;
                        calcHeight += getItemHeight;


                        item.localPosition = new Vector3(delta.x, delta.y + calcHeight - getItemHeight * 0.5f, 0);
                        item.GetComponent<InfiniteItemBehavior>().m_ItemWorldY = item.localPosition.y;
                        calcHeight += 2;
                        //m_CalcHeight[i] = item.localPosition.y; ;

                    }
                    else
                    {
                        item.localPosition = new Vector3(delta.x, delta.y + cellHeight * (m_nTotalDataCount - 1 - i), 0);
                    }

                    if (enableLog)
                    {
                        Debug.Log(item.name + "::" + item.tag);
                    }
                    j--;


                }
                else // end of pool
                {

                    break;
                }
            }
        }

    }

    /// <summary>
    /// 在末尾加入,后一个item挤前一个item。
    /// </summary>
    /// <param name="nCount"></param>
    public void AddItemDataLast(int nCount)
    {
        int itemMax = 0; ///itempool中最后一个可见的
        InfiniteItemBehavior[] behs = m_Table.GetComponentsInChildren<InfiniteItemBehavior>();
        foreach (InfiniteItemBehavior bb in behs)
        {

            if (bb.itemDataIndex > itemMax)
            {
                itemMax = bb.itemDataIndex;
            }

        }

        //删除itemdataindex后，数据会上移动一位，这样最后一个和可见的item要设为不可见


        int startPoint = 0;
        if (nCount < poolSize)
        {
            startPoint = 0;
        }
        else
        {
            startPoint = nCount - poolSize;
        }
        RefreshVisualItemByHeight(itemMax, startPoint, nCount);


    }
    #endregion

    #region 循环逻辑
    void InitTableViewImp(int inStartIndex)
    {
        startIndex = inStartIndex;
        scrollCursor = inStartIndex;
        dataTracker.Clear();

        //第一次需要创建prefab，第二次不需要
        if (m_bFirstRefresh == false)
        {
            StartCoroutine(RefreshPoolCo(inStartIndex));
        }
        else
        {
            for (int i = 0; i < itemsPool.Count; i++)
            {
                Transform t = itemsPool[i];
                t.gameObject.SetActive(false);
            }
            int j = 0;
            for (int i = startIndex; i < m_nTotalDataCount; i++)
            {
                Transform item = GetItemFromPool(j);
                if (item != null)
                {
                    InitListItemWithIndex(item, i, j);
                    if (enableLog)
                    {
                        Debug.Log(item.name + "::" + item.tag);
                    }
                    j++;
                }
                else // end of pool
                {
                    break;
                }
            }

            if (m_bFirstItemBottom)
            {
                RepositionList();
            }
            else
            {
                //Invoke("RepositionList", 0.3f);
                RepositionList();
            }

        }



    }
    public bool m_bFirstItemBottom = false;
    public void SetFirstItemBottom()
    {
        Transform item = GetItemFromPool(0);
        if (m_bGrid == false && null != m_Table)
        {
            //Debug.LogError(m_Table.transform.localPosition);
            int deltaH = (int)cellHeight;
            if (m_bSelfCaculateItemHeight)
            {
                deltaH = 0;
            }
            float h = m_UIPanel.clipOffset.y - m_UIPanel.baseClipRegion.w * 0.5f - m_Table.transform.localPosition.y + deltaH;
            item.localPosition = new Vector3(item.localPosition.x, h, 0);
        }

        //item.localPosition = new Vector3(item.localPosition.x, item.localPosition.y - ((int)(m_UIPanel.baseClipRegion.w / cellHeight) -1)*cellHeight, 0);
    }
    void RepositionList()
    {
        if (m_bGrid)
        {
            m_Grid.Reposition();
        }
        else
        {
            if (m_bSelfCaculateItemHeight == false)
            {
                if (m_Table)
                {
                    m_Table.SetInit(m_UIPanel);
                }
                m_Table.Reposition();
            }
        }
        // make sure we have a correct poistion sequence

        if (m_bFirstItemBottom)
        {
            if (m_nTotalDataCount > 0)
            {
                ///uitable进行初始化，不然初始位置不正确
                ///i
                ///
                //m_table一开始禁用，不然等到start后会修改为位置，

                if (m_Table)
                {
                    m_Table.SetInit(m_UIPanel);
                    m_Table.Reposition();
                }

                SetFirstItemBottom();
            }
        }
        else
        {
            m_ScrollView.SetDragAmount(0, 0, false);
        }
    }
    // items
    void InitListItemWithIndex(Transform item, int dataIndex, int poolIndex)
    {
        item.GetComponent<InfiniteItemBehavior>().itemDataIndex = dataIndex;
        item.GetComponent<InfiniteItemBehavior>().listPopulator = this;
        item.GetComponent<InfiniteItemBehavior>().panel = m_UIPanel;
        item.name = "item" + dataIndex;
        item.transform.localScale = new Vector3(1, 1, 1); //scale
        item.gameObject.SetActive(true);
        dataTracker.Add(itemsPool[poolIndex].GetComponent<InfiniteItemBehavior>().itemDataIndex, itemsPool[poolIndex].GetComponent<InfiniteItemBehavior>().itemNumber);
        PopulateListItemWithIndex(item, dataIndex);
        if (item.GetComponent<InfiniteItemBehavior>().thisCollider &&
            item.GetComponent<InfiniteItemBehavior>().thisCollider.enabled == false)
        {
            item.GetComponent<InfiniteItemBehavior>().thisCollider.enabled = true;
        }
    }
    void PrepareListItemWithIndex(Transform item, int newIndex, int oldIndex)
    {
        if (m_bSelfCaculateItemHeight)
        {
            PrepareListItemWithIndexSelfCalcHeight(item, newIndex, oldIndex);
            return;
        }
        if (m_bGrid == false)
        {
            if (m_Movement == UIScrollView.Movement.Horizontal)
            {
                if (newIndex < oldIndex)
                    item.localPosition -= new Vector3((poolSize) * cellHeight, 0, 0);
                else
                    item.localPosition += new Vector3((poolSize) * cellHeight, 0, 0);

            }
            else if (m_Movement == UIScrollView.Movement.Vertical)
            {
                if (newIndex < oldIndex)
                    item.localPosition += new Vector3(0, (poolSize) * cellHeight, 0);
                else
                    item.localPosition -= new Vector3(0, (poolSize) * cellHeight, 0);
            }
        }
        else
        {
            if (m_Movement == UIScrollView.Movement.Vertical)
            {
                float deltaY = poolSize / m_Grid.maxPerLine;
                if (newIndex < oldIndex)
                    item.localPosition += new Vector3(0, (deltaY) * m_Grid.cellHeight, 0);
                else
                    item.localPosition -= new Vector3(0, (deltaY) * m_Grid.cellHeight, 0);
            }

        }


        item.gameObject.SetActive(true);
        item.GetComponent<InfiniteItemBehavior>().itemDataIndex = newIndex;
        item.name = "item" + (newIndex);
        dataTracker.Add(newIndex, (int)(dataTracker[oldIndex]));
        PopulateListItemWithIndex(item, newIndex);
        dataTracker.Remove(oldIndex);
    }


    /// <summary>
    /// 动态高度计算，目前只支持纵向方式
    /// </summary>
    /// <param name="item"></param>
    /// <param name="newIndex"></param>
    /// <param name="oldIndex"></param>
    void PrepareListItemWithIndexSelfCalcHeight(Transform item, int newIndex, int oldIndex)
    {
        item.gameObject.SetActive(true);
        item.GetComponent<InfiniteItemBehavior>().itemDataIndex = newIndex;
        item.name = "item" + (newIndex);
        dataTracker.Add(newIndex, (int)(dataTracker[oldIndex]));
        PopulateListItemWithIndex(item, newIndex);
        dataTracker.Remove(oldIndex);
        //item.localPosition = new Vector3(item.localPosition.x, m_CalcHeight[newIndex], item.localPosition.z);

        float selfItemHeight = item.GetComponent<InfiniteItemBehavior>().m_ItemHeight;
        if (newIndex < oldIndex)
        {
            if (dataTracker.ContainsKey(newIndex + 1))
            {
                Transform item2 = itemsPool[(int)(dataTracker[newIndex + 1])];
                float h = item2.GetComponent<InfiniteItemBehavior>().m_ItemWorldY;
                float itemHeight = item2.GetComponent<InfiniteItemBehavior>().m_ItemHeight;

                item.localPosition = new Vector3(item.localPosition.x, h + itemHeight * 0.5f + 2 + selfItemHeight * 0.5f, item.localPosition.z);
                item.GetComponent<InfiniteItemBehavior>().m_ItemWorldY = item.localPosition.y;


                //item.localPosition = new Vector3(item.localPosition.x, h + selfItemHeight, item.localPosition.z);
                //item.GetComponent<InfiniteItemBehavior>().m_ItemWorldY = item.localPosition.y;
            }

        }
        else
        {
            if (dataTracker.ContainsKey(newIndex - 1))
            {
                Transform item2 = itemsPool[(int)(dataTracker[newIndex - 1])];
                float h = item2.GetComponent<InfiniteItemBehavior>().m_ItemWorldY;
                float h2 = item2.GetComponent<InfiniteItemBehavior>().m_ItemHeight;

                item.localPosition = new Vector3(item.localPosition.x, h - h2 * 0.5f - 2 - selfItemHeight * 0.5f, item.localPosition.z);
                item.GetComponent<InfiniteItemBehavior>().m_ItemWorldY = item.localPosition.y;

                //item.localPosition = new Vector3(item.localPosition.x, h - h2, item.localPosition.z);
                //item.GetComponent<InfiniteItemBehavior>().m_ItemWorldY = item.localPosition.y;
            }
            else
            {
                if (enableLog)
                {
                    Debug.LogError("error " + (newIndex - 1).ToString());
                }
            }
        }
    }
    // the main logic for "infinite scrolling"...
    private bool isUpdatingList = false;
    public IEnumerator ItemIsInvisible(int itemNumber)
    {
        if (isUpdatingList) yield return null;
        isUpdatingList = true;
        if (m_nTotalDataCount > poolSize)// we need to do something "smart"... 
        {
            Transform item = itemsPool[itemNumber];


            int itemDataIndex = item.GetComponent<InfiniteItemBehavior>().itemDataIndex;

            int indexToCheck = 0;
            InfiniteItemBehavior infItem = null;

            if (dataTracker.ContainsKey(itemDataIndex + 1))
            {
                //Debug.LogError("1 itemNumber = " + itemNumber + " data idx " + itemDataIndex);
                infItem = itemsPool[(int)(dataTracker[itemDataIndex + 1])].GetComponent<InfiniteItemBehavior>();


                if ((infItem != null && infItem.verifyVisibility()))
                {
                    // dragging upwards (scrolling down)
                    indexToCheck = itemDataIndex - (extraBuffer / 2);
                    if (dataTracker.ContainsKey(indexToCheck))
                    {
                        //  Debug.LogError("indexToCheck1  = " + indexToCheck.ToString());
                        //do we have an extra item(s) as well?
                        for (int i = 0; i <= indexToCheck; i++)
                        {
                            if (dataTracker.ContainsKey(i))
                            {
                                infItem = itemsPool[(int)(dataTracker[i])].GetComponent<InfiniteItemBehavior>();

                                if ((infItem != null && !infItem.verifyVisibility()))
                                {
                                    item = itemsPool[(int)(dataTracker[i])];
                                    if ((i) + poolSize < m_nTotalDataCount && i > -1)
                                    {
                                        PrepareListItemWithIndex(item, i + poolSize, i);

                                    }
                                }
                            }

                        }
                    }
                }
            }
            if (dataTracker.ContainsKey(itemDataIndex - 1))
            {
                //Debug.LogError("2 itemNumber = " + itemNumber + " data idx " + itemDataIndex);
                infItem = itemsPool[(int)(dataTracker[itemDataIndex - 1])].GetComponent<InfiniteItemBehavior>();


                if ((infItem != null && infItem.verifyVisibility()))
                {
                    //dragging downwards check the item below
                    indexToCheck = itemDataIndex + (extraBuffer / 2);

                    if (dataTracker.ContainsKey(indexToCheck))
                    {
                        //   Debug.LogError("indexToCheck2 " + indexToCheck.ToString());
                        // if we have an extra item
                        for (int i = m_nTotalDataCount - 1; i >= indexToCheck; i--)
                        {
                            if (dataTracker.ContainsKey(i))
                            {
                                infItem = itemsPool[(int)(dataTracker[i])].GetComponent<InfiniteItemBehavior>();

                                if ((infItem != null && !infItem.verifyVisibility()))
                                {
                                    item = itemsPool[(int)(dataTracker[i])];
                                    if ((i) - poolSize > -1 && (i) < m_nTotalDataCount)
                                    {

                                        PrepareListItemWithIndex(item, i - poolSize, i);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        isUpdatingList = false;
    }

    //ItemIsInvisible相同的函数，主要是协程调试不方便
    public void TestItemIsInvisible(int itemNumber)
    {

        if (isUpdatingList) return;
        isUpdatingList = true;
        if (m_nTotalDataCount > poolSize)// we need to do something "smart"... 
        {
            Transform item = itemsPool[itemNumber];
            int itemDataIndex = item.GetComponent<InfiniteItemBehavior>().itemDataIndex;
            int indexToCheck = 0;
            InfiniteItemBehavior infItem = null;

            if (dataTracker.ContainsKey(itemDataIndex + 1))
            {
                infItem = itemsPool[(int)(dataTracker[itemDataIndex + 1])].GetComponent<InfiniteItemBehavior>();


                if ((infItem != null && infItem.verifyVisibility()))
                {
                    // dragging upwards (scrolling down)
                    indexToCheck = itemDataIndex - (extraBuffer / 2);
                    if (dataTracker.ContainsKey(indexToCheck))
                    {
                        //do we have an extra item(s) as well?
                        for (int i = indexToCheck; i >= 0; i--)
                        {
                            if (dataTracker.ContainsKey(i))
                            {
                                infItem = itemsPool[(int)(dataTracker[i])].GetComponent<InfiniteItemBehavior>();

                                if ((infItem != null && !infItem.verifyVisibility()))
                                {
                                    item = itemsPool[(int)(dataTracker[i])];
                                    if ((i) + poolSize < m_nTotalDataCount && i > -1)
                                    {
                                        PrepareListItemWithIndex(item, i + poolSize, i);

                                    }
                                }
                            }
                            else
                            {
                                scrollCursor = itemDataIndex - 1;
                                break;
                            }
                        }
                    }
                }
            }
            if (dataTracker.ContainsKey(itemDataIndex - 1))
            {
                infItem = itemsPool[(int)(dataTracker[itemDataIndex - 1])].GetComponent<InfiniteItemBehavior>();


                if ((infItem != null && infItem.verifyVisibility()))
                {
                    //dragging downwards check the item below
                    indexToCheck = itemDataIndex + (extraBuffer / 2);

                    if (dataTracker.ContainsKey(indexToCheck))
                    {
                        // if we have an extra item
                        for (int i = indexToCheck; i < m_nTotalDataCount; i++)
                        {
                            if (dataTracker.ContainsKey(i))
                            {
                                infItem = itemsPool[(int)(dataTracker[i])].GetComponent<InfiniteItemBehavior>();

                                if ((infItem != null && !infItem.verifyVisibility()))
                                {
                                    item = itemsPool[(int)(dataTracker[i])];
                                    if ((i) - poolSize > -1 && (i) < m_nTotalDataCount)
                                    {

                                        PrepareListItemWithIndex(item, i - poolSize, i);
                                    }
                                }
                            }
                            else
                            {
                                scrollCursor = itemDataIndex + 1;
                                break;
                            }
                        }
                    }
                }
            }
        }
        isUpdatingList = false;
    }
    #endregion

    #region items callbacks and helpers

    public void itemIsPressed(int itemDataIndex, bool isDown)
    {
        if (enableLog)
        {
            Debug.Log("Pressed down item " + itemDataIndex + " " + isDown);
        }

        if (InfiniteItemIsPressedEvent != null)
            InfiniteItemIsPressedEvent(itemDataIndex, isDown);
    }
    public void itemClicked(int itemDataIndex)
    {
        if (enableLog)
        {
            Debug.Log("Clicked item " + itemDataIndex);
        }

        if (InfiniteItemIsClickedEvent != null)
            InfiniteItemIsClickedEvent(itemDataIndex);
    }
    #endregion

    #region HelpFunction
    /// <summary>
    /// 当前页第一个item的真是数据id，通过FindItemByDataIdx找到对应的item
    /// </summary>
    /// <returns></returns>
    public int GetCurViewFirstDataID()
    {
        int firstDataIndex = int.MaxValue;
        UIWidgetContainer theTemp = m_Table;
        if (m_bGrid)
        {
            theTemp = m_Grid;
        }
        if (theTemp == null)
        {
            return -1;
        }
        InfiniteItemBehavior[] behs = theTemp.GetComponentsInChildren<InfiniteItemBehavior>();
        if (behs.Length == 0)
        {
            return -1;
        }
        foreach (InfiniteItemBehavior bb in behs)
        {
            if (bb.itemDataIndex < firstDataIndex)
            {
                firstDataIndex = bb.itemDataIndex;
            }
        }
        return firstDataIndex;
    }

    /// <summary>
    ///  当前页最后一个item的真是数据id，通过FindItemByDataIdx找到对应的item
    /// </summary>
    /// <returns></returns>
    public int GetCurViewLastDataID()
    {
        int itemMax = 0;
        ///itempool中最后一个可见的
        UIWidgetContainer theTemp = m_Table;
        if (m_bGrid)
        {
            theTemp = m_Grid;
        }
        if (theTemp == null)
        {
            return -1;
        }
        InfiniteItemBehavior[] behs = theTemp.GetComponentsInChildren<InfiniteItemBehavior>();
        if (behs.Length <= 0)
        {
            return -1;
        }
        foreach (InfiniteItemBehavior bb in behs)
        {
            if (bb.itemDataIndex > itemMax)
            {
                itemMax = bb.itemDataIndex;
            }

        }
        return itemMax;
    }
    #endregion
    #region ItemPool创建

    Transform GetItemFromPool(int i)
    {
        if (i >= 0 && i < poolSize && i < itemsPool.Count)//加一层判断，针对一个crash
        {
            if (itemsPool[i] != null && itemsPool[i].gameObject != null)
            {
                itemsPool[i].gameObject.SetActive(true);
            }
            return itemsPool[i];
        }
        else
        {
            return null;
        }
    }

    void TransformIdentity(Transform t)
    {
        t.localPosition = new Vector3(0, 0, 0);
        t.localRotation = new Quaternion();
        t.localScale = new Vector3(1, 1, 1);
    }
    public bool m_bFirstRefresh = false;
    void ResetPool()
    {
        for (int i = 0; i < poolSize; i++) // the pool will use itemPrefab as a default
        {
            Transform item = itemsPool[i];
            TransformIdentity(item);

            item.gameObject.SetActive(false);
            item.name = "item" + i;
            if (m_bGrid)
            {
                item.parent = m_Grid.transform;
            }
            else
            {
                item.parent = m_Table.transform;
            }
        }
    }
    private IEnumerator RefreshPoolCo(int inStartIndex)
    {
        if (m_bGrid == false)
        {
            if (m_Movement == UIScrollView.Movement.Horizontal)
            {
                poolSize = (int)(m_UIPanel.baseClipRegion.z / cellHeight) + extraBuffer;
            }
            else if (m_Movement == UIScrollView.Movement.Vertical)
            {
                poolSize = (int)(m_UIPanel.baseClipRegion.w / cellHeight) + extraBuffer;
            }
            else
            {
                poolSize = (int)(m_UIPanel.baseClipRegion.w / cellHeight) + extraBuffer;
            }
        }
        else
        {
            poolSize = extraBuffer; //通过外部指定个数一定是倍数
        }

        if (enableLog)
        {
            Debug.Log("REFRESH POOL SIZE:::" + poolSize);
        }

        // destroy current items
        for (int i = 0; i < itemsPool.Count; i++)
        {
            Object.DestroyImmediate(itemsPool[i].gameObject);
        }
        itemsPool.Clear();

        int j = startIndex;
        for (int i = 0; i < poolSize; i++) // the pool will use itemPrefab as a default
        {
            if (m_nFirstCreateCnt > 0 && i == m_nFirstCreateCnt)
            {
                m_bFirstRefresh = true;
                RepositionList();
                yield return null;
            }

            Transform item = Instantiate(itemPrefab) as Transform;
            if (item != null)
            {
                InfiniteItemBehavior behav = item.GetComponent<InfiniteItemBehavior>();
                if (behav == null)
                {
                    behav = item.gameObject.AddComponent<InfiniteItemBehavior>();
                }
                TransformIdentity(item);

                item.gameObject.SetActive(false);
                behav.itemNumber = i;
                behav.listPopulator = this;
                item.name = "item" + i;
                if (m_bGrid)
                {
                    item.parent = m_Grid.transform;
                }
                else
                {
                    item.parent = m_Table.transform;
                }

                itemsPool.Add(item);

                if (i >= startIndex && i < m_nTotalDataCount)
                {
                    InitListItemWithIndex(item, j, i);
                    j++;
                }
            }
        }
        m_bFirstRefresh = true;
        RepositionList();
    }
    #endregion
}
