/*
Copyright (c) 2017 Boštjan Golob, Adacta d.o.o.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.AX.Metadata.XppCompiler;
using System.Xml.Linq;

namespace SourceDependencyCheck_AD
{
    [DataContract]
    class SourceDependencyChanged : CustomDiagnosticItem
    {
        public const string DiagnosticMoniker = "SourceDependencyChanged";
        private const string ExpectedHashKey = "expectedHash";
        private const string CalculatedHashKey = "calculatedHash";

        [DataMember]
        public string ExpectedHash { get; private set; }

        [DataMember]
        public string CalculatedHash { get; private set; }

        public SourceDependencyChanged(XElement element)
            : base(element)
        {
        }

        public SourceDependencyChanged(string path, string elementType, TextPosition textPosition, string expectedHash, string calculatedHash)
            : base(path, elementType, textPosition, DiagnosticType.BestPractices, Severity.Error, DiagnosticMoniker, Messages.SourceDependencyChanged, expectedHash, calculatedHash)
        {
            // Validate parameters - calculated must always be specified, expected can be blank
            if (string.IsNullOrWhiteSpace(calculatedHash))
            {
                throw new ArgumentNullException("calculatedHash");
            }

            this.ExpectedHash = expectedHash;
            this.CalculatedHash = calculatedHash;
        }

        public SourceDependencyChanged(Stack<Ast> context, TextPosition textPosition, string expectedHash, string calculatedHash)
            : base(context, textPosition, DiagnosticType.BestPractices, Severity.Error, DiagnosticMoniker, Messages.SourceDependencyChanged, expectedHash, calculatedHash)
        {
            // Validate parameters - calculated must always be specified, expected can be blank
            if (string.IsNullOrWhiteSpace(calculatedHash))
            {
                throw new ArgumentNullException("calculatedHash");
            }

            this.ExpectedHash = expectedHash;
            this.CalculatedHash = calculatedHash;
        }

        protected override void ReadItemSpecificFields(XElement itemSpecificNode)
        {
            this.ExpectedHash = base.ReadCustomField(itemSpecificNode, ExpectedHashKey);
            this.CalculatedHash = base.ReadCustomField(itemSpecificNode, CalculatedHashKey);
        }

        protected override void WriteItemSpecificFields(XElement itemSpecificNode)
        {
            this.WriteCustomField(itemSpecificNode, ExpectedHashKey, this.ExpectedHash);
            this.WriteCustomField(itemSpecificNode, CalculatedHashKey, this.CalculatedHash);
        }
    }
}
