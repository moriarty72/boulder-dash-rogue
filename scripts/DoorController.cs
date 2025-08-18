using Godot;
using ProceduralDungeon.Level;
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
        Red,
        Green,
        Blue,
        Yellow,
        Silver,
        Brown
    }

    public State CurrentState = State.Locked;
    public Color CurrentColor = Color.Blue;

    public DungeonRoomConnection RoomConnection = null;

    public override void ProcessAndUpdate(double delta)
    {
        if (NodeObject == null)
            return;
            
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