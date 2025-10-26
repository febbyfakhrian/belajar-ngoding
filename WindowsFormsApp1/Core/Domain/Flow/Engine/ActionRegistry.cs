using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.Core.Domain.Flow.Engine
{
    /// <summary>
    /// Registry for flow actions that maps action keys to their implementations
    /// </summary>
    public sealed class ActionRegistry : IActionRegistry
    {
        /// <summary>
        /// Dictionary mapping action keys to their implementations
        /// </summary>
        private readonly Dictionary<string, IFlowAction> _map = new Dictionary<string, IFlowAction>(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// Registers an action with the registry
        /// </summary>
        /// <param name="action">The action to register</param>
        public void Register(IFlowAction action) => _map[action.Key] = action;
        
        /// <summary>
        /// Gets an action by its key
        /// </summary>
        /// <param name="key">The action key</param>
        /// <returns>The registered action</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no action is registered with the specified key</exception>
        public IFlowAction Get(string key) => _map.TryGetValue(key, out var a) ? a : throw new KeyNotFoundException(key);
        
        /// <summary>
        /// Attempts to get an action by its key
        /// </summary>
        /// <param name="key">The action key</param>
        /// <param name="action">The registered action if found, otherwise null</param>
        /// <returns>True if an action was found, false otherwise</returns>
        public bool TryGet(string key, out IFlowAction action) => _map.TryGetValue(key, out action);
    }
}