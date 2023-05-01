using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MenuInputManager : MonoBehaviour 
{
    public bool blockOnSelected;
    public GameObject blockedObject;
    public UnityEvent onA;
    public UnityEvent onB;
    public UnityEvent onX;
    public UnityEvent onY;
    public UnityEvent onStart;
    public InputActionReference inputA;
    public InputActionReference inputB;
    public InputActionReference inputX;
    public InputActionReference inputY;
    public InputActionReference inputStart;

    private void OnEnable()
    {
        inputA.action.performed += onPressA;
        inputB.action.performed += onPressB;
        inputX.action.performed += onPressX;
        inputY.action.performed += onPressY;
        inputStart.action.performed += onPressStart;

    }
    
    private void OnDisable()
    {
        inputA.action.performed -= onPressA;
        inputB.action.performed -= onPressB;
        inputX.action.performed -= onPressX;
        inputY.action.performed -= onPressY;
        inputStart.action.performed -= onPressStart;
    }

    void onPressA(InputAction.CallbackContext input)
    {
        if (blockOnSelected && EventSystem.current.currentSelectedGameObject == blockedObject) return;
        Debug.Log("A Pressed");
        onA.Invoke();
    }
    
    void onPressB(InputAction.CallbackContext input)
    {
        if (blockOnSelected && EventSystem.current.currentSelectedGameObject == blockedObject) return;
        Debug.Log("B Pressed");
        onB.Invoke();
    }
    
    void onPressX(InputAction.CallbackContext input)
    {
        if (blockOnSelected && EventSystem.current.currentSelectedGameObject == blockedObject) return;
        Debug.Log("X Pressed");
        onX.Invoke();
    }

    void onPressY(InputAction.CallbackContext input)
    {
        if (blockOnSelected && EventSystem.current.currentSelectedGameObject == blockedObject) return;
        Debug.Log("Y Pressed");
        onY.Invoke();
    }

    void onPressStart(InputAction.CallbackContext input)
    {
        if (blockOnSelected && EventSystem.current.currentSelectedGameObject == blockedObject) return;
        Debug.Log("Start Pressed");
        onStart.Invoke();
    }
    

    //this fixes a bug with the above code 
    //when you close the window, it 
    //reselects the button for navigation,
    //but since we enabled it on the same frame,
    //it also selects it

    //this adds a very small, but unnoticable delay
    //so we don't trigger the button's selection
    private IEnumerator InvokeEvent(UnityEvent e)
    {
        yield return new WaitForSecondsRealtime(0.05f);
        e.Invoke();
    }
}
