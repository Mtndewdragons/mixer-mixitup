﻿using Jace;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class SpecialIdentifierAction : ActionBase
    {
        private const string TextProcessorFunctionRegexPatternFormat = "{0}\\([^)]+\\)";

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return SpecialIdentifierAction.asyncSemaphore; } }

        [DataMember]
        public string SpecialIdentifierName { get; set; }

        [DataMember]
        public string SpecialIdentifierReplacement { get; set; }

        [DataMember]
        public bool MakeGloballyUsable { get; set; }

        [DataMember]
        public bool SpecialIdentifierShouldProcessMath { get; set; }

        public SpecialIdentifierAction()
            : base(ActionTypeEnum.SpecialIdentifier)
        {
            this.MakeGloballyUsable = true;
        }

        public SpecialIdentifierAction(string specialIdentifierName, string specialIdentifierReplacement, bool makeGloballyUsable, bool specialIdentifierShouldProcessMath)
            : this()
        {
            this.SpecialIdentifierName = specialIdentifierName;
            this.SpecialIdentifierReplacement = specialIdentifierReplacement;
            this.MakeGloballyUsable = makeGloballyUsable;
            this.SpecialIdentifierShouldProcessMath = specialIdentifierShouldProcessMath;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            string replacementText = await this.ReplaceStringWithSpecialModifiers(this.SpecialIdentifierReplacement, user, arguments);

            if (this.SpecialIdentifierShouldProcessMath)
            {
                try
                {
                    // Process Math
                    CalculationEngine engine = new CalculationEngine(new System.Globalization.CultureInfo("en-US"));
                    engine.AddFunction("random", Random);
                    engine.AddFunction("randomrange", RandomRange);

                    double result = engine.Calculate(replacementText);
                    replacementText = result.ToString();
                }
                catch (Exception ex)
                {
                    // Calculation failed, log and set to 0
                    Logger.Log(ex, false, false);
                    replacementText = "0";
                }
            }
            else
            {
                replacementText = await this.ProcessStringFunction(replacementText, "removespaces", (text) => { return Task.FromResult(text.Replace(" ", string.Empty)); });
                replacementText = await this.ProcessStringFunction(replacementText, "tolower", (text) => { return Task.FromResult(text.ToLower()); });
                replacementText = await this.ProcessStringFunction(replacementText, "toupper", (text) => { return Task.FromResult(text.ToUpper()); });
            }

            if (this.MakeGloballyUsable)
            {
                SpecialIdentifierStringBuilder.AddCustomSpecialIdentifier(this.SpecialIdentifierName, replacementText);
            }
            else
            {
                this.extraSpecialIdentifiers[this.SpecialIdentifierName] = replacementText;
            }
        }

        private async Task<string> ProcessStringFunction(string text, string functionName, Func<string, Task<string>> processor)
        {
            foreach (Match match in Regex.Matches(text, string.Format(TextProcessorFunctionRegexPatternFormat, functionName)))
            {
                string textToProcess = match.Value.Substring(functionName.Length + 1);
                textToProcess = textToProcess.Substring(0, textToProcess.Length - 1);
                text = text.Replace(match.Value, await processor(textToProcess));
            }
            return text;
        }
        
        private double Random(double max)
        {
            return this.RandomRange(1, max);
        }

        private double RandomRange(double min, double max)
        {
            return RandomHelper.GenerateRandomNumber((int)min, (int)max);
        }
    }
}
