using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestList : MonoBehaviour
{
    public List<IEnumerator> EnemyCommands = new List<IEnumerator>();

    public IEnumerator comand0()
    {
        yield return null;
    }
    public IEnumerator comand1()
    {
        yield return null;
    }
    public IEnumerator comand2()
    {
        yield return null;
    }
    private void Start()
    {
        EnemyCommands.Add(comand0());
        EnemyCommands.Add(comand1());
        EnemyCommands.Add(comand2());

        EnemyCommands.RemoveAt(0);
        Debug.Log(EnemyCommands[0]);
    }
}
