using System.IO;
using System.Text;

namespace osq.TreeNode {
    [DirectiveAttribute("inc(lude)?")]
    internal class IncludeNode : DirectiveNode {
        public TokenNode Filename {
            get;
            private set;
        }

        private Parser parentParser;

        public IncludeNode(DirectiveInfo info) :
            base(info) {
            Filename = ExpressionRewriter.Rewrite(Token.ReadTokens(info.ParametersReader));

            this.parentParser = info.Parser;
        }

        protected override bool EndsWith(NodeBase node) {
            return node == this;
        }

        public override string Execute(ExecutionContext context) {
            var output = new StringBuilder();

            string filePath = Filename.Evaluate(context) as string;

            if(filePath == null) {
                throw new DataTypeException("Need string for filename", this);
            }

            if(Location != null && Location.FileName != null) {
                filePath = Path.GetDirectoryName(Location.FileName) + Path.DirectorySeparatorChar + filePath;
            }

            // TODO Clean up
            using(var inputFile = File.Open(filePath, FileMode.Open, FileAccess.Read)) {
                using(var reader = new LocatedTextReaderWrapper(inputFile, new Location(filePath), wrapperOwnsStream: false)) {
                    foreach(var node in (new Parser(parentParser, reader)).ReadNodes()) {
                        output.Append(node.Execute(context));
                    }
                }
            }

            if(!context.Dependencies.Contains(filePath)) {
                context.Dependencies.Add(filePath);
            }

            return output.ToString();
        }
    }
}