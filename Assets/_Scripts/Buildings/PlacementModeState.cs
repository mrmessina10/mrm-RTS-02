// Flag compartido entre todos los controllers de "modo colocación" (edificio único, muro, y los que se sumen a
// futuro). SelectionManager lo chequea una sola vez en vez de conocer cada controller de colocación individualmente.
public static class PlacementModeState
{
    public static bool IsActive;
}
