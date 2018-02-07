using System;
using System.Collections.Generic;
using UnityEngine;

public enum Controls
{
    LeftStick,
    RightStick
}

class InputManager : Singleton<InputManager>
{
    public Vector2 GetStick(Controls control)
    {

        switch (control)
        {
            case Controls.LeftStick:
                return new Vector2(Input.GetAxis("J_LeftStickX"), Input.GetAxis("J_LeftStickY"));
            case Controls.RightStick:
                return new Vector2(Input.GetAxis("J_RightStickX"), Input.GetAxis("J_RightStickY"));
        }
        return Vector2.zero;
    }

    public bool GetButtonDown(Controls control)
    {
        switch (control)
        {
            case Controls.LeftStick:
                return Input.GetButtonDown("J_LeftStickPress");
            case Controls.RightStick:
                return Input.GetButtonDown("J_RightStickPress");
        }
        return false;
    }

    private int Xbox_One_Controller = 0;
    private int PS4_Controller = 0;
    void Update()
    {
        string[] names = Input.GetJoystickNames();
        for (int x = 0; x < names.Length; x++)
        {
            print(names[x].Length);
            if (names[x].Length == 19)
            {
                print("PS4 CONTROLLER IS CONNECTED");
                PS4_Controller = 1;
                Xbox_One_Controller = 0;
            }
            if (names[x].Length == 33)
            {
                print("XBOX ONE CONTROLLER IS CONNECTED");
                //set a controller bool to true
                PS4_Controller = 0;
                Xbox_One_Controller = 1;

            }
        }


        if (Xbox_One_Controller == 1)
        {
            //do something
        }
        else if (PS4_Controller == 1)
        {
            //do something
        }
        else
        {
            // there is no controllers
        }
    }
}
