using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionTracker : MonoBehaviour
{
    public List<GameObject> objectsToTrack;
    public List<GameObject> trackers;

    [SerializeField]
    private GameObject trackerArrow;

    public float margin = 0.05f;

    private void Start() {
        trackers = new List<GameObject>();
        objectsToTrack = new List<GameObject>();
    }

    public void AddNewTracker(GameObject g) {
        if (trackers == null) trackers = new List<GameObject>();
        if (objectsToTrack == null) objectsToTrack = new List<GameObject>();
        objectsToTrack.Add(g);
        trackers.Add(Instantiate(trackerArrow,transform));
    }
    public void RemoveTracker(GameObject g) {
        objectsToTrack.Remove(g);
    }

    private bool outOfScreen(Vector3 pos) {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(pos);
        return (screenPos.x < 0 || screenPos.x > Screen.width ||
                screenPos.y < 0 || screenPos.y > Screen.height);
    }

    private Vector2 limitToScreen(Vector2 pos) {
        Vector2 ScreenOriginPoint = (Vector2)Camera.main.ScreenToWorldPoint(new Vector2(0,0)) + new Vector2(margin, margin);
        Vector2 ScreenEndPoint = (Vector2)Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height)) - new Vector2(margin, margin);
        return Vector2.Max(ScreenOriginPoint, Vector2.Min(ScreenEndPoint, pos));
    }

    public void Update() {
        int i = 0;
        foreach(GameObject g in objectsToTrack) {
            if(outOfScreen(g.transform.position)) {
                trackers[i].GetComponentInChildren<SpriteRenderer>().enabled = true;
                trackers[i].transform.position = limitToScreen(g.transform.position);
                Vector2 dir = g.transform.position - trackers[i].transform.position;
                trackers[i].transform.rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg) - 90.0f);
            }else {
                trackers[i].GetComponentInChildren<SpriteRenderer>().enabled = false;
            }
            i++;
        }
    }
}
