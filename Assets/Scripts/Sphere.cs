using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sphere : MonoBehaviour
{
    Rigidbody _sphere;
    InputAction _fireAction;
    InputAction _pointAction;
    InputAction _bounceAction;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start sphere");
        _fireAction = InputSystem.actions.FindAction("Attack");
        _pointAction = InputSystem.actions.FindAction("Point");
        _bounceAction = InputSystem.actions.FindAction("Bounce");
        Debug.Log("Start sphere 2");
        _sphere = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_fireAction.WasPressedThisFrame())
        {
            Debug.Log("Attack action!");
            _sphere.AddForce(new Vector3(0, 150, 0));
        }
        var point = _pointAction.ReadValue<Vector2>();
        Debug.Log($"mouse   x: {point.x}   y: {point.y}");
        if (_bounceAction.WasPerformedThisFrame())
        {
            Debug.Log("******************* Performed!");
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        Debug.Log("Attack!");
    }
}
