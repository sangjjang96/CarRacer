using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class EnemyMove : MonoBehaviour
{
    public GameObject target;
    NavMeshAgent nav;
    float currentTime;
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Target");
        nav = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {        
        currentTime += Time.deltaTime;
        // navigator를 이용하여 target의 posision까지 경로를 탐색하여 찾아감
        if(currentTime > 3)
        {
            nav.SetDestination(target.transform.position);
        }
    }
}
