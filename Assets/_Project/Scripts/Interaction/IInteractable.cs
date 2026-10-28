namespace TheLightHouse
{
    public interface IInteractable
    {
        string Prompt { get; }
        void Interact();
        void StopInteract();
    }
}
