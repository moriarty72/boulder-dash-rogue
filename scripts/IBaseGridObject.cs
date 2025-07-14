using Godot;

public interface IBaseGridObject
{
    public void Process(Main mainController, BaseGridObject gridObject, double delta);
    public void Update(Main mainController, BaseGridObject gridObject, double delta);
}