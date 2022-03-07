using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GPS : MonoBehaviour
{
    public Text timeStamp_, gpsStatus_, currentLocation_, pigLocation_, distanceToPig_, minotaurLocation_, distanceToMinotaur_;
    private double la_current, lo_current,
                   la_pig, lo_pig,              
                   la_minotaur, lo_minotaur,
                   timeStamp, prevTimeStamp;    
    private float distanceToEnter = 2f;           // distance to consider user reached location, in meters
    private float distanceToExit = 5f;            // distance to consider user exited location, in meters, longer to allow user to explore mesh
    private bool gpsInitialized,isInsidePig, isInsideMinotaur;
    private int maxGPSWait = 20;

    void Start(){
        SetObjectsLocation();
        StartCoroutine("InitializeGPS"); // If all goes correctly it calls an invoke repeating to get the location data and calculate distance
    }

    private void Update(){
        timeStamp = Input.location.lastData.timestamp;
        // only calculates things if gets a new reading
        if ( timeStamp != prevTimeStamp){
            GetLocationAndCalculateDistance();  // inside it tests if gps is initialized and running
            prevTimeStamp = timeStamp;
        }
    }

    private void SetObjectsLocation(){
        la_pig = 35.7053909301758D;
        lo_pig = 139.801376342773D;
        pigLocation_.text = $"lat: {la_pig}\nlong: {lo_pig}";
        la_minotaur = 35.705265045166D;
        lo_minotaur = 139.801391601563D;
        minotaurLocation_.text = $"lat: {la_minotaur}\nlong: {lo_minotaur}";
    }
    IEnumerator InitializeGPS(){
        gpsStatus_.text = "Starting GPS";
        if (!Input.location.isEnabledByUser){                       // Check if GPS is enabled
            gpsStatus_.text = "Is DISABLED by user";
            yield break; 
        } else gpsStatus_.text = "Is enabled by user";

        // Start service, args are
        //    desiredAccuracyInMeters
        //         The service accuracy of the device's last location coordinates you want to use, in meters.
        //         * Higher values like 500 don't require the device to use its GPS chip and thus save battery power.
        //         * Lower values like 5-10 provide the best accuracy but require the GPS chip and thus use more battery power.
        //         * Default value is 10 meters.
        //    updateDistanceInMeters
        //         The minimum distance, in meters, that the device must move laterally before Unity updates Input.location.
        //         * Higher values like 500 produce fewer updates and are less resource intensive to process.
        //         * Default is 10 meters.
        Input.location.Start(.1f, .1f);
        
        while (Input.location.status == LocationServiceStatus.Initializing && maxGPSWait > 0){      // Wait for service to initialize
            gpsStatus_.text = $"Initializing, waiting time: {maxGPSWait}";
            yield return new WaitForSeconds(1);
            maxGPSWait--;
            if (maxGPSWait < 1){
                gpsStatus_.text = "Initialization Timed Out";
                yield break;
            }
        }
        if (Input.location.status == LocationServiceStatus.Failed) gpsStatus_.text = "Initialization Failed";
        else gpsStatus_.text = "Initialized Correctly";
    }

    // other available data from GPS are altitude and horizontalAccuracy
    private void GetLocationAndCalculateDistance() {
        if (Input.location.status == LocationServiceStatus.Running) {                                              
                gpsStatus_.text = "Running";                                                // shows status
                prevTimeStamp = timeStamp;                                                  // get current timeStamp as previous for next testing
                timeStamp_.text = $"{timeStamp}";                                           // shows timestamp
                la_current = Input.location.lastData.latitude;                              // gets last reading of latitutde
                lo_current = Input.location.lastData.longitude;                             // gets last reading of longitude
                currentLocation_.text = $"lat:{la_current}\nlong: {lo_current}";            // shows location
                float distanceToPig = HaversineInM(la_current, lo_current, la_pig, lo_pig); // calculates distance to pig
                distanceToPig_.text = $"{distanceToPig}(m)";                                // shows distance to pig
                if (isInsidePig && distanceToPig > distanceToExit){                         // control exiting creature area
                    GameEvents.current.ExitedPig();
                    isInsidePig = false;
                }
                else if (!isInsidePig && distanceToPig < distanceToEnter){                  // control entering creature area
                    GameEvents.current.EnteredPig();
                    isInsidePig = true;
                }
                float distanceToMinotaur = HaversineInM(la_current, lo_current, la_minotaur, lo_minotaur);
                distanceToMinotaur_.text = $"{distanceToMinotaur}(m)";
                if (isInsideMinotaur && distanceToMinotaur > distanceToExit){
                    GameEvents.current.ExitedMinotaur();
                    isInsideMinotaur = false;
                }
                else if (!isInsideMinotaur && distanceToMinotaur < distanceToEnter){
                    GameEvents.current.EnteredMinotaur();
                    isInsideMinotaur = true;
                }
        }
        else gpsStatus_.text = "Stopped Running";
    }


    double _eQuatorialEarthRadius = 6378.1370D;
    double _d2r = (Math.PI / 180D);
    private double HaversineInKM(double lat1, double long1, double lat2, double long2){
            double dlong = (long2 - long1) * _d2r;
            double dlat = (lat2 - lat1) * _d2r;
            double a = Math.Pow(Math.Sin(dlat / 2D), 2D) + Math.Cos(lat1 * _d2r)
             * Math.Cos(lat2 * _d2r) * Math.Pow(Math.Sin(dlong / 2D), 2D);
            double c = 2D * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1D - a));
            double d = _eQuatorialEarthRadius * c;

            return d;
        }
    private float HaversineInM(double lat1, double long1, double lat2, double long2){
        return (float)(1000D * HaversineInKM(lat1, long1, lat2, long2));
    }
}

