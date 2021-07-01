using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dialogue.Submodules.TextVariable;
using UnityEngine;

public class TimeTextVariable : TextVariable
{
    private string[] variableText = new[]
    {
        "NOW",
        "UTCNOW",
        "YEAR",
        "MONTH",
        "DAY",
        "HOUR",
        "MINUTE",
        "SECOND"
    };

    internal override bool Detect(string originText)
    {
        return variableText.Contains(originText.ToUpper());
    }

    internal override string Process(string originText)
    {
        switch (originText.ToUpper())
        {
            case "NOW":
                originText = System.DateTime.Now.ToString(CultureInfo.InvariantCulture);
                break;
            case "UTCNOW":
                originText = System.DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                break;
            case "YEAR":
                originText = System.DateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
                break;
            case "MONTH":
                originText = System.DateTime.Now.Month.ToString(CultureInfo.InvariantCulture);
                break;
            case "DAY":
                originText = System.DateTime.Now.Day.ToString(CultureInfo.InvariantCulture);
                break;
            case "HOUR":
                originText = System.DateTime.Now.Hour.ToString(CultureInfo.InvariantCulture);
                break;
            case "MINUTE":
                originText = System.DateTime.Now.Minute.ToString(CultureInfo.InvariantCulture);
                break;
            case "SECOND":
                originText = System.DateTime.Now.Second.ToString(CultureInfo.InvariantCulture);
                break;
        }
        return originText;
    }
}