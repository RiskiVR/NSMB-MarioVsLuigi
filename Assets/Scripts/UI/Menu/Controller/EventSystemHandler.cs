using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemHandler : MonoBehaviour
{
    public static void SetSelectedButton(GameObject SetActiveSelection)
    {
        EventSystem.current.SetSelectedGameObject(null); // EventSystem needs to be set to nothing before it can be updated to a new object
        EventSystem.current.SetSelectedGameObject(SetActiveSelection);
    }
}