using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GPS : MonoBehaviour
{
    public Text readingsCount, gpsStatus, currentLocation, desiredLocation, distanceToDesiredPlace;
    private double la_current, lo_current, la_desired, lo_desired;
    public double distance;
    private int maxGPSWait = 20;
    private int count = 0;

    void Start() => StartCoroutine("InitializeGPS"); // If all goes correctly it calls an invoke repeating to get the location data and calculate distance

    IEnumerator InitializeGPS(){
        la_desired = 35.5519180297852;
        lo_desired = 139.647079467773;
        desiredLocation.text = $"Desired Location\nlat: {la_desired}\nlong: {lo_desired}";

        gpsStatus.text = "Status: Starting GPS";
        // Check if GPS is enabled
        if (!Input.location.isEnabledByUser){
            gpsStatus.text = "GPS Status: Is DISABLED by user";
            yield break; 
        } else gpsStatus.text = "GPS Status: Is enabled by user";

        // Start service, args are
        //--- desiredAccuracyInMeters: The service accuracy you want to use, in meters.
        // This determines the accuracy of the device's last location coordinates.
        // Higher values like 500 don't require the device to use its GPS chip and thus save battery power.
        // Lower values like 5-10 provide the best accuracy but require the GPS chip and thus use more battery power.
        // The default value is 10 meters.
        //--- updateDistanceInMeters: The minimum distance, in meters, that the device must move laterally before Unity updates Input.location.
        // Higher values like 500 produce fewer updates and are less resource intensive to process. The default is 10 meters.
        Input.location.Start(5, 1);

        // Wait for service to initialize
        while (Input.location.status == LocationServiceStatus.Initializing && maxGPSWait > 0){
            gpsStatus.text = $"GPS Status: Initializing, waiting time: {maxGPSWait}";
            yield return new WaitForSeconds(1);
            maxGPSWait--;
            if (maxGPSWait < 1){
                gpsStatus.text = "GPS Status: Initialization Timed Out";
                yield break;
            }
        }
        if (Input.location.status == LocationServiceStatus.Failed){
            gpsStatus.text = "GPS Status: Initialization Failed";
            yield break;
        }else{
            gpsStatus.text = "GPS Status: Initialized Correctly";
            InvokeRepeating("GetLocationAndCalculateDistance", 1f, .33f); // Waits arg[0], gets the data and calculate distance every arg[1]
        }
    }

    private void GetLocationAndCalculateDistance(){
        if (Input.location.status == LocationServiceStatus.Running){
            gpsStatus.text = "GPS Status: Running";
            // other available data is altitude, horizontalAccuracy, timeStampValue
            la_current = Input.location.lastData.latitude;
            lo_current = Input.location.lastData.longitude;
            currentLocation.text = $"Current Location\nlat: {la_current}\nlong: {lo_current}";
            // Found 2 ways, they are probably the same but left here for testing if one might be better than other
            
            //distance = getDistance_A(la_current, lo_current, la_desired, lo_desired);
            distance = getDistance_B(la_current, la_desired, lo_current, lo_desired);
            distanceToDesiredPlace.text = $"Distance to Desired Location (m)\n{String.Format("{0:0.00000000}", distance)}";
            // just for sanity
            readingsCount.text = $"{count} reads";
            count++;
        }
        else gpsStatus.text = " GPS Status: Stopped Running";


        double getDistance_A(double lat1, double lon1, double lat2, double lon2){
            int R = 6371; // Radius of the earth in km
            double dLat = deg2rad(lat2 - lat1);  // deg2rad below
            double dLon = deg2rad(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = R * c; // Distance in km
            d /= 1000;        // Distance in m  
            return d;
        }

        double deg2rad(double deg){
            return deg * (Math.PI / 180);
        }

        double getDistance_B(double lat1, double lat2, double lon1, double lon2){
            lon1 = toRadians(lon1);
            lon2 = toRadians(lon2);
            lat1 = toRadians(lat1);
            lat2 = toRadians(lat2);

            // Haversine formula
            double dlon = lon2 - lon1;
            double dlat = lat2 - lat1;
            double a = Math.Pow(Math.Sin(dlat / 2f), 2f) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Pow(Math.Sin(dlon / 2f), 2f);
            double c = 2f * Math.Asin(Math.Sqrt(a));
            double r = 6371f;                             // Radius of earth in kilometers. Use 3956 for miles
            return (c * r) / 1000;                        // calculate the result in meters
        }


        double toRadians(double angleIn10thofaDegree){
            return (angleIn10thofaDegree * Mathf.PI) / 180;
        }

    }
}

