using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting
{
    /// <summary>
    /// Represents a unit within a flow graph, defining its behavior, ports, connections, and visualization in the graph editor.
    /// </summary>
    public interface IUnit : IGraphElementWithDebugData
    {
        /// <summary>
        /// Gets the flow graph to which this unit belongs.
        /// </summary>
        new FlowGraph graph { get; }

        #region Definition

        /// <summary>
        /// Gets a value indicating whether this unit can be defined.
        /// </summary>
        bool canDefine { get; }

        /// <summary>
        /// Gets a value indicating whether this unit is currently defined.
        /// </summary>
        bool isDefined { get; }

        /// <summary>
        /// Gets a value indicating whether this unit failed to define.
        /// </summary>
        bool failedToDefine { get; }

        /// <summary>
        /// Gets the exception that occurred during the definition process, if the unit failed to define.
        /// </summary>
        Exception definitionException { get; }

        /// <summary>
        /// Defines this unit by setting up its ports, connections, and other requirements.
        /// </summary>
        void Define();

        /// <summary>
        /// Ensures that this unit is properly defined, invoking definition if necessary.
        /// </summary>
        void EnsureDefined();

        /// <summary>
        /// Removes any unconnected invalid ports from this unit.
        /// </summary>
        void RemoveUnconnectedInvalidPorts();

        #endregion

        #region Default Values

        /// <summary>
        /// Gets a dictionary of default values used by the unit.
        /// The keys represent port names, and the values represent their corresponding defaults.
        /// </summary>
        Dictionary<string, object> defaultValues { get; }

        #endregion

        #region Ports

        /// <summary>
        /// Gets the collection of control input ports for this unit.
        /// </summary>
        IUnitPortCollection<ControlInput> controlInputs { get; }

        /// <summary>
        /// Gets the collection of control output ports for this unit.
        /// </summary>
        IUnitPortCollection<ControlOutput> controlOutputs { get; }

        /// <summary>
        /// Gets the collection of value input ports for this unit.
        /// </summary>
        IUnitPortCollection<ValueInput> valueInputs { get; }

        /// <summary>
        /// Gets the collection of value output ports for this unit.
        /// </summary>
        IUnitPortCollection<ValueOutput> valueOutputs { get; }

        /// <summary>
        /// Gets the collection of invalid input ports for this unit, which represent connections with unresolved compatibility issues.
        /// </summary>
        IUnitPortCollection<InvalidInput> invalidInputs { get; }

        /// <summary>
        /// Gets the collection of invalid output ports for this unit, which represent connections with unresolved compatibility issues.
        /// </summary>
        IUnitPortCollection<InvalidOutput> invalidOutputs { get; }

        /// <summary>
        /// Gets all input ports (both valid and invalid) for this unit.
        /// </summary>
        IEnumerable<IUnitInputPort> inputs { get; }

        /// <summary>
        /// Gets all output ports (both valid and invalid) for this unit.
        /// </summary>
        IEnumerable<IUnitOutputPort> outputs { get; }

        /// <summary>
        /// Gets the valid input ports for this unit.
        /// </summary>
        IEnumerable<IUnitInputPort> validInputs { get; }

        /// <summary>
        /// Gets the valid output ports for this unit.
        /// </summary>
        IEnumerable<IUnitOutputPort> validOutputs { get; }

        /// <summary>
        /// Gets all ports (valid and invalid) for this unit.
        /// </summary>
        IEnumerable<IUnitPort> ports { get; }

        /// <summary>
        /// Gets the invalid ports for this unit.
        /// </summary>
        IEnumerable<IUnitPort> invalidPorts { get; }

        /// <summary>
        /// Gets the valid ports for this unit.
        /// </summary>
        IEnumerable<IUnitPort> validPorts { get; }

        /// <summary>
        /// Called to notify that ports in the unit have changed.
        /// </summary>
        void PortsChanged();

        /// <summary>
        /// Event triggered when ports in the unit have changed.
        /// </summary>
        event Action onPortsChanged;

        #endregion

        #region Connections

        /// <summary>
        /// Gets the collection of port-to-port relationships (or connections) in this unit.
        /// </summary>
        IConnectionCollection<IUnitRelation, IUnitPort, IUnitPort> relations { get; }

        /// <summary>
        /// Gets all connections associated with this unit.
        /// </summary>
        IEnumerable<IUnitConnection> connections { get; }

        #endregion

        #region Analysis

        /// <summary>
        /// Gets a value indicating whether this unit is the control root of its graph.
        /// </summary>
        bool isControlRoot { get; }

        #endregion

        #region Widget

        /// <summary>
        /// Gets or sets the position of this unit in the graph editor interface.
        /// </summary>
        Vector2 position { get; set; }

        #endregion
    }

    public static class XUnit
    {
        public static ValueInput CompatibleValueInput(this IUnit unit, Type outputType)
        {
            Ensure.That(nameof(outputType)).IsNotNull(outputType);

            return unit.valueInputs
                .Where(valueInput => ConversionUtility.CanConvert(outputType, valueInput.type, false))
                .OrderBy((valueInput) =>
                {
                    var exactType = outputType == valueInput.type;
                    var free = !valueInput.hasValidConnection;

                    if (free && exactType)
                    {
                        return 1;
                    }
                    else if (free)
                    {
                        return 2;
                    }
                    else if (exactType)
                    {
                        return 3;
                    }
                    else
                    {
                        return 4;
                    }
                }).FirstOrDefault();
        }

        public static ValueOutput CompatibleValueOutput(this IUnit unit, Type inputType)
        {
            Ensure.That(nameof(inputType)).IsNotNull(inputType);

            return unit.valueOutputs
                .Where(valueOutput => ConversionUtility.CanConvert(valueOutput.type, inputType, false))
                .OrderBy((valueOutput) =>
                {
                    var exactType = inputType == valueOutput.type;
                    var free = !valueOutput.hasValidConnection;

                    if (free && exactType)
                    {
                        return 1;
                    }
                    else if (free)
                    {
                        return 2;
                    }
                    else if (exactType)
                    {
                        return 3;
                    }
                    else
                    {
                        return 4;
                    }
                }).FirstOrDefault();
        }
    }
}
