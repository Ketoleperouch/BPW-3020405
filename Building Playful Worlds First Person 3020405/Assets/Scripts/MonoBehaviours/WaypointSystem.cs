using UnityEngine;
using UnityEngine.UI;

public class WaypointSystem : MonoBehaviour {

    public Transform waypoint;
    public GameObject marker;
    public Sprite onScreen;
    public Sprite offScreen;

    private static WaypointSystem system;

    private void Start()
    {
        system = this;
    }

    private void LateUpdate()
    {
        ShowMarkers();
    }

    public static void SetWaypoint(Transform waypoint)
    {
        system.waypoint = waypoint;
    }

    //For use through UnityEvents
    public void SetWayPointNonStatic(Transform waypoint)
    {
        system.waypoint = waypoint;
    }

    private void ShowMarkers()
    {
        if (waypoint == null)
        {
            marker.SetActive(false);
            return;
        }
        else
        {
            marker.SetActive(true);
        }
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(waypoint.transform.position);

        //Check if on screen or offscreen
        if (screenPosition.z > 0 && screenPosition.x > 0 && screenPosition.x < Screen.width && screenPosition.y > 0 && screenPosition.y < Screen.height)
        {
            //On screen, display on screen marker
            marker.GetComponent<Image>().sprite = onScreen;
            marker.GetComponent<RectTransform>().rotation = Quaternion.identity;
            marker.GetComponent<RectTransform>().position = screenPosition;
        }
        else
        {
            //Offscreen

            Vector3 screenCenter = new Vector3(Screen.width, Screen.height) / 2;

            //Set {0, 0} as the screen center
            screenPosition -= screenCenter;

            //Reverse marker if target is behind player
            if (screenPosition.z < 0)
            {
                screenPosition *= -1;
            }

            //Magic stuff and dangerous mathematics
            float relativeAngle = Mathf.Atan2(screenPosition.y, screenPosition.x);
            relativeAngle -= 90 * Mathf.Deg2Rad;

            float cosAngle = Mathf.Cos(relativeAngle);
            float sinAngle = -Mathf.Sin(relativeAngle);

            screenPosition = screenCenter + new Vector3(sinAngle * 150, cosAngle * 150);

            float m = cosAngle / sinAngle;

            Vector3 screenBounds = screenCenter * 0.9f;

            //Check if target is up or down out of screen bounds
            screenPosition = new Vector3(cosAngle > 0 ? screenBounds.y / m : -screenBounds.y / m, cosAngle > 0 ? screenBounds.y : -screenBounds.y);

            //Check if target is left or right out of screen bounds
            if (screenPosition.x > screenBounds.x)
            {
                screenPosition = new Vector3(screenBounds.x, screenBounds.x * m);
            }
            else if (screenPosition.x < -screenBounds.x)
            {
                screenPosition = new Vector3(-screenBounds.x, -screenBounds.x * m);
            }

            //Set {0, 0} back to normal
            screenPosition += screenCenter;

            //Display out-of-bounds marker
            marker.GetComponent<Image>().sprite = offScreen;
            marker.GetComponent<RectTransform>().position = screenPosition;
            marker.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, relativeAngle * Mathf.Rad2Deg);

        }
    }

}
