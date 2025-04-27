using UnityEngine;
using System;
using System.Threading.Tasks;
using HidSharp;

public class DualSenseGyroReader : MonoBehaviour
{
    private HidDevice dualSenseDevice;
    private HidStream hidStream;
    private byte[] inputBuffer;
    private Vector3 gyroRotation;
    public float sensitivity = 0.05f;

    public CarController carController; // อย่าลืมโยง CarController จาก Inspector

    async void Start()
    {
        FindDualSense();

        if (dualSenseDevice != null)
        {
            Debug.Log("DualSense found!");
            inputBuffer = new byte[dualSenseDevice.GetMaxInputReportLength()];
            hidStream = dualSenseDevice.Open();
            await ReadGyroLoop();
        }
        else
        {
            Debug.LogError("DualSense not found! Please connect it via USB or Bluetooth.");
        }
    }

    void FindDualSense()
    {
        var devices = DeviceList.Local.GetHidDevices();

        foreach (var device in devices)
        {
            if (device.VendorID == 0x054C && (device.ProductID == 0x0CE6 || device.ProductID == 0x0DF2))
            {
                dualSenseDevice = device;
                break;
            }
        }
    }

    async Task ReadGyroLoop()
    {
        while (hidStream != null && hidStream.CanRead)
        {
            int bytesRead = await hidStream.ReadAsync(inputBuffer, 0, inputBuffer.Length);

            if (bytesRead > 20)
            {
                ParseGyro(inputBuffer);
            }

            await Task.Delay(5);
        }
    }

    void ParseGyro(byte[] data)
    {
        short gyroX = BitConverter.ToInt16(data, 15);
        short gyroY = BitConverter.ToInt16(data, 17);
        short gyroZ = BitConverter.ToInt16(data, 19);

        Vector3 raw = new Vector3(gyroX, gyroY, gyroZ);
        Vector3 normalized = raw / 32768f;

        // แปลงค่าจาก Gyro ไปยัง steerInput
        gyroRotation.y += normalized.y * sensitivity;

        // จำกัดการหมุนจาก -45 ถึง 45 องศา
        gyroRotation.y = Mathf.Clamp(gyroRotation.y, -45f, 45f);

        // ส่งค่าการหมุนไปยัง carController
        if (carController != null)
        {
            // ส่งค่าจาก Gyro ไปยัง steerInput
            //carController.UpdateSteerInput(gyroRotation.y);
        }
    }

    private void OnDestroy()
    {
        if (hidStream != null)
        {
            hidStream.Dispose();
            hidStream = null;
        }
    }
}
