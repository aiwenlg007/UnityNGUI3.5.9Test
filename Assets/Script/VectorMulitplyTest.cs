using UnityEngine;
using System.Collections;

public class VectorMulitplyTest : MonoBehaviour {

    public Vector3 mVelocity = new Vector3(1, 1, 1);
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public Vector3 mTestVector3 = new Vector3();
    protected virtual void OnDrawGizmos()
    {
        if (!enabled) return;
        //Gizmos.color = Color.green;
        //Gizmos.DrawSphere(transform.position, 1f);
        //Gizmos.DrawLine(transform.position - transform.forward, transform.position + transform.forward);
        //Gizmos.DrawLine(transform.position - transform.right, transform.position + transform.right);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.05f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + mVelocity * 0.3f);

        Vector3 vecY = new Vector3(); 
        vecY = (transform.position + mVelocity * 0.3f);
        Vector3 vecZ = new Vector3(0, 0, 1); 
        Vector3 vecX = new Vector3();
        vecX = Vector3.Cross(vecY , vecZ);// 叉乘 得到结果 是垂直于原始两个向量的一个向量

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, mTestVector3);
        Gizmos.color = Color.cyan;
        Vector3 testVec = new Vector3();
        testVec = Vector3.Project(mTestVector3, vecX.normalized);
        Gizmos.DrawLine(transform.position, testVec);
        //Gizmos.DrawLine(transform.position, -vecX.normalized / 2);
    }
}
