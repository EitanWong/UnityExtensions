// Copyright(c) 2017 Untitled Games   | Developed by: Chris Bellini                    | http://untitledgam.es/contact
// http://transformpro.untitledgam.es | http://transformpro.untitledgam.es/quick-start | http://transformpro.untitledgam.es/api

namespace TransformPro.Scripts
{
    using UnityEngine;

    /// <summary>
    ///     Exposes string providers for a requested language. You can get the system language provider from here too.
    /// </summary>
    public static class TransformProStrings
    {
        /// <summary>
        ///     The english language string provider.
        /// </summary>
        private static TransformProStringsEnglish english;

        /// <summary>
        ///     Gets the english language string provider.
        /// </summary>
        public static TransformProStringsEnglish English { get { return TransformProStrings.english ?? (TransformProStrings.english = new TransformProStringsEnglish()); } }

        /// <summary>
        ///     Gets the string provider for the current system language. If the language is not available English is used as a
        ///     fall back.
        /// </summary>
        public static TransformProStringsBase SystemLanguage
        {
            get
            {
                switch (Application.systemLanguage)
                {
                    case UnityEngine.SystemLanguage.English:
                        return TransformProStrings.English;

                    default:
                    case UnityEngine.SystemLanguage.Unknown:
                        // Unknown or unhandled language
                        return TransformProStrings.English;
                }
            }
        }
    }
}