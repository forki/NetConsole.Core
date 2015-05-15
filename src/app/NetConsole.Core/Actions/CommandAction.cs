﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NetConsole.Core.Extensions;
using NetConsole.Core.Interfaces;

namespace NetConsole.Core.Actions
{
    public class CommandAction : IAction
    {

        # region Public Properties

        public ICommand Command { get; private set; }

        public int Status { get; private set; }

        public MethodInfo Body { get; private set; }
        
        public string Message { get; private set; }

        public object[] Arguments { get; set; }

        public string Name
        {
            get { return Body.Name; }
        }

        # endregion

        # region Constructors

        public CommandAction(string message,  int status, ICommand cmd = null, MethodInfo action = null, object[] args = null)
        {
            Status = status;
            Message = message;       
            if (status == 0 && action != null)
            {
                Command = cmd;
                Body = action;
                Arguments = args;
            }
        }

        public CommandAction(ICommand cmd, MethodInfo action, object[] args = null)
        {
            Message = "Command action for script";
            Status = 0;
            Body = action;
            Arguments = args;
            Command = cmd;
        }

        # endregion

        # region Public Methods

        public object[] Perform()
        {
            var output = Command.Perform(Body, Arguments);
            Status = Command.Status;
            return new[] {output};
        }

        # endregion
    }
}
