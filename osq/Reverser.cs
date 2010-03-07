﻿using System;
using System.Collections.Generic;
using System.Text;
using osq.TreeNode;

namespace osq {
    internal class ConvertedNode {
        public string OriginalScript {
            get;
            set;
        }

        public NodeBase Node {
            get;
            set;
        }

        public string NodeOutput {
            get;
            set;
        }
    }

    public class Reverser {
        private readonly List<ConvertedNode> scriptNodes = new List<ConvertedNode>();

        public string Parse(LocatedTextReaderWrapper source) {
            return Parse(source, new ExecutionContext());
        }

        public string Parse(LocatedTextReaderWrapper source, ExecutionContext context) {
            scriptNodes.Clear();

            var output = new StringBuilder();

            using(var bufferingReader = new BufferingTextReaderWrapper(source))
            using (var myReader = new LocatedTextReaderWrapper(bufferingReader, source.Location)) { // Sorry we have to do this...
                NodeBase node;

                while((node = Parser.ReadNode(myReader)) != null) {
                    string curOutput = node.Execute(context);

                    output.Append(curOutput);

                    var converted = new ConvertedNode {
                        Node = node,
                        NodeOutput = curOutput,
                        OriginalScript = bufferingReader.BufferedText
                    };

                    scriptNodes.Add(converted);

                    bufferingReader.ClearBuffer();
                }
            }

            return output.ToString();
        }

        public string Reverse(string modifiedSource) {
            var output = new StringBuilder();

            foreach(var convertedNode in scriptNodes) {
                int index = modifiedSource.IndexOf(convertedNode.NodeOutput);

                if(index < 0) {
                    continue;
                }

                output.Append(modifiedSource.Substring(0, index));
                output.Append(convertedNode.OriginalScript);

                modifiedSource = modifiedSource.Substring(index + convertedNode.NodeOutput.Length);
            }

            output.Append(modifiedSource);

            return output.ToString();
        }
    }
}
