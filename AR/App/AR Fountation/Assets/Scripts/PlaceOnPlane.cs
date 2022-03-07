using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// AR raycasts will only hit detected trackables like feature points and planes.
/// If a raycast hits a trackable, the chosen prefab is instantiated and moved to the hit position.

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane : MonoBehaviour{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    UnityEvent placementUpdate;
    [SerializeField]
    public GameObject Pig, Minotaur;
    private GameObject SpawnedPig, SpawnedMinotaur;
    private bool canPlacePig, canPlaceMinotaur;
    GameObject GPS;
    Vector2 screenCoordinatesToAutoPlace;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    ARRaycastManager m_RaycastManager;

    private void Start(){
        GameEvents.current.onEnteredPig += CanPlacePig;
        GameEvents.current.onExitedPig += CanDeletePig;
        GameEvents.current.onEnteredMinotaur += CanPlaceMinotaur;
        GameEvents.current.onExitedMinotaur += CanDeleteMinotaur;
    }

    void Awake(){
        screenCoordinatesToAutoPlace = new Vector2(Screen.width / 2, Screen.height / 2); // center of screen
        m_RaycastManager = GetComponent<ARRaycastManager>();
        if (placementUpdate == null) placementUpdate = new UnityEvent();
    }

    void Update(){
        HandleObjectPlacement();
        HandleObjectDeletion();
    }

    private void HandleObjectPlacement(){
        if (canPlacePig) PlaceObject("Pig");
        else if (canPlaceMinotaur) PlaceObject("Minotaur");
    }

    private void HandleObjectDeletion(){
        if (!canPlacePig && SpawnedPig != null) Destroy(SpawnedPig);
        else if (!canPlaceMinotaur && SpawnedMinotaur != null) Destroy(SpawnedMinotaur);
    }

    private void PlaceObject(string s){
        // only if raycast test to screen center is true, start seeing which object is to place
        if (m_RaycastManager.Raycast(screenCoordinatesToAutoPlace, s_Hits, TrackableType.PlaneWithinPolygon)){
            var hitPose = s_Hits[0].pose;
            if (s.Equals("Pig") && SpawnedPig == null) SpawnedPig = Instantiate(Pig, hitPose.position, hitPose.rotation);
            else if (s.Equals("Minotaur") && SpawnedMinotaur == null) SpawnedMinotaur = Instantiate(Minotaur, hitPose.position, hitPose.rotation);
            placementUpdate.Invoke();
        } 
    }

    private void CanPlacePig()  => canPlacePig = true;
    private void CanDeletePig() => canPlacePig = false;
    private void CanPlaceMinotaur() => canPlaceMinotaur = true;
    private void CanDeleteMinotaur() => canPlaceMinotaur = false;


    // if using touch, left this here just in case
    //bool TryGetTouchPosition(out Vector2 touchPosition){
    //    if (Input.touchCount > 0){
    //        touchPosition = Input.GetTouch(0).position;
    //        return true;
    //    }
    //    touchPosition = default;
    //    return false;
    //}
}