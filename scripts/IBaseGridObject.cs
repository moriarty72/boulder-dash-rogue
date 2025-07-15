using Godot;

public interface IBaseGridObject
{
    public void Process(Main mainController, BaseGridObjectController gridObject, double delta);
    public void Update(Main mainController, BaseGridObjectController gridObject, double delta);
}