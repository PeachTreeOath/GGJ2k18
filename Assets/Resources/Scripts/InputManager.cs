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
}
