﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace osq2osb.Parser.TreeNode {
    class EachNode : DirectiveNode {
        public string Variable {
            get;
            private set;
        }

        public TokenNode Values {
            get;
            private set;
        }

        public EachNode(DirectiveInfo info) :
            base(info) {
            Token token = Token.ReadToken(info.ParametersReader);

            if(token == null) {
                throw new ParserException("Need a variable name for #let", info.ParametersReader.Location);
            }

            if(token.Type != TokenType.Identifier) {
                throw new ParserException("Need a variable name for #let", token.Location);
            }

            this.Variable = token.Value.ToString();

            this.Values = ExpressionRewriter.Rewrite(Token.ReadTokens(info.ParametersReader));
        }

        protected override bool EndsWith(NodeBase node) {
            var endDirective = node as EndDirectiveNode;

            if(endDirective != null && endDirective.TargetDirectiveName == this.DirectiveName) {
                return true;
            }

            return false;
        }

        public override void Execute(TextWriter output, ExecutionContext context) {
            object expr = Values.Evaluate(context);

            object[] values = expr as object[];

            if(values == null) {
                values = new object[] { expr };
            }

            foreach(var value in values) {
                context.SetVariable(Variable, value);

                ExecuteChildren(output, context);
            }
        }
    }
}
