using System;
using UnityEngine;

namespace UnityExtensions.EditorBase.ContentDrawer.Attribute
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class CustomContent : PropertyAttribute
    {
        public string ContentName { get; private set; }

        public CustomContent(string contentName)
        {
            this.ContentName = contentName;
        }
    }
}