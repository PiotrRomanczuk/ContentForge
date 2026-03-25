namespace ContentForge.Domain.Enums;

// Enums in C# are like: const ContentStatus = { Draft: 0, Generated: 1, ... } as const;
// But they're a proper type — you can't assign random strings/numbers to them.
public enum ContentStatus
{
    Draft,
    Generated,
    Rendered,
    Queued,
    Publishing,
    Published,
    Failed
}
