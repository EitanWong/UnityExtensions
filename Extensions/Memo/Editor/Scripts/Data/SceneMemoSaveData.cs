using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityExtensions.Memo;

internal class SceneMemoSaveData : ScriptableObject {

    [SerializeField]
    public List<SceneMemoScene> Scene  = new List<SceneMemoScene>();

    //======================================================================
    // public
    //======================================================================

    public void AddSceneMemo( GameObject obj, int localIdentifierInFile ) {
        var guid = AssetDatabase.AssetPathToGUID( obj.scene.path );
        if ( string.IsNullOrEmpty( guid ) ) {
            Debug.LogWarning( "添加备忘录前请先保存当前场景." );
            return;
        }
        var scene = getSceneFromGuid( guid );
        scene.AddMemo( obj, localIdentifierInFile );
    }

    public SceneMemo GetSceneMemo( GameObject obj, int localIdentifierInFile ) {
        var guid = AssetDatabase.AssetPathToGUID( obj.scene.path );
        if ( string.IsNullOrEmpty( guid ) )
            return null;

        var scene = GetScene( guid );
        if ( scene == null )
            return null;
        return scene.GetMemo( obj );
    }

    public SceneMemo GetSceneMemo( string scenePath, int localIdentifierInFile ) {
        var guid = AssetDatabase.AssetPathToGUID( scenePath );
        if ( string.IsNullOrEmpty( guid ) )
            return null;

        var scene = GetScene( guid );
        if ( scene == null )
            return null;
        return scene.GetMemo( localIdentifierInFile );
    }

    public SceneMemoScene GetScene( string guid ) {
        return Scene.FirstOrDefault( m => m == guid );
    }

    //======================================================================
    // private
    //======================================================================

    private SceneMemoScene getSceneFromGuid( string guid ) {
        var scene = GetScene( guid );
        if ( scene != null )
            return scene;

        return addSceneData( guid );
    }

    private SceneMemoScene addSceneData( string guid ) {
        var sceneData = new SceneMemoScene( guid );
        Scene.Add( sceneData );
        return sceneData;
    }

}
