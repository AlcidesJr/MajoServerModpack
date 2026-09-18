using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MajoServerModpack.Core.Patching
{
    public sealed class PatchSurfaceSnapshot
    {
        public PatchSurfaceSnapshot(string surfaceId, string ownerModuleId, IReadOnlyList<string> consumers)
        {
            SurfaceId = surfaceId;
            OwnerModuleId = ownerModuleId;
            Consumers = consumers;
        }

        public string SurfaceId { get; }
        public string OwnerModuleId { get; }
        public IReadOnlyList<string> Consumers { get; }
    }

    public sealed class PatchCoordinator
    {
        private sealed class Entry
        {
            public string OwnerModuleId;
            public readonly HashSet<string> Consumers = new HashSet<string>(StringComparer.Ordinal);
        }

        private readonly Dictionary<string, Entry> _entries =
            new Dictionary<string, Entry>(StringComparer.Ordinal);

        public int Count => _entries.Count;

        public void RegisterOwner(string surfaceId, string ownerModuleId)
        {
            ValidateToken(surfaceId, nameof(surfaceId));
            ValidateToken(ownerModuleId, nameof(ownerModuleId));

            Entry entry;
            if (!_entries.TryGetValue(surfaceId, out entry))
            {
                entry = new Entry();
                _entries.Add(surfaceId, entry);
            }

            if (entry.OwnerModuleId != null &&
                !string.Equals(entry.OwnerModuleId, ownerModuleId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Patch surface '" + surfaceId + "' already owned by '" + entry.OwnerModuleId + "'.");
            }

            entry.OwnerModuleId = ownerModuleId;
        }

        public void RegisterConsumer(string surfaceId, string consumerModuleId)
        {
            ValidateToken(surfaceId, nameof(surfaceId));
            ValidateToken(consumerModuleId, nameof(consumerModuleId));

            Entry entry;
            if (!_entries.TryGetValue(surfaceId, out entry))
            {
                entry = new Entry();
                _entries.Add(surfaceId, entry);
            }

            entry.Consumers.Add(consumerModuleId);
        }

        public IReadOnlyList<PatchSurfaceSnapshot> Snapshot()
        {
            var keys = new List<string>(_entries.Keys);
            keys.Sort(StringComparer.Ordinal);

            var result = new List<PatchSurfaceSnapshot>(keys.Count);
            foreach (var key in keys)
            {
                var entry = _entries[key];
                var consumers = new List<string>(entry.Consumers);
                consumers.Sort(StringComparer.Ordinal);
                result.Add(new PatchSurfaceSnapshot(
                    key,
                    entry.OwnerModuleId,
                    new ReadOnlyCollection<string>(consumers)));
            }

            return new ReadOnlyCollection<PatchSurfaceSnapshot>(result);
        }

        private static void ValidateToken(string value, string parameter)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value is required.", parameter);
            }
        }
    }
}
