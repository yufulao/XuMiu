using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestIEnumerator : MonoBehaviour
{
    public List<IEnumerator> list=new List<IEnumerator>();
    private void Start()
    {
        ReloadList();
        SelectPattern();
    }

    IEnumerator A()
    {
        Debug.Log("AAA");
        yield return new WaitForSeconds(2f);
        SelectPattern();
    }
    IEnumerator B()
    {
        Debug.Log("BBB");
        yield return new WaitForSeconds(2f);
        SelectPattern();

    }
    IEnumerator C()
    {
        Debug.Log("CCC");
        yield return new WaitForSeconds(2f);
        SelectPattern();
    }

    public void SelectPattern()
    {
        if (list.Count==0)
        {
            Debug.Log("½áÊø");
            return;
        }
        StartCoroutine(list[0]);
        list.RemoveAt(0);
    }

    public void ReloadList()
    {
        list.Add(A());
        list.Add(B());
        list.Add(C());
    }
}
