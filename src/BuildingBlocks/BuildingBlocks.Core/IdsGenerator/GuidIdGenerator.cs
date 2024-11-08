using BuildingBlocks.Abstractions.Core;

namespace BuildingBlocks.Core.IdsGenerator;

public partial class GuidIdGenerator : IIdGenerator<Guid>
{
    public Guid New()
    {
        return Guid.NewGuid();
    }
}
