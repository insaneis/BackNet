﻿using System.Collections.Generic;
using Shared;

namespace ClientCommands
{
    public interface IClientCommand : ICommand
    {
        /// <summary>
        /// Complete description of the command,
        /// doesn't include the syntax, it will be added with the syntaxHelper variable
        /// </summary>
        string description { get; }


        /// <summary>
        /// Correct syntax to use (example: upload [filePath])
        /// Separate each possible syntax with a '\n'
        /// </summary>
        string syntaxHelper { get; }


        /// <summary>
        /// Is the command only executed locally or also from the server side ?
        /// </summary>
        bool isLocal { get; }


        /// <summary>
        /// List of all possible arguments combinaisons, null if none.
        /// Explicitly name the argument, or '?' for a string, or '0' for an integer
        /// </summary>
        List<string> validArguments { get; }
    }
}