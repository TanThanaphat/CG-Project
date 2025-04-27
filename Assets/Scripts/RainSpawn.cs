using System.Collections;
using UnityEngine;

public class RainSpawn : MonoBehaviour
{
    public ParticleSystem rain;
    private bool rainIsActivated = false;
    private float rainDuration = 0;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f) {
            timer = 0f;
            if (!rainIsActivated) {
                float rainChance = Random.Range(0, 100);

                if (rainChance >= 95) {
                    rainIsActivated = true;
                    rainDuration = Random.Range(20f, 120f);
                    float rainIntensity = Random.Range(100f, 3000f);

                    var emission = rain.emission;
                    emission.rateOverTime = rainIntensity;

                    Debug.Log("Rain spawned! Duration: " + rainDuration + ", Intensity: " + rainIntensity);
                }
                return;
            }
            rainDuration -= 1f;
            if (rainDuration <= 0) {
                rainIsActivated = false;
                var emission = rain.emission;
                emission.rateOverTime = 0;
                Debug.Log("Rain stopped!");
            }
        }
    }
}
