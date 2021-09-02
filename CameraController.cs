using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    //CinemachineVirtualCamera takes over the main camera and fully controls it
    // it allows to not have many cameras in the game and just have one rendering cam
    // main camera needs to have CinemachineBrain components to be taken over
  
    [SerializeField] private Transform playerCameraTransform = null;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float screenBoarderThickness = 10f;
    [SerializeField] private Vector2 screenXLimits = Vector2.zero;
    [SerializeField] private Vector2 screenZLimits = Vector2.zero;

    private Vector2 previousInput;

    private Controls controls;

    public override void OnStartAuthority()
    {
        // in order to only have one gameobject with the CVC activated
        // we set it active once has have the authority == once a
        // client has control over this script and thus setting only
        // once camera active in this client's instance of the game
        playerCameraTransform.gameObject.SetActive(true);

        // initializing our controls for new unity input system
        controls = new Controls();

        // MoveCamera is the name we gave to the actions in the giver controls file
        // performed is when the key is down and canceled is when the key get's up
        controls.Player.MoveCamera.performed += SetPrivousInput;
        controls.Player.MoveCamera.canceled += SetPrivousInput;

        // New input system does not require to unsubscribe from the events as
        // it is handled for you
        controls.Enable();
    }

    // only for clients
    [ClientCallback]
    private void Update()
    {
        // Applications.isFocused tells the a user is focused on the game or if
        // the user is tabbed out
        // Currently used in order not to move camera when we are not playing
        if (!hasAuthority || !Application.isFocused) { return; }

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector3 pos = playerCameraTransform.position;

        // if we have no keyboard input
        // vector2.zero is (0,0) which is the same when there is not input 
        if(previousInput == Vector2.zero)
        {
            Vector3 cursorMovement = Vector3.zero;

            Vector2 cursorPosition = Mouse.current.position.ReadValue();

            // if you are heigher then the edge of the screen - a little
            // bit of boarder so it starts earlier
            // the first one uses y as it's a vector 2
            if(cursorPosition.y >= Screen.height - screenBoarderThickness)
            {
                cursorMovement.z += 1;
            }
            // bottom of the screen is zero
            else if(cursorPosition.y <= screenBoarderThickness)
            {
                cursorMovement.z -= 1;
            }

            if (cursorPosition.x >= Screen.width - screenBoarderThickness)
            {
                cursorMovement.x += 1;
            }
            // bottom of the screen is zero
            else if (cursorPosition.x <= screenBoarderThickness)
            {
                cursorMovement.x -= 1;
            }

            // normalized makes the vector always have a value of 1
            // thus diagnal movement isn't faster
            // Time.deltaTime makes it frame rate independent 
            pos += cursorMovement.normalized * speed * Time.deltaTime;
        }
        else // if we did have keyboard input we add that
        {
            // the new input systerm returns a vector2 (x,z)
            // in which the first value is corresponding to the left and right inputs
            // and is either 1 or -1, and the second value that is correcponding
            // with up or down beeing 1 or -1 as well

            pos += new Vector3(previousInput.x, 0f, previousInput.y) * speed * Time.deltaTime;
        }

        // screenXLimits and screenZLimist are just vector2 in which
        // we store the min and the max bondaries of the screen
        // space we want to be playable
        // Mathf.Clamp returns the result between min and max value
        // if it's below the min returns the min
        // if it's above the max returns the max
        pos.x = Mathf.Clamp(pos.x, screenXLimits.x, screenXLimits.y);
        pos.z = Mathf.Clamp(pos.z, screenZLimits.x, screenZLimits.y);

        // pos is basically the current cam position + the change made by input system
        // multiplied by the speed and Time.deltaTime to give frame rate independancy

        playerCameraTransform.position = pos;
    }

    private void SetPrivousInput(InputAction.CallbackContext ctx)
    {
        // InputAction.CallbackContext get's returned after the action is
        // performed or canceled
        // the ctx does not know what type of data will be comming so we
        // specify it using <Vector2>
        previousInput = ctx.ReadValue<Vector2>();
    }
}
