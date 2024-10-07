using UnityEngine;
using UnityEngine.InputSystem;

public class Sphere : MonoBehaviour
{
    Rigidbody _sphere;
    InputAction _bounceAction;

    void Start()
    {
        _bounceAction = InputSystem.actions.FindAction("Bounce");
        _sphere = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (_bounceAction.WasPerformedThisFrame())
            _sphere.AddForce(new Vector3(0, 150, 0));
    }
}
