using UnityEngine;
using TMPro; 

public class CarGearUI : MonoBehaviour
{
    public CarController carController;
    public TMP_Text gearText;
    public TMP_Text speedText;

    void Update()
    {
        if (carController != null)
        {
            if (gearText != null)
                gearText.text = "Gear: " + carController.CurrentGear.ToString();

            if (speedText != null)
                speedText.text = "Speed: " + carController.CurrentSpeed.ToString("F1") + " km/h";
        }
    }
}
