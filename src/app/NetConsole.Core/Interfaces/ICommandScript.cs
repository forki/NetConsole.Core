﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetConsole.Core.Actions;
using NetConsole.Core.Grammar;

namespace NetConsole.Core.Interfaces
{
    public interface ICommandScript : IRegistrable
    {

        IAction[] Actions { get; }

    }
}
