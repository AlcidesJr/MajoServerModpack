using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MajoServerModpack.Core.Input
{
    public enum InputDevice
    {
        Keyboard,
        Gamepad
    }

    public sealed class InputBinding : IEquatable<InputBinding>
    {
        public InputBinding(InputDevice device, string control)
        {
            if (string.IsNullOrWhiteSpace(control))
            {
                throw new ArgumentException("Input control is required.", nameof(control));
            }

            Device = device;
            Control = control.Trim();
        }

        public InputDevice Device { get; }
        public string Control { get; }

        public bool Equals(InputBinding other)
        {
            return other != null &&
                   Device == other.Device &&
                   string.Equals(Control, other.Control, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InputBinding);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Device * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Control);
            }
        }

        public override string ToString()
        {
            return Device + ":" + Control;
        }
    }

    public sealed class InputActionDescriptor
    {
        public InputActionDescriptor(
            string actionId,
            string moduleId,
            string context,
            InputBinding defaultBinding,
            bool allowSharedBinding = false,
            int priority = 0,
            string description = "")
        {
            if (string.IsNullOrWhiteSpace(actionId))
            {
                throw new ArgumentException("ActionId is required.", nameof(actionId));
            }

            if (string.IsNullOrWhiteSpace(moduleId))
            {
                throw new ArgumentException("ModuleId is required.", nameof(moduleId));
            }

            if (string.IsNullOrWhiteSpace(context))
            {
                throw new ArgumentException("Input context is required.", nameof(context));
            }

            ActionId = actionId.Trim();
            ModuleId = moduleId.Trim();
            Context = context.Trim();
            DefaultBinding = defaultBinding;
            AllowSharedBinding = allowSharedBinding;
            Priority = priority;
            Description = description ?? string.Empty;
        }

        public string ActionId { get; }
        public string ModuleId { get; }
        public string Context { get; }
        public InputBinding DefaultBinding { get; }
        public bool AllowSharedBinding { get; }
        public int Priority { get; }
        public string Description { get; }
    }

    public sealed class InputActionSnapshot
    {
        public InputActionSnapshot(InputActionDescriptor descriptor, InputBinding currentBinding)
        {
            Descriptor = descriptor;
            CurrentBinding = currentBinding;
        }

        public InputActionDescriptor Descriptor { get; }
        public InputBinding CurrentBinding { get; }
    }

    public sealed class InputConflict
    {
        public InputConflict(string actionId, string conflictingActionId, InputBinding binding)
        {
            ActionId = actionId;
            ConflictingActionId = conflictingActionId;
            Binding = binding;
        }

        public string ActionId { get; }
        public string ConflictingActionId { get; }
        public InputBinding Binding { get; }
    }

    public sealed class BindingChangeResult
    {
        public BindingChangeResult(bool applied, IReadOnlyList<InputConflict> conflicts)
        {
            Applied = applied;
            Conflicts = conflicts;
        }

        public bool Applied { get; }
        public IReadOnlyList<InputConflict> Conflicts { get; }
    }

    public sealed class InputRegistry
    {
        private sealed class Entry
        {
            public Entry(InputActionDescriptor descriptor)
            {
                Descriptor = descriptor;
                CurrentBinding = descriptor.DefaultBinding;
            }

            public InputActionDescriptor Descriptor { get; }
            public InputBinding CurrentBinding { get; set; }
        }

        private readonly Dictionary<string, Entry> _entries =
            new Dictionary<string, Entry>(StringComparer.Ordinal);
        private readonly List<string> _order = new List<string>();

        public int Count => _entries.Count;

        public IReadOnlyList<InputConflict> Register(InputActionDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (_entries.ContainsKey(descriptor.ActionId))
            {
                throw new InvalidOperationException("Duplicate ActionId: " + descriptor.ActionId);
            }

            var entry = new Entry(descriptor);
            _entries.Add(descriptor.ActionId, entry);
            _order.Add(descriptor.ActionId);

            return FindConflicts(descriptor.ActionId, entry.CurrentBinding);
        }

        public BindingChangeResult TrySetBinding(string actionId, InputBinding binding)
        {
            Entry entry;
            if (!_entries.TryGetValue(actionId, out entry))
            {
                throw new KeyNotFoundException("Unknown ActionId: " + actionId);
            }

            var conflicts = FindConflicts(actionId, binding);
            if (conflicts.Count > 0)
            {
                return new BindingChangeResult(false, conflicts);
            }

            entry.CurrentBinding = binding;
            return new BindingChangeResult(true, conflicts);
        }

        public IReadOnlyList<InputConflict> FindConflicts(string actionId)
        {
            Entry entry;
            if (!_entries.TryGetValue(actionId, out entry))
            {
                throw new KeyNotFoundException("Unknown ActionId: " + actionId);
            }

            return FindConflicts(actionId, entry.CurrentBinding);
        }

        public IReadOnlyList<InputActionSnapshot> Snapshot()
        {
            var snapshots = new List<InputActionSnapshot>(_order.Count);
            foreach (var actionId in _order)
            {
                var entry = _entries[actionId];
                snapshots.Add(new InputActionSnapshot(entry.Descriptor, entry.CurrentBinding));
            }

            return new ReadOnlyCollection<InputActionSnapshot>(snapshots);
        }

        private IReadOnlyList<InputConflict> FindConflicts(string actionId, InputBinding binding)
        {
            var conflicts = new List<InputConflict>();
            if (binding == null)
            {
                return new ReadOnlyCollection<InputConflict>(conflicts);
            }

            var candidate = _entries[actionId];
            foreach (var otherId in _order)
            {
                if (string.Equals(actionId, otherId, StringComparison.Ordinal))
                {
                    continue;
                }

                var other = _entries[otherId];
                if (other.CurrentBinding == null || !binding.Equals(other.CurrentBinding))
                {
                    continue;
                }

                if (!ContextsOverlap(candidate.Descriptor.Context, other.Descriptor.Context))
                {
                    continue;
                }

                if (candidate.Descriptor.AllowSharedBinding && other.Descriptor.AllowSharedBinding)
                {
                    continue;
                }

                conflicts.Add(new InputConflict(actionId, otherId, binding));
            }

            return new ReadOnlyCollection<InputConflict>(conflicts);
        }

        private static bool ContextsOverlap(string left, string right)
        {
            return string.Equals(left, "Global", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(right, "Global", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }
    }
}
