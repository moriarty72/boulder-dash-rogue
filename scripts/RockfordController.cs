using Godot;

public partial class RockfordController : BaseGridObjectController
{
    private const double IDLE_INPUT_TIME = 0.15;

    public enum State
    {
        Alive,
        Dead
    }

    public enum MoveDirection
    {
        left,
        right,
        up,
        down,
        none
    };

    public State CurrentState { get; set; } = State.Alive;

    private double idleInputDelayTime = 0;
    private bool firePressed = false;

    public State rockfordState = State.Alive;

    private void ProcessPosition(double delta, MoveDirection moveDirection)
    {
        if (moveDirection == MoveDirection.none)
        {
            (NodeObject as Rockford).PlayAnimation(moveDirection);
            return;
        }

        PrevGridPosition = new(GridPosition.X, GridPosition.Y);
        if (!firePressed)
        {
            if (moveDirection == MoveDirection.up)
            {
                GridPosition.Y--;
            }

            if (moveDirection == MoveDirection.left)
            {
                GridPosition.X--;
            }

            if (moveDirection == MoveDirection.right)
            {
                GridPosition.X++;
            }

            if (moveDirection == MoveDirection.down)
            {
                GridPosition.Y++;
            }

            bool canPlayerMove = mainController.CanRockfordMove(GridPosition, moveDirection);
            if (canPlayerMove)
                (NodeObject as Rockford).PlayAnimation(moveDirection);
            else
                GridPosition = PrevGridPosition;
        }
        else
        {
            mainController.RockfordFireAction(GridPosition, moveDirection);
        }
    }

    private void ProcessUserInput(double delta)
    {
        Main.UserEvent inputEvent = mainController.GetInputEvent(delta);

        firePressed = (inputEvent & Main.UserEvent.ueFire) == Main.UserEvent.ueFire;

        if (inputEvent == Main.UserEvent.ueNone)
        {
            idleInputDelayTime += delta;
            if (idleInputDelayTime > IDLE_INPUT_TIME)
            {
                idleInputDelayTime = 0;
                ProcessPosition(delta, MoveDirection.none);
            }
        }

        if ((inputEvent & Main.UserEvent.ueLeft) == Main.UserEvent.ueLeft)
        {
            idleInputDelayTime = 0;
            ProcessPosition(delta, MoveDirection.left);
        }
        else if ((inputEvent & Main.UserEvent.ueRight) == Main.UserEvent.ueRight)
        {
            idleInputDelayTime = 0;
            ProcessPosition(delta, MoveDirection.right);
        }
        else if ((inputEvent & Main.UserEvent.ueUp) == Main.UserEvent.ueUp)
        {
            idleInputDelayTime = 0;
            ProcessPosition(delta, MoveDirection.up);
        }
        else if ((inputEvent & Main.UserEvent.ueDown) == Main.UserEvent.ueDown)
        {
            idleInputDelayTime = 0;
            ProcessPosition(delta, MoveDirection.down);
        }
    }

    public override void Process(double delta)
    {
        if (CurrentState == State.Dead)
            return;

        ProcessUserInput(delta);
    }

    public override void Update(double delta)
    {
        if (CurrentState == State.Dead)
            return;

        if (UpdateNodeObjectPosition())
            (NodeObject as Rockford).PlayAudio();
    }

    public override void Dead()
    {
        base.Dead();
        CurrentState = State.Dead;
    }
}