﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using NetConsole.Core.Actions;
using NetConsole.Core.Commands;
using NetConsole.Core.Caching;
using NetConsole.Core.Factories;
using NetConsole.Core.Grammar;
using NetConsole.Core.Managers;
using NUnit.Framework;

namespace NetConsole.Core.Tests
{
    [TestFixture]
    public class CommandGrammarTest
    {

        private CommandExtractor _extractor;
        private CommandGrammarLexer _lexer;
        private CommonTokenStream _tokens;
        private CommandGrammarParser _parser;
        private CommandCache _cache;

        [SetUp]
        public void SetUp()
        {
            _cache = CommandCache.GetEmptyCache();
            _cache.RegisterAll(new CommandFactory());
            _extractor = new CommandExtractor(_cache);
        }

        [Test]
        public void Test_DefaultActionEcho()
        {
            // Act
            var outputs = Connect("echo \"Hello World\" ");

            // Assert
            Assert.AreEqual(1, outputs.Length);
            Assert.AreEqual(0, _extractor.LastOperationStatus);
            Assert.AreEqual(ReflectorHelper.GetActionName((EchoCommand cmd) => cmd.Echoed()), outputs[0].Name);
        }

        [Test]
        public void Test_DefaultActionPrompt()
        {
            // Act
            var outputs = Connect("prompt");
            
            // Assert
            Assert.AreEqual(1, outputs.Length);
            Assert.AreEqual(0, _extractor.LastOperationStatus);
            Assert.AreEqual(ReflectorHelper.GetActionName((PromptCommand cmd) => cmd.Get()), outputs[0].Name);
        }

        [Test]
        public void Test_SetActionPrompt()
        {
            // Act
            var outputs = Connect(@"prompt : set ""^"" ");

            // Assert
            Assert.AreEqual(1, outputs.Length);
            Assert.AreEqual(0, _extractor.LastOperationStatus);
            Assert.AreEqual(ReflectorHelper.GetActionName((PromptCommand cmd) => cmd.Set(null)), outputs[0].Name);
        }

        [Test]
        public void Test_EchoedActionEchoCommand()
        {
            // Act
            var outputs = Connect("echo:echoed Hello my Dear");

            // Assert
            Assert.AreEqual(1, outputs.Length);
            Assert.AreEqual(0, _extractor.LastOperationStatus);
            Assert.AreEqual(ReflectorHelper.GetActionName((EchoCommand cmd) => cmd.Echoed()), outputs[0].Name);
        }

        [Test]
        public void Test_AndOperator()
        {
            // Act
            var outputs = Connect("echo Testing and operator && prompt");

            // Assert
            Assert.AreEqual(2, outputs.Length);
            Assert.AreEqual(0, _extractor.LastOperationStatus);
            Assert.AreEqual(ReflectorHelper.GetActionName((EchoCommand cmd) => cmd.Echoed()), outputs[0].Name);
            Assert.AreEqual(ReflectorHelper.GetActionName((PromptCommand cmd) => cmd.Get()), outputs[1].Name);
        }

        [Test]
        public void Test_AndOperatorFail()
        {
            // Act
            var outputs = Connect("echo && prompt");

            // Assert
            Assert.AreEqual(1, outputs.Length);
            Assert.AreEqual(1, _extractor.LastOperationStatus);
            Assert.AreEqual(1, outputs[0].Status);
            Assert.AreEqual("There is not any compatible action for this command.", outputs[0].Message);
        }

        [Test]
        public void Test_OrOperatorFailFirst()
        {
            var outputs = Connect("prompt : which abfia || echo \"Previous command was wrong!!\" ");

            Assert.AreEqual(2, outputs.Length);
            Assert.AreEqual(0, _extractor.LastOperationStatus);
            Assert.AreEqual("There is not any compatible action for this command.", outputs[0].Message);
            Assert.AreEqual(ReflectorHelper.GetActionName((EchoCommand cmd) => cmd.Echoed(null)), outputs[1].Name);
        }

        [Test]
        public void Test_PipeOperator()
        {
            // Act
            var outputs = Connect("prompt : set amour | echo : echoed");

            // Assert
            Assert.AreEqual(1, outputs.Length);
            Assert.AreEqual(0, _extractor.LastOperationStatus);
            Assert.AreEqual(ReflectorHelper.GetActionName((EchoCommand c) => c.Echoed(null)), outputs[0].Name);
        }

        [Test]
        public void Test_DoublePipe()
        {
            // Act
            var outputs = Connect("echo true | echo : echoed | prompt : set");

            // Asert
            Assert.AreEqual(1, outputs.Length);
            Assert.AreEqual(0, _extractor.LastOperationStatus);
            Assert.AreEqual(ReflectorHelper.GetActionName((PromptCommand c) => c.Set(null)), outputs[0].Name);
        }

        [Test]
        public void Test_PipeFail()
        {
            // Act
            var outputs = Connect("prompt:avi hello | echo:echoed");

            // Assert
            Assert.AreEqual(1, outputs.Length);
            Assert.AreEqual(1, outputs[0].Status);
            Assert.AreEqual(1, _extractor.LastOperationStatus);
        }

        [Test]
        public void Test_HelpOption()
        {
            // Act
            var output = Connect("echo --help");

            // Assert
            Assert.NotNull(output);
            Assert.AreEqual(1, output.Length);
            Assert.AreEqual(0, output[0].Status);
            Assert.AreEqual("Help", output[0].Name);
        }

        [Test]
        public void Test_ListOption()
        {
            // Act
            var output = Connect("prompt --list");

            // Assert
            Assert.NotNull(output);
            Assert.AreEqual(1, output.Length);
            Assert.AreEqual(0, output[0].Status);
            Assert.AreEqual("List", output[0].Name);
        }

        [Test]
        public void Test_HelpOverridesCommandExecution()
        {
            // Act
            var output = Connect("echo hello world --help");

            // Assert
            Assert.AreEqual(1, output.Length);
            Assert.AreEqual(0, output[0].Status);
            Assert.AreEqual("Help", output[0].Name);
        }

        private CommandAction[] Connect(string input)
        {
            _lexer = new CommandGrammarLexer(new AntlrInputStream(input));
            _tokens = new CommonTokenStream(_lexer);
            _parser = new CommandGrammarParser(_tokens);
            var tree = _parser.compile();
            return _extractor.Visit(tree);
        }
    }
}
