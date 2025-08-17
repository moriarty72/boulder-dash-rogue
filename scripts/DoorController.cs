using Godot;
using System;
using System.Data;
using System.Dynamic;

public partial class DoorController : BaseGridObjectController
{
    public enum State
    {
        Locked,
        Opened
    }

    public enum Color
    {
        Brown,
        Red,
        Green,
        Blue,
        Yellow,
        Silver
    }

    public State CurrentState = State.Opened;
    public Color CurrentColor = Color.Brown;

    public override void ProcessAndUpdate(double delta)
    {
        if (CurrentState == State.Opened)
        {
            (NodeObject as Door).SetColorAndState(CurrentColor.ToString(), 1);
        }
        else
        {
            (NodeObject as Door).SetColorAndState(CurrentColor.ToString(), 0);
        }
    }
}