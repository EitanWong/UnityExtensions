using System.Collections.Generic;
using UnityEngine;
using UnityExtensions.Memo;

internal class MemoSaveData : ScriptableObject
{

    [SerializeField]
    public List<MemoCategory> Category = new List<MemoCategory>();
    public string[] LabelTag = new string[ 5 ];

    public void AddCategory( MemoCategory category )
    {
        Category.Add( category );
    }

    public void SortCategory()
    {
        Category.Sort( compareCategory );
    }

    private int compareCategory( MemoCategory a, MemoCategory b )
    {
        if ( a.Name.Equals( "default" ) )
            return 1;
        if ( b.Name.Equals( "default" ) )
            return -1;
        return string.Compare( a.Name, b.Name );
    }

}
