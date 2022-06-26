﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Variables
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A variable defined as a set of child key/value pairs (such as those destined to populate an INPUT_OBJECT).
    /// </summary>
    [DebuggerDisplay("InputFieldSet: {Name} (Count = {Fields.Count})")]
    public class InputFieldSetVariable : InputVariable, IInputFieldSetVariable, IResolvableFieldSet
    {
        private readonly Dictionary<string, IInputVariable> _fields;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputFieldSetVariable"/> class.
        /// </summary>
        /// <param name="name">The name of the variable as defined by the user.</param>
        public InputFieldSetVariable(string name)
            : base(name)
        {
            _fields = new Dictionary<string, IInputVariable>();
        }

        /// <summary>
        /// Adds the new variable as a field of this field set variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public void AddVariable(IInputVariable variable)
        {
            _fields.Add(variable.Name, variable);
        }

        /// <summary>
        /// Gets the dictionary of fields defined for this field set variable.
        /// </summary>
        /// <value>The fields.</value>
        public IReadOnlyDictionary<string, IInputVariable> Fields => _fields;

        /// <summary>
        /// Attempts to retrieve a field by its name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="field">The field that was found, if any.</param>
        /// <returns><c>true</c> if the field was found and successfully returned, <c>false</c> otherwise.</returns>
        public bool TryGetField(string fieldName, out IResolvableValueItem field)
        {
            field = null;
            var found = _fields.TryGetValue(fieldName, out var item);
            if (found)
                field = item;

            return found;
        }

        /// <summary>
        /// Gets the dictionary of fields defined for this field set variable.
        /// </summary>
        /// <value>The fields.</value>
        //public IEnumerable<IResolvableValueItem> Fields
        //{
        //    get
        //    {
        //        throw new System.Exception();
        //        foreach (var kvp in _fields)
        //        {
        //             //yield return new KeyValuePair<string, IResolvableValueItem>(kvp.Key, kvp.Value);
        //        }
        //    }
        //}
    }
}