using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CameraData : EditorWindow
{
    //Unity 2020.3.11f1

    [MenuItem("Window/SceneCameraTool")]
    static void OpenWidnow()
    {
        GetWindow<CameraData>("CameraTool");
    }

    //position rotation position_name color
    List<Vector3> pos = new List<Vector3>();
    List<Quaternion> rot = new List<Quaternion>();
    List<string> data_name = new List<string>();
    List<Color> col = new List<Color>();
    //textField
    string[] txt = new string[2];
    Vector2 scrollPosition;
    //icon images
    Texture2D[] tex;
    bool isLoaded = false;
    string path = "Assets/Editor/camData.asset";
    int selectDataNum = 0;
    int dataNum = 0;

    void OnEnable()
    {
        if(tex == null)
        {
            tex = new Texture2D[6];
            tex[0] = EditorGUIUtility.Load("TreeEditor.Trash") as Texture2D;
            tex[1] = EditorGUIUtility.Load("SaveActive") as Texture2D;
            tex[2] = EditorGUIUtility.Load("NavMeshAgent Icon") as Texture2D;
            tex[3] = EditorGUIUtility.Load("Transform Icon") as Texture2D;
            tex[4] = EditorGUIUtility.Load("winbtn_mac_max_h") as Texture2D;
            tex[5] = EditorGUIUtility.Load("winbtn_mac_close_h") as Texture2D;
        }

        if(!isLoaded)
        {
            isLoaded = true;

            LoadFromScriptable();
        }
    }

    void OnGUI()
    {
        //about save data
        using(new GUILayout.HorizontalScope())
        {
            string[] dataName = new string[dataNum];
            for(int i = 0;i < dataName.Length;i++) dataName[i] = cu.datas[i].saveData_name;
            if(dataName.Length == 0) dataName = new string[]{"Please Make Save Data"};

            EditorGUI.BeginChangeCheck();
            selectDataNum = EditorGUILayout.Popup("SaveDatas", selectDataNum, dataName, EditorStyles.toolbarPopup);
            if(EditorGUI.EndChangeCheck()) SaveDataChange(selectDataNum);

            txt[1] = EditorGUILayout.TextField(txt[1]);

            using (new EditorGUI.DisabledGroupScope(string.IsNullOrWhiteSpace(txt[1])))
            if(GUILayout.Button(new GUIContent(tex[4], "Make Save Data\n(must write the name to use)")))
            {
                MakeSaveData();
            }

            using (new EditorGUI.DisabledGroupScope(dataNum == 0))
            {
                if(GUILayout.Button(new GUIContent(tex[5], "Delete current Save Data")))
                {
                    if(EditorUtility.DisplayDialog("", "Delete " + dataName[selectDataNum] + " ?", "Yes", "No"))
                    {
                        DeleteSaveData();
                    }
                }
            }
        }

        EditorGUILayout.Space(2);

        //about Align
        using(new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button(new GUIContent("Scene → Game", "Align the Scene Camera to the Game Camera")))
            {
                SceneView sceneView = SceneView.lastActiveSceneView;
                Transform gameCamera = Camera.main.transform;
                sceneView.AlignViewToObject(gameCamera.transform);
            }

            if (GUILayout.Button(new GUIContent("Game → Scene", "Align the Game Camera(Camera's root) to the Scene Camera")))
            {
                SceneView sceneView = SceneView.lastActiveSceneView;
                Transform gameCamera = Camera.main.transform.root;   //Camera's root
                gameCamera.transform.position = sceneView.camera.transform.position;
                gameCamera.transform.rotation = sceneView.rotation;
            }
        }

        //about to save position
        using(new GUILayout.HorizontalScope())
        {
            txt[0] = EditorGUILayout.TextField("Position Name", txt[0]);

            using (new EditorGUI.DisabledGroupScope(dataNum == 0))
            {
                if(GUILayout.Button(new GUIContent("Save", tex[3], "Save the Position of current Scene Camera"), GUILayout.Width(80), GUILayout.Height(18)))
                {
                    SaveAs();
                    GUI.FocusControl("");
                }
            }
        }

        EditorGUILayout.Space(5);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        //about position info
        for(int i = 0;i < pos.Count;i++) {
            using(new GUILayout.HorizontalScope())
            {
                using(new GUILayout.VerticalScope())
                {
                    using(new GUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        col[i] = EditorGUILayout.ColorField(GUIContent.none, col[i], false, false, false, GUILayout.Width(20), GUILayout.Height(24));

                        data_name[i] = EditorGUILayout.TextField(data_name[i], GUI.skin.box, GUILayout.Height(20));
                        if(EditorGUI.EndChangeCheck()) SaveToScriptable();
                    }

                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.Vector3Field("Position", pos[i]);
                        EditorGUILayout.Vector3Field("Rotation", rot[i].eulerAngles);
                    }
                }
                
                //---Button---
                if(GUILayout.Button(new GUIContent(tex[2], "Go"), GUILayout.Width(60), GUILayout.Height(102)))
                {
                    SceneView sceneView = SceneView.lastActiveSceneView;
                    GameObject o = new GameObject();
                    o.transform.position = pos[i];
                    o.transform.rotation = rot[i];
                    sceneView.AlignViewToObject(o.transform);
                    DestroyImmediate(o);
                }

                using(new GUILayout.VerticalScope())
                {
                    if(GUILayout.Button(new GUIContent(tex[0], "Delete"), GUILayout.Width(30), GUILayout.Height(30)))
                    {
                        if(EditorUtility.DisplayDialog("", "Delete " + data_name[i] + " ?", "Yes", "No"))
                        {
                            RemoveData(i);
                        }
                    }

                    if(GUILayout.Button(new GUIContent(tex[1], "Overwrite the Position, Rotation"), GUILayout.Width(30), GUILayout.Height(30)))
                    {
                        SceneView sceneView = SceneView.lastActiveSceneView;
                        pos[i] = sceneView.camera.transform.position;
                        rot[i] = sceneView.rotation;

                        SaveToScriptable();
                    }

                    using(new EditorGUI.DisabledGroupScope(i == 0))
                    {
                        if(GUILayout.Button(new GUIContent("↑", "Move Up"), GUILayout.Width(30), GUILayout.Height(18)))
                        {
                            DataSwap(i, i - 1);
                        }
                    }
                    using(new EditorGUI.DisabledGroupScope(i == (pos.Count - 1)))
                    {
                        if(GUILayout.Button(new GUIContent("↓", "Move Down"), GUILayout.Width(30), GUILayout.Height(18)))
                        {
                            DataSwap(i, i + 1);
                        }
                    }
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    //save position
    void SaveAs()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        pos.Add(sceneView.camera.transform.position);
        rot.Add(sceneView.rotation);
        data_name.Add(txt[0]);
        col.Add(Color.white);
        txt[0] = "";

        SaveToScriptable();
    }

    //delete position data
    void RemoveData(int i)
    {
        pos.RemoveAt(i);
        rot.RemoveAt(i);
        data_name.RemoveAt(i);
        col.RemoveAt(i);

        SaveToScriptable();
    }

    //reset position data
    void ResetData()
    {
        pos.Clear();
        rot.Clear();
        data_name.Clear();
        col.Clear();
    }

    //swap position data
    void DataSwap(int i, int j)
    {
        Vector3 vtmp = pos[i];
        pos[i] = pos[j];
        pos[j] = vtmp;

        Quaternion qtmp = rot[i];
        rot[i] = rot[j];
        rot[j] = qtmp;

        string ntmp = data_name[i];
        data_name[i] = data_name[j];
        data_name[j] = ntmp;

        Color ctmp = col[i];
        col[i] = col[j];
        col[j] = ctmp;
    }

    //import position data
    void SaveDataChange(int i)
    {
        ResetData();

        if(dataNum == 0) {
            return;
        }

        pos.AddRange(cu.datas[i].pos);
        rot.AddRange(cu.datas[i].rot);
        data_name.AddRange(cu.datas[i].data_name);
        col.AddRange(cu.datas[i].col);
    }

    SceneCameraTool cu;

    //make scriptable file
    void MakeScriptable()
    {
        cu = ScriptableObject.CreateInstance<SceneCameraTool>();
        AssetDatabase.CreateAsset(cu, path);
        cu = AssetDatabase.LoadAssetAtPath<SceneCameraTool>(path);
    }

    //write data to file
    void SaveToScriptable()
    {
        if(dataNum == 0) return;

        cu.SetAt(selectDataNum, pos.ToArray(), rot.ToArray(), data_name.ToArray(), col.ToArray());

        EditorUtility.SetDirty(cu);
    }

    //create save data
    void MakeSaveData()
    {
        cu.AddEmptyData(txt[1]);

        txt[1] = "";
        GUI.FocusControl("");

        if(dataNum > 0) selectDataNum++;
        dataNum++;

        SaveToScriptable();
    }

    //delete selected save data
    void DeleteSaveData()
    {
        cu.RemoveAt(selectDataNum);

        if((selectDataNum + 1) == dataNum && selectDataNum > 0) selectDataNum--;
        dataNum--;

        SaveDataChange(selectDataNum);
        SaveToScriptable();
    }

    //load from file
    void LoadFromScriptable()
    {
        cu = AssetDatabase.LoadAssetAtPath<SceneCameraTool>(path);

        if(cu == null) MakeScriptable();

        dataNum = cu.datas.Count;
        if(cu.datas.Count == 0) return;
        
        pos.AddRange(cu.datas[selectDataNum].pos);
        rot.AddRange(cu.datas[selectDataNum].rot);
        data_name.AddRange(cu.datas[selectDataNum].data_name);
        col.AddRange(cu.datas[selectDataNum].col);
    }
}


public class SceneCameraTool : ScriptableObject
{
    [System.Serializable]
    public struct Datas
    {
        public Vector3[] pos;
        public Quaternion[] rot;
        public string[] data_name;
        public Color[] col;
        public string saveData_name;
    }

    //data list
    public List<Datas> datas = new List<Datas>();
    
    //save
    public void SetAt(int i, Vector3[] pos, Quaternion[] rot, string[] data_name, Color[] col)
    {
        Datas d = datas[i];
        d.pos = pos;
        d.rot = rot;
        d.data_name = data_name;
        d.col = col;
        datas[i] = d;
    }

    //add empty save data at first
    public void AddEmptyData(string saveData_name)
    {
        Datas d = new Datas();
        d.pos = new Vector3[0];
        d.rot = new Quaternion[0];
        d.data_name = new string[0];
        d.col = new Color[0];
        d.saveData_name = saveData_name;
        datas.Insert(0, d);
    }

    //remove save data
    public void RemoveAt(int i)
    {
        datas.RemoveAt(i);
    }
}