using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GPS : MonoBehaviour
{
    public Text gpsStatus, location, distanceToDesiredPlace;
    float la, lo, dist;

    void Start() => StartCoroutine("InitializeGPS"); // If all goes correctly it calls an invoke repeating to get the data

    IEnumerator InitializeGPS(){
        gpsStatus.text = "Status: Starting GPS";
        // Check if GPS is enabled
        if (!Input.location.isEnabledByUser){
            gpsStatus.text = "Status: GPS is disabled by user";
            yield break; 
        }
        gpsStatus.text = "Status: GPS is enabled by user";

        // Wait for service to initialize
        Input.location.Start();
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0){
            gpsStatus.text = $"Status: Initializing GPS, waiting time: {maxWait}";
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1){
            gpsStatus.text = "Status: GPS initialization timed out";
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed){
            gpsStatus.text = "Status: GPS initialization failed";
            yield break;
        }
        else{
            gpsStatus.text = "Status: GPS initialized correctly";
            InvokeRepeating("GetGPS", 0.5f, 1f); // Waits 0.5s and gets the data every 1s
        }
    }

    private void GetGPS(){
        if (Input.location.status == LocationServiceStatus.Running){
            gpsStatus.text = "Status: GPS is running";
            la = Input.location.lastData.latitude;
            lo = Input.location.lastData.longitude;
            location.text = $"Location (lat/long): {la}    -    {lo}";
            distanceToDesiredPlace.text = $"Distance to Desired Location (m): {dist}";

            // other available data is altitude, horizontalAccuracy, timeStampValue
        }
        else gpsStatus.text = "Status: GPS stopped running";
    }
}
