namespace MajoServerModpack.Core.Runtime
{
    public enum ExecutionContextKind
    {
        MenuOrPreWorld,
        DedicatedServer,
        ListenServer,
        ConnectedClient,
        LocalWorld,
        Unknown
    }

    public sealed class ExecutionContextSnapshot
    {
        public ExecutionContextSnapshot(ExecutionContextKind kind, string detail)
        {
            Kind = kind;
            Detail = detail ?? string.Empty;
        }

        public ExecutionContextKind Kind { get; }
        public string Detail { get; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Detail) ? Kind.ToString() : Kind + " (" + Detail + ")";
        }
    }

    public interface IExecutionContextProvider
    {
        ExecutionContextSnapshot Detect();
    }
}
