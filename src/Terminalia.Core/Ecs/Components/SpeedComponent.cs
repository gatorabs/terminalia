using Terminalia.Core.Abstractions.Ecs;

namespace Terminalia.Core.Ecs.Components;

public sealed class SpeedComponent : IComponent
{
    public SpeedComponent(int cellsPerStep)
    {
        CellsPerStep = cellsPerStep;
    }

    public int CellsPerStep { get; set; }
}
