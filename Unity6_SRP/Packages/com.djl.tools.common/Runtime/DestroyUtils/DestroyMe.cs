using UnityEngine;
using System.Collections;

public class DestroyMe : MonoBehaviour{

    float timer;
    public float deathtimer = 10;
	void Update ()
    {
        Simulate(Time.deltaTime);
    }

    public void Simulate(float deltaTime)
    {
        timer += deltaTime;
        if (timer >= deathtimer)
        {
            Destroy(gameObject);
        }
    }
}
