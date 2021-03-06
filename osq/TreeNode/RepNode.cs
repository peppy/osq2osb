﻿using System.Text;

namespace osq.TreeNode {
    [DirectiveAttribute("rep")]
    internal class RepNode : DirectiveNode {
        public TokenNode Value {
            get;
            private set;
        }

        public RepNode(DirectiveInfo info) :
            base(info) {
            Value = ExpressionRewriter.Rewrite(Token.ReadTokens(info.ParametersReader));
        }

        protected override bool EndsWith(NodeBase node) {
            var endDirective = node as EndDirectiveNode;

            return endDirective != null && endDirective.TargetDirectiveName == DirectiveName;
        }

        public override string Execute(ExecutionContext context) {
            var output = new StringBuilder();

            object value = Value.Evaluate(context);
            double count = (double)value;

            for(int i = 0; i < count; ++i) {
                output.Append(ExecuteChildren(context));
            }

            return output.ToString();
        }
    }
}