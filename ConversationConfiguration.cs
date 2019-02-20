
using System;
using System.Globalization;
using Newtonsoft.Json;

namespace HackedDesign.ClaireBot
{
    public class ConversationConfiguration
    {
        public ConversationConfiguration()
        : this(CultureInfo.CurrentCulture)
        {
            Console.WriteLine("Test");
        }

        public ConversationConfiguration(CultureInfo culture)
        {
            Console.WriteLine(culture.TwoLetterISOLanguageName);
        }


    };

    public class IntentConfiguration
    {
        public string intent;
        public string description;
        public string dialogue;

    };

    public class DialogueConfiguration
    {
        public string id;
        public string description;
        public string next;
    };
}