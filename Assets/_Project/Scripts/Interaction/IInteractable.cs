namespace TheLightHouse
{
    public interface IInteractable
    {
        string Prompt { get; }
        bool RequiresHold { get; }
        void Interact();
        void StopInteract();
    }
}
