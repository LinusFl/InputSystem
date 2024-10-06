using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sphere : MonoBehaviour
{
    Rigidbody _sphere;
    InputAction _attackAction;
    InputAction _bounceAction;

    // Start is called before the first frame update
    void Start()
    {
        _attackAction = InputSystem.actions.FindAction("Attack");
        _bounceAction = InputSystem.actions.FindAction("Bounce");
        _sphere = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_attackAction.WasPressedThisFrame())
        {
            _sphere.AddForce(new Vector3(0, 150, 0));
        }

        var point = _bounceAction.ReadValue<Vector2>();
        var distance = point.magnitude;
        var angle = Mathf.Atan2(point.y, point.x);

        // Debug.Log($"mouse   x: {point.x}   y: {point.y}");
        Debug.Log($"distance: {distance}   angle: {angle * Mathf.Rad2Deg}");
        if (_bounceAction.WasPerformedThisFrame())
        {
            // Debug.Log("******************* Performed!");
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        Debug.Log("Attack!");
    }
}
