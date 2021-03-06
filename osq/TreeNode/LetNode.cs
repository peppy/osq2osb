﻿using System;
using System.IO;

namespace osq.TreeNode {
    [DirectiveAttribute("let")]
    internal class LetNode : DirectiveNode {
        public string Variable {
            get;
            private set;
        }

        public LetNode(DirectiveInfo info) :
            base(info) {
            Token token = Token.ReadToken(info.ParametersReader);

            if(token == null) {
                throw new MissingDataException("Variable name", info.ParametersReader.Location);
            }

            if(token.TokenType != TokenType.Identifier) {
                throw new MissingDataException("Variable name", token.Location);
            }

            Variable = token.Value.ToString();

            info.ParametersReader.SkipWhiteSpace();

            foreach(var node in (new Parser(info.Parser, info.ParametersReader)).ReadNodes()) {
                ChildrenNodes.Add(node);
            }
        }

        protected override bool EndsWith(NodeBase node) {
            if(ChildrenNodes.Count != 0 && node == this) {
                return true;
            }

            var endDirective = node as EndDirectiveNode;

            return endDirective != null && endDirective.TargetDirectiveName == DirectiveName;
        }

        public override string Execute(ExecutionContext context) {
            string varValue = ExecuteChildren(context);

            context.SetVariable(Variable, varValue.Trim(Environment.NewLine.ToCharArray()));

            return "";
        }
    }
}