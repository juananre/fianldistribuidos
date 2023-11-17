using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class InputController : MonoBehaviour
{

    public static InputController _Instance { get; set; }
    public event Action<Axis> onAxisChange;

    private static Axis axis = new Axis { Horizontal = 0, Vertical =0};
    Axis LastAxis = new Axis { Horizontal = 0, Vertical =0};

    void Start()
    {
        _Instance = this;

    }

    // Update is called once per frame
    void Update()
    {
        var verticalInput = Input.GetAxis("Vertical");
        var horizontalInput = Input.GetAxis("Horizontal");

        axis.Vertical = Mathf.RoundToInt(verticalInput);
        axis.Horizontal = Mathf.RoundToInt(horizontalInput);

    }

    private void LateUpdate()
    {
        if (AxisChange())
        {
            LastAxis = new Axis { Horizontal = axis.Horizontal, Vertical = axis.Vertical };
            //NetworkController._Instance.Socket.Emit("move", axis);
            onAxisChange?.Invoke(axis);
        }
    }
 

    private bool AxisChange()
    {
        return (axis.Vertical != LastAxis.Vertical || axis.Horizontal !=LastAxis.Horizontal);
    }
}

public class Axis
{
    public int Horizontal;
    public int Vertical;
}


