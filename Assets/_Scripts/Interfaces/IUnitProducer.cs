public interface IUnitProducer
{
    int QueuedCount { get; }
    float CurrentProgress { get; }
    bool TryEnqueueUnit(UnitType unitType);
}
