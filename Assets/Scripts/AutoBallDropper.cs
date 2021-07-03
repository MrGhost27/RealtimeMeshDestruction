using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBallDropper : MonoBehaviour
{
    // Start is called before the first frame update
    public float time = 0;
    public float maxTime = 2;

    public GameObject DropMe;
    public float ObjectLifeTime;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time > maxTime)
        {
            DropBall();
            time -= maxTime;
        }


    }

    void DropBall()
    {
        if (DropMe == null)
        {
            Debug.Log("No Gameobject set to Drop");
            return;
        }
        GameObject GO = Instantiate(DropMe, this.transform.position, Quaternion.identity);
        GameObject.Destroy(GO, ObjectLifeTime);
    }
}
