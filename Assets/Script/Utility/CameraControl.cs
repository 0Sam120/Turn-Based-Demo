using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class CameraControl : MonoBehaviour
{
    private CameraMovement cameraActions; // Input actions asset for the camera
    private InputAction movement;         // Reference to the movement input action

    private Transform cameraTransform;    // Reference to the camera's transform

    // Horizontal movement settings
    [SerializeField] private float maxSpeed = 5f;          // Maximum movement speed
    [SerializeField] private float acceleration = 10f;     // How quickly camera accelerates
    [SerializeField] private float damping = 15f;          // How quickly camera slows down
    private float speed;                                   // Current calculated speed

    // Zoom settings
    [SerializeField] private float stepSize = 2f;          // Amount to zoom per scroll input
    [SerializeField] private float zoomDampening = 7.5f;   // Smoothness of zoom transition
    [SerializeField] private float minHeight = 5f;         // Minimum camera height
    [SerializeField] private float maxHeight = 50f;        // Maximum camera height
    [SerializeField] private float zoomSpeed = 2f;         // Speed multiplier for zoom

    // Rotation settings
    [SerializeField] private float maxRotationSpeed = 1f;  // Maximum rotation speed

    // Internal working variables
    private Vector3 targetPosition;    // Target position to move toward
    private float zoomHeight;          // Target height for zooming
    private float currentRotationInput = 0f; // Current rotation input value
    private Vector3 horizontalVelocity; // Smoothed horizontal movement
    private Vector3 lastPosition;       // Last frame's position (to calculate velocity)

    private void Awake()
    {
        // Initialize input actions and find the camera transform
        cameraActions = new CameraMovement();
        cameraTransform = this.GetComponentInChildren<Camera>().transform;
    }

    private void OnEnable()
    {
        // Initialize zoom height and camera look-at
        zoomHeight = cameraTransform.localPosition.y;
        cameraTransform.LookAt(this.transform);

        // Initialize last position for velocity calculation
        lastPosition = this.transform.position;

        // Setup input action bindings
        movement = cameraActions.Camera.Movement;
        cameraActions.Camera.RotateCamera.started += RotateCamera;
        cameraActions.Camera.RotateCamera.canceled += RotateCamera;
        cameraActions.Camera.ZoomCamera.performed += ZoomCamera;
        cameraActions.Camera.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe input action events
        cameraActions.Camera.RotateCamera.started -= RotateCamera;
        cameraActions.Camera.RotateCamera.canceled -= RotateCamera;
        cameraActions.Camera.ZoomCamera.performed -= ZoomCamera;
        cameraActions.Disable();
    }

    private void Update()
    {
        // Update every frame
        GetKeyboardMovement();
        UpdateVelocity();
        UpdateCameraPostition();
        UpdateBasePosition();
    }

    private void UpdateVelocity()
    {
        // Calculate current horizontal velocity based on position delta
        horizontalVelocity = (this.transform.position - lastPosition) / Time.deltaTime;
        horizontalVelocity.y = 0; // Ignore vertical velocity
        lastPosition = this.transform.position;
    }

    private void GetKeyboardMovement()
    {
        // Read movement input and calculate intended movement vector
        Vector3 inputValue = movement.ReadValue<Vector2>().x * GetCameraRight()
                           + movement.ReadValue<Vector2>().y * GetCameraForward();

        inputValue = inputValue.normalized; // Normalize to avoid faster diagonal movement

        if (inputValue.sqrMagnitude > 0.1f)
            targetPosition += inputValue;
    }

    private Vector3 GetCameraRight()
    {
        // Get right direction relative to camera, ignoring vertical axis
        Vector3 right = cameraTransform.right;
        right.y = 0;
        return right;
    }

    private Vector3 GetCameraForward()
    {
        // Get forward direction relative to camera, ignoring vertical axis
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        return forward;
    }

    private void UpdateBasePosition()
    {
        if (targetPosition.sqrMagnitude > 0.1f)
        {
            // Accelerate and move toward target position
            speed = Mathf.Lerp(speed, maxSpeed, Time.deltaTime * acceleration);
            transform.position += targetPosition * speed * Time.deltaTime;
        }
        else
        {
            // No input — apply damping to gradually stop
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.deltaTime * damping);
            transform.position += horizontalVelocity * Time.deltaTime;
        }

        targetPosition = Vector3.zero; // Reset for next frame
    }

    private void RotateCamera(InputAction.CallbackContext inputValue)
    {
        // Update rotation input value when rotate input is triggered
        currentRotationInput = inputValue.ReadValue<float>();
        Debug.Log($"Rotation input value: {currentRotationInput}");
    }

    private void ZoomCamera(InputAction.CallbackContext inputValue)
    {
        // Handle zoom input
        float value = -inputValue.ReadValue<Vector2>().y; // Invert for natural scrolling

        if (Mathf.Abs(value) > 0.1f)
        {
            zoomHeight = cameraTransform.localPosition.y + value * stepSize;
            
            // Clamp zoom height to limits
            if (zoomHeight < minHeight)
                zoomHeight = minHeight;
            else if (zoomHeight > maxHeight)
                zoomHeight = maxHeight;
        }
    }

    private void UpdateCameraPostition()
    {
        // Handle zooming and rotation per frame

        // Target position for zoom
        Vector3 zoomTarget = new Vector3(cameraTransform.localPosition.x, zoomHeight, cameraTransform.localPosition.z);

        // Adjust zoom target to move slightly forward when zooming
        zoomTarget -= zoomSpeed * (zoomHeight - cameraTransform.localPosition.y) * Vector3.forward;

        if (Mathf.Abs(currentRotationInput) != 0f)
        {
            // Rotate the camera's parent (the whole rig) based on input
            float rotationAmount = currentRotationInput * maxRotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, rotationAmount);
        }

        // Smoothly interpolate the camera's local position toward the zoom target
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, zoomTarget, Time.deltaTime * zoomDampening);

        // Always have the camera look at the parent object (the focal point)
        cameraTransform.LookAt(this.transform);
    }
}
