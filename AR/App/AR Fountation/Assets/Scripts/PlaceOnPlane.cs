using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// Listens for touch events and performs an AR raycast from the screen touch point.
/// AR raycasts will only hit detected trackables like feature points and planes.
/// If a raycast hits a trackable, the <see cref="placedPrefab"/> is instantiated and moved to the hit position.

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane : MonoBehaviour{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

UnityEvent placementUpdate;

[SerializeField]
GameObject GPS;
GameObject visualObject;
public bool automaticallyPlace;
Vector2 screenCoordinatesToAutoPlace;

/// The prefab to instantiate
public GameObject placedPrefab{
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
}

/// The object instantiated as a result of a successful raycast intersection with a plane.
public GameObject spawnedObject { get; private set; }

void Awake(){
    screenCoordinatesToAutoPlace = new Vector2(Screen.width / 2, Screen.height / 2);
    m_RaycastManager = GetComponent<ARRaycastManager>();
    if (placementUpdate == null) placementUpdate = new UnityEvent();
    placementUpdate.AddListener(DiableVisual);
}

bool TryGetTouchPosition(out Vector2 touchPosition){
    if (Input.touchCount > 0){
        touchPosition = Input.GetTouch(0).position;
        return true;
    }
    touchPosition = default;
    return false;
}

void Update(){
    // places prefab automatically if person is on the target location and there is a plane on screenCoordinatesToAutoPlace using raycast
    if (automaticallyPlace){
        if (spawnedObject == null && GPS.GetComponent<GPS>().distance < 0.1f &&
            m_RaycastManager.Raycast(screenCoordinatesToAutoPlace, s_Hits, TrackableType.PlaneWithinPolygon))
        { 
            var hitPose = s_Hits[0].pose;
            spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
            placementUpdate.Invoke();
        }   
    } else { 
        if (!TryGetTouchPosition(out Vector2 touchPosition)) return;
        // Raycast hits are sorted by distance, so the first one will be the closest hit.
        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon)){  
            var hitPose = s_Hits[0].pose;
            if (spawnedObject == null) spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
            else                       spawnedObject.transform.position = hitPose.position; //repositioning of the object 
        placementUpdate.Invoke();
        }
    }
}

public void DiableVisual() => visualObject.SetActive(false);

static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
ARRaycastManager m_RaycastManager;
}