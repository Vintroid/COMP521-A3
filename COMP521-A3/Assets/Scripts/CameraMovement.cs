using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Affecting movement speed
    [SerializeField] float keyboardInputSensitivity = 1f;
    [SerializeField] float mouseInputSensitivityHorizontal = 1f;
    [SerializeField] float mouseInputSensitivityVertical = 1f;
    [SerializeField] float scrollSensitivity = 1f;

    // Camera position bounds
    [SerializeField] Transform bottomLeftBorder;
    [SerializeField] Transform topRightBorder;

    // Keyboard input vector
    private Vector3 input;

    // Mouse input floats
    private float leftRightMouse;
    private float upDownMouse;


    // Update is called once per frame
    void Update()
    {
        NullInput();
        MoveCameraInput();

        MoveCamera();
    }

    // Resetting input elements for the next update
    private void NullInput()
    {
        input.x = 0;
        input.y = 0;
        input.z = 0;
    }

    // To box the camera movement so that it doesn't go too far.
    private void MoveCamera()
    {
        Vector3 position = transform.position;
        position += (input * Time.deltaTime);
        position.x = Mathf.Clamp(position.x, bottomLeftBorder.position.x, topRightBorder.position.x);
        position.z = Mathf.Clamp(position.z, bottomLeftBorder.position.z, topRightBorder.position.z);
        transform.position = position;
    }

    // Moving camera according to keyboard and mouse inputs
    private void MoveCameraInput()
    {
        AxisInput();
        MouseInput();
        ScrollInput();
    }

    // Zooming camera according to orthographic camera mode
    private void ScrollInput()
    {
        this.gameObject.GetComponent<Camera>().orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
    }

    // Rotating camera following mouse inputs
    private void MouseInput()
    {
        leftRightMouse += mouseInputSensitivityHorizontal * Input.GetAxis("Mouse X");
        upDownMouse -= mouseInputSensitivityVertical * Input.GetAxis("Mouse Y");

        transform.eulerAngles = new Vector3(upDownMouse, leftRightMouse, 0f);
    }

    // Get movement from keyboard depending on keyboard movement speed
    private void AxisInput()
    {
        input.x += Input.GetAxisRaw("Horizontal") * keyboardInputSensitivity;
        input.z += Input.GetAxisRaw("Vertical") * keyboardInputSensitivity;
    }
}
