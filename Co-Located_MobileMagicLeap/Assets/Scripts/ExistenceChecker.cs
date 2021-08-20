using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExistenceChecker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.LogFormat("I exist!! \n\n   at {0}", gameObject.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
