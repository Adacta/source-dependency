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
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.AX.Framework.BestPractices.Extensions;
using Microsoft.Dynamics.AX.Metadata.XppCompiler;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModelV2;
using System.Globalization;
using System.Security.Cryptography;

namespace SourceDependencyCheck_AD
{
    [BestPracticeRule(
        SourceDependencyChanged.DiagnosticMoniker,
        typeof(Messages),
        SourceDependencyChanged.DiagnosticMoniker + "Description",
        BestPracticeCheckerTargets.Class)]
    class SourceDependencyCheck : BestPracticeAstChecker<BestPracticeCheckerPayload>
    {        
        public SourceDependencyCheck()
        {
        }

        protected override object VisitMethod(BestPracticeCheckerPayload payload, Method method)
        {
            foreach (Microsoft.Dynamics.AX.Metadata.XppCompiler.Attribute attribute in method.Attributes.Attributes)
            {
                if (string.Compare(attribute.Name, "AdSourceDependency", true, CultureInfo.InvariantCulture) == 0 || 
                    string.Compare(attribute.Name, "AdSourceDependencyAttribute", true, CultureInfo.InvariantCulture) == 0)
                {
                    var hashLiteral = attribute.Parameters.ElementAt(2).Literal as IntrinsicAttributeLiteral;
                    var hashString = hashLiteral.Arg1 ?? default(string);

                    string methodSource = getMethodSource(attribute);                    
                    string md5MethodHash = getMethodHash(methodSource);

                    if (string.Compare(hashString, md5MethodHash, true, CultureInfo.InvariantCulture) != 0)
                    {
                        this.ExtensionContext.AddErrorMessage(new SourceDependencyChanged(this.Context, method.Position, hashString, md5MethodHash));
                    }
                }
            }

            return base.VisitMethod(payload, method);
        }

        private string getMethodSource(Microsoft.Dynamics.AX.Metadata.XppCompiler.Attribute attribute)
        {
            var elementNameLiteral = attribute.Parameters.ElementAt(0).Literal as IntrinsicAttributeLiteral;
            var methodNameLiteral = attribute.Parameters.ElementAt(1).Literal as IntrinsicAttributeLiteral;            
            var metadataProvider = this.ExtensionContext.MetadataProvider;

            string methodSource = "";

            if (elementNameLiteral == null)
            {
                return methodSource;
            }

            var elementName = elementNameLiteral.Arg1;            

            if (string.Compare(elementNameLiteral.FunctionName, "classStr", true, CultureInfo.InvariantCulture) == 0)
            {
                // Check against a class                        
                AxClass depedentClass = metadataProvider.Classes.Read(elementName);

                AxMethod dependentMethod = depedentClass.Methods[methodNameLiteral.Arg2] ?? null;
                methodSource = dependentMethod.Source ?? "";
            }
            else if (string.Compare(elementNameLiteral.FunctionName, "tableStr", true, CultureInfo.InvariantCulture) == 0)
            {
                // Check against a table
                AxTable depedentTable = metadataProvider.Tables.Read(elementName);

                AxMethod dependentMethod = depedentTable.Methods[methodNameLiteral.Arg2] ?? null;
                methodSource = dependentMethod.Source ?? "";
            }
            else
            {
                // Unsupported yet
            }

            return methodSource;
        }

        private string getMethodHash(string methodSource)
        {
            string md5MethodHash;
            using (var md5 = MD5.Create())
            {
                byte[] hashVal = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(methodSource));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashVal.Length; i++)
                {
                    sb.Append(hashVal[i].ToString("x2"));
                }
                md5MethodHash = sb.ToString();
            }

            return md5MethodHash;
        }

    }
}
