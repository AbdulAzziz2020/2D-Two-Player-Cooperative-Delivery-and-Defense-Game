namespace Game
{
    public interface IInteractable
    {
        void SetHighlight(bool isHighlighted);
        bool CanInteract(Player player);
        void Interact(Player player);
    }
}